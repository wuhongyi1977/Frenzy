using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class OneShotHealth : Card
{
    //positive values for healing, negative values for damage
    public int ownerHealth = 0;
    public int opponentHealth = 0;

    public bool isSelectable = true;

    //checks to see if the card is ready for a target to be selected
    private bool waitingForTarget = false;
    //checks to see if the card has been given a target yet
    private bool targetSelected = false;

    private GameObject currentTarget;


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
        cardTitleTextBox = gameObject.GetComponentInChildren<Text>();
        cardTitleTextBox.text = cardTitle;


    }
    public override void Update()               //Abstract method for Update
    {
        //get references to player objects if not assigned
        GetPlayers();

        //If the card is Not in the graveyard and is in the summon zone
        if (!inGraveyard && inSummonZone)
        {
            //IF the current time is larger than or equal to the cast time
            isDraggable = false;

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
 
                if (currentTime <= 0)
                {
                    summonZoneTextBox.text = "";
                    //reset the timer
                    currentTime = 0;
                    //Set state of card to being in the graveyard
                    inGraveyard = true;
                    //Set state of card to not being in the summon zone
                    inSummonZone = false;
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
            /*
            if (target == "Player" || currentTarget.tag == "Player2")
            {
                //handle health changes to self (local player here is card owner)
                localPlayer.GetComponent<PlayerController>().ChangeHealth(ownerHealthChange);
          
            }
            else if(target == "Creature" || currentTarget.tag == "CreatureCard")
            {
                localPlayer.GetComponent<PlayerController>().CardTargetDamage(gameObject, cardHandPos, currentTarget);
            }
            */
            //send to graveyard
            //localPlayer.GetComponent<PlayerController>().sendToGraveyard(gameObject);

        }

        else //if this is the network copy
        {
            if (target == "Player" || currentTarget.tag == "Player2")
            {
                //handle health changes to opponent (local player here is opponent)
                //localPlayer.GetComponent<PlayerController>().ChangeHealth(opponentHealthChange);
              
            }
            else if (target == "Creature" || currentTarget.tag == "CreatureCard")
            {

            }

            //send to opponents graveyard
            //networkOpponent.GetComponent<PlayerController>().sendToGraveyard(gameObject);
        }
    }
    public override void OnMouseOver()
    {
        
        if (waitingForTarget == true && targetSelected == false)//(creatureCanAttack)
        {
            if (Input.GetMouseButtonDown(0))
            {
                localPlayer.GetComponent<PlayerController>().drawLineOn();

            }

        }
        

        if (photonView.isMine)
        {
            localPlayer.GetComponent<PlayerController>().setMousedOverCard(gameObject);
        }
        else
        {
            networkOpponent.GetComponent<PlayerController>().setMousedOverCard(gameObject);
        }
       
    }
    
    public override void OnMouseUp()
    {
        if (photonView.isMine && isSelectable == true)
        {
            //if card is being put into play
            if (dropped == false)
            {
                //make drag line invisible
                localPlayer.GetComponent<PlayerController>().makeLineInvisible();
                localPlayer.GetComponent<PlayerController>().drawLineOff();
                //set card as dropped
                dropped = true;
                //drop card for local player (network player drops it by rpc call)
                localPlayer.GetComponent<PlayerController>().cardIsDropped(gameObject, cardHandPos);
                //if the target can only be the player, begin countdown
                if(target == "Player")
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
               
            }
            else if (waitingForTarget == true)
            {
                Debug.Log("Target is: " + localPlayer.GetComponent<PlayerController>().CardIsTargetted(gameObject, cardHandPos));
                //if the target is not null, store the target and set targetSelected to true
                if (localPlayer.GetComponent<PlayerController>().CardIsTargetted(gameObject, cardHandPos) != null)
                {
                    targetSelected = true;
                    currentTarget = localPlayer.GetComponent<PlayerController>().CardIsTargetted(gameObject, cardHandPos);
                }
            }
            //Makes sure summon zone textbox is assigned
            GetSummonZoneText();
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