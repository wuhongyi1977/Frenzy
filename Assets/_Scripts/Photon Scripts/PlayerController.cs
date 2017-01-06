using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public delegate void LoseEvent();
    public static event LoseEvent OnLose;

    // Components
    GameObject PlayerManager;
    PlayerController opponent;
    PhotonView photonView;
    private Text healthTextBox;   
    Vector3 handZone; //< The empty game object for the position of the handzone
    Vector3 graveyardPos; //< The position of the graveyard. (off screen) 
    Vector3 cardPool; //< The position of the card pool (off screen)
    private GameObject line; 


    //PLAYER STATS
    int startingHealth = 20; //< Players initial health
    int health;  //< Player's current health


    /// <summary>
    /// All code from playercontroller
    /// </summary>
    //The total number of shuffles when the game starts
    private int maxInitialShuffles = 10;
    //The number of times the deck is shuffled before game starts
    private int numbShuffles;
    //The player ID (1 is local player, 2 is opponent)
    private int playerID = 1; // TODO change to Enum (Unassigned, Local, Opponent)
    
    //the maximum number of cards a player can hold
    int maxHandSize = 7;
    //the number of cards to draw at the start of the game
    int startingHandSize = 5;
    //The speed in seconds in which the player draws a card from the deck
    int libraryDrawSpeed = 7;
    //the number of cards in a player's deck
    int deckSize = 40;
    
    //The list of summoning zones
    public GameObject[] SummonZones = new GameObject[3];
    //The list that represents the player's hand
    //public Dictionary<int, GameObject> playerHand = new Dictionary<int, GameObject>();//< stores a card at an index
    public GameObject[] playerHand = new GameObject[7];
    //public List<GameObject> playerHand = new List<GameObject>(7);

    public List<int> cardDeck = new List<int>(60); //CHANGED 


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
    
    //The card that the player is moused over
    private GameObject mousedOverCard;
   
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
        //set the health text box of this controller to the playerhealthbox
        healthTextBox = PlayerManager.transform.FindChild("PlayerUI").GetComponentInChildren<Text>();
        //get the player's attack line
        line = PlayerManager.transform.FindChild("PlayerLine").gameObject;
        //get the position of the graveyard
        graveyardPos = PlayerManager.transform.FindChild("Graveyard").position;
        //get the position of the card pool
        cardPool = PlayerManager.transform.FindChild("CardPool").position;
        // get summon zones
        int index = 0;
        foreach (Transform t in PlayerManager.transform)
        {
            if (t.name == "SummonZone")
            { SummonZones[index++] = t.gameObject; }
        }
        //set initial text for health text box
        healthTextBox.text = "Life: " + startingHealth;
    }

    // TODO Do this only on local player, remote copy doesnt need to know all cards in deck (i think)
    //However, all cards should be instantiated at start and held in a pool so they can be accessed quickly
    //Instantiated cards can be held in an off screen area (card pool), moved on screen by local version when put into deck zone
    //Loads a particular deck into the card deck list
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
            //get the prefab name associated with this item id
            string cardToDraw = PlayFabDataStore.cardPrefabs[itemId]; //< TODO make all cards use the same prefab, attach proper behavior in code
            //Instantiate the passed card over photon network, place it in the local card pool
            GameObject cardToAdd = PhotonNetwork.Instantiate(("Cards/" + cardToDraw), cardPool, Quaternion.identity, 0);
            //get the card's view Id to reference the same card
            int viewId = cardToAdd.GetComponent<PhotonView>().viewID;
            //INITIALIZE CARD
            cardToAdd.GetComponent<Card>().playerID = playerID;
            cardToAdd.GetComponent<Card>().InitializeCard(itemId);
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
        //if this controller belongs to the local player
        if(opponent == null && photonView.isMine && GameObject.Find("NetworkOpponent") != null)
        {
            opponent = GameObject.Find("NetworkOpponent").GetComponent<PlayerController>();
        }
        //keep health text updated to current health
        healthTextBox.text = "Life: " + health;

        //COMMENTED OUT TEMPORARILY
        //CheckForLoss();
        DrawTimer();
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
            cardToAdd.transform.position = new Vector3(handZone.x + (currentHandSize * 5f), handZone.y, handZone.z);
            //set the card's player id
            cardToAdd.GetComponent<Card>().playerID = playerID;
            //add it to the player's hand
            //playerHand.Add(cardToAdd);
            cardToAdd.GetComponent<Card>().handIndex = AddToHand(cardToAdd); //< returns index of card                                                                             //increment the index of the deck (since a card has now been taken)
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

    void RemoveFromHand(int index)
    {
        if (playerHand[index] != null)
        {
            Debug.Log("CARD REMOVED FROM INDEX: "+ index);
            Debug.Log("CURRENT HAND SIZE: " + currentHandSize);
            playerHand[index] = null;
            //decrement hand size
            currentHandSize--;
            //shift cards down
            for (int i = (index +1); i <= currentHandSize; i++)
            {
                if (playerHand[i] != null)
                {
                    playerHand[i].transform.position = new Vector3(handZone.x + ((i - 1) * 5f), handZone.y, 0);
                    //set card's hand index
                    playerHand[i].GetComponent<Card>().handIndex = (i - 1);
                }                                 
            }
        }
        else { Debug.Log("Unable to remove card from hand, no card at index");}
    }


    void CheckForLoss()
    {

        //if health is 0 or less
        if (health <= 0)
        {
            if (photonView.isMine)
            { Lose(); }
            else if (!photonView.isMine) // if this is the opponent
            { Win(); }
        }

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
        for (int i = 0; i < SummonZones.Length; i++)
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
						if (card.GetComponent<Card> ().cardType == "Creature") {
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
        RemoveFromHand(card.GetComponent<Card>().handIndex);
    }
   
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

    //NEW VERSION FOR NEW CARDS
    //Resolves damage when it involves creatures (creature to creature or spell to creature)
    [PunRPC]
    public void ResolveDamage(int targetId, int attackerId)
    {
        //target is always a creature, attacker could be creature or spell

        GameObject attacker = PhotonView.Find(attackerId).gameObject;
        GameObject target = PhotonView.Find(targetId).gameObject;
        Card attackerScript = attacker.GetComponent<Card>();
        Card targetScript = target.GetComponent<Card>();

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
                targetCreature.health += attackerScript.opponentHealthChange;
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
            Card attackCardScript = attackingCard.GetComponent<Card>();
           
            //if the target is a card
            if(currentTarget.GetComponent<Card>() != null)
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
                    Debug.Log(attackingCard+ " is a spell and is dealing" + attackCardScript.opponentHealthChange + " damage to opponent");
                    //damage/healing to deal to owner (calls change health in this script)
                    ChangeHealth(attackCardScript.ownerHealthChange);
                    //damage/healing to deal to opponent (playerid is 2 for opponent)
                    opponent.ChangeHealth(attackCardScript.opponentHealthChange);
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
                gameManager.dealDamage(card.GetComponent<DamageCard>().damageToDeal, playerID);
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
        for (int i = 0; i < SummonZones.Length; i++)
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
        for (int i = 0; i < SummonZones.Length; i++)
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
    /*
    public static List<string> shuffleDeck(List<string> library)
    {
        System.Random _random = new System.Random();
        string temp;

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
    */
   
    

    

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
