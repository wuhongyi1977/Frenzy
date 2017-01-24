using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class CreatureCard : BaseCard
{
    //STATS
    public int health;
    public float attackSpeed;
    //The damage that this card will deal
    public int damageToDeal;
    //END STATS
    //IF the card is a creature card
    public bool isCreature = true;
    //The bool variable that turns off the summoning timer after the creature has been summoned
    protected bool stopCastingTimer = false;
    public bool creatureCanAttack = false;
    protected float creatureAttackSpeedTimer;
    public Text[] textBoxes;
    private Text creatureStatsTextBox;
    public bool inBattlefield = false;

    public int increaseDamageAmount, increaseAttackSpeedAmount, increaseHealthAmount;
    private int startingDamage, startingHealth;
    private float startingAttackSpeed;

    public bool isFrozen;
    public bool isSelectable;
    public bool isAttackable;
    public bool rush;
    public bool elusive;
	//After the first time the sound CardBuildup was used(when currentTime < 0) it was only played once. 
	//After this the sound needed to play again so this variable is used instead of playedCardBuildup to make sure everything works correctly
	private bool playedCardBuildUpOnce;
    public override void Start()
    {
        localPlayer = GameObject.Find("LocalPlayer");
        networkOpponent = GameObject.Find("NetworkOpponent");
        
        doneAddingToGraveyard = false;
      
        inSummonZone = false;
        summonZoneTextBox = null;
       
        textBoxes = gameObject.GetComponentsInChildren<Text>();
        for (int i = 0; i < textBoxes.Length; i++)
        {
            if (textBoxes[i].name == "CardTitle")
                cardTitleTextBox = textBoxes[i];
            if (textBoxes[i].name == "Stats")
                creatureStatsTextBox = textBoxes[i];
            if (textBoxes[i].name == "CastTime")
                castTimeTextBox = textBoxes[i];
        }
		audioManager = GameObject.Find ("AudioManager").GetComponent<AudioManager>();
		playedCardSelectedSound = false;
        /*
        //get card stats from stat variables in parent script
        startingDamage = attackPower;
        startingHealth = defensePower;
        startingAttackSpeed = rechargeTime;
        //end assignment
        creatureAttackSpeedTimer = attackSpeed;
        cardTitleTextBox.text = cardTitle;
        creatureStatsTextBox.text = damageToDeal + "/" + health + "/" + attackSpeed;
        //startingDamage = damageToDeal;
        //startingAttackSpeed = attackSpeed;
        //startingHealth = health;
        */
        isSelectable = true;
        isFrozen = false;
        isAttackable = true;
    }
    public override void Update()
    {
        if (cardTitleTextBox != null)
        {
            cardTitleTextBox.text = cardTitle;
        }
        if (castTimeTextBox != null)
        {
            castTimeTextBox.text = castTime.ToString();
        }
        //get references to player objects if not assigned
        GetPlayers();


        creatureStatsTextBox.text = damageToDeal + "/" + health + "/" + attackSpeed;
       
        
        //If the card is Not in the graveyard and is in the summon zone
       if (!inGraveyard && inSummonZone)
        {
			if (!playedCardInSpellSlotSound) 
			{
				playedCardInSpellSlotSound = true;
				audioManager.playCardInSpellSlot ();
			}
            if (!stopCastingTimer)
            {
                

            }
            //IF the current time is larger than or equal to the cast time
            if (!isAttackable)
            {
               
            }
			if (!playedCardBuildUpOnce) 
			{
				playedCardBuildUpOnce = true;
				audioManager.playCardBuildUp ();
			}
            
                if (!stopCastingTimer)
                {
                    stopCastingTimer = true;
                    summonZoneTextBox.text = "";
                    if (creatureAbilities.Contains("Rush"))
                    {
                        creatureCanAttack = true;
                    }                      
                    inBattlefield = true;
                    //call the event that a creature has entered the battlefield
                    localPlayer.GetComponent<PlayerController>().creatureEntered();
					//reset the bool to allow the Pickup sound to play again when the player picks up another card
					playedCardPickupSound = false;
					audioManager.playCardRelease ();
                }

                if (creatureCanAttack == false)
                {
						
                    creatureAttackSpeedTimer -= Time.deltaTime;
                    summonZoneTextBox.text = creatureAttackSpeedTimer.ToString("F1");
					if (creatureAttackSpeedTimer <= 3.25f && !playedCardBuildupSound) 
					{
						audioManager.playCardBuildUp ();
						playedCardBuildupSound = true;
					}
                    if (creatureAttackSpeedTimer < 0)
                    {
                        summonZoneTextBox.text = "";
                        creatureCanAttack = true;
						playedCardBuildupSound = false;
                        creatureAttackSpeedTimer = attackSpeed;
                    }
                }

                //Add code here to deal damage to creature or player
                if (health <= 0 && !inGraveyard)
                {
                    summonZoneTextBox.text = "";
                    inGraveyard = true;
                    inSummonZone = false;
                    inBattlefield = false;
                    creatureCanAttack = false;
                    //reset creature's stats to default
                    damageToDeal = startingDamage;
                    attackSpeed = startingAttackSpeed;
                    health = startingHealth;
                    //if this is the local card object
                    if (photonView.isMine)
                    {
                        localPlayer.GetComponent<PlayerController>().sendToGraveyard(gameObject, -1);
                        localPlayer.GetComponent<PlayerController>().creatureDied();

                    }
                    else
                    {
                        networkOpponent.GetComponent<PlayerController>().sendToGraveyard(gameObject, -1);
                        networkOpponent.GetComponent<PlayerController>().creatureDied();

                    }
                }
            
        }
    }
    protected override void OnMouseDown()
    {
        if (photonView.isMine  && isSelectable == true)
        {
            cardHandPos = gameObject.transform.position;
            //screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            //offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
			if (!playedCardPickupSound) 
			{
				audioManager.playCardPickup ();
				playedCardPickupSound = true;
			}
        }
    }
    protected override void OnMouseUp()
    {
		//reset the bool to allow the Pickup sound to play again when the player picks up another card
		playedCardPickupSound = false;

        if (photonView.isMine && isSelectable == true)
        {
            localPlayerController.makeLineInvisible();
            localPlayerController.drawLineOff();

            
            //if this is the local card object
            if (photonView.isMine)
            {

                if (creatureCanAttack)
                {

                    //GameObject currentTarget = localPlayerController.CardIsTargetted();//gameObject, cardHandPos);
                    targetObject = localPlayerController.CardIsTargetted();
                    //check if target is a possible target for this card
                    if (VerifyTarget())
                    {
                        localPlayerController.CardTargetDamage(gameObject, cardHandPos, targetObject);//currentTarget);
                        audioManager.playCardRelease();   
                    }
                    
                    //networkOpponent.GetComponent<PlayerController>().ChangeHealth(damageToDeal * -1);
                    //Debug.Log("HERE");
                    //localPlayer.GetComponent<PlayerController>().creatureCardIsDropped(gameObject, cardHandPos);
                }
                else
                {
                    //localPlayerController.cardIsDropped(gameObject, cardHandPos);
                }
            }
            else
            {
                if (creatureCanAttack)
                {

                    //opponentPlayerController.creatureCardIsDropped(gameObject, cardHandPos);
                }
                else
                {
                    //opponentPlayerController.cardIsDropped(gameObject, cardHandPos);
                }
            }
            
        }

    }
    protected override void OnMouseOver()
    {
        if(photonView.isMine && localPlayerController != null)
        {

            //localPlayerController.setMousedOverCard(gameObject);
			if (!playedCardSelectedSound) 
			{
				audioManager.playCardSelect ();
				playedCardSelectedSound = true;
			}
            if (creatureCanAttack)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    localPlayerController.drawLineOn();
                }

            }
        }
        else if(opponentPlayerController != null)
        {
            //opponentPlayerController.setMousedOverCard(gameObject);

        }

       
    }

    //verify that the proper type of target is being selected
    public override bool VerifyTarget()
    {
        Debug.Log("Verifying target");
        //copy this block into all cards, cards cannot target nothing or themselves
        if (targetObject == null || targetObject == this.gameObject)
        {
            if (targetObject == null)
            {
                Debug.Log("Target is null");
            }
            else
            {
                Debug.Log("Target is self");
            }
           
            return false;
        }

        else if (targetObject.tag == "Player2")
        {
            Debug.Log("Target is opponent");
            return true;
        }
        //test if target is proper
        else if (targetObject.tag == "CreatureCard" && targetObject.GetComponent<CreatureCard>().inBattlefield == true)
        {
            Debug.Log("Target is "+targetObject.GetComponent<CreatureCard>().cardTitle);
            return true;
        }
        else
        {
            Debug.Log("No target found");
            return false;
        }

    }
    public void increaseStats(int dmg, int h, int attkSpd)
    {
        //if (photonView.isMine) {
        damageToDeal += dmg;
        attackSpeed -= attkSpd;
        health += h;
        //}
    }
    public void decreaseStats(int dmg, int h, int attkSpd)
    {
        //if (photonView.isMine) {
        damageToDeal -= dmg;
        attackSpeed += attkSpd;
        health -= h;
        //}
    }
    public void freezeCreature(int numbSecs)
    {
        isFrozen = true;
        isSelectable = false;
        creatureCanAttack = false;
        
    }
    public void makeUnattackable(int numbSecs)
    {
        isAttackable = false;
        
    }

    public override void UpdateInternalVariables()
    {
        //get card stats from stat variables in parent script
        startingDamage = attackPower;
        startingHealth = defensePower;
        startingAttackSpeed = rechargeTime;
        damageToDeal = startingDamage;
        attackSpeed = startingAttackSpeed;
        health = startingHealth;

        //end assignment
        creatureAttackSpeedTimer = attackSpeed;
        //cardTitleTextBox.text = cardTitle;
        if (creatureStatsTextBox != null)
        {
            creatureStatsTextBox.text = damageToDeal + "/" + health + "/" + attackSpeed;
        }
        if(cardTitleTextBox != null)
        {
            cardTitleTextBox.text = cardTitle;
        }
        if (castTimeTextBox != null)
        {
            castTimeTextBox.text = castTime.ToString();
        }


    }
}