﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class BasicCreatureCard : Card
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
    //public Text[] textBoxes;
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
    public override void Start()
    {
        localPlayer = GameObject.Find("LocalPlayer");
        networkOpponent = GameObject.Find("NetworkOpponent");
        p1Manager = GameObject.Find("Player1Manager");
        p2Manager = GameObject.Find("Player2Manager");
        doneAddingToGraveyard = false;
        currentTime = castTime;
        inSummonZone = false;
        summonZoneTextBox = null;
        isDraggable = true;
        textBoxes = gameObject.GetComponentsInChildren<Text>();
        for (int i = 0; i < textBoxes.Length; i++)
        {
            if (textBoxes[i].name == "CardTitle")
                cardTitleTextBox = textBoxes[i];
            if (textBoxes[i].name == "Stats")
                creatureStatsTextBox = textBoxes[i];
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
        creatureStatsTextBox.text = damageToDeal + "/" + health + "/" + attackSpeed;
        if (networkOpponent == null)
            networkOpponent = GameObject.Find("NetworkOpponent");
        if (localPlayer == null)
            localPlayer = GameObject.Find("LocalPlayer");
        //If the creature is frozen
        if (isFrozen)
        {
            currentTime -= Time.deltaTime;
            summonZoneTextBox.text = currentTime.ToString("F1");
            if (currentTime <= 0)
            {
                summonZoneTextBox.text = "";
                isFrozen = false;
                isSelectable = true;
                creatureCanAttack = true;
            }
        }
        //If the card is Not in the graveyard and is in the summon zone
        else if (!inGraveyard && inSummonZone)
        {
			if (!playedCardInSpellSlotSound) 
			{
				playedCardInSpellSlotSound = true;
				audioManager.playCardInSpellSlot ();
			}
            if (!stopCastingTimer)
            {
                //Increment the current Time
                isDraggable = false;
                currentTime -= Time.deltaTime;
                if (summonZoneTextBox == null)
                {
                    //Makes sure summon zone textbox is assigned
                    GetSummonZoneText();
                }
                else
                {
                    summonZoneTextBox.text = currentTime.ToString("F1");
                }

            }
            //IF the current time is larger than or equal to the cast time
            if (!isAttackable)
            {
                currentTime -= Time.deltaTime;
                summonZoneTextBox.text = currentTime.ToString("F1");
                if (currentTime <= 0)
                {
                    summonZoneTextBox.text = "";
                }
            }
			if (currentTime <= 3.25f && !playedCardBuildupSound) 
			{
				playedCardBuildupSound = true;
				audioManager.playCardBuildUp ();
			}
            if (currentTime <= 0)
            {
                if (!stopCastingTimer)
                {
                    stopCastingTimer = true;
                    summonZoneTextBox.text = "";
                    if (rush)
                        creatureCanAttack = true;
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
                    if (creatureAttackSpeedTimer < 0)
                    {
                        summonZoneTextBox.text = "";
                        creatureCanAttack = true;
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
                        localPlayer.GetComponent<PlayerController>().sendToGraveyard(gameObject);
                        localPlayer.GetComponent<PlayerController>().creatureDied();

                    }
                    else
                    {
                        networkOpponent.GetComponent<PlayerController>().sendToGraveyard(gameObject);
                        networkOpponent.GetComponent<PlayerController>().creatureDied();

                    }
                }
            }
        }
    }
    public override void OnMouseDown()
    {
        if (isDraggable == true && isSelectable == true)
        {
            cardHandPos = gameObject.transform.position;
            screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
			if (!playedCardPickupSound) 
			{
				audioManager.playCardPickup ();
				playedCardPickupSound = true;
			}
        }
    }
    public override void OnMouseUp()
    {
		//reset the bool to allow the Pickup sound to play again when the player picks up another card
		playedCardPickupSound = false;

        if (isSelectable == true)
        {
            localPlayer.GetComponent<PlayerController>().makeLineInvisible();
            localPlayer.GetComponent<PlayerController>().drawLineOff();

            dropped = true;
            //if this is the local card object
            if (photonView.isMine)
            {

                if (creatureCanAttack)
                {
                    networkOpponent.GetComponent<PlayerController>().ChangeHealth(damageToDeal * -1);
                    Debug.Log("HERE");
                    localPlayer.GetComponent<PlayerController>().creatureCardIsDropped(gameObject, cardHandPos);
                }
                else
                {
                    localPlayer.GetComponent<PlayerController>().cardIsDropped(gameObject, cardHandPos);
                }
            }
            else
            {
                if (creatureCanAttack)
                {

                    networkOpponent.GetComponent<PlayerController>().creatureCardIsDropped(gameObject, cardHandPos);
                }
                else
                {
                    networkOpponent.GetComponent<PlayerController>().cardIsDropped(gameObject, cardHandPos);
                }
            }
            //Makes sure summon zone textbox is assigned
            GetSummonZoneText();
        }

    }
    public override void OnMouseOver()
    {
		if (!playedCardSelectedSound) 
		{
			audioManager.playCardSelect ();
			playedCardSelectedSound = true;
		}
        if (creatureCanAttack)
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
        if (creatureCanAttack)
        {
            if (photonView.isMine)
            {
                localPlayer.GetComponent<PlayerController>().drawLineOn();
            }
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
        currentTime = numbSecs;
    }
    public void makeUnattackable(int numbSecs)
    {
        isAttackable = false;
        currentTime = numbSecs;
    }

    public override void UpdateInternalVariables()
    {
        cardTitleTextBox.text = cardTitle;
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
    }
}
