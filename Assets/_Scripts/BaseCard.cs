using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
/// <summary>
/// A script to handle the functions that should be available to every
/// card in the game. This is an abstract object. It will be used as a
/// base for all refined cards
/// </summary>
public abstract class BaseCard : MonoBehaviour
{
    // Event for field handler
    public delegate void FieldHandlerCheck(int viewId, int zoneIndex); //< receives the success of the call
    public static event FieldHandlerCheck NotifyEnter;
    public static event FieldHandlerCheck NotifyExit;

    //Events for Audio Handler
    public delegate void SoundEffectEvent(string soundEffectName); 
    public static event SoundEffectEvent PlaySound;



    //PHOTON COMPONENTS
    public PhotonView photonView;
    protected CardAbilityList cardAbilityList;

    //Card State
    public enum cardState { OutOfPlay, InHand, Held, WaitForCastTarget,
                            WaitForTarget, Casting, InPlay, InGraveyard };
    public cardState currentCardState = cardState.OutOfPlay;

    //CARD COMPONENTS
    //contains all visible components on card
    protected Transform cardLayoutCanvas;
    protected Canvas cardCanvasScript;//< script for canvas that contains all card elements 
    protected Image cardArtImage;//< the image to display for the card art
    protected Text descriptionText;//< the text displayed to the player listing the card's abilities
    protected Text summonZoneTextBox;//< shows the casting timer
    protected Text cardTitleTextBox; //< displays the card title
    protected Text castTimeTextBox; //< displays the cast time
    protected Text attackPowerTextBox; //< shows the attack power (creature only)
    protected Text defensePowerTextBox; //< shows the defense power (creature only)
    protected Text rechargeTimeTextBox; //< shows the recharge time (creature only)
    protected Image inactiveFilter; //< makes card grayed out when inactive (e.g. casting)
    protected Image cardBack; //< blocks card info when in opponents hand   
    protected LineRenderer targetLine;
    protected GameObject targetReticle;

    public GameObject targetObject = null;

    //Scaling variables and sorting layer
    protected Vector3 startingScale, zoomScale;
  
    //The position that the card was at when the player picks up the card. 
    //This is used for when a player makes an invalid placement the card is placed back in it's original hand position
    public Vector3 cardHandPos;
    public int handIndex;

    bool casting = false;
    float castCountdown;

    //reference to zone this card is occupying
    public int zoneIndex;

    private Color initialTextColor;


    //STATS 
    //all card stats are here, are assigned by custom data from Playfab
    //name of art asset to use for card art
    public string artName;
    //the type of card classification this card has (creature, spell, etc.)
    public string cardType;
    //The time it takes for the card to be casted
    public float castTime;
    public string castTarget = null;
    //The potential targets for this cards abilities (could be creature, player, all, autocast)
    //does not include types of targets for a creature attack, but DOES include targets for special creature abilites (like direct damage on summon)
    public string target;
    //The faction that the card belongs to. Neutral means it is available to all factions
    public string faction = "Neutral";

    // abilities that trigger on cast
    protected List<string> castAbilities = new List<string>();
    // holds values associated with abilities
    public Dictionary<string, string> abilityValues = new Dictionary<string, string>();
    // holds values associated with creature stats
    public Dictionary<string, string> creatureStatValues = new Dictionary<string, string>();
    //the list of standard abilities a creature has (Rush, Elusive, etc.)
    //to handle abilities, have each possible occurence check if hashset contains ability
    public HashSet<string> creatureAbilities = new HashSet<string>();

    //The itemId of the Card (not unique, used to reference custom data)
    public string cardId;
    //the custom data of the card
    private string customData;
    //The name of the card
    public string cardTitle;

    //The ID of the player that this card belongs to
    public int playerID;

    public GameObject localPlayer;
    public GameObject networkOpponent;
    public PlayerController localPlayerController;
    public PlayerController opponentPlayerController;


    protected void OnEnable()
    {
        FieldManager.OnEnter += EnterSuccess;
        FieldManager.OnExit += ExitSuccess;
    }

    protected void OnDisable()
    {
        FieldManager.OnEnter -= EnterSuccess;
        FieldManager.OnExit -= ExitSuccess;
    }

