using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CreatureDebuffDiesIfAttkIsZeroCard : CreatureDebuffCard {

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
	}
	
	// Update is called once per frame
	public override void Update ()				//Abstract method for Update
	{
		//If the card is Not in the graveyard and is in the summon zone
		if (!inGraveyard && inSummonZone) 
		{

			//Increment the current Time
			currentTime -= Time.deltaTime;
			if(summonZoneTextBox == null)
			{ summonZoneTextBox = localPlayer.GetComponent<PlayerController>().getSummonZone(gameObject); }
			//TEST
			//summonZoneTextBox = p1Manager.GetComponent<Player1Manager> ().getSummonZone (gameObject);
			else
			{ summonZoneTextBox.text = currentTime.ToString("F1"); }

			//cardTimerBox.text = currentTime.ToString ("F1");
			//IF the current time is larger than or equal to the cast time
			isDraggable = false;
			if (currentTime <= 0) 
			{
				//reset the timer
				currentTime = 0;
				//Set state of card to being in the graveyard
				inGraveyard = true;
				//Set state of card to not being in the summon zone
				inSummonZone = false;
				//Apply the debuff to the targeted creature
				theCreature.GetComponent<CreatureCard> ().decreaseStats (changeDamageAmt, changeHealthAmt, changeAttackSpeedAmt);
				//if the attack damage of the creature is zero
				if (theCreature.GetComponent<CreatureCard> ().damageToDeal <= 0) 
				{
					
					//The creature dies, send it to the graveyard
					theCreature.GetComponent<CreatureCard>().setGraveyardVariables();
					Debug.Log ("Creature attack damage: " + theCreature.GetComponent<CreatureCard> ().damageToDeal);
					networkOpponent.GetComponent<PlayerController> ().sendToGraveyard (theCreature);
				}
			}

		}
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
		}
	}
}
