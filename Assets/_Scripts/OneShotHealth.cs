using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class OneShotHealth : Card
{
    //positive values for healing, negative values for damage
    public int ownerHealth = 0;
    public int opponentHealth = 0;

    public bool isSelectable = true;

    private bool canTarget = false;
    //checks to see if the card is ready for a target to be selected
    private bool waitingForTarget = false;
    //checks to see if the card has been given a target yet
    private bool targetSelected = false;

    private GameObject currentTarget;

    public Text[] textBoxes;



    public override void Start()                //Abstract method for start
    {
        //set card data variables
        //cardTitle = PlayFabDataStore.cardCustomData[]
        //find components
        localPlayer = GameObject.Find("LocalPlayer");
        networkOpponent = GameObject.Find("NetworkOpponent");
        p1Manager = GameObject.Find("Player1Manager");
        p2Manager = GameObject.Find("Player2Manager");
        doneAddingToGraveyard = false;
        currentTime = castTime;
        inSummonZone = false;
        summonZoneTextBox = null;
        isDraggable = true;
        //gameObject.GetComponentInChildren<Text>();
        textBoxes = gameObject.GetComponentsInChildren<Text>();
        for (int i = 0; i < textBoxes.Length; i++)
        {
            if (textBoxes[i].name == "CardTitle")
                cardTitleTextBox = textBoxes[i];
            if (textBoxes[i].name == "CastTime")
                castTimeTextBox = textBoxes[i];
        }
        //cardTitleTextBox = gameObject.GetComponentInChildren<Text>();
        cardTitleTextBox.text = cardTitle;
		audioManager = GameObject.Find ("AudioManager").GetComponent<AudioManager>();

    }
    public override void Update()               //Abstract method for Update
    {
        /*
        if (cardTitleTextBox != null)
        {
            cardTitleTextBox.text = cardTitle;
        }
        */
        if (castTimeTextBox != null)
        {
            castTimeTextBox.text = castTime.ToString();
        }
        //get references to player objects if not assigned
        GetPlayers();

        //If the card is Not in the graveyard and is in the summon zone
        if (!inGraveyard && inSummonZone)
        {
            //IF the current time is larger than or equal to the cast time
            isDraggable = false;
			if (!playedCardInSpellSlotSound) 
			{
				playedCardInSpellSlotSound = true;
				audioManager.playCardInSpellSlot ();
			}
           

            //if the target has been selected, begin countdown
            if (targetSelected == true)
            {
               
                //Increment the current Time
                currentTime -= Time.deltaTime;
                if (summonZoneTextBox == null)
                {
                    GetSummonZoneText();
                }
                else
                {
                    summonZoneTextBox.text = currentTime.ToString("F1");
                }
				if (currentTime <= 3.25f && !playedCardBuildupSound) 
				{
					playedCardBuildupSound = true;
					audioManager.playCardBuildUp ();
				}
                if (currentTime <= 0)
                {
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
						audioManager.playCardRelease ();
					}
                }
            }

        }
        //If the card is in the graveyard and manager code hasn't been executed yet
        if (inGraveyard && doneAddingToGraveyard == false)
        {
            
            //run code for sending card to graveyard
            //photonView.RPC("SendToGraveyard", PhotonTargets.All);

            
            SendToGraveyard();  
        }
    }

    //handles effects of card when sent to graveyard
    public override void OnGraveyard()
    {
        Debug.Log("Executing OnGraveyard for: "+ gameObject);
        //Graveyard effect goes here
        //if this is the local object
        if (photonView.isMine)
        {
            Debug.Log("Dealing damage to : " + currentTarget);
            localPlayer.GetComponent<PlayerController>().CardTargetDamage(gameObject, cardHandPos, currentTarget);
        }

    }
    public override void OnMouseOver()
    {
		if (!playedCardSelectedSound) 
		{
			audioManager.playCardSelect ();
			playedCardSelectedSound = true;
		}
        if (waitingForTarget == true && targetSelected == false)//(creatureCanAttack)
        {
            if (Input.GetMouseButtonDown(0))
            {
                localPlayer.GetComponent<PlayerController>().drawLineOn();

            }

        }
        

        if (photonView.isMine)
        {
            localPlayerController.setMousedOverCard(gameObject);
        }
        else
        {
            opponentPlayerController.setMousedOverCard(gameObject);
        }
       
    }
    
    public override void OnMouseUp()
    {
		//reset the bool to allow the Pickup sound to play again when the player picks up another card
		playedCardPickupSound = false;

        if (photonView.isMine && isSelectable == true)
        {

            dropped = true;
            
                //if card is being put into play
                if (!canTarget)
                {
                    
                    //make drag line invisible
                    localPlayerController.makeLineInvisible();
                    localPlayerController.drawLineOff();
                    //set card as dropped
                    
                    //drop card for local player (network player drops it by rpc call)
                    localPlayerController.cardIsDropped(gameObject, cardHandPos);
                    //if the target can only be the player, begin countdown
                    if (target == "Player")
                    {
                        currentTarget = GameObject.FindGameObjectWithTag("Player2");
                        targetSelected = true;
                        waitingForTarget = false;
                    }
                    //if the player has options for target, wait to countdown until selected
                    else
                    {
                        targetSelected = false;
                        waitingForTarget = true;
                    }
                canTarget = true;
                
               
            }
            else if (waitingForTarget == true)
            {
                Debug.Log("Attempt targeting");
                Debug.Log("Target is: " + localPlayerController.CardIsTargetted(gameObject, cardHandPos));
                //if the target is not null, store the target and set targetSelected to true
                if (localPlayerController.CardIsTargetted(gameObject, cardHandPos) != null)
                {
                    targetSelected = true;
                    currentTarget = localPlayerController.CardIsTargetted(gameObject, cardHandPos);
                }
            }
            //Makes sure summon zone textbox is assigned
            GetSummonZoneText();
        }   
    }

    public override void OnMouseDown()
    {
        if (photonView.isMine && isDraggable == true && isSelectable == true)
        {
            cardHandPos = gameObject.transform.position;
            screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
			audioManager.playCardPickup ();
        }
    }
    //Photon Serialize View
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(target);
            stream.SendNext(targetSelected);


        }
        else
        {
            //Network player, receive data   
            target = (string)stream.ReceiveNext();
            targetSelected = (bool)stream.ReceiveNext();
            //cardTitleTextBox.text = cardTitle;
        }

    }
}