    //This can be used for initialization code that is identical on ALL cards
    protected virtual void Awake()
    {
        //get photon view component of this card
        photonView = GetComponent<PhotonView>();
        cardAbilityList = GetComponent<CardAbilityList>();
       
        targetReticle = transform.FindChild("TargetReticle").gameObject;
        targetReticle.SetActive(false);
        
        //if the layout canvas of this card is found, locate its components
        if (cardLayoutCanvas = transform.FindChild("CardLayoutCanvas"))
        {
            //Get all components
            descriptionText = cardLayoutCanvas.FindChild("DescriptionText").GetComponent<Text>();
            summonZoneTextBox = cardLayoutCanvas.FindChild("Counter").GetComponent<Text>(); 
            cardTitleTextBox = cardLayoutCanvas.FindChild("CardTitle").GetComponent<Text>();
            castTimeTextBox = cardLayoutCanvas.FindChild("CastTime").GetComponent<Text>(); 
            attackPowerTextBox = cardLayoutCanvas.FindChild("AttackPower").GetComponent<Text>();
            defensePowerTextBox = cardLayoutCanvas.FindChild("DefensePower").GetComponent<Text>();
            rechargeTimeTextBox = cardLayoutCanvas.FindChild("RechargeTime").GetComponent<Text>();
            cardArtImage = cardLayoutCanvas.FindChild("CardArtImage").GetComponent<Image>();
            inactiveFilter = cardLayoutCanvas.FindChild("InactiveFilter").GetComponent<Image>();
            inactiveFilter.enabled = false;
            cardBack = cardLayoutCanvas.FindChild("CardBack").GetComponent<Image>();
            cardBack.enabled = false;
            targetLine = GetComponent<LineRenderer>();
            targetLine.enabled = false;

            cardCanvasScript = cardLayoutCanvas.GetComponent<Canvas>();
        }
        initialTextColor = summonZoneTextBox.color;
        startingScale = transform.localScale;
        zoomScale = startingScale * 2;
        cardTitleTextBox.text = cardTitle;
    }

    //resets variables if card is returned to hand
    [PunRPC]
    public virtual void Reset()
    {
        currentCardState = cardState.OutOfPlay;
        castCountdown = castTime;
        casting = false;
        summonZoneTextBox.color = initialTextColor;
        targetReticle.SetActive(false);
        inactiveFilter.enabled = false;
        cardBack.enabled = false;
        targetLine.enabled = false;
        targetObject = null;
        summonZoneTextBox.text = "";
    }

    public virtual void Start()             //Abstract method for start
    {
     
        
    }
    public virtual void Update()                //Abstract method for Update
    {
        GetPlayers();
        
        //handle casting countdown
        if(casting)
        {
            castCountdown -= Time.deltaTime;
            summonZoneTextBox.text = castCountdown.ToString("F1");
            if(castCountdown <= 0)
            {
                casting = false;
                //clear summon zone text
                summonZoneTextBox.text = "";
                //call entrance event for field manager
                NotifyEnter(photonView.viewID, zoneIndex);
            }
        }

    }

    /// <summary>
    /// Standard functions for all cards
    /// </summary>

    protected void EnterSuccess(int viewId, bool success)
    {
        //if this was the card that just entered and it entered successfully
        if(viewId == photonView.viewID && photonView.isMine)
        {
            if (success) //< if successful, activate OnPlay
            {
                photonView.RPC("PutIntoPlay", PhotonTargets.All);
            } 
            else //< if not successful, destroy
            {
                //call exit event for field manager
                NotifyFieldManagerExit();
            }  
                
        }
        else //< if this is not the owned card, handle any events due to a card entering play
        {
            CardEntersPlayTrigger(viewId);
        }
    }

    protected void ExitSuccess(int viewId, bool success)
    {
        //if this was the card that just left and it left successfully
        if (viewId == photonView.viewID && photonView.isMine)
        {
            if (success) //< if successful, send to graveyard
            {
                photonView.RPC("SendToGraveyard", PhotonTargets.All);
            }
            // if leaving play was unsuccessful, do nothing (card remains in play)
        }
        else //< if this is not the owned card, handle any events due to a card leaving play
        {
            CardLeavesPlayTrigger(viewId);
        }
    }

