using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class RelicCreatureBuffWhileActiveCard : RelicCard {
	//If the buff this relic provides has been applied
	public bool buffApplied;
	//Increase the damage stat of all creatures by an amount
	public int changeDamageAmount;
	//Increase the health stat of all creatures by an amount
	public int changeHealthAmount;
	//Increase the attack speed stat of all creatures by an amount
	public int changeAttackSpeedAmount;

	//The creature card that the buff will be applied to
	private GameObject theCreature;
	// Use this for initialization
	public override void Start ()				//Abstract method for start
	{
		localPlayer = GameObject.Find ("LocalPlayer");
		networkOpponent = GameObject.Find ("NetworkOpponent");
		p1Manager = GameObject.Find ("Player1Manager");
		p2Manager = GameObject.Find ("Player2Manager");
		doneAddingToGraveyard = false;
		currentTime = castTime;
		inSummonZone = false;
		summonZoneTextBox = null;
		isDraggable = true;
		//gameObject.GetComponentInChildren<Text>();
		cardTitleTextBox = gameObject.GetComponentInChildren<Text>();
		cardTitleTextBox.text = cardTitle;
		relicActive = false;
		buffApplied = false;
	}

	// Update is called once per frame
	public override void Update ()				//Abstract method for Update
	{
		//If the card is Not in the graveyard and is in the summon zone
		if (!inGraveyard && inSummonZone) 
		{
			if (!relicActive) {
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
				//TEST
				//summonZoneTextBox = p1Manager.GetComponent<Player1Manager> ().getSummonZone (gameObject);
				else {
					summonZoneTextBox.text = currentTime.ToString ("F1");
				}

				//cardTimerBox.text = currentTime.ToString ("F1");
				//IF the current time is larger than or equal to the cast time
				isDraggable = false;
				if (currentTime <= 0) {
					//reset the timer
					currentTime = 0;
					relicActive = true;
					summonZoneTextBox.text = "";
					/*
				//Set state of card to being in the graveyard
					inGraveyard = true;
					//Set state of card to not being in the summon zone
					inSummonZone = false;*/
				}
			}
		}
		//If the relic is active but the buff hasn't been applied yet
		if (relicActive && buffApplied == false) {
			Debug.Log ("RELIC IS ACTIVE");
			buffApplied = true;
			//When the relic is active apply the buff to the creatures already active
			localPlayer.GetComponent<PlayerController> ().increaseCreatureStats (changeDamageAmount, changeAttackSpeedAmount, changeHealthAmount);
		} else if (inGraveyard && doneAddingToGraveyard == false) {
			relicActive = false;
		}

		/*
		//If the card is in the graveyard and manager code hasn't been executed yet
		if (inGraveyard && doneAddingToGraveyard == false) 
		{
			//If the card beings to player 1
			if (playerID == 1) 
			{
				summonZoneTextBox.text = "";
				//Set this to false to prevent multiple executions of this block
				doneAddingToGraveyard = true;
				//Execute the game manager code
				localPlayer.GetComponent<PlayerController>().sendToGraveyard(gameObject);
				//TEST
				//p1Manager.GetComponent<Player1Manager> ().sendToGraveyard (gameObject);
			} 
			else 
			{
				//Logic for player2
				summonZoneTextBox.text = "";
				//Set this to false to prevent multiple executions of this block
				doneAddingToGraveyard = true;
				//Execute the game manager code
				networkOpponent.GetComponent<PlayerController>().sendToGraveyard(gameObject);
				//TEST
				//p2Manager.GetComponent<Player2Manager> ().sendToGraveyard (gameObject);
			}
		}*/
	}
	//EVENT CODE BEGIN
	/// <summary>
	/// Event used for when a creature enters while the relic is already on the field
	/// The buff is applied to the creature that is entering the battlefield
	/// </summary>
	void OnEnable()
	{
		PlayerController.creatureHasEntered += applyBuffToCreature;
	}
	void OnDisable()
	{
		PlayerController.creatureHasEntered -= applyBuffToCreature;
	}
	void applyBuffToCreature()
	{
		Debug.Log ("HERE");
		//If the relic is active
		if (relicActive) {
			Debug.Log ("HERE2");
			//get the creature that just entered
			theCreature = localPlayer.GetComponent<PlayerController> ().getCreatureThatJustEntered ();
			//If this card is Relic of Fury
			if (gameObject.name.CompareTo("Relic of Fury")>0) {
				Debug.Log ("HERE3");
				if (theCreature.GetComponent<CreatureCard> ().playerID == 1) {
					Debug.Log ("HERE4");
					//check to see if its attack speed is bigger than 3
					if (theCreature.GetComponent<CreatureCard> ().attackSpeed > 3) {
						Debug.Log ("HERE5");
						//If so, then apply the buff
						theCreature.GetComponent<CreatureCard> ().attackSpeed -= changeAttackSpeedAmount;
					}
				}
			}
		}
	}
	//EVENT CODE END
}
