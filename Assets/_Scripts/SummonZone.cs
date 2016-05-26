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
		textBox = GameObject.Find (gameObject.name+"TextBox").GetComponent<Text> ();
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
