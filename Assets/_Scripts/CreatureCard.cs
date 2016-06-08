using UnityEngine;
using System.Collections;

public class CreatureCard : DamageCard {
	public int health;
	public int attackSpeed;

	//IF the card is a creature card
	public bool isCreature = true;
	//The bool variable that turns off the summoning timer after the creature has been summoned
	private bool stopCastingTimer = false;
	private bool creatureCanAttack = false;
	public override void Update()
	{
		//If the card is Not in the graveyard and is in the summon zone
		if (!inGraveyard && inSummonZone) 
		{
			if (!stopCastingTimer) {
				//Increment the current Time
				currentTime -= Time.deltaTime;
				summonZoneTextBox.text = currentTime.ToString ("F1");
			}
			//cardTimerBox.text = currentTime.ToString ("F1");
			//IF the current time is larger than or equal to the cast time
			if (currentTime <= 0) 
			{
				if (!stopCastingTimer) 
				{
					stopCastingTimer = true;
					summonZoneTextBox.text = "";
					creatureCanAttack = true;
				}
				//Add code here to deal damage to creature or player
				if (creatureCanAttack) 
				{
					
				}




				/*
				if (isCreature) 
				{
				} 
				else 
				{
					//reset the timer
					currentTime = 0;
					//Set state of card to being in the graveyard
					inGraveyard = true;
					//Set state of card to not being in the summon zone
					inSummonZone = false;
				}*/
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
				p1Manager.GetComponent<Player1Manager> ().cardDoneCasting (gameObject);
			} 
			else 
			{
				//Logic for player2
				summonZoneTextBox.text = "";
				//Set this to false to prevent multiple executions of this block
				doneAddingToGraveyard = true;
				//Execute the game manager code
				p2Manager.GetComponent<Player2Manager> ().cardDoneCasting (gameObject);
			}
		}
	}
	public override void OnMouseUp()			
	{
		
		dropped = true;
		if (playerID == 1) {
			if (creatureCanAttack) 
			{
				p1Manager.GetComponent<Player1Manager> ().creatureCardIsDropped (gameObject, cardHandPos);
			}
			else
				p1Manager.GetComponent<Player1Manager> ().cardIsDropped (gameObject, cardHandPos);

		} 
		else 
		{
			p2Manager.GetComponent<Player2Manager> ().cardIsDropped (gameObject, cardHandPos);
			//p2Manager.GetComponent<Player2Manager> ().creatureCardIsDropped (gameObject, cardHandPos);
		}
		//finds the text box that corresponds to the summon zone
		if (summonZoneTextBox == null) 
		{
			if (playerID == 1)
				summonZoneTextBox = p1Manager.GetComponent<Player1Manager> ().getSummonZone (gameObject);
			else
				summonZoneTextBox = p2Manager.GetComponent<Player2Manager> ().getSummonZone (gameObject);
		}

		//if(playerID == 2)
		//GameObject.Find ("Player2Manager").GetComponent<Player2Manager> ().cardIsDropped (gameObject);
		//gameObject.transform.position = new Vector3 (0, -3.79f, 0);
	}
}