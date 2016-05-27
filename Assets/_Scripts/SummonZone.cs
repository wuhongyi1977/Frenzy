using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SummonZone : MonoBehaviour {
	//The id of the player that this summon zone belongs to
	public int playerID;
	public Text textBox;
	//The state of whether there is a card in the summon zone
	public bool isOccupied;
	// Use this for initialization
	void Start () {
		//sets this zones text box to the first text component it
        //finds in its children (which is the summonZoneTextBox)
        textBox = gameObject.GetComponentInChildren<Text>();
		//player1HealthTextBox.text = "Life: " + 20;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (isOccupied) 
		{
			
		}
	}
}
