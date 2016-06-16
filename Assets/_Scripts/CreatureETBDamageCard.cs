using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class CreatureETBDamageCard : CreatureCard {
	public int damageToDealOnEnter;

	public override void Start()
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
		textBoxes = gameObject.GetComponentsInChildren<Text>();
		for (int i = 0; i < textBoxes.Length; i++) {
			if (textBoxes [i].name == "CardTitle")
				textBoxes [i].text = cardTitle;
			if (textBoxes [i].name == "Stats")
				textBoxes [i].text = damageToDeal + "/" + health + "/" + attackSpeed;
		}
		creatureAttackSpeedTimer = attackSpeed;
	}

	public override void Update()
	{
		if(networkOpponent == null)
			networkOpponent = GameObject.Find ("NetworkOpponent");
		//If the card is Not in the graveyard and is in the summon zone
		if (!inGraveyard && inSummonZone) {

			if (!stopCastingTimer) {
				//Increment the current Time
				isDraggable = false;
				currentTime -= Time.deltaTime;
				if(summonZoneTextBox == null)
					summonZoneTextBox = p1Manager.GetComponent<Player1Manager> ().getSummonZone (gameObject);
				else
					summonZoneTextBox.text = currentTime.ToString ("F1");
				

			}
			//IF the current time is larger than or equal to the cast time
			if (currentTime <= 0) 
			{
				if (!stopCastingTimer) {
					stopCastingTimer = true;
					summonZoneTextBox.text = "";
					creatureCanAttack = true;

					//Add any entering the battle field effects here

					Debug.Log (gameObject.name + " enters battlefield, deals " + damageToDealOnEnter + " damage.");
					networkOpponent.GetComponent<PlayerController> ().ChangeHealth (damageToDealOnEnter * -1);
				}

				if (creatureCanAttack == false) {
					creatureAttackSpeedTimer -= Time.deltaTime;
					summonZoneTextBox.text = creatureAttackSpeedTimer.ToString ("F1");
					if (creatureAttackSpeedTimer < 0) {
						summonZoneTextBox.text = "";
						creatureCanAttack = true;
						creatureAttackSpeedTimer = attackSpeed;
					}
				}

				//Add code here to deal damage to creature or player
				if (health == 0 && !inGraveyard) {
					summonZoneTextBox.text = "";
					inGraveyard = true;
					inSummonZone = false;
					if (playerID == 1) {
						p1Manager.GetComponent<Player1Manager> ().sendToGraveyard (gameObject);
					} 
					else 
					{
						p2Manager.GetComponent<Player2Manager> ().sendToGraveyard (gameObject);
					}
				}
			}
		}
	}
}
