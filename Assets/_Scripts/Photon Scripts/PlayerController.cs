using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    //Player State
    public enum clickState { Empty,  HoldingCard, Targetting};
    public clickState currentClickState = clickState.Empty;

    //PLAYER SETTINGS (TODO assign from playfab)
    //the number of cards in a player's deck
    const int deckSize = 40;
    int startingHealth = 40; //< Players initial health
    //the maximum number of cards a player can hold
    int maxHandSize = 7;
    //the number of cards to draw at the start of the game
    int startingHandSize = 5;
    //The speed in seconds in which the player draws a card from the deck
    int libraryDrawSpeed = 7;
    int numberOfSummonZones = 3;

    public delegate void LoseEvent();
    public static event LoseEvent OnLose;

    // COMPONENTS
    GameObject PlayerAvatar;
    GameObject PlayerManager;
    PlayerController opponent;
    public PhotonView photonView;
    private Text healthTextBox;   
    Vector3 handZone; //< The empty game object for the position of the handzone
    Dictionary<int, Vector3> handPositions = new Dictionary<int, Vector3>();//< position of cards in hand by index
    float cardSpacing = 5.0f;
    Vector3 graveyardPos; //< The position of the graveyard. (off screen) 
    Vector3 cardPool; //< The position of the card pool (off screen)
    private GameObject line;


    //references to currently selected card
    private Transform selectedCard = null;
    private BaseCard selectedCardScript = null;

    //PLAYER STATS
    int health;  //< Player's current health
    //The player ID (1 is local player, 2 is opponent)
    private int playerID = 1; // TODO change to Enum (Unassigned, Local, Opponent)

    //The list of summoning zones
    public GameObject[] SummonZones;
    public Dictionary<int, bool> OccupiedZones = new Dictionary<int, bool>();//< stores whether a zone is occupied

    //The list that represents the player's hand
    public GameObject[] playerHand;

    public List<int> cardDeck;


    //The list that represents the player's graveyard
    public List<GameObject> graveyard = new List<GameObject>(60);
    //The list that represents the player's library
    //public List<GameObject> library = new List<GameObject>(60);

    public List<string> library = new List<string>(60);//CHANGED


    //the next card to draw from the deck (index of decksize array)
    int currentCardIndex = 0;
    int currentHandSize = 0;
    float libraryDrawCounterTimer = 0;
    //the player's deck, this gets loaded into the list at start up in a shuffled manner
    //public List<GameObject> cardDeck = new List<GameObject>(60);

    //The game manager object
    GameManager gameManager;

  
    void Awake()
    {
        //get this object's photon view component
        photonView = GetComponent<PhotonView>();
        //spawn avatar
        PlayerAvatar = PhotonNetwork.Instantiate(("PlayerAvatar"), transform.position, Quaternion.identity,0);

        //initialize containers to proper sizes
        SummonZones = new GameObject[numberOfSummonZones];
        playerHand = new GameObject[maxHandSize];
        // TODO put back commented section, hard coded for testing only
        cardDeck = new List<int>(20);//(deckSize);      
        //get a reference to the game manager
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //Check if the local player or network opponent owns this controller
        CheckOwnerAndGetComponents();
        //set your current health to the starting health value
        health = startingHealth;
    }

    /**
  * Checks whether this controller is owned by the local player or the opponent.
  * Sets references to all components
  * Loads deck if local player
  **/
    void CheckOwnerAndGetComponents()
    {
        //if this controller belongs to the local player
        if (photonView.isMine)
        {
            playerID = 1;
            //name this gameobject localplayer
            gameObject.name = "LocalPlayer";
            if (PlayerManager = GameObject.Find("Player1Manager"))
            {
                GetPlayerComponents();
                //Load deck
                //photonView.RPC("LoadDeck", PhotonTargets.All);
                LoadDeck();
            }
        }
        else //if this controller belongs to the opponent
        {
            playerID = 2;
            //name this gameobject networkopponent
            gameObject.name = "NetworkOpponent";
            if (PlayerManager = GameObject.Find("Player2Manager"))
            {
                GetPlayerComponents();
            }
        }
    }

    void GetPlayerComponents()
    {
        //get player's handzone
        handZone = PlayerManager.transform.FindChild("HandZone").position;
        for(int i = 0; i < playerHand.Length; i++)
        {
            handPositions[i] = handZone + new Vector3(cardSpacing*i, 0, 0);
        }
        //set the health text box of this controller to the playerhealthbox
        healthTextBox = PlayerManager.transform.FindChild("PlayerUI").GetComponentInChildren<Text>();
        //get the player's attack line
        line = PlayerManager.transform.FindChild("PlayerLine").gameObject;
        //get the position of the graveyard
        graveyardPos = PlayerManager.transform.FindChild("Graveyard").position;
        //get the position of the card pool
        cardPool = PlayerManager.transform.FindChild("CardPool").position;
        // get summon zones
        for(int index = 0; index < numberOfSummonZones; index++ )
        {

            Transform zone = PlayerManager.transform.Find("SummonZone"+index);
            SummonZones[index] = zone.gameObject;
            OccupiedZones.Add(index, false);

        }   
        //set initial text for health text box
        healthTextBox.text = "Life: " + startingHealth;
    }

    
  
    //Instantiates cards in deck in an offscreen area (card pool)
    //Loads the deck into the card deck list, shuffles list
    public void LoadDeck()
    {
        //retrieves all cards in the cardsInDeck array (which should reference the current deck)
        //card ids holds UNIQUE identifiers for each card instance (item instance id)
        string[] cardIds = (string[])PlayFabDataStore.cardsInDeck.ToArray();
        Debug.Log("Number of cards in deck: " + cardIds.Length);
        //for each card in the cardids array, find its item id and add it to deck
        for (int i = 0; i < cardIds.Length; i++)
        {
            //retrieve item id for this item instance id
            string itemId = PlayFabDataStore.itemIdCollection[cardIds[i]];
            //Instantiate the passed card over photon network, place it in the local card pool
            GameObject cardToAdd = PhotonNetwork.Instantiate(("BaseCardPrefab"), cardPool, Quaternion.identity, 0);
            //get the card's view Id to reference the same card
            int viewId = cardToAdd.GetComponent<PhotonView>().viewID;
            //INITIALIZE CARD 
            cardToAdd.GetComponent<CardTypeSelector>().SetCardType(playerID, itemId);          
            //add that card to the deck
            cardDeck.Add(viewId);
        }
        Debug.Log("Card data retrieved, begin loading deck");

        //Shuffles the local player's deck
        ShuffleDeck();

        //draw a hand of (startingHandSize) cards
        for (int i = 0; i < startingHandSize; i++)
        {
            //get the viewid of the card being drawn
            int viewId = cardDeck[currentCardIndex];
            photonView.RPC("DrawCard", PhotonTargets.All, viewId);
        }      
        //after this resolves, the player's hand should have 5 cards and the current card index 
        //should be at 5 (since indecies 0-4 have been taken)
    }

    // Update is called once per frame
    void Update ()
    {
        //checks for mouse clicks and collisions
        CheckInput();
        //if this controller belongs to the local player
        if(opponent == null && photonView.isMine && GameObject.Find("NetworkOpponent") != null)
        {opponent = GameObject.Find("NetworkOpponent").GetComponent<PlayerController>();}
        //keep health text updated to current health
        healthTextBox.text = "Life: " + health;
        //TODO put this back
        //CheckForLoss();
        DrawTimer();
    }

    void CheckInput()
    {      
        if (Input.GetMouseButtonDown(0))
        {
            CheckClickDown();
        }
        else if (Input.GetMouseButton(0))
        {
            CheckDrag();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            CheckClickUp();
        }   
         
    }

    Transform GetTransformUnderMouse()
    {
        RaycastHit2D hit;
        if (hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100.0F))
        { return hit.transform;}       
        return null;    
    }

    void CheckClickDown()
    {
        Transform hit = GetTransformUnderMouse();
        if (hit != null && currentClickState == clickState.Empty)
        {
            Debug.Log("Hit: " + hit.name);
            BaseCard cardScript;
            //if the player clicks a card and they own the card
            if (hit.tag == "Card" && (cardScript = hit.GetComponent<BaseCard>()) && cardScript.IsOwner())
            {              
                switch (cardScript.GetCardState())
                {
                    case (BaseCard.cardState.InHand):
                        //pick up card
                        SetSelectedCard(hit);
                        currentClickState = clickState.HoldingCard;
                        cardScript.Pickup();
                        break;
                    case (BaseCard.cardState.WaitForCastTarget):
                        Debug.Log("Cardstate is: " + cardScript.GetCardState());
                        currentClickState = clickState.Targetting;
                        //handle target selection before casting
                        SetSelectedCard(hit);
                        break;
                    case (BaseCard.cardState.InPlay):
                        //handle click abilities (targetting, attacking, etc.)
                        break;
                    default:
                        Debug.Log("No click function on card in this state");
                        break;
                }
            }         
        }
    }

    private void SetSelectedCard(Transform hit)
    {
        if(hit == null)
        {
            selectedCard = null;
            selectedCardScript = null;
        }
        else
        {
            selectedCard = hit;
            selectedCardScript = hit.GetComponent<BaseCard>();
        }       
    }

    void CheckDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);      
        if (currentClickState == clickState.HoldingCard)
        {
            curPosition.z = selectedCard.position.z;
            selectedCard.position = curPosition;
        }
        else if(currentClickState == clickState.Targetting)
        {
            selectedCardScript.MoveReticle(curPosition);
        }
    }

    void CheckClickUp()
    {
        if (currentClickState == clickState.HoldingCard)
        {
            //drop card
            currentClickState = clickState.Empty;
            selectedCardScript.Drop();
            SetSelectedCard(null);
        }
        else if (currentClickState == clickState.Targetting)
        {
            //drop card
            currentClickState = clickState.Empty;
            selectedCardScript.SetTarget(GetTransformUnderMouse());
            SetSelectedCard(null);
        }
    }

    //Checks if enough time has passed to draw a card
    void DrawTimer()
    {     
        if (photonView.isMine)
        {
            //increase counter for draw timer
            libraryDrawCounterTimer += Time.deltaTime;  
            if (libraryDrawCounterTimer > libraryDrawSpeed)
            {
                libraryDrawCounterTimer = 0;
                if (currentHandSize < maxHandSize)
                {                  
                    //get the viewid of the card being drawn
                    int viewId = cardDeck[currentCardIndex];
                    //tell local and remote copy of this player to draw this card from card pool
                    photonView.RPC("DrawCard", PhotonTargets.All, viewId);
                }
            }
        }
    }

    //NEW CARD DRAWING FUNCTION
    //Sets up card variables and position of card in hand
    [PunRPC]
    public void DrawCard(int viewId)
    {
        GameObject cardToAdd = null;
        //find the spawned card, Set cards position
        if (cardToAdd = PhotonView.Find(viewId).gameObject)
        {           
            //add it to the player's hand
            //playerHand.Add(cardToAdd);
            BaseCard cardScript = cardToAdd.GetComponent<BaseCard>();
            int handIndex = AddToHand(cardToAdd); //< returns index of card 
            cardToAdd.transform.position = handPositions[handIndex];//new Vector3(handZone.x + (currentHandSize * 5f), handZone.y, handZone.z);
            //set cards hand index and InHand state
            cardScript.photonView.RPC("AddCardToHand", PhotonTargets.All, handIndex); 
            //increment the index of the deck (since a card has now been taken)
            currentCardIndex++;
        }
    }

    //places card into proper slot in hand
    int AddToHand(GameObject cardToAdd)
    {
        for (int i = 0; i < maxHandSize; i++)
        {
            //if no card is stored at this index
            if (playerHand[i] == null)
            {
                playerHand[i] = cardToAdd;
                //increment hand size
                currentHandSize++;
                return i;
            }
        }
        Debug.Log("Failed to add card to hand");
        return -1;
    }

    //removes card from specific index in hand
    //shifts cards down to fill empty slot
    void RemoveFromHand(int index)
    {
        Debug.Log("Trying to remove index: " + index);
        if (playerHand[index] != null)
        {
            playerHand[index] = null;
            //decrement hand size
            currentHandSize--;

            for (int i = index+1; i < playerHand.Length; i++)
            {
                if (playerHand[i] != null)
                {
                    //set card's hand index
                    int cardHandindex = (playerHand[i].GetComponent<BaseCard>().handIndex -= 1);
                    playerHand[i].transform.position = handPositions[cardHandindex];
                    playerHand[i - 1] = playerHand[i];
                    playerHand[i] = null;
                }
            }
        }
        else { Debug.Log("Unable to remove card from hand, no card at index");}
    }

    public void ReturnToHand(GameObject card)
    {
        BaseCard cardScript = card.GetComponent<BaseCard>();
        if (cardScript.currentCardState == BaseCard.cardState.InPlay) //< if the card is returned to hand from play
        {
            // TODO handle return to hand from play
        }
        else//< if card is dropped in an invalid place
        { card.transform.position = cardScript.cardHandPos; }   
    }

    // checks if a player's health has reached 0
    void CheckForLoss()
    {
        //if health is 0 or less
        if (health <= 0)
        {
            if (photonView.isMine)
            {
                //call lose function on gamemanager
                //displays ending panel and message
                gameManager.Lose();
            }
            else if (!photonView.isMine) // if this is the opponent
            {
                //call win function on gamemanager
                //displays ending panel and message
                gameManager.Win();
            }
        }
    }
   
    //HANDLE DAMAGE OVER NETWORK
    [PunRPC]
    public void ChangeHealth(int amount)
    {
        //this should never happen, but if it does, return immediately
        if(amount == 0)
        { return; }
        
        health += amount;
  
    }

    
    //Method called for when a card is dropped
    public bool cardIsDropped(GameObject card, int zoneIndex)
    {
        //if zone is unoccupied
        if(OccupiedZones[zoneIndex] == false)
        {
            Debug.Log("Zone is not occupied, playing card in zone: "+zoneIndex);
            OccupiedZones[zoneIndex] = true;
            photonView.RPC("PlayCard", PhotonTargets.All, card.GetComponent<PhotonView>().viewID, zoneIndex);
            return true;
        }
        else
        {
            Debug.Log("Zone "+ zoneIndex+ " is occupied, invalid play");
            return false;
        }
      /*
        //If the card is a creature card, add it to the list of creature cards in play
        if (card.GetComponent<Card> ().cardType == "Creature") {
            creatureThatJustEntered = card;
            creatureCardsInPlay.Add (card);
     */
       
    }
        

    //New Play Card Function
    [PunRPC]
    public void PlayCard(int cardId, int zoneIndex)
    {
        Debug.Log("Card played in index "+ zoneIndex);
        //find the spawned card
        GameObject card = PhotonView.Find(cardId).gameObject;
        //find the proper summon zone
        GameObject summonZone = SummonZones[zoneIndex];
        /*
        //sets the zone as occupied, passes the card to occupy
        summonZone.GetComponent<SummonZone>().SetOccupied(card.GetComponent<BaseCard>(), true);
        */

        //Puts the card in the summoning zone
        card.transform.position = new Vector3(summonZone.transform.position.x, summonZone.transform.position.y, card.transform.position.z);
        //Remove card from hand
        RemoveFromHand(card.GetComponent<BaseCard>().handIndex);
        
    }
   /*
	[PunRPC]
	public void ResolveCreatureDamage(GameObject attacker, GameObject defender)
	{
        //get CreatureCard component of both attacker and defender
        CreatureCard attackerScript = attacker.GetComponent<CreatureCard>();
        CreatureCard defenderScript = defender.GetComponent<CreatureCard>();

        //if the attacker is not elusive, handle damage to attacker
        if(!attackerScript.creatureAbilities.Contains("Elusive"))
        {
           attackerScript.health -= defenderScript.damageToDeal;
        }
        //if the defender is not elusive, handle damage to defender
        if (!defenderScript.creatureAbilities.Contains("Elusive"))
        {
            defenderScript.health -= attackerScript.damageToDeal;
        }
    }
    */
    

    //function for determining target when a card is dragged to a target
    public GameObject CardIsTargetted()//(GameObject card, Vector3 cardHandPos)
    {
        Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);
        //if the raycast hits an object
        if (hit)
        {
            Debug.Log("Card is targetted returned: "+hit.transform.gameObject.name);
            //store the hit object
            return hit.transform.gameObject;
        }
        else
        {
            Debug.Log("Card is targetted returned null");
            return null;
        }
    }
    /*
    //NEW VERSION FOR NEW CARDS
    //Resolves damage when it involves creatures (creature to creature or spell to creature)
    [PunRPC]
    public void ResolveDamage(int targetId, int attackerId)
    {
        //target is always a creature, attacker could be creature or spell

        GameObject attacker = PhotonView.Find(attackerId).gameObject;
        GameObject target = PhotonView.Find(targetId).gameObject;
        BaseCard attackerScript = attacker.GetComponent<BaseCard>();
        BaseCard targetScript = target.GetComponent<BaseCard>();

        Debug.Log("Resolving damage from "+attacker.name+" to "+target.name);
        CreatureCard targetCreature = target.GetComponent<CreatureCard>();
        //if the creature can be attacked
        if (targetCreature.inBattlefield && targetCreature.isAttackable)
        {
            //if the target of the damage is a creature and the damage dealer is a creature
            if (attackerScript.cardType == "Creature")
            {

                //store attacking creature's script
                CreatureCard attackerCreature = attacker.GetComponent<CreatureCard>();


                attackerCreature.creatureCanAttack = false;
                //if the attacking creature doesnt have elusive, deal it damage
                //Elusive prevents damage from other creatures
                if (!attackerCreature.creatureAbilities.Contains("Elusive"))
                {
                    attackerCreature.health -= targetScript.attackPower;
                }
                //if the defending creature doesnt have elusive, deal it damage    
                if (!targetCreature.creatureAbilities.Contains("Elusive"))
                {
                    targetCreature.health -= attackerScript.attackPower;
                }
                   
                
            }

            if (attackerScript.cardType == "Spell")
            {
                //this is += because opponent health change uses negatives for damage
                targetCreature.health += attackerScript.targetHealthChange;
            }
        }
    }

    //Function for dragged targeting (creature / spell damage)
    public void CardTargetDamage(GameObject attackingCard, Vector3 cardHandPos, GameObject currentTarget)
    {
        //if the damaging card has a target
        if (photonView.isMine && currentTarget != null)
        {
            Debug.Log(attackingCard + " dealing damage to " + currentTarget);

            //get the card component of the dragged card
            BaseCard attackCardScript = attackingCard.GetComponent<BaseCard>();
           
            //if the target is a card
            if(currentTarget.GetComponent<BaseCard>() != null)
            {
                //if the hit object is a creature card
                if (currentTarget.tag == "CreatureCard")
                {
                    //RESOLVE DAMAGE
                    photonView.RPC("ResolveDamage", PhotonTargets.All, currentTarget.GetPhotonView().viewID, attackingCard.GetPhotonView().viewID);      
                }
                else
                {
                    Debug.Log("Cannot damage non-creature card");
                }
            }
           //if the object is the player
            else if (currentTarget.tag == "Player2")
            {
                Debug.Log("Target verified as opposing player");
                if(attackCardScript.cardType == "Creature")
                {
                    Debug.Log("Attacking card verified as creature");
                    attackingCard.GetComponent<CreatureCard>().creatureCanAttack = false;
                    //damage/healing to deal to opponent (playerid is 2 for opponent)
                    opponent.ChangeHealth(-1* attackCardScript.attackPower);
                    Debug.Log("opponent change health: "+ attackCardScript.attackPower);
                }
                else if(attackCardScript.cardType == "Spell")
                {
                    Debug.Log(attackingCard+ " is a spell and is dealing" + attackCardScript.targetHealthChange + " damage to opponent");
                    //damage/healing to deal to owner (calls change health in this script)
                    ChangeHealth(attackCardScript.ownerHealthChange);
                    //damage/healing to deal to opponent (playerid is 2 for opponent)
                    opponent.ChangeHealth(attackCardScript.targetHealthChange);
                }
               
            }
        }
    }
    //Standard creature card dropped function
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
						//if your creature doesn't have elusive then deal damage
						if(!card.GetComponent<CreatureCard>().elusive)
							card.GetComponent<CreatureCard> ().health -= enemyObjectUnderMouse.GetComponent<CreatureCard> ().damageToDeal;
						//if the enemy creature doesn't have elusive then deal damage
						if(!enemyObjectUnderMouse.GetComponent<CreatureCard>().elusive)
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
                //gameManager.dealDamage(card.GetComponent<DamageCard>().damageToDeal, playerID);
            }
        }
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
					for (int i = 0; i < SummonZones.Length; i++) 
					{
						if (!SummonZones [i].GetComponent<SummonZone> ().isOccupied) 
						{
							//Get's the position of the zone
							Vector3 zonePosition = SummonZones[i].transform.position;
							//Play card, pass the card, the position of the zone, and the index of the zone
							//card.GetComponent<CreatureTargetSpellCard>().setSummonZoneTextBox(SummonZones[i].GetComponent<SummonZone>().textBox);
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
						sendToGraveyard (card, card.GetComponent<BaseCard>().zoneIndex);
					}
                    
				}
			}
		}
	}
   */

    //This method is called when the card is done casting
    public void sendToGraveyard(GameObject card, int zoneIndex)
    {
        // set occupied zone as empty
        OccupiedZones[zoneIndex] = false;
        //Add card to graveyard
        graveyard.Add(card);
        //Moves card to the graveyard
        card.transform.position = graveyardPos;

        /*
		//If the card being sent to the graveyard is a creature card, remove it from the list of creature cards in play
		if (card.GetComponent<CreatureCard>() != null)
			creatureCardsInPlay.Remove (card);
            */
    }
   

    //Takes the carddeck and shuffles its contents
    private void ShuffleDeck()
    {
        for (int i = 0; i < cardDeck.Count; i++)
        {
            int temp = cardDeck[i];
            int randomIndex = Random.Range(i, cardDeck.Count);
            cardDeck[i] = cardDeck[randomIndex];
            cardDeck[randomIndex] = temp;
        } 
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

/*
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
 */
    
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
    /*
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
    */

    
}
