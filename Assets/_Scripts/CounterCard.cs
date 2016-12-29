using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class CounterCard : Card /*Monobehavior*/ {
	private bool readyToCounter;
	 //Use this for initialization
	public override void Start () 
	{
		base.Start ();
		readyToCounter = false;
		audioManager = GameObject.Find ("AudioManager").GetComponent<AudioManager>();
		textBoxes = gameObject.GetComponentsInChildren<Text>();
		for (int i = 0; i < textBoxes.Length; i++)
		{
			if (textBoxes[i].name == "DescriptionText")
				descriptionText = textBoxes[i];
			if (textBoxes[i].name == "CardTitle")
				cardTitleTextBox = textBoxes[i];
			//if (textBoxes[i].name == "Stats")
				//creatureStatsTextBox = textBoxes[i];
			if (textBoxes[i].name == "CastTime")
				castTimeTextBox = textBoxes[i];
			
		}
	}
	
	// Update is called once per frame
	public override void Update () 
	{
		//get references to player objects if not assigned
		GetPlayers();
		//If the card is Not in the graveyard, in the summon zone, and not ready to counter 
		if (!inGraveyard && inSummonZone && !readyToCounter) 
		{

			//Increment the current Time
			currentTime -= Time.deltaTime;
			//make sure summon zone text is assigned
			if (summonZoneTextBox == null)
			{ 
				GetSummonZoneText(); 
			}
			else
			{ 
				summonZoneTextBox.text = currentTime.ToString("F1"); 
			}

			//IF the current time is larger than or equal to the cast time
			isDraggable = false;
			if (currentTime <= 0) 
			{
				//if the opponent has a counter card and it is my view
				if (opponentPlayerController.getNumberOfCounterCards () > 0 && photonView.isMine) 
				{						
					//resolve countering card
					Debug.Log (cardTitle + " COUNTERED");
					//rpc call to send this card to the graveyard
					photonView.RPC ("SendToGraveyard", PhotonTargets.All);
					//register to the opponent that a counter has been used
					opponentPlayerController.decreaseNumbCounterCards ();

				} 
				else 
				{
					//clear summon zone text
					summonZoneTextBox.text = "";
					//reset the timer
					currentTime = 0;
					//if it is my view then increment my counter and add it to the list
					if(photonView.isMine)
						localPlayerController.increaseNumbCounterCards (gameObject);
					readyToCounter = true;
					//Set state of card to being in the graveyard
					//inGraveyard = true;
					//Set state of card to not being in the summon zone
					//inSummonZone = false;
				}
			}
		}
	}
}