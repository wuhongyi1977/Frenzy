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
        if (networkOpponent == null)
        {
            networkOpponent = GameObject.Find("NetworkOpponent");
        }

        if (localPlayer == null)
        {
            localPlayer = GameObject.Find("LocalPlayer");
        }

        //If the card is Not in the graveyard and is in the summon zone
        if (!inGraveyard && inSummonZone)
        {
            //IF the current time is larger than or equal to the cast time
            isDraggable = false;
            //if the card is in the summon zone and isnt waiting for a target yet, make it true
            if (waitingForTarget == false)
            {
                waitingForTarget = true;
            }

            //if the target is always the player or 
            //if the target has been selected, begin countdown
            if (target == "Player" || targetSelected == true)
            {

                //Increment the current Time
                currentTime -= Time.deltaTime;
                if (summonZoneTextBox == null)
                {
                    //if this is the local object
                    if (photonView.isMine)
                    {
                        summonZoneTextBox = localPlayer.GetComponent<PlayerController>().getSummonZone(gameObject);
                    }
                    else //if this is the network copy
                    {
                        summonZoneTextBox = networkOpponent.GetComponent<PlayerController>().getSummonZone(gameObject);
                    }
                }
                else
                {
                    summonZoneTextBox.text = currentTime.ToString("F1");
                }
 
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

        }
        //If the card is in the graveyard and manager code hasn't been executed yet
        if (inGraveyard && doneAddingToGraveyard == false)
        {
            summonZoneTextBox.text = "";
            //Set this to false to prevent multiple executions of this block
            doneAddingToGraveyard = true;
            //if this is the local object
            if (photonView.isMine)
            {
                //handle health changes to self (local player here is card owner)
                localPlayer.GetComponent<PlayerController>().ChangeHealth(ownerHealthChange);
                //send to graveyard
                localPlayer.GetComponent<PlayerController>().sendToGraveyard(gameObject);
            }
            else //if this is the network copy
            {
                //handle health changes to opponent (local player here is opponent)
                localPlayer.GetComponent<PlayerController>().ChangeHealth(opponentHealthChange);
                //send to opponents graveyard
                networkOpponent.GetComponent<PlayerController>().sendToGraveyard(gameObject);
            }

           
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
        /*
        if (waitingForTarget == true && targetSelected == false)//(creatureCanAttack)
        {
            if (photonView.isMine)
            {
                localPlayer.GetComponent<PlayerController>().drawLineOn();
            }
        }
        */
    }
    /*
    public override void OnMouseUp()
    {
        if (isSelectable == true)
        {
            localPlayer.GetComponent<PlayerController>().makeLineInvisible();
            localPlayer.GetComponent<PlayerController>().drawLineOff();

            dropped = true;

            
            //if this is the local card object
            if (photonView.isMine)
            {

                if (target == "Creature" || target == "All")//(creatureCanAttack)
                {
                    networkOpponent.GetComponent<PlayerController>().ChangeHealth(opponentHealthChange);
                    localPlayer.GetComponent<PlayerController>().creatureCardIsDropped(gameObject, cardHandPos);

                }
                else
                {
                    localPlayer.GetComponent<PlayerController>().cardIsDropped(gameObject, cardHandPos);

                }
            }
            else
            {
                if (target == "Creature" || target == "All")//(creatureCanAttack)
                {

                    networkOpponent.GetComponent<PlayerController>().creatureCardIsDropped(gameObject, cardHandPos);

                }
                else
                {
                    networkOpponent.GetComponent<PlayerController>().cardIsDropped(gameObject, cardHandPos);
                }        
            }
            
            //finds the text box that corresponds to the summon zone
            if (summonZoneTextBox == null)
            {
                //if this is the local card object
                if (photonView.isMine)
                {
                    summonZoneTextBox = localPlayer.GetComponent<PlayerController>().getSummonZone(gameObject);

                }
                else
                {
                    summonZoneTextBox = networkOpponent.GetComponent<PlayerController>().getSummonZone(gameObject);

                }

            }
            
        }
        

    }
    */
    /*
    //ADDED TO ATTEMPT TARGETED DAMAGE
    public override void OnMouseDown()
    {
        if (isDraggable == true )//&& isSelectable == true)
        {
            cardHandPos = gameObject.transform.position;
            screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        }
    }
    public override void OnMouseUp()
    {
        if (isSelectable == true)
        {
            localPlayer.GetComponent<PlayerController>().makeLineInvisible();
            localPlayer.GetComponent<PlayerController>().drawLineOff();
           
            dropped = true;
            //if this is the local card object
            if (photonView.isMine)
            {

                if (target == "Creature" || target == "All")//(creatureCanAttack)
                {
                    networkOpponent.GetComponent<PlayerController>().ChangeHealth(opponentHealthChange);
                    localPlayer.GetComponent<PlayerController>().creatureCardIsDropped(gameObject, cardHandPos);
                   
                }
                else
                {
                    localPlayer.GetComponent<PlayerController>().cardIsDropped(gameObject, cardHandPos);
                   
                }
            }
            else
            {
                if (target == "Creature" || target == "All")//(creatureCanAttack)
                {

                    networkOpponent.GetComponent<PlayerController>().creatureCardIsDropped(gameObject, cardHandPos);
                   
                }
                else
                {
                    networkOpponent.GetComponent<PlayerController>().cardIsDropped(gameObject, cardHandPos);
                }

              

            }
            //finds the text box that corresponds to the summon zone
            if (summonZoneTextBox == null)
            {
                //if this is the local card object
                if (photonView.isMine)
                {
                    summonZoneTextBox = localPlayer.GetComponent<PlayerController>().getSummonZone(gameObject);
                   
                }
                else
                {
                    summonZoneTextBox = networkOpponent.GetComponent<PlayerController>().getSummonZone(gameObject);
                   
                }

            }
        }

    }
    */



}