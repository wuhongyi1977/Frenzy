using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	//The text box that the enemie's health is placed
	private Text textBox;
	//The health of the players
	int player1Health = 20;
	int player2Health = 20;
	// Use this for initialization
	void Start () {
		textBox = GameObject.Find ("Text").GetComponent<Text> ();
		textBox.text = "Enemy Life: " + 20;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//Method for when a damage card is done casting
	public void dealDamage(int damage, int playerID)
	{
		//If the card belongs to player 1
		if (playerID == 1) 
		{
			//Subtract the health of player 2
			player2Health -= damage;
			Debug.Log ("Player 2 Health: " + player2Health);
			//Update the text box
			textBox.text = "Enemy Life: " + player2Health;
		} 
		//If the card belongs to player 2
		else 
		{
			player1Health -= damage;
			Debug.Log ("Player 1 Health: " + player1Health);
		}
	}
}