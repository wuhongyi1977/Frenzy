using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// A script to handle the functions that should be available to every
/// card in the game. This is an abstract object. It will be used as a
/// base for all refined cards
/// </summary>
public abstract class Card : MonoBehaviour {
	//Used to prevent staying step out of a chunk of code in derived Card Update methods
	public bool doneAddingToGraveyard = false;
	//If the card is in the graveyard or not
	public bool inGraveyard;
	//If the card is in the summon zone
	public bool inSummonZone;
	//The ID of the player that this card belongs to
	public int playerID;
	//The card number. Used to correctly delete from the player's hand
	public int cardNumber;
	//Checks to see if the player dropped the card.
	private bool dropped;
	//The time it takes for the card to be casted
	public float castTime;
	private float currentTime;
	public Text summonZoneTextBox;
	//Something for the dragging of cards
	private Vector3 screenPoint;
	//Something fo the dragging of cards
	private Vector3 offset;

	// Use this for initialization
	public void Start ()				//Abstract method for start
	{
		doneAddingToGraveyard = false;
		currentTime = castTime;
	}
	public void Update ()				//Abstract method for Update
	{
		//If the card is Not in the graveyard and is in the summon zone
		if (!inGraveyard && inSummonZone) 
		{
			//Increment the current Time
			currentTime -= Time.deltaTime;
			summonZoneTextBox.text = currentTime.ToString ("F1");
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
				summonZoneTextBox.text = "";
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

	//Registers that the player has clicked on the card
	public void OnMouseDown()			
	{
		screenPoint = Camera.main.WorldToScreenPoint (gameObject.transform.position);
		offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
	}

	//Registers that the player has let go of the card
	public  void OnMouseUp()			
	{
		dropped = true;
		if (playerID == 1)
			GameObject.Find ("Player1Manager").GetComponent<Player1Manager> ().cardIsDropped (gameObject);
		//finds the text box that corresponds to the summon zone
		if (summonZoneTextBox == null) {
			if (playerID == 1)
				summonZoneTextBox = GameObject.Find ("Player1Manager").GetComponent<Player1Manager> ().getSummonZone (gameObject);
			//else
			//summonZoneTextBox = gameObject.Find ("Player2Manager").GetComponent<Player2Manager> ().getSummonZone (gameObject);
		}
		//if(playerID == 2)
			//GameObject.Find ("Player2Manager").GetComponent<Player2Manager> ().cardIsDropped (gameObject);
		//gameObject.transform.position = new Vector3 (0, -3.79f, 0);
	}

	//Registers that the card is being dragged
	public  void OnMouseDrag()			
	{
		Vector3 curScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
		Vector3 curPosition = Camera.main.ScreenToWorldPoint (curScreenPoint) + offset;
		transform.position = curPosition;
	}

	//method to return the dropped variable
	public bool isDropped()
	{
		return dropped;
	}
	//method to set the dropped variable
	public void setDroppedState(bool b)
	{
		dropped = b;
	}
	public float currentCastingTime()
	{
		return currentTime;
	}
}
