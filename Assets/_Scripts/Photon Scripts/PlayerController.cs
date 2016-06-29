using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    PhotonView photonView;
    //PLAYER STATS
    int startingHealth = 20;
    

    //The text box that the controller's health is placed
    private Text healthTextBox;

    /// <summary>
    /// All code from playercontroller
    /// </summary>
    //The total number of shuffles when the game starts
    private int maxInitialShuffles = 10;
    //The number of times the deck is shuffled before game starts
    private int numbShuffles;
    //The player ID (1 is local player, 2 is opponent)
    private int playerID = 1;
    //Player's health
    int health;
    //the maximum number of cards a player can hold
    int maxHandSize = 7;
    //the number of cards to draw at the start of the game
    int startingHandSize = 5;
    //The speed in seconds in which the player draws a card from the deck
    int libraryDrawSpeed = 7;
    //the number of cards in a player's deck
    int deckSize = 60;
    //The empty game object for the position of the handzone
    public GameObject handZone;
    //The list of summoning zones
    public List<GameObject> SummonZones = new List<GameObject>(3);
    //The list that represents the player's hand
    public List<GameObject> playerHand = new List<GameObject>(7);
    //The list that represents the player's graveyard
    public List<GameObject> graveyard = new List<GameObject>(60);
    //The list that represents the player's library
    public List<GameObject> library = new List<GameObject>(60);
    //the next card to draw from the deck (index of decksize array)
    int currentCardIndex = 0;
    int currentHandSize = 0;
    float libraryDrawCounterTimer = 0;
    //the player's deck, this gets loaded into the list at start up in a shuffled manner
    public List<GameObject> cardDeck = new List<GameObject>(60);
    //The game manager object
    GameManager gameManager;
    //The position of the graveyard. It is off screen so that that player doesn't see it but the player won't 
    //be able to interact directly with the cards in the graveyard
    Vector3 graveyardPos;
    //The card that the player is moused over
    private GameObject mousedOverCard;
    private GameObject line;
    private GameObject enemyObjectUnderMouse;


	public delegate void CreatureDied();
	public static event CreatureDied creatureHasDied;

	public delegate void CreatureEnteredBattlefield();
	public static event CreatureEnteredBattlefield creatureHasEntered;

	public int increaseDamageAmount, increaseAttackSpeedAmount, increaseHealthAmount;
	public List<GameObject> creatureCardsInPlay = new List<GameObject>(3);
	private GameObject creatureThatJustEntered;
    void Awake()
    {
        //get this object's photon view component
        photonView = GetComponent<PhotonView>();
        //get a reference to the game manager
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //if this controller belongs to the local player
        if (photonView.isMine)
        {
            playerID = 1;
            //name this gameobject localplayer
            gameObject.name = "LocalPlayer";
            //get player's handzone
            handZone = GameObject.Find("Player1HandZone");

            //set the health text box of this controller to the player1healthbox
            healthTextBox = GameObject.Find("Player1HealthBox").GetComponent<Text>();

            //get the player's attack line
            line = GameObject.Find("Player1Line");
            //get the position of the graveyard
            graveyardPos = GameObject.Find("Player1Graveyard").transform.position;
        }
        else //if this controller belongs to the opponent
        {
            playerID = 2;
            //name this gameobject networkopponent
            gameObject.name = "NetworkOpponent";

            //get player's handzone
            handZone = GameObject.Find("Player2HandZone");
            //set the health text box of this controller to the player2healthbox
            healthTextBox = GameObject.Find("Player2HealthBox").GetComponent<Text>();
            //get the player's attack line
            line = GameObject.Find("Player2Line");
            //get the position of the graveyard
            graveyardPos = GameObject.Find("Player2Graveyard").transform.position;
        }
    }
    // Use this for initialization
    void Start ()
    {
       
        //set your current health to the starting health value
        health = startingHealth;
        //if this controller belongs to the local player
        if (photonView.isMine)
        {
            
           
            //Finds the summon zones
            GameObject[] zones = GameObject.FindGameObjectsWithTag("Player1SummonZone");
            //Adds the summon zones to a list
            for (int i = 0; i < zones.Length; i++)
            {
                //Set the ID of the zone to whomever it belongs too
                zones[i].GetComponent<SummonZone>().playerID = playerID;
                SummonZones.Add(zones[i]);
            }
            //load card deck
			//StartCoroutine(DelayedLoadDeck(1.0f));
            photonView.RPC("LoadDeck", PhotonTargets.All);
            //LoadDeck(numberOfShuffles);

        }
        else //if this controller belongs to the opponent
        {
           
            //Finds the summon zones
            GameObject[] zones = GameObject.FindGameObjectsWithTag("Player2SummonZone");
            //Adds the summon zones to a list
            for (int i = 0; i < zones.Length; i++)
            {
                //Set the ID of the zone to whomever it belongs too
                zones[i].GetComponent<SummonZone>().playerID = playerID;
                SummonZones.Add(zones[i]);
            }
        }
        //set initial text for health text box
        healthTextBox.text = "Life: " + startingHealth;

    }

    // Update is called once per frame
    void Update ()
    {
        //keep health text updated to current health
        healthTextBox.text = "Life: " + health;

        //if health is 0 or less
        if(health <= 0)
        {
            if (photonView.isMine)
            {Lose();}
            else if (!photonView.isMine) // if this is the opponent
            {Win();}
        }
        //if this is the local player, handle draw
        //if this isnt, drawing will be handled by received rpc calls
        if (photonView.isMine)
        {
            //increase counter for draw timer
            libraryDrawCounterTimer += Time.deltaTime;
            //Debug.Log ("P1:" + libraryDrawCounterTimer);
            if (libraryDrawCounterTimer > libraryDrawSpeed)
            {
                libraryDrawCounterTimer = 0;
                //draw a card
                drawFromLibrary();
            }
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

    //Loads a particular deck into the card deck list
    //currently is hard coded, later on will be retrieved by playfab
    [PunRPC]
    public void LoadDeck()
    {

        //will call playfab and retrieve currently active deck names as array later
        //put all card names here
        string[] cardNames =
        {
            
			"Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd",
			"Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd",
			"Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd",
			"Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd",
			"Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd",
			"Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd","Blend With the Crowd",
			"Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound",
			"Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound",
			"Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound",
			"Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound",
			"Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound",
			"Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound","Fiend Hound",
            


        };
        for (int i = 0; i < deckSize; i++)
        {
            //as long as there are enough cards in the list to load
            if(cardNames.Length > i)
            {
                //load each card name from resources folder and add them to card deck
                cardDeck.Add((GameObject)Resources.Load("Cards/" + cardNames[i]));
            }
            else
            {
                //fill all remaining spaces with magma bolts
                cardDeck.Add((GameObject)Resources.Load("Cards/Magma Bolt"));
            }
          
        }
        //Populate deck
        for (int i = 0; i < deckSize; i++)
        {
            library.Add(cardDeck[i]);
            library[i].GetComponent<Card>().playerID = playerID;
            library[i].GetComponent<Card>().cardNumber = i;
            //Put the card in the card deck
            //cardDeck [i] = card;
            //Set the card's ID to whomever it belongs too
            //cardDeck [i].GetComponent<Card> ().playerID = playerID;
            //Give the card an ID(currently all the same for all card since there is only one card going into the deck multiple times)
            //cardDeck [i].GetComponent<Card> ().cardNumber = i;
        }
        //if this is the local player, shuffle the deck and draw
        //if this isnt, drawing will be handled by received rpc calls
        if (photonView.isMine)
        {
            int numbShuffles = Random.Range(1, maxInitialShuffles);
            for (int i = 0; i < numbShuffles; i++)
            { library = shuffleDeck(library); }
               
            //draw a hand of (startingHandSize) cards
            for (int i = 0; i < startingHandSize; i++)
            {
                drawFromLibrary();
            }
        }
        //after this resolves, the player's hand should have 5 cards and the current card index 
        //should be at 5 (since indecies 0-4 have been taken)

    }
    void Lose()
    {
        Debug.Log("YOU LOST!!!");
        //call lose function on gamemanager
        //displays ending panel and message
        gameManager.Lose();
        //wait for a set number of seconds before returning to menu
        StartCoroutine("FinishingGame");
    }
    void Win()
    {
        Debug.Log("YOU WIN!!!");
        //call lose function on gamemanager
        //displays ending panel and message
        gameManager.Win();
        //wait for a set number of seconds before returning to menu
        StartCoroutine("FinishingGame");
    }

    IEnumerator FinishingGame()
    {
        yield return new WaitForSeconds(3);
        //leave the current room
        PhotonNetwork.LeaveRoom();

    }
    //HANDLE DAMAGE OVER NETWORK
    [PunRPC]
    public void ChangeHealth(int amount)
    {
        //this should never happen, but if it does, return immediately
        if(amount == 0)
        { return; }

        //HEALING
        //if the amount is positive (if this is healing)
        if(amount > 0)
        {
            //if this is the local player, add health
            if(photonView.isMine)
            {
                health += amount;
            }
        }
        //DAMAGE
        //if the amount is negative (if this is damage)
        else if(amount < 0)
        {
            //if this is the local player, add health
            //since damage is negative, it will be subtracted
            if (photonView.isMine)
            {
                health += amount;
            }
            //if this is the network opponent
            else if(!photonView.isMine)
            {
                //call the damage rpc on the opponent's local player
                photonView.RPC("ChangeHealth",PhotonTargets.Others,amount);
            }
        }
      
    }
    //////////////////////FUNCTIONS FROM P1MANAGER//////////////////////////////////////////////////////////////////////
   

    //Method called for when a card is dropped
    public void cardIsDropped(GameObject card, Vector3 cardHandPos)
    {

        //Set the state of being dropped to false
        card.GetComponent<Card>().setDroppedState(false);

        //Check which zone it was dropped on
        for (int i = 0; i < SummonZones.Count; i++)
        {
            //If the summoning zone isn't occupied
            if (!SummonZones[i].GetComponent<SummonZone>().isOccupied)
            {
                //Get's the position of the zone
                Vector3 zonePosition = SummonZones[i].transform.position;
                //Checks if the card is within a square surrounding the zone
                if (card.transform.position.x > (zonePosition.x - 3) && card.transform.position.x < (zonePosition.x + 3))
                {
                    if (card.transform.position.y > (zonePosition.y - 3) && card.transform.position.y < (zonePosition.y + 3))
                    {
                        //Play card, pass the card, the position of the zone, and the index of the zone
                        photonView.RPC("PlayCard", PhotonTargets.All, card.GetComponent<PhotonView>().viewID,  i);
                        //PlayCard(card, zonePosition, i);
                        //photonView.RPC("PlayCardNetwork", PhotonTargets.Others, card.GetComponent<Card>().handIndex,  i);
						//If the card is a creature card, add it to the list of creature cards in play
						if (card.GetComponent<CreatureCard> () != null) {
							creatureThatJustEntered = card;
							creatureCardsInPlay.Add (card);
						}
                    }
                }
            }
        }

        //If the player picks up the card and drops it anywhere else the card will be placed back in the hand zone
        if (card.GetComponent<Card>().inSummonZone == false)
            card.transform.position = cardHandPos;
    }
   
    //New Play Card Function
    [PunRPC]
    public void PlayCard(int cardId, int i)
    {
        //find the spawned card
        GameObject card = PhotonView.Find(cardId).gameObject;
        //Puts the card in the summoning zone
        card.transform.position = SummonZones[i].transform.position;
        //Sets the state of the zone to be occupied
        SummonZones[i].GetComponent<SummonZone>().isOccupied = true;
        //Sets the state of the card to being in a summon zone
        card.GetComponent<Card>().inSummonZone = true;
        playerHand.Remove(card);
        currentHandSize--;
        shiftCardsDown();

    }
   
	[PunRPC]
	public void ResolveCreatureDamage(GameObject creature, GameObject otherCreature)
	{
		creature.GetComponent<CreatureCard> ().health -= otherCreature.GetComponent<CreatureCard> ().damageToDeal;
		otherCreature.GetComponent<CreatureCard> ().health -= creature.GetComponent<CreatureCard> ().damageToDeal;
	}
    
    public void creatureCardIsDropped(GameObject card, Vector3 cardHandPos)
    {
        //card.transform.position = cardHandPos;

        Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

        if (hit)
        {
            enemyObjectUnderMouse = hit.transform.gameObject;
            if (enemyObjectUnderMouse.tag == "CreatureCard")
            {
                if (enemyObjectUnderMouse.GetComponent<CreatureCard>().inBattlefield)
                {
					if (enemyObjectUnderMouse.GetComponent<CreatureCard> ().isAttackable) {
						card.GetComponent<CreatureCard> ().creatureCanAttack = false;
						card.GetComponent<CreatureCard> ().health -= enemyObjectUnderMouse.GetComponent<CreatureCard> ().damageToDeal;
						enemyObjectUnderMouse.GetComponent<CreatureCard> ().health -= card.GetComponent<CreatureCard> ().damageToDeal;
						Debug.Log ("Your creature's health: " + card.GetComponent<CreatureCard> ().health);
						Debug.Log ("Enemy creature's health: " + enemyObjectUnderMouse.GetComponent<CreatureCard> ().health);
						photonView.RPC ("ResolveCreatureDamage", PhotonTargets.Others, card, enemyObjectUnderMouse);
					} 
					else
						Debug.Log ("NOT ATTACKABLE");
                }
            }
            else if (enemyObjectUnderMouse.tag == "Player2")
            {
                card.GetComponent<CreatureCard>().creatureCanAttack = false;
                gameManager.dealDamage(card.GetComponent<DamageCard>().damageToDeal, playerID);
            }
        }

        //Debug.Log ("HERE - " + objectHit.name);

    }
	public void creatureTargetCardIsDropped(GameObject card, Vector3 cardHandPos)
	{
		Debug.Log ("IN FUNCTION");
		card.transform.position = cardHandPos;
		Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
		RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

		if (hit) {
			Debug.Log ("HIT");
			enemyObjectUnderMouse = hit.transform.gameObject;
			Debug.Log (enemyObjectUnderMouse.name);
			//If the object is a creature card
			if (enemyObjectUnderMouse.tag == "CreatureCard") {
				Debug.Log ("A CREATURE");
				//If the creature card is in the battlefield
				if (enemyObjectUnderMouse.GetComponent<CreatureCard> ().inBattlefield) 
				{
					//Find an open summoning slot
					for (int i = 0; i < SummonZones.Count; i++) 
					{
						if (!SummonZones [i].GetComponent<SummonZone> ().isOccupied) 
						{
							//Get's the position of the zone
							Vector3 zonePosition = SummonZones[i].transform.position;
							//Play card, pass the card, the position of the zone, and the index of the zone
							card.GetComponent<CreatureTargetSpellCard>().setSummonZoneTextBox(SummonZones[i].GetComponent<SummonZone>().textBox);
							card.GetComponent<CreatureTargetSpellCard>().setValidCreatureToDebuff(enemyObjectUnderMouse);
                            //PlayCard(card, zonePosition, i);
                            //photonView.RPC("PlayCardNetwork", PhotonTargets.Others, card.GetComponent<Card>().handIndex,  i);
                            photonView.RPC("PlayCard", PhotonTargets.All, card.GetComponent<PhotonView>().viewID, SummonZones[i].transform.position, i);
                        }
					}
					if (card.GetComponent<CreatureTargetSpellCard> ().hasTextBox() == false) 
					{
						Debug.Log ("INVALID PLAY, SEND TO GY");
						card.GetComponent<CreatureTargetSpellCard>().setGraveyardVariables();
						sendToGraveyard (card);
					}
				}
			}
		}
	}
   

    //This method is called when the card is done casting
    public void sendToGraveyard(GameObject card)
    {

        //Find the zone that the card is in and set it to unoccupied
        for (int i = 0; i < SummonZones.Count; i++)
        {
            //Debug.Log ("HERE");
            if (card.transform.position.x == SummonZones[i].transform.position.x)
            {
                //Debug.Log ("HERE2");
                SummonZones[i].GetComponent<SummonZone>().isOccupied = false;
            }
        }

        //Add card to graveyard
        graveyard.Add(card);

        //Moves card to the graveyard
        card.transform.position = graveyardPos;
		//If the card being sent to the graveyard is a creature card, remove it from the list of creature cards in play
		if (card.GetComponent<CreatureCard>() != null)
			creatureCardsInPlay.Remove (card);
    }
    public Text getSummonZone(GameObject card)
    {
        Text textBoxToReturn = null;
        for (int i = 0; i < SummonZones.Count; i++)
        {
            //If the summoning zone is occupied
            if (SummonZones[i].GetComponent<SummonZone>().isOccupied)
            {
                //Get's the position of the zone
                Vector3 zonePosition = SummonZones[i].transform.position;
                //Checks if the card is within a square surrounding the zone
                if (card.transform.position.x > (zonePosition.x - 4) && card.transform.position.x < (zonePosition.x + 4))
                {
                    if (card.transform.position.y > (zonePosition.y - 4) && card.transform.position.y < (zonePosition.y + 4))
                    {
                        Debug.Log("" + SummonZones[i].GetComponent<SummonZone>().textBox.name);
                        textBoxToReturn = SummonZones[i].GetComponent<SummonZone>().textBox;
                    }
                }
            }
        }
        return textBoxToReturn;
    }
    public static List<GameObject> shuffleDeck(List<GameObject> library)
    {
        System.Random _random = new System.Random();
        GameObject temp;

        int n = library.Count;
        for (int i = 0; i < n; i++)
        {

            int r = i + (int)(_random.NextDouble() * (n - i));
            temp = library[r];
            library[r] = library[i];
            library[i] = temp;
        }
        return library;

    }
    
    public void drawFromLibrary()
    {
        if (currentHandSize < startingHandSize)
        {
            //set the first card (starting at index 0) of the players hand to
            //the first card (starting at index 0) in the players deck
            library[currentCardIndex].GetComponent<Card>().playerID = playerID;
            //drawCard(library[currentCardIndex]);

            //get name of drawn card
            string cardToDraw = library[currentCardIndex].name;
            //send name of drawn card to clones
            //photonView.RPC("drawCard", PhotonTargets.All,cardToDraw);

            //MODIFIED TO INSTANTIATE ON NETWORK
            //Instantiate the passed card over photon network
            GameObject cardToAdd = PhotonNetwork.Instantiate(("Cards/" + cardToDraw), Vector3.zero, Quaternion.identity, 0);
            //get the card's view Id to reference the same card
            int cardId = cardToAdd.GetComponent<PhotonView>().viewID;
            //Put the card into proper place
            photonView.RPC("DrawCard", PhotonTargets.All, cardId);

        }
    }
//NEW CARD DRAWING FUNCTION
    //Sets up card variables and position of card in hand
    [PunRPC]
    public void DrawCard(int cardId)
    {
        //find the spawned card
        GameObject cardToAdd = PhotonView.Find(cardId).gameObject;
        //Set cards position
        cardToAdd.transform.position = new Vector3(handZone.transform.position.x + (currentHandSize * 5f), handZone.transform.position.y, handZone.transform.position.z);
        //set the card's player id
        cardToAdd.GetComponent<Card>().playerID = playerID;
        //add it to the player's hand
        playerHand.Add(cardToAdd);
        //set card's hand index
        cardToAdd.GetComponent<Card>().handIndex = playerHand.IndexOf(cardToAdd);
        //increment the index of the deck (since a card has now been taken)
        currentCardIndex++;
        //increment hand size
        currentHandSize++;

    }

   
    public void setMousedOverCard(GameObject card)
    {
        mousedOverCard = card;
    }
   
    public void drawLineOn()
    {
        line.GetComponent<DrawLine>().isDrawing = true;
    }
    public void drawLineOff()
    {
        line.GetComponent<DrawLine>().isDrawing = false;
    }
    public void makeLineInvisible()
    {
        line.GetComponent<DrawLine>().makeLineInvisible();
    }
    public void shiftCardsDown()
    {
        for (int i = 0; i < playerHand.Count; i++)
        {
            playerHand[i].transform.position = new Vector3(handZone.transform.position.x + (i * 5f), handZone.transform.position.y, 0);
            //set card's hand index
            playerHand[i].GetComponent<Card>().handIndex = i;
        }
       
    }


	public void creatureDied()
	{
		if (photonView.isMine) {
			if (creatureHasDied != null)
				creatureHasDied ();
		}
	}
	public void creatureEntered()
	{
		if (photonView.isMine) {
			if (creatureHasEntered != null)
				creatureHasEntered ();
		}
	}
 
    
    /// <summary>
    /// PHOTON SERIALZE VIEW
    /// syncs any data that needs to be passed to all network copies constantly
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            //sync health
            stream.SendNext(health);

        }
        else
        {
            //Network player, receive data          
           health = (int)stream.ReceiveNext();
        }
       
	}

	IEnumerator DelayedLoadDeck(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		photonView.RPC("LoadDeck", PhotonTargets.All);
	}
	//When called it will increase the stats of all the creatures by the amount passed
	public void increaseCreatureStats(int dmg, int attkSpd, int h)
	{
		if(photonView.isMine)
		{
			Debug.Log ("INCREASING CREATURE STATS");
			increaseDamageAmount += dmg;
			increaseAttackSpeedAmount += attkSpd;
			increaseHealthAmount += h;
			//increase the stats of creatures currently in play
			for (int i = 0; i < creatureCardsInPlay.Count; i++) {
				if(creatureCardsInPlay[i].GetComponent<CreatureCard>().playerID == 1)
					creatureCardsInPlay [i].GetComponent<CreatureCard> ().increaseStats (dmg, attkSpd, h);
				}
		}
	}
	//When called it will decrease the stats of all the creatures by the amount passed
	public void decreaseCreatureStats(int dmg, int attkSpd, int h)
	{
		if (photonView.isMine) {
			increaseDamageAmount -= dmg;
			increaseAttackSpeedAmount -= attkSpd;
			increaseHealthAmount -= h;
			for (int i = 0; i < creatureCardsInPlay.Count; i++) {
				creatureCardsInPlay [i].GetComponent<CreatureCard> ().decreaseStats (dmg, attkSpd, h);
			}
		}
	}
	//Passes the card object of the creature that just entered
	public GameObject getCreatureThatJustEntered()
	{
		return creatureThatJustEntered;
	}
}
