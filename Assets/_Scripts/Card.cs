using UnityEngine;
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
    public float attackPower;
    //the defense power of this card (if its a creature)
    public float defensePower;
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

    //ADDED CODE FOR HAND INDEX
    public int handIndex;

    //PHOTON COMPONENTS
    protected PhotonView photonView;

    //This can be used for initialization code that is identical on ALL cards
    public virtual void Awake()
    {
        //get photon view component of this card
        photonView = GetComponent<PhotonView>();
    }
	public virtual void Start ()				//Abstract method for start
	{
       

		localPlayer = GameObject.Find ("LocalPlayer");
		networkOpponent = GameObject.Find ("NetworkOpponent");
		p1Manager = GameObject.Find ("Player1Manager");
		p2Manager = GameObject.Find ("Player2Manager");
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
		//If the card is Not in the graveyard and is in the summon zone
		if (!inGraveyard && inSummonZone) 
		{
			
			//Increment the current Time
			currentTime -= Time.deltaTime;
			if(summonZoneTextBox == null)
            { summonZoneTextBox = localPlayer.GetComponent<PlayerController>().getSummonZone(gameObject); }
                //TEST
				//summonZoneTextBox = p1Manager.GetComponent<Player1Manager> ().getSummonZone (gameObject);
			else
            { summonZoneTextBox.text = currentTime.ToString("F1"); }
				
			//cardTimerBox.text = currentTime.ToString ("F1");
			//IF the current time is larger than or equal to the cast time
			isDraggable = false;
			if (currentTime <= 0) 
			{
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
			//If the card beings to player 1
			if (photonView.isMine) 
			{
				summonZoneTextBox.text = "";
				//Set this to false to prevent multiple executions of this block
				doneAddingToGraveyard = true;
                //Execute the game manager code
                localPlayer.GetComponent<PlayerController>().sendToGraveyard(gameObject);
                //TEST
                //p1Manager.GetComponent<Player1Manager> ().sendToGraveyard (gameObject);
            } 
			else 
			{
				//Logic for player2
				summonZoneTextBox.text = "";
				//Set this to false to prevent multiple executions of this block
				doneAddingToGraveyard = true;
                //Execute the game manager code
                networkOpponent.GetComponent<PlayerController>().sendToGraveyard(gameObject);
                //TEST
                //p2Manager.GetComponent<Player2Manager> ().sendToGraveyard (gameObject);
            }
		}
	}

	//Registers that the player has clicked on the card
	public virtual void OnMouseDown()			
	{
		if (isDraggable == true) 
		{
			cardHandPos = gameObject.transform.position;
			screenPoint = Camera.main.WorldToScreenPoint (gameObject.transform.position);
			offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

		}
	}

	//Registers that the player has let go of the card
	public virtual void OnMouseUp()			
	{
        
       
        dropped = true;
		if (photonView.isMine)
        {
            
            localPlayer.GetComponent<PlayerController>().cardIsDropped(gameObject, cardHandPos);
            //TEST
            //p1Manager.GetComponent<Player1Manager> ().cardIsDropped (gameObject, cardHandPos);
        }
        
		else
        {
            networkOpponent.GetComponent<PlayerController>().cardIsDropped(gameObject, cardHandPos);
            //TEST
            //p2Manager.GetComponent<Player2Manager>().cardIsDropped(gameObject, cardHandPos);
        }
        
			
		//finds the text box that corresponds to the summon zone
		if (summonZoneTextBox == null) 
		{
			if (photonView.isMine)
            {
                summonZoneTextBox = localPlayer.GetComponent<PlayerController>().getSummonZone(gameObject);
                //TEST
                //summonZoneTextBox = p1Manager.GetComponent<Player1Manager>().getSummonZone(gameObject);
            }
			else
            {
                summonZoneTextBox =networkOpponent.GetComponent<PlayerController>().getSummonZone(gameObject);
                //TEST
                //summonZoneTextBox = p2Manager.GetComponent<Player2Manager>().getSummonZone(gameObject);
            }
				
		}
		//if(playerID == 2)
			//GameObject.Find ("Player2Manager").GetComponent<Player2Manager> ().cardIsDropped (gameObject);
		//gameObject.transform.position = new Vector3 (0, -3.79f, 0);
	}

	//Registers that the card is being dragged
	public  void OnMouseDrag()			
	{
		if (isDraggable == true) {
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
            localPlayer.GetComponent<PlayerController>().setMousedOverCard(gameObject);
            //TEST
            //p1Manager.GetComponent<Player1Manager> ().setMousedOverCard (gameObject);
        } 
		else 
		{
            networkOpponent.GetComponent<PlayerController>().setMousedOverCard(gameObject);
            //TEST
            //p2Manager.GetComponent<Player2Manager> ().setMousedOverCard (gameObject);
        }
	}
	//Registers what card is under the mouse
	public virtual void OnMouseExit()
	{
		Debug.Log ("test");
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

    //Photon Serialize View
    //Registers that the player has clicked on the card
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            //sync health
            //stream.SendNext(health);

        }
        else
        {
            //Network player, receive data          
            //health = (int)stream.ReceiveNext();
        }

    }

    public void InitializeCard(string id)
    {
        Debug.Log("id of this card is: " + id);
        //set the card's id
        cardId = id;
        //set cards name 
        cardTitle = PlayFabDataStore.cardNameList[id];
        //set the cards custom data to be the custom data associated with this card id
        SetCustomData(PlayFabDataStore.cardCustomData[id]);
    }
    //takes a string of custom data and stores it
    public void SetCustomData(string data)
    {
        //store custom data for this card
        customData = data;
        //define where to split the custom data to retrive variables
        string[] stringSeparators = new string[] { "{\"", "\":\"", "\",\"", "\"}" };
        //split custom data using seperators
        string[] splitResultTest = customData.Split(stringSeparators, System.StringSplitOptions.None);
        //iterate through each string in the split data
        //goes to 1 less than total length because the last variable doesnt need to be checked and nextString will fail
        for (int j = 0; j < splitResultTest.Length -1; j++)
        {
            //stores the current string being viewed
            string currentString = splitResultTest[j];
            //store the next string in the list
            string nextString = splitResultTest[j+1];
            Debug.Log(currentString);
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
                    attackPower = float.Parse(nextString);
                    break;
                case "DefensePower":
                    defensePower = float.Parse(nextString);
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

    }
    //takes a string key, looks through custom data to find respective value
    public string GetCustomDataValue(string key)
    {
        //create variable for storing value retrieved
        string value = null;
        //create string for storing all custom data for this card
        string customDataString = null;
        //create separator based on key to locate (separates at key":")
        string[] stringSeparators = new string[] { key+"\":\"" , "\""};
        //assign custom data to string variable
        if (cardId != null)
        {
            customDataString = PlayFabDataStore.cardCustomData[cardId];
        }
        else
        {
            Debug.Log("Failed to Locate Custom Data Key (no card id for this card)");
            return null;
        }
        //locate respective key from list of custom data
        //splits custom data into 3 (preceding data/key, value, remaining data)
        string[] result = customDataString.Split(new string[] { "[stop]" }, System.StringSplitOptions.None);
        value = result[1];

        //return data
        if (value == null)
        {
            Debug.Log("Failed to Locate Custom Data Key (no value found)");
        }
        return value;
    }

    public void AssignCardInfo()
    {
       // cardTitle = GetCustomDataValue()
    }
}
