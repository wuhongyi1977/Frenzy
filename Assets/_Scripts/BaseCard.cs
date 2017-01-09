﻿using UnityEngine;
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
    //CARD COMPONENTS
    //contains all visible components on card
    protected Transform cardLayoutCanvas;
    //the image to display for the card art
    protected Image cardArtImage;
    //the text displayed to the player listing the card's abilities
    protected Text descriptionText;
    protected Text summonZoneTextBox;
    protected Text cardTitleTextBox;
    protected Text castTimeTextBox;
    LineRenderer targetLine;

    //Scaling variables
    protected Vector3 startingScale, zoomScale;

    //Card State
    public enum cardState { OutOfPlay, InHand, Held, OnField, InGraveyard };
    public cardState currentCardState = cardState.OutOfPlay;

    //asst. variables
    public bool isHeld = false; //< checks if card is currently being dragged by player
    public bool onField = false; //< checks if card is on the field (ie in play)
    public bool touchingSummonZone = false; //< checks if card is held over summon zone
   
    public int zoneIndex; 
    //The position that the card was at when the player picks up the card. 
    //This is used for when a player makes an invalid placement the card is placed back in it's original hand position
    public Vector3 cardHandPos;


    //STATS 
    //all card stats are here, are assigned by custom data from Playfab
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
    //the list of standard abilities a creature has (Rush, Elusive, etc.)
    //to handle abilities, have each possible occurence check if hashset contains ability
    public HashSet<string> creatureAbilities = new HashSet<string>();
    //the list of effects a card has
    //have each possible occurence check if hashset contains effect
    public HashSet<string> cardEffects = new HashSet<string>();


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
   

    protected float currentTime;
   
    //Something for the dragging of cards
    protected Vector3 screenPoint;
    //Something fo the dragging of cards
    protected Vector3 offset;
   
   
    // Use this for initialization
    protected bool isDraggable;

    public GameObject localPlayer;
    public GameObject networkOpponent;
    protected PlayerController localPlayerController;
    protected PlayerController opponentPlayerController;

    protected SpriteRenderer spriteRender;

    protected GameObject targetObject = null;

    //ADDED CODE FOR HAND INDEX
    public int handIndex;

    //PHOTON COMPONENTS
    protected PhotonView photonView;

    //SOUND VARIABLES
    //The variable for the script attached to the AudioManager object
    protected AudioManager audioManager;
    //variables to prevent spamming of sounds
    protected bool playedCardSelectedSound;
    protected bool playedCardPickupSound;
    protected bool playedCardBuildupSound;
    protected bool playedCardReleaseSound;
    protected bool playedCardInSpellSlotSound;
    //END SOUND VARIABLES 



    //This can be used for initialization code that is identical on ALL cards
    protected virtual void Awake()
    {
        //get photon view component of this card
        photonView = GetComponent<PhotonView>();
        localPlayer = GameObject.Find("LocalPlayer");
        networkOpponent = GameObject.Find("NetworkOpponent");
        //if the layout canvas of this card is found, locate its components
        if(cardLayoutCanvas = transform.FindChild("CardLayoutCanvas"))
        {
            //Get all components
            descriptionText = cardLayoutCanvas.FindChild("DescriptionText").GetComponent<Text>();
            summonZoneTextBox = cardLayoutCanvas.FindChild("Counter").GetComponent<Text>(); 
            cardTitleTextBox = cardLayoutCanvas.FindChild("CardTitle").GetComponent<Text>(); 
            castTimeTextBox = cardLayoutCanvas.FindChild("CastTime").GetComponent<Text>();
            cardArtImage = cardLayoutCanvas.FindChild("CardArtImage").GetComponent<Image>();
            targetLine = GetComponent<LineRenderer>();
            targetLine.enabled = false;
        }
        startingScale = transform.localScale;
        zoomScale = startingScale * 2;
        //audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
       

    }
    public virtual void Start()             //Abstract method for start
    {
        doneAddingToGraveyard = false;
        currentTime = castTime;
        inSummonZone = false;
        summonZoneTextBox = null;
        isDraggable = true;
       
        cardTitleTextBox.text = cardTitle;


    }
    public virtual void Update()                //Abstract method for Update
    {
        //get references to player objects if not assigned
        GetPlayers();

        //If the card is Not in the graveyard and is in the summon zone
        if (currentCardState == cardState.OnField)
        {
            //Increment the current Time
            currentTime -= Time.deltaTime;
           
            summonZoneTextBox.text = currentTime.ToString("F1"); 

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
    public void AddCardToHand(int indexToSet)
    {
        handIndex = indexToSet;
        currentCardState = cardState.InHand;
    }
    //Registers that the player has clicked on the card
    protected virtual void OnMouseDown()
    {
        if (photonView.isMine)
        {
            transform.localScale = startingScale;
            if (currentCardState == cardState.InHand)
            {
                currentCardState = cardState.Held;
                cardHandPos = gameObject.transform.position;
                screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
                offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
                //audioManager.playCardPickup();
            }
        }
    }

    //Registers that the player has let go of the card
    public virtual void OnMouseUp()
    {
        //if this is the local player, drop the card
        //network player receives drop through RPC call in PlayerController (void PlayCard)
        if (photonView.isMine)
        {
            if(currentCardState == cardState.Held)
            {
                RaycastHit2D[] hits;
                hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100.0F);
                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit2D hit = hits[i];
                    if (hit.transform.tag == "Player1SummonZone")
                    {
                        currentCardState = cardState.OnField;
                        localPlayerController.cardIsDropped(gameObject, hit.transform.GetComponent<SummonZone>().GetZoneIndex());
                        break;
                    }
                }
                if (currentCardState != cardState.OnField)
                {
                    //return to hand
                    currentCardState = cardState.InHand;
                    transform.position = cardHandPos;
                    //localPlayerController.ReturnToHand(gameObject);
                }
            }
        }

    }

    //Registers that the card is being dragged
    public void OnMouseDrag()
    {
        if (photonView.isMine && currentCardState == cardState.Held)
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            transform.position = curPosition;
        }
    }
    
    //Registers what card is under the mouse
    protected virtual void OnMouseOver()
    {
        
        if (photonView.isMine && currentCardState == cardState.InHand)
        {
        
            transform.localScale = zoomScale;
        }
        
    }
    //Registers what card is under the mouse
    public virtual void OnMouseExit()
    {
        playedCardSelectedSound = false;
        transform.localScale = startingScale;
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
        //set cards description
        descriptionText.text = PlayFabDataStore.cardDescriptions[id];

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
            switch (currentString)
            {
               
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
                case "CreatureAbility":
                    string ability = nextString;
                    creatureAbilities.Add(ability); //= int.Parse(nextString);
                    break;
                default:
                    break;
            }
        }
        Debug.Log("Finished setting variables");
        //Set the proper art for the card
        SetArt();
        //set all internal variables on a card using this function
        UpdateInternalVariables();

    }

    public void SetArt()
    {
        if (artName != null && artName != "temp")
        {
            cardArtImage.overrideSprite = Resources.Load<Sprite>("CardArt/" + artName);
            Debug.Log(Resources.Load("CardArt/" + artName));
        }

    }
    //this function should be overridden in child script to update any
    //internal variables that should be set on card initialization
    public virtual void UpdateInternalVariables()
    {
        //variables go here
    }

    //this function should be overridden in child script to verify that
    //the proper type of target is being selected
    public virtual bool VerifyTarget()
    {
        //copy this block into all cards, cards cannot target nothing or themselves
        if (targetObject == null || targetObject == this.gameObject)
        { return false; }

        //test if target is proper

        //else
        return true;
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
        Debug.Log(cardTitle + "sent to graveyard");

        //Set this to false to prevent multiple executions of this block
        doneAddingToGraveyard = true;

        summonZoneTextBox.text = "";
        //reset the timer
        currentTime = 0;
        //Set state of card to being in the graveyard
        inGraveyard = true;
        //Set state of card to not being in the summon zone
        inSummonZone = false;
        if (!playedCardReleaseSound)
        {
            playedCardReleaseSound = true;
            audioManager.playCardRelease();
        }

        //If the card beings to player 1
        if (photonView.isMine)
        {
            //Execute the game manager code
            localPlayerController.sendToGraveyard(gameObject);
        }
        else
        {
            //Execute the game manager code
            opponentPlayerController.sendToGraveyard(gameObject);
        }
        //handle graveyard effect of this card, if any
        OnGraveyard();
    }
    //handles the functions for returning a card to a player's hand
    public void ReturnToHand()
    {
        if (photonView.isMine)
        {
            Debug.Log(cardTitle + " returned to " + photonView.owner + " hand");



            //RESET VARIABLES
            if (cardType == "Creature")
            {
                //get creature script
                CreatureCard creatureScript = GetComponent<CreatureCard>();
                //reset creature variables
                creatureScript.isSelectable = true;
                creatureScript.isFrozen = false;
                creatureScript.isAttackable = true;
                //remove this card from the list of creatures in play
                localPlayerController.creatureCardsInPlay.Remove(gameObject);
            }

            //reset the timer
            currentTime = 0;
            //Set state of card to being in the graveyard
            inGraveyard = false;
            //Set state of card to not being in the summon zone
            inSummonZone = false;
            //set graveyard variable to false
            doneAddingToGraveyard = false;
            //clear summon zone text box
            summonZoneTextBox = null;
            //allow dragging
            isDraggable = true;


            //play card return sound
            /*
            if (!playedCardReleaseSound)
            {
                playedCardReleaseSound = true;
                audioManager.playCardRelease();
            }
            */

            //RETURN TO HAND

            //Put the card into proper place
            photonView.RPC("DrawCard", PhotonTargets.All, photonView.viewID);
        }


    }

    //used to hold a card's effect upon being put into play (if it has any)
    public virtual void OnCast()
    {
        //cast effect goes here
    }

    //used to hold a card's effect upon being sent to graveyard (if it has any)
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