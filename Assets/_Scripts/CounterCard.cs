using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CounterCard : Card /*Monobehavior*/ {
	private bool readyToCounter;
	//public Text[] textBoxes;
	 //Use this for initialization
	public override void Start () {
		base.Start ();
		readyToCounter = false;
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

		//If the card is Not in the graveyard and is in the summon zone
		if (!inGraveyard && inSummonZone && !readyToCounter) 
		{

			//Increment the current Time
			currentTime -= Time.deltaTime;
			//make sure summon zone text is assigned
			if (summonZoneTextBox == null)
			{ GetSummonZoneText(); }
			else
			{ summonZoneTextBox.text = currentTime.ToString("F1"); }

			//IF the current time is larger than or equal to the cast time
			isDraggable = false;
			if (currentTime <= 0) 
			{
				//clear summon zone text
				summonZoneTextBox.text = "";
				//reset the timer
				currentTime = 0;
				//photonView.RPC ("incOpponentNumbCounterCards", PhotonTargets.All);
				if(photonView.isMine)
					localPlayerController.increaseNumbCounterCards ();
				readyToCounter = true;
				//Set state of card to being in the graveyard
				//inGraveyard = true;
				//Set state of card to not being in the summon zone
				//inSummonZone = false;
			}

		}
		//If the card is in the graveyard and manager code hasn't been executed yet
		if (inGraveyard && doneAddingToGraveyard == false) 
		{
			photonView.RPC("SendToGraveyard", PhotonTargets.All);
			//SendToGraveyard();
		}
	}
}