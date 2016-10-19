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
    //checks if the cards target type has been assigned
    private bool targetAssignment = false;

    private GameObject currentTarget;

    public Text[] textBoxes;

    LineRenderer targetLine;

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
        targetLine = GetComponent<LineRenderer>();
        targetLine.enabled = false;




    }
    public override void Update()               //Abstract method for Update
    {
        /*
        if (cardTitleTextBox != null)
        {
            cardTitleTextBox.text = cardTitle;
        }
        */
        
        //get references to player objects if not assigned
        GetPlayers();

        //If the card is Not in the graveyard and is in the summon zone
        if (!inGraveyard && inSummonZone)
        {
			if (summonZoneTextBox == null)
			{
				GetSummonZoneText();
			}
			if (castTimeTextBox != null)
			{
				castTimeTextBox.text = castTime.ToString();
			}
            //IF the current time is larger than or equal to the cast time
            isDraggable = false;
            //allow player to choose target
            canTarget = true;
            //assign this cards target based on CustomData
            SetTarget();
            
            if (!playedCardInSpellSlotSound) 
			{
				playedCardInSpellSlotSound = true;
				audioManager.playCardInSpellSlot ();
			}
           

            //if the target has been selected, begin countdown
            if (targetSelected == true)
            {
                
                //Decrement the current Time
                currentTime -= Time.deltaTime;
               
                //set summon zone text to current time
                summonZoneTextBox.text = currentTime.ToString("F1");
                //play card builduup sound when current time is near 0
				if (currentTime <= 3.25f && !playedCardBuildupSound) 
				{
					playedCardBuildupSound = true;
					audioManager.playCardBuildUp ();
				}
                //if the cast time completes
                if (currentTime <= 0)
                {
                    //run this card's OnCast function
                    OnCast();

                    //send to graveyard
                    SendToGraveyard();
                   
                }
            }

        }

    }

    
    //handles card's function upon casting
    public override void OnCast()
    {
        Debug.Log(photonView.owner + " cast "+ cardTitle + " on " + currentTarget.name +" successfully!");

        //disable targetting line
        targetLine.enabled = false;
        //if this card belongs to the local player, run casting code
        if(photonView.isMine && currentTarget != null)
        {

            //CREATURE TARGET FUNCTIONS
            if(currentTarget.tag == "CreatureCard")
            {
                CreatureCard targetCreatureScript = currentTarget.GetComponent<CreatureCard>();

                if(cardEffects.Contains("ReturnCreature"))
                {
                    targetCreatureScript.ReturnToHand();
                }

            }

            //SELF TARGET FUNCTIONS
            if (currentTarget.tag == "Player1")
            {

            }
            //OPPONENT TARGET FUNCTIONS
            if(currentTarget.tag == "Player2")
            {

            }


            //DAMAGE
            //if this card changes either player's health, run damage code
            if (ownerHealthChange != 0 || opponentHealthChange != 0)
            {
                Debug.Log("Dealing damage to : " + currentTarget);
                localPlayer.GetComponent<PlayerController>().CardTargetDamage(gameObject, cardHandPos, currentTarget);
            }

        }    
    }

    public override void OnMouseOver()
    {
       
       
        if (photonView.isMine && localPlayerController != null)
        {
            if (!playedCardSelectedSound)
            {
                audioManager.playCardSelect();
                playedCardSelectedSound = true;
            }

            localPlayerController.setMousedOverCard(gameObject);
            //Debug.Log("waitingForTarget: " + waitingForTarget + "/" + "targetSelected: " + targetSelected);
            if (waitingForTarget == true && targetSelected == false)//(creatureCanAttack)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Mouse Drag On: " + cardTitle);
                    localPlayerController.drawLineOn();
                }

            }
        }
        else if(opponentPlayerController != null)
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
            //make drag line invisible
            localPlayerController.makeLineInvisible();
            localPlayerController.drawLineOff();
            //set card as dropped
            dropped = true;
            
            //if card is being put into play
            if (!canTarget)
            {     
                //drop card for local player (network player drops it by rpc call)
                localPlayerController.cardIsDropped(gameObject, cardHandPos);  
            }
            else if (waitingForTarget == true)
            {
                Debug.Log("Attempt targeting");
                Debug.Log("Target is: " + localPlayerController.CardIsTargetted());//(gameObject, cardHandPos));
                //if the target is not null, store the target and set targetSelected to true
                targetObject = localPlayerController.CardIsTargetted();
                //if the target is a possible target for this card
                if (VerifyTarget())
                {
                   
                    targetSelected = true;
                    currentTarget = targetObject;//localPlayerController.CardIsTargetted();
                    //draw the target line to the position of the current target
                    int targetViewId = -1;
                    if (currentTarget.GetPhotonView() != null)
                    {
                        targetViewId = currentTarget.GetPhotonView().viewID;
                    }
                   
                    string targetTag = currentTarget.tag;
                    photonView.RPC("DrawTargetLine",PhotonTargets.All, targetViewId, targetTag);
                    
                    
                }
              
            }
            //Makes sure summon zone textbox is assigned
            GetSummonZoneText();
        }   
    }

    public override void OnMouseDown()
    {
        Debug.Log("Mouse Down on: "+cardTitle);
        if (photonView.isMine && isDraggable == true && isSelectable == true)
        {
            cardHandPos = gameObject.transform.position;
            screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
			audioManager.playCardPickup ();
        }
    }

    public void SetTarget()
    {
        if (targetAssignment == false)
        {
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
            targetAssignment = true;
        }
    }

    //verify that the proper type of target is being selected
    public override bool VerifyTarget()
    {
        //copy this block into all cards, cards cannot target nothing or themselves
        if (targetObject == null || targetObject == this.gameObject)
        { return false; }

        else if(targetObject.tag == "Player2" || targetObject.tag == "Player1")
        {
            return true;
        }
        //test if target is proper
        else if(targetObject.tag == "CreatureCard" && targetObject.GetComponent<CreatureCard>().inBattlefield == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    
    }
    [PunRPC]
    public void DrawTargetLine(int viewId, string targetTag)
    {  
        GameObject objectToTarget;
        targetLine.enabled = true;
        //set line color based on if this is your card or opponents
        if(photonView.isMine)
        {
            objectToTarget = currentTarget;
            targetLine.SetColors(Color.blue, Color.green);
        }
        else
        {
            //if the casting player targeted their opponent
            if(targetTag == "Player2")
            {
                //show the line drawn to self on opponents screen
                objectToTarget = GameObject.FindGameObjectWithTag("Player1");
            }
            //if the casting player targeted themself
            else if (targetTag == "Player1")
            {
                //show the line drawn to opponents opponent 
                objectToTarget = GameObject.FindGameObjectWithTag("Player2");
            }
            else
            {
                objectToTarget = PhotonView.Find(viewId).gameObject;
            }
           
            targetLine.SetColors(Color.yellow, Color.red);
        }
        
        //set the first component of the line renderer to the position of the card
        targetLine.SetPosition(0, transform.position);

        //set the second component of the line renderer to the position of the target
        targetLine.SetPosition(1, objectToTarget.transform.position);

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