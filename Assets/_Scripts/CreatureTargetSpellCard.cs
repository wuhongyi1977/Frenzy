﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CreatureTargetSpellCard : Card {
	
	protected GameObject theCreature;


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
			}

		}
		//If the card is in the graveyard and manager code hasn't been executed yet
		if (inGraveyard && doneAddingToGraveyard == false) 
		{
			//If the card beings to player 1
			if (photonView.isMine) 
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
	//Registers that the player has clicked on the card
	public override void OnMouseDown()			
	{
		if (isDraggable == true) 
		{
			cardHandPos = gameObject.transform.position;
			screenPoint = Camera.main.WorldToScreenPoint (gameObject.transform.position);
			offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

		}
	}

	//Registers that the player has let go of the card
	public override void OnMouseUp()			
	{
		dropped = true;
		if (photonView.isMine)
		{
			localPlayer.GetComponent<PlayerController>().creatureTargetCardIsDropped(gameObject, cardHandPos);
			//TEST
			//p1Manager.GetComponent<Player1Manager> ().cardIsDropped (gameObject, cardHandPos);
		}
		else
		{
			networkOpponent.GetComponent<PlayerController>().creatureTargetCardIsDropped(gameObject, cardHandPos);
			//TEST
			//p2Manager.GetComponent<Player2Manager>().cardIsDropped(gameObject, cardHandPos);
		}

		//finds the text box that corresponds to the summon zone
		if (summonZoneTextBox == null) 
		{
			if (photonView.isMine)
			{
				summonZoneTextBox = localPlayer.GetComponent<PlayerController>().getSummonZone(gameObject);
				//TEST
				//summonZoneTextBox = p1Manager.GetComponent<Player1Manager>().getSummonZone(gameObject);
			}
			else
			{
				summonZoneTextBox =networkOpponent.GetComponent<PlayerController>().getSummonZone(gameObject);
				//TEST
				//summonZoneTextBox = p2Manager.GetComponent<Player2Manager>().getSummonZone(gameObject);
			}

		}
		//if(playerID == 2)
		//GameObject.Find ("Player2Manager").GetComponent<Player2Manager> ().cardIsDropped (gameObject);
		//gameObject.transform.position = new Vector3 (0, -3.79f, 0);
	}

	//Registers what card is under the mouse
	public override void OnMouseOver()
	{
		Debug.Log (gameObject.name);
		if (photonView.isMine) 
		{
			localPlayer.GetComponent<PlayerController>().setMousedOverCard(gameObject);
			//TEST
			//p1Manager.GetComponent<Player1Manager> ().setMousedOverCard (gameObject);
		} 
		else 
		{
			networkOpponent.GetComponent<PlayerController>().setMousedOverCard(gameObject);
			//TEST
			//p2Manager.GetComponent<Player2Manager> ().setMousedOverCard (gameObject);
		}
	}
	//Registers what card is under the mouse
	public override void OnMouseExit()
	{
		Debug.Log ("test");
	}
	public void setValidCreatureToDebuff(GameObject card)
	{
		Debug.Log ("SETTING CREATURE TO DEBUFF");
		theCreature = card;
	}
	public bool hasTextBox()
	{
		if (summonZoneTextBox == null)
			return false;
		else
			return true;
	}
}
