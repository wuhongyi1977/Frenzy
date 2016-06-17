using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class DamageHealCard : Card {
	public int healAmount = 0;
	public int damageToDeal = 0;
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
	public override void Update ()				//Abstract method for Update
	{
		if(networkOpponent == null)
			networkOpponent = GameObject.Find ("NetworkOpponent");
		if(localPlayer == null)
			localPlayer = GameObject.Find ("LocalPlayer");
		//If the card is Not in the graveyard and is in the summon zone
		if (!inGraveyard && inSummonZone) 
		{

			//Increment the current Time
			currentTime -= Time.deltaTime;
			if(summonZoneTextBox == null)
            {
                summonZoneTextBox = localPlayer.GetComponent<PlayerController>().getSummonZone(gameObject);
                //TEST
                //summonZoneTextBox = p1Manager.GetComponent<Player1Manager>().getSummonZone(gameObject);
            }	
			else
            {
                summonZoneTextBox.text = currentTime.ToString("F1");
            }
				
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
			}

		}
		//If the card is in the graveyard and manager code hasn't been executed yet
		if (inGraveyard && doneAddingToGraveyard == false) 
		{
			summonZoneTextBox.text = "";
			//Set this to false to prevent multiple executions of this block
			doneAddingToGraveyard = true;
			localPlayer.GetComponent<PlayerController> ().ChangeHealth (healAmount);
			networkOpponent.GetComponent<PlayerController> ().ChangeHealth (damageToDeal * -1);
            localPlayer.GetComponent<PlayerController>().sendToGraveyard(gameObject);
            //TEST
            //p1Manager.GetComponent<Player1Manager> ().sendToGraveyard (gameObject);
            /*
			//If the card beings to player 1
			if (playerID == 1) 
			{
				summonZoneTextBox.text = "";
				//Set this to false to prevent multiple executions of this block
				doneAddingToGraveyard = true;				
				//Execute the game manager code
				p1Manager.GetComponent<Player1Manager> ().sendToGraveyard (gameObject);
			} 
			else 
			{
				//Logic for player2
				summonZoneTextBox.text = "";
				//Set this to false to prevent multiple executions of this block
				doneAddingToGraveyard = true;
				//Execute the game manager code
				p2Manager.GetComponent<Player2Manager> ().sendToGraveyard (gameObject);
			}*/
        }
	}
}
