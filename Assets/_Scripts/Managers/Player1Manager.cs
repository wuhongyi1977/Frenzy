﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class Player1Manager : MonoBehaviour {
	//The total number of shuffles when the game starts
	private int maxInitialShuffles = 10;
	//The number of times the deck is shuffled before game starts
	private int numbShuffles;
	//The player ID
	private static int playerID = 1;
	//Player's health
	public int health = 20;
	//the maximum number of cards a player can hold
	public int maxHandSize = 7;
	//the number of cards to draw at the start of the game
	public int startingHandSize = 5;
	//The speed in seconds in which the player draws a card from the deck
	public int libraryDrawSpeed = 7;
	//the number of cards in a player's deck
	public int deckSize = 60;
	//The empty game object for the position of the handzone
	public GameObject handZone;
	//The list of summoning zones
	public List<GameObject> SummonZones = new List<GameObject> (3);
	//The list that represents the player's hand
	public List<GameObject> playerHand = new List<GameObject> (7);
	//The list that represents the player's graveyard
	public List<GameObject> graveyard = new List<GameObject> (60);
	//The list that represents the player's library
	public List<GameObject> library = new List<GameObject> (60);
	//the next card to draw from the deck (index of decksize array)
	int currentCardIndex = 0;
	int currentHandSize = 0;
	float libraryDrawCounterTimer = 0;
	//the player's deck, this gets loaded into the list at start up in a shuffled manner
	public GameObject[] cardDeck;
	//The game manager object
	GameManager gameManager;
	//The position of the graveyard. It is off screen so that that player doesn't see it but the player won't 
	//be able to interact directly with the cards in the graveyard
	Vector3 graveyardPos;
	//The card that the player is moused over
	private GameObject mousedOverCard;
	private GameObject line;
	private GameObject enemyObjectUnderMouse;
    //NETWORK COMPONENTS
    PhotonView photonView;

	// Use this for initialization
	void Start () {
		line = GameObject.Find ("Player1Line");
        //get this manager's photon view
        photonView = GetComponent<PhotonView>();
		//Find the game manager
		gameManager = GameObject.Find ("GameManager").GetComponent<GameManager>();
		//get the position of the graveyard
		graveyardPos = GameObject.Find ("Player1Graveyard").transform.position;
		//Finds the summon zones
		GameObject[] zones = GameObject.FindGameObjectsWithTag("Player1SummonZone");
		//initialize player hand array to the hand size
		//cardDeck = new GameObject[deckSize];

		//Populate deck
		for (int i = 0; i < deckSize; i++) {
			library.Add (cardDeck [i]);
			library[i].GetComponent<Card> ().playerID = playerID;
			library[i].GetComponent<Card> ().cardNumber = i;
			//Put the card in the card deck
			//cardDeck [i] = card;
			//Set the card's ID to whomever it belongs too
			//cardDeck [i].GetComponent<Card> ().playerID = playerID;
			//Give the card an ID(currently all the same for all card since there is only one card going into the deck multiple times)
			//cardDeck [i].GetComponent<Card> ().cardNumber = i;
		}
		int numbShuffles = Random.Range(1, maxInitialShuffles);
		for(int i = 0; i < numbShuffles; i++)
			library = shuffleDeck(library);
		//draw a hand of (startingHandSize) cards
		for(int i = 0; i < startingHandSize; i++)
		{
			drawFromLibrary ();
		}
		//after this resolves, the player's hand should have 5 cards and the current card index 
		//should be at 5 (since indecies 0-4 have been taken)


		//Adds the summon zones to a list
		for (int i = 0; i < zones.Length; i++) 
		{
			//Set the ID of the zone to whomever it belongs too
			zones [i].GetComponent<SummonZone> ().playerID = playerID;
			SummonZones.Add (zones [i]);
		}

		//shuffle the deck


	}
	
	// Update is called once per frame
	void Update () 
	{
		libraryDrawCounterTimer += Time.deltaTime;
		//Debug.Log ("P1:" + libraryDrawCounterTimer);
		if (libraryDrawCounterTimer > libraryDrawSpeed) 
		{
			libraryDrawCounterTimer = 0;
            //draw a card
			drawFromLibrary ();
            //if connected to photon, make your network player
            //on your opponents screen draw a card
            /*
            if (PhotonNetwork.connected && !gameManager.versusAi)
            {
                photonView.RPC("drawFromLibrary", PhotonTargets.Others);
            }
            */
            //draw from the deck
        }
		/*
		Debug.Log ("HERE");
		//move cards down the hand
		for(int j = 0; j < playerHand.Count;j++)
		{
			//if(!playerHand[j].GetComponent<Card>().inSummonZone)
				playerHand[j].transform.position = new Vector3(handZone.transform.position.x+currentHandSize,handZone.transform.position.y, handZone.transform.position.z);
		}*/
	}

	//Method called for when a card is dropped
	public void cardIsDropped(GameObject card,Vector3 cardHandPos)
	{
		//Set the state of being dropped to false
		card.GetComponent<Card> ().setDroppedState(false);

		//Check which zone it was dropped on
		for (int i = 0; i < SummonZones.Count; i++) 
		{			
			//If the summoning zone isn't occupied
			if (!SummonZones [i].GetComponent<SummonZone> ().isOccupied) {
				//Get's the position of the zone
				Vector3 zonePosition = SummonZones [i].transform.position;
				//Checks if the card is within a square surrounding the zone
				if (card.transform.position.x > (zonePosition.x - 3) && card.transform.position.x < (zonePosition.x + 3)) {
					if (card.transform.position.y > (zonePosition.y - 3) && card.transform.position.y < (zonePosition.y + 3)) {
                        //Play card, pass the card, the position of the zone, and the index of the zone
                        PlayCard(card, zonePosition, i);
						shiftCardsDown ();
                        //NETWORK CODE
                        if (PhotonNetwork.connected && !gameManager.versusAi)
                        {
                            photonView.RPC("PlayCard", PhotonTargets.Others, photonView.viewID, card, zonePosition, i);
                        }
                        //END NETWORK CODE
                    }
				}				
			} 
		}

		//If the player picks up the card and drops it anywhere else the card will be placed back in the hand zone
		if(card.GetComponent<Card>().inSummonZone == false)
			card.transform.position = cardHandPos;
	}
	public void creatureCardIsDropped(GameObject card, Vector3 cardHandPos)
	{
		//card.transform.position = cardHandPos;

		Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
		RaycastHit2D hit=Physics2D.Raycast(rayPos, Vector2.zero, 0f);

		if (hit)
		{
			enemyObjectUnderMouse = hit.transform.gameObject;
			if (enemyObjectUnderMouse.tag == "CreatureCard") {
				if (enemyObjectUnderMouse.GetComponent<CreatureCard> ().inBattlefield) 
				{
					card.GetComponent<CreatureCard> ().creatureCanAttack = false;
					card.GetComponent<CreatureCard> ().health -= enemyObjectUnderMouse.GetComponent<CreatureCard> ().damageToDeal;
					enemyObjectUnderMouse.GetComponent<CreatureCard> ().health -= card.GetComponent<CreatureCard> ().damageToDeal;
					Debug.Log ("Your creature's health: " + card.GetComponent<CreatureCard> ().health);
					Debug.Log ("Enemy creature's health: " + enemyObjectUnderMouse.GetComponent<CreatureCard> ().health);
						
				}
			} 
			else if (enemyObjectUnderMouse.tag == "Player2") 
			{
				card.GetComponent<CreatureCard> ().creatureCanAttack = false;
				gameManager.dealDamage (card.GetComponent<DamageCard> ().damageToDeal, playerID);
			}
		}

		//Debug.Log ("HERE - " + objectHit.name);

	}
    public void PlayCard(GameObject card, Vector3 zonePosition, int i)
    {
        //Puts the card in the summoning zone
        card.transform.position = zonePosition;
        //Sets the state of the zone to be occupied
        SummonZones[i].GetComponent<SummonZone>().isOccupied = true;
        //Sets the state of the card to being in a summon zone
        card.GetComponent<Card>().inSummonZone = true;
		playerHand.Remove (card);
        currentHandSize--;
    }

    //This method is called when the card is done casting
    public void sendToGraveyard(GameObject card)
	{
		
		//Find the zone that the card is in and set it to unoccupied
		for (int i = 0; i < SummonZones.Count; i++) {
			//Debug.Log ("HERE");
			if (card.transform.position.x == SummonZones [i].transform.position.x) {
				//Debug.Log ("HERE2");
				SummonZones [i].GetComponent<SummonZone> ().isOccupied = false;
			}
		}

		//Add card to graveyard
		graveyard.Add(card);

		//Moves card to the graveyard
		card.transform.position = graveyardPos;
	}
	public Text getSummonZone(GameObject card)
	{
		Text textBoxToReturn = null;
		for (int i = 0; i < SummonZones.Count; i++) 
		{			
			//If the summoning zone is occupied
			if (SummonZones [i].GetComponent<SummonZone> ().isOccupied) {
				//Get's the position of the zone
				Vector3 zonePosition = SummonZones [i].transform.position;
				//Checks if the card is within a square surrounding the zone
				if (card.transform.position.x > (zonePosition.x - 4) && card.transform.position.x < (zonePosition.x + 4)) {
					if (card.transform.position.y > (zonePosition.y - 4) && card.transform.position.y < (zonePosition.y + 4)) {
						Debug.Log (""+SummonZones [i].GetComponent<SummonZone>().textBox.name);
						textBoxToReturn = SummonZones [i].GetComponent<SummonZone>().textBox;
					}
				}				
			}
		}
		return textBoxToReturn;
	}
	public static List<GameObject> shuffleDeck(List<GameObject> library)
	{
		System.Random _random = new System.Random ();
		GameObject temp;

		int n = library.Count;
		for (int i = 0; i < n; i++) {

			int r = i+(int)(_random.NextDouble()*(n-i));
			temp = library [r];
			library [r] = library [i];
			library [i] = temp;
		}
		return library;

	}
	public void drawFromLibrary()
	{
		if(currentHandSize < startingHandSize)
		{
			//set the first card (starting at index 0) of the players hand to
			//the first card (starting at index 0) in the players deck
			library[currentCardIndex].GetComponent<Card>().playerID = playerID;
			drawCard (library [currentCardIndex]);
			//increment the index of the deck (since a card has now been taken)
			currentHandSize++;
			currentCardIndex++;
		}
	}
	public void setMousedOverCard(GameObject card)
	{
		mousedOverCard = card;
	}
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
    public void drawLineOn()
	{
		line.GetComponent<DrawLine> ().isDrawing = true;
	}
	public void drawLineOff()
	{
		line.GetComponent<DrawLine> ().isDrawing = false;
	}
	public void makeLineInvisible()
	{
		line.GetComponent<DrawLine> ().makeLineInvisible ();
	}
	public void shiftCardsDown()
	{
		for (int i = 0; i < playerHand.Count; i++) 
		{
			playerHand [i].transform.position = new Vector3 (handZone.transform.position.x + (i*5f), handZone.transform.position.y, 0);
		}
	}
	public void drawCard(GameObject card)
	{
		playerHand.Add ((GameObject)Instantiate (card,new Vector3(handZone.transform.position.x+(currentHandSize*5f),handZone.transform.position.y,handZone.transform.position.z),Quaternion.identity));
	}
}