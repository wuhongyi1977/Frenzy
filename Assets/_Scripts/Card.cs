using UnityEngine;
using System.Collections;

/// <summary>
/// A script to handle the functions that should be available to every
/// card in the game.
/// </summary>
public abstract class Card : MonoBehaviour {
	private Vector3 screenPoint;
	private Vector3 offset;
    //the amount of time a card takes after being played to become active
   //float castTime;

	// Use this for initialization
	public abstract void Start ();				//Abstract method for start
	public abstract void Update ();				//Abstract method for Update

	//Registers that the player has clicked on the card
	public void OnMouseDown()			
	{
		screenPoint = Camera.main.WorldToScreenPoint (gameObject.transform.position);
		offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
	}
	//Registers that the player has let go of the card
	public  void OnMouseUp()			
	{
		gameObject.transform.position = new Vector3 (0, -3.79f, 0);
	}
	//Registers that the card is being dragged
	public  void OnMouseDrag()			
	{
		Vector3 curScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
		Vector3 curPosition = Camera.main.ScreenToWorldPoint (curScreenPoint) + offset;
		transform.position = curPosition;
	}
}
