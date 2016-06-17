using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// This is the base for any damage card. Any future
/// Damage cards such as a creature or spell card
/// would inherit this and add their own functionalities 
/// </summary>
public class DamageCard : Card
{
	//The damage that this card will deal
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
	//The current time from 0 that it is since the card has been summoned
	//private float currentTime;
	//private Text cardTimerBox;
	/*
	public override void Start()
	{
		doneAddingToGraveyard = false;
		currentTime = castTime;
		//cardTimerBox = GameObject.Find ("cardTimerBox").GetComponent<Text> ();
	}

	public override void Update()
	{
		//If the card is Not in the graveyard and is in the summon zone
		if (!inGraveyard && inSummonZone) 
		{
			//Increment the current Time
			currentTime -= Time.deltaTime;
			//cardTimerBox.text = currentTime.ToString ("F1");
			//IF the current time is larger than or equal to the cast time
			if (currentTime <= 0) {
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
			//If the card beings to player 1
			if (playerID == 1) 
			{
				//Set this to false to prevent multiple executions of this block
				doneAddingToGraveyard = true;
				//Execute the game manager code
				GameObject.Find ("Player1Manager").GetComponent<Player1Manager> ().cardDoneCasting (gameObject);
			} 
			else 
			{
				//Logic for player2
			}
		}
	}

	public float currentCastingTime()
	{
		return currentTime;
	}*/
}