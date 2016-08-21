﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// A script to handle the functions that should be available to every
/// card in the game. This is an abstract object. It will be used as a
/// base for all refined cards
/// </summary>
public abstract class Card : MonoBehaviour
{

//STATS 
    //all card stats are here, are assigned by custom data from Playfab

    //name of prefab that this card will utilize
    public string prefabName;
    //name of art asset to use for card art
    public string artName;
    //name of art asset to use for border art
    public string borderArt;
    //The faction that the card belongs to. Neutral means it is available to all factions
    public string faction = "Neutral";
    //the type of card classification this card has (creature, spell, etc.)
    public string cardType;
    //The potential targets for this card (could be creature, player, all)
    public string target;
    //The time it takes for the card to be casted
    public float castTime;
    //the time it takes for this card to recharge after a use (ex: time it takes a creature before it can attack again)
    public float rechargeTime;
    //the attack power of this card (if its a creature)
    public int attackPower;
    //the defense power of this card (if its a creature)
    public int defensePower;
    //the amount to change the opponents health (can be direct damage or healing)
    public int opponentHealthChange;
    //the amount to change the owners health (can be direct damage or healing)
    public int ownerHealthChange;

//END STATS

    //The itemId of the Card (not unique, used to reference custom data)
    public string cardId;
    //the custom data of the card
    private string customData;
	//The name of the card
	public string cardTitle;
	//Used to prevent staying step out of a chunk of code in derived Card Update methods
	public bool doneAddingToGraveyard = false;
	//If the card is in the graveyard or not
	public bool inGraveyard;
	//If the card is in the summon zone
	public bool inSummonZone;
	//The ID of the player that this card belongs to
	public int playerID;
	//The card number. Used to correctly delete from the player's hand
	public int cardNumber;
	//Checks to see if the player dropped the card.
	protected bool dropped;
	
	protected float currentTime;
	public Text summonZoneTextBox;
	public Text cardTitleTextBox;
	//Something for the dragging of cards
	protected Vector3 screenPoint;
	//Something fo the dragging of cards
	protected Vector3 offset;
	//The game object for player 1 manager
	protected GameObject p1Manager;
	//The game object for player 2 manager
	protected GameObject p2Manager;
	//The position that the card was at when the player picks up the card. 
	//This is used for when a player makes an invalid placement the card is placed back in it's original hand position
	protected Vector3 cardHandPos;
	// Use this for initialization
	protected bool isDraggable;

	public GameObject localPlayer;
	public GameObject networkOpponent;
    protected PlayerController localPlayerController;
    protected PlayerController opponentPlayerController;

    protected SpriteRenderer spriteRender;
	//The variable for the script attached to the AudioManager object
	protected AudioManager audioManager;

	protected bool playedCardSelectedSound;
    //ADDED CODE FOR HAND INDEX
    public int handIndex;

    //PHOTON COMPONENTS
    protected PhotonView photonView;

    //This can be used for initialization code that is identical on ALL cards
    public virtual void Awake()
    {
        //get photon view component of this card
        photonView = GetComponent<PhotonView>();
        localPlayer = GameObject.Find("LocalPlayer");
        networkOpponent = GameObject.Find("NetworkOpponent");
        p1Manager = GameObject.Find("Player1Manager");
        p2Manager = GameObject.Find("Player2Manager");
    }
	public virtual void Start ()				//Abstract method for start
	{
		doneAddingToGraveyard = false;
		currentTime = castTime;
		inSummonZone = false;
		summonZoneTextBox = null;
		isDraggable = true;
		//gameObject.GetComponentInChildren<Text>();
		cardTitleTextBox = gameObject.GetComponentInChildren<Text>();
        cardTitleTextBox.text = cardTitle;


    }
	public virtual void Update ()				//Abstract method for Update
	{
        //get references to player objects if not assigned
        GetPlayers();

        //If the card is Not in the graveyard and is in the summon zone
        if (!inGraveyard && inSummonZone) 
		{
			
			//Increment the current Time
			currentTime -= Time.deltaTime;
            //make sure summon zone text is assigned
            if (summonZoneTextBox == null)
            { GetSummonZoneText(); }
            else
            { summonZoneTextBox.text = currentTime.ToString("F1"); }
	
			//IF the current time is larger than or equal to the cast time
			isDraggable = false;
			if (currentTime <= 0) 
			{
                //clear summon zone text
                summonZoneTextBox.text = "";
                //reset the timer
                currentTime = 0;
				//Set state of card to being in the graveyard
				inGraveyard = true;
				//Set state of card to not being in the summon zone
				inSummonZone = false;
			}

		}
		//If the card is in the graveyard and manager code hasn't been executed yet
		if (inGraveyard && doneAddingToGraveyard == false) 
		{
            photonView.RPC("SendToGraveyard", PhotonTargets.All);
            //SendToGraveyard();
        }
	}