    protected virtual void CardEntersPlayTrigger(int viewId)
    {
        //override in sub class for proper behavior
    }
    protected virtual void CardLeavesPlayTrigger(int viewId)
    {
        //override in sub class for proper behavior
    }

    public void NotifyFieldManagerEntrance() //< call exit event for field manager
    { NotifyEnter(photonView.viewID, zoneIndex);}

    public void NotifyFieldManagerExit() //< call exit event for field manager
    { NotifyExit(photonView.viewID, zoneIndex);}

    //returns the cards state to calling script
    public cardState GetCardState()
    {
        return currentCardState;
    }

    //returns true if this card belongs to the local player
    public bool IsOwner()
    {
        return photonView.isMine;
    }

    //Draws card
    [PunRPC]
    public void AddCardToHand(int indexToSet)
    {   
        handIndex = indexToSet;
        currentCardState = cardState.InHand;
        if (!photonView.isMine)
        {
            cardBack.enabled = true;
        }
    }
 
   
    //Picks up a card (while in hand)
    public virtual void Pickup()
    {
        transform.localScale = startingScale;
        currentCardState = cardState.Held;
        cardHandPos = gameObject.transform.position;
        if (PlaySound != null)
        {
            PlaySound("playCardPickup");
        }
    }
    
    public void MoveReticle(Vector3 pos)
    {
        targetReticle.transform.position = new Vector3(pos.x, pos.y, targetReticle.transform.position.z);
    }

    public void SetTarget(Transform potentialTarget)
    {
        //copy this block into all cards, cards cannot target nothing or themselves
        if (potentialTarget == null || potentialTarget == this.transform)
        {
            MoveReticle(transform.position);
            return;
        }
        CreatureCard targetCreatureScript;
        if (currentCardState == cardState.WaitForCastTarget)
        {
            //check against cast target list
            if (castTarget == "All")
            {
                if(potentialTarget.tag == "Player1" || potentialTarget.tag == "Player2")
                {
                    AcceptTargetAndCast(potentialTarget);
                    return;
                }
                else if(targetCreatureScript = potentialTarget.GetComponent<CreatureCard>())
                {
                    if(targetCreatureScript.currentCardState == BaseCard.cardState.InPlay ||
                        targetCreatureScript.currentCardState == BaseCard.cardState.WaitForTarget)
                    {
                        AcceptTargetAndCast(potentialTarget);
                        return;
                    }
                   
                }
            }            
            if(castTarget == "Player" && (potentialTarget.tag == "Player1" || potentialTarget.tag == "Player2"))
            {
                AcceptTargetAndCast(potentialTarget);
                return;
            }
            else if (castTarget == "Creature" && (targetCreatureScript = potentialTarget.GetComponent<CreatureCard>()))
            {
                if (targetCreatureScript.currentCardState == BaseCard.cardState.InPlay ||
                        targetCreatureScript.currentCardState == BaseCard.cardState.WaitForTarget)
                {
                    AcceptTargetAndCast(potentialTarget);
                    return;
                }
            }
            else
            {
                MoveReticle(transform.position);
                return;
            }
        }
        else if (currentCardState == cardState.WaitForTarget)
        {
            if (potentialTarget.tag == "Player1" || potentialTarget.tag == "Player2")
            {
                targetObject = potentialTarget.gameObject;
                return;
            }
            else if (targetCreatureScript = potentialTarget.GetComponent<CreatureCard>())
            {
                if (targetCreatureScript.currentCardState == BaseCard.cardState.InPlay ||
                        targetCreatureScript.currentCardState == BaseCard.cardState.WaitForTarget)
                {
                    targetObject = potentialTarget.gameObject;
                    return;
                }
                
            }
        }
        else if (currentCardState == cardState.InPlay)
        {
            // check against normal target list
        }
    }

    protected void AcceptTargetAndCast(Transform target)
    {
        MoveReticle(target.position);
        targetObject = target.gameObject;
        ///Draw Target Line
        int id = target.GetComponent<PhotonView>().viewID;
        photonView.RPC("DrawTargetLine", PhotonTargets.All, id);
        //cast card      
        photonView.RPC("Cast", PhotonTargets.All);
    }