	//Registers that the player has clicked on the card
	public virtual void OnMouseDown()			
	{
		if (photonView.isMine && isDraggable == true) 
		{
			cardHandPos = gameObject.transform.position;
			screenPoint = Camera.main.WorldToScreenPoint (gameObject.transform.position);
			offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

		}
	}

	//Registers that the player has let go of the card
	public virtual void OnMouseUp()			
	{
        //if this is the local player, drop the card
        //network player receives drop through RPC call in PlayerController (void PlayCard)
		if (photonView.isMine)
        {
            dropped = true;
            localPlayerController.cardIsDropped(gameObject, cardHandPos);
        }

        //Makes sure summon zone textbox is assigned
        GetSummonZoneText();
		
	}

	//Registers that the card is being dragged
	public  void OnMouseDrag()			
	{
		if (photonView.isMine && isDraggable == true)
        {
			Vector3 curScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
			Vector3 curPosition = Camera.main.ScreenToWorldPoint (curScreenPoint) + offset;
			transform.position = curPosition;
		}
	}
	//Registers what card is under the mouse
	public virtual void OnMouseOver()
	{
		Debug.Log (gameObject.name);
		if (photonView.isMine) 
		{
            localPlayerController.setMousedOverCard(gameObject);
            //TEST
            //p1Manager.GetComponent<Player1Manager> ().setMousedOverCard (gameObject);
        } 
	}
	//Registers what card is under the mouse
	public virtual void OnMouseExit()
	{
		playedCardSelectedSound = false;
	}

	//method to return the dropped variable
	public bool isDropped()
	{
		return dropped;
	}
	//method to set the dropped variable
	public void setDroppedState(bool b)
	{
		dropped = b;
	}
	public float currentCastingTime()
	{
		return currentTime;
	}
	public void setGraveyardVariables()
	{
		inSummonZone = false;
		inGraveyard = true;
		doneAddingToGraveyard = true;
	}
	public void setSummonZoneTextBox(Text card)
	{
		summonZoneTextBox = card;
	}
	public Text getSummonZoneTextBox()
	{
		return summonZoneTextBox;
	}

   
    
    public void InitializeCard(string id)
    {
    
        Debug.Log("id of this card is: " + id);
        /*
        //set the card's id
        cardId = id;
        //set cards name 
        cardTitle = PlayFabDataStore.cardNameList[id];   
        //set the cards custom data to be the custom data associated with this card id
        SetCustomData(PlayFabDataStore.cardCustomData[id]); 
        */
        photonView.RPC("SetCustomData", PhotonTargets.All, id);
    }

    //takes a string of custom data and stores it
    [PunRPC]
    public void SetCustomData(string id)//string[] data)
    {
        //set the card's id
        cardId = id;
        //set cards name 
        cardTitle = PlayFabDataStore.cardNameList[id];

        string[] data = PlayFabDataStore.cardCustomData[id];
        /*
        //store custom data for this card
        customData = data;
        //define where to split the custom data to retrive variables
        string[] stringSeparators = new string[] { "{\"", "\":\"", "\",\"", "\"}" };
        //split custom data using seperators
        string[] splitResultTest = customData.Split(stringSeparators, System.StringSplitOptions.None);
        */
        //iterate through each string in the split data
        //goes to 1 less than total length because the last variable doesnt need to be checked and nextString will fail
        for (int j = 0; j < data.Length - 1; j++)//splitResultTest.Length -1; j++)
        {
            //stores the current string being viewed
            string currentString = data[j];//splitResultTest[j];
            //store the next string in the list
            string nextString = data[j + 1];//splitResultTest[j+1];

            //Debug.Log(currentString);

            //Assign variables
            switch(currentString)
            {
                case "PrefabName":
                    prefabName = nextString;
                    break;
                case "ArtName":
                    artName = nextString;
                    break;
                case "BorderArt":
                    borderArt = nextString;
                    break;
                case "Faction":
                    faction = nextString;
                    break;
                case "CardType":
                    cardType = nextString;
                    break;
                case "Target":
                    target = nextString;
                    break;
                case "CastTime":
                    castTime = float.Parse(nextString);
                    break;
                case "RechargeTime":
                    rechargeTime = float.Parse(nextString);
                    break;
                case "AttackPower":
                    attackPower = int.Parse(nextString);
                    break;
                case "DefensePower":
                    defensePower = int.Parse(nextString);
                    break;
                case "OpponentHealthChange":
                    opponentHealthChange = int.Parse(nextString);
                    break;
                case "OwnerHealthChange":
                    ownerHealthChange = int.Parse(nextString);
                    break;
                default:
                    break;
            }
        }
        //set all internal variables on a card using this function
        UpdateInternalVariables();    

    }
    //this function should be overridden in child script to update any
    //internal variables that should be set on card initialization
    public virtual void UpdateInternalVariables()
    {
        //variables go here
    }

    //checks if the card has a reference to summon zone text
    //assigns it if not
    public void GetSummonZoneText()
    {
        //finds the text box that corresponds to the summon zone
        if (summonZoneTextBox == null)
        {
            if (photonView.isMine)
            {
                summonZoneTextBox = localPlayerController.getSummonZone(gameObject);
            }
            else
            {
                summonZoneTextBox = opponentPlayerController.getSummonZone(gameObject);         
            }
        }
    }

    //store all data for player objects
    public void GetPlayers()
    {
        //if network opponent object has not been assigned
        if (networkOpponent == null)
        {
            //find and assign network opponent object
            networkOpponent = GameObject.Find("NetworkOpponent");
        }
        else
        {
            //if network opponent has been assigned, store reference to PlayerController script
            opponentPlayerController = networkOpponent.GetComponent<PlayerController>();
        }
        //if local player has not been assigned
        if (localPlayer == null)
        {
            //find and assign local player object
            localPlayer = GameObject.Find("LocalPlayer");
        }
        else
        {
            //if local player has been assigned, store reference to PlayerController script
            localPlayerController = localPlayer.GetComponent<PlayerController>();
        }
    }
    [PunRPC]
    public void SendToGraveyard()
    {
        Debug.Log(gameObject + "sent to graveyard");
       
        //Set this to false to prevent multiple executions of this block
        doneAddingToGraveyard = true;

        //If the card beings to player 1
        if (photonView.isMine)
        {
            //Execute the game manager code
            localPlayer.GetComponent<PlayerController>().sendToGraveyard(gameObject);
        }
        else
        {      
            //Execute the game manager code
            networkOpponent.GetComponent<PlayerController>().sendToGraveyard(gameObject);
        }
        //handle graveyard effect of this card, if any
        OnGraveyard();
    }
    //used to hold a cards effect upon being sent to graveyard (if it has any)
    public virtual void OnGraveyard()
    {
        //Graveyard effect goes here
    }
    

    //Photon Serialize View
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            //sync health
            //stream.SendNext(cardId);
            //stream.SendNext(cardTitle);
            

        }
        else
        {
            //Network player, receive data   
            //cardId = (string)stream.ReceiveNext();
            //cardTitle = (string)stream.ReceiveNext();
            //cardTitleTextBox.text = cardTitle;
        }

    }
}