    //Drops a held card
    //handles drop over summon zone and drop over other areas
    public void Drop()
    {
        bool droppedSuccess = false;
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100.0F);
        for (int i = 0; i < hits.Length; i++)
        {
            Transform hit = hits[i].transform;
            //if a hit happens, card is over an unoccupied zone
            if (hit.tag == "Player1SummonZone")
            {             
                zoneIndex = int.Parse(hit.name.Substring(hit.name.Length - 1));
                droppedSuccess = localPlayerController.cardIsDropped(gameObject, zoneIndex);
                if ( droppedSuccess== true)
                {
                    inactiveFilter.enabled = true;
                    handIndex = -1;
                    GetCastingTarget();
                    if (PlaySound != null)
                    {
                        PlaySound("playCardInSpellSlot");
                    }
                }
                break;
            }
        }
        if (!droppedSuccess)
        {
            //return to hand
            currentCardState = cardState.InHand;
            transform.position = cardHandPos;
        }
    }

    protected virtual void GetCastingTarget()
    {
        if(castTarget != null)
        {
            //handle target selection
            targetReticle.SetActive(true);
            currentCardState = cardState.WaitForCastTarget;
        }
        else
        {
            //cast the card
            photonView.RPC("Cast", PhotonTargets.All);
        }     
    }

    //runs casting countdown timer, plays card when timer reaches 0
    [PunRPC]
    public void Cast()
    {
       
        Debug.Log("Casting card....");
        //if target != autocast
        // wait for target to be selected before casting
        currentCardState = cardState.Casting;
     
        cardBack.enabled = false;
        inactiveFilter.enabled = true;
        castCountdown = castTime;
        //begins countdown in update
        casting = true;
    }

    //called when casting finishes successfully
    [PunRPC]
    protected virtual void PutIntoPlay()
    {
        if (PlaySound != null)
        {
            PlaySound("playCardBuildUp");
        }
        Debug.Log("Card entering play");
        currentCardState = cardState.InPlay;
        inactiveFilter.enabled = false;
        targetReticle.SetActive(false);
        if(photonView.isMine)
        {
            OnPlay();
        }
        
    }


    //used to hold a card's effect upon being put into play (if it has any)
    protected virtual void OnPlay()
    {
        //cast effect goes here
    }

    [PunRPC]
    public void SendToGraveyard()
    {
        if (currentCardState != cardState.InGraveyard)
        {          

            Debug.Log(cardTitle + " sent to graveyard");
            //Set state of card to being in the graveyard
            currentCardState = cardState.InGraveyard;
            summonZoneTextBox.text = "";
            targetLine.enabled = false;
            if(PlaySound != null)
            {
                PlaySound("playCardRelease");
            }
            
            //If the card beings to player 1
            if (photonView.isMine)
            { localPlayerController.sendToGraveyard(gameObject, zoneIndex); }
            else
            { opponentPlayerController.sendToGraveyard(gameObject, zoneIndex); }
            //handle graveyard effect of this card, if any
            OnGraveyard();
        }
    }


    public void ZoomCard(bool zoomIn)
    {
        if(zoomIn)
        {
            cardCanvasScript.sortingLayerName = "ActiveCard";
            //cardCanvasScript.sortingLayerID = 0;
            transform.localScale = zoomScale;
        }
        else
        {
            cardCanvasScript.sortingLayerName = "Default";
            transform.localScale = startingScale;
        }
       
    }

    public void OnMouseEnter()
    {
        if (currentCardState == cardState.InHand && PlaySound != null)
        {
            PlaySound("playCardSelect");
        }
    }


    public void InitializeCard(int ownerID, string id)
    {
        playerID = ownerID;
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
        cardTitleTextBox.text = cardTitle;
        //set cards description
        descriptionText.text = PlayFabDataStore.cardDescriptions[id];
        // Retrieve the custom data for this card type
        string[] data = PlayFabDataStore.cardCustomData[id];
        //iterate through each string in the split data
        //goes to 1 less than total length because the last variable doesnt need to be checked and nextString will fail
        for (int j = 0; j < data.Length - 1; j++)//splitResultTest.Length -1; j++)
        {
            //stores the current string being viewed
            string currentString = data[j];
            //store the next string in the list
            string nextString = data[j + 1];

            //Assign variables
            switch (currentString)
            {
                case "ArtName":
                    artName = nextString;
                    break;
                case "Faction":
                    faction = nextString;
                    break;
                case "CardType":
                    cardType = nextString;
                    break;
                case "CastTarget":
                    castTarget = nextString;
                    break;
                case "CastTime":
                    castTime = float.Parse(nextString);
                    castTimeTextBox.text = castTime.ToString();
                    break;
                case "RechargeTime":
                    creatureStatValues.Add(currentString, nextString);
                    break;
                case "AttackPower":
                    creatureStatValues.Add(currentString, nextString);
                    break;
                case "DefensePower":
                    creatureStatValues.Add(currentString, nextString);
                    break;

                //creature abilities
                case "Rush":
                case "Elusive":
                    creatureAbilities.Add(currentString);
                    break;
                case "Consume":
                    creatureAbilities.Add(currentString);
                    abilityValues.Add(currentString, nextString);
                    break;
                ///Card abilities w/ values
                case "TargetHealthChange":
                case "OwnerHealthChange":
                case "GrantCreatureAbility":
                case "FreezeCreature":
                case "ChangeCreatureAttackPower":
                case "ChangeAllOwnCreatureRecharge":
                case "Counter":
                    castAbilities.Add(currentString);
                    abilityValues.Add(currentString, nextString);
                    break;
                ///card abilities without values
                case "ReturnCardToHand":
                case "DiscardOnCast":
                    castAbilities.Add(currentString);
                    break;

                default:
                    break;
            }
        }
        
        //Set the proper art for the card
        SetArt();
        InitializeStats();
    }

    protected virtual void InitializeStats()
    {
        // overridden in child class to update internal stats retrieved from custom data
    }

    public void SetArt()
    {
        if (artName != null && artName != "temp")
        {
            cardArtImage.overrideSprite = Resources.Load<Sprite>("CardArt/" + artName);
        }
    }

    //store all data for player objects
    public void GetPlayers()
    {
        if (localPlayerController == null && GameObject.Find("LocalPlayer"))
        {
           localPlayerController = GameObject.Find("LocalPlayer").GetComponent<PlayerController>();
        }
    
        if (opponentPlayerController == null && GameObject.Find("NetworkOpponent"))
        {
            opponentPlayerController = GameObject.Find("NetworkOpponent").GetComponent<PlayerController>();
        }
    }

    //handles the functions for returning a card to a player's hand
    public void ReturnToHand()
    {
       
        Debug.Log(cardTitle + " returned to " + photonView.owner + " hand");
        //play card return sound
        if (PlaySound != null)
        {
            PlaySound("playCardRelease");
        }
        //RETURN TO HAND

        //Put the card into proper place
        if (photonView.isMine)
        {
            localPlayerController.ReturnToHand(this.gameObject, photonView.viewID, zoneIndex);
        }
        else
        {
            opponentPlayerController.ReturnToHand(this.gameObject, photonView.viewID, zoneIndex);
        }
           
    }


   
    //used to hold a card's effect upon being sent to graveyard (if it has any)
    public virtual void OnGraveyard()
    {
        //Graveyard effect goes here
    }

    [PunRPC]
    public void DrawTargetLine(int viewId)
    {
        targetLine.enabled = true;
        if(photonView.isMine)
        {
            targetLine.startColor = Color.blue;
            targetLine.endColor = Color.green;
        }
        else
        {
            targetLine.startColor = Color.red;
            targetLine.endColor = Color.magenta;
        }
           
        //set the first component of the line renderer to the position of the card
        targetLine.SetPosition(0, transform.position);

        //set the second component of the line renderer to the position of the target
        targetLine.SetPosition(1, PhotonView.Find(viewId).transform.position);       
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
            stream.SendNext(currentCardState);


        }
        else
        {
            //Network player, receive data   
            //cardId = (string)stream.ReceiveNext();
            //cardTitle = (string)stream.ReceiveNext();
            //cardTitleTextBox.text = cardTitle;
            currentCardState = (cardState)stream.ReceiveNext();
        }

    }
}