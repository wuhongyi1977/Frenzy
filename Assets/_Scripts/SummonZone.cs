using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SummonZone : MonoBehaviour {
	//The id of the player that this summon zone belongs to
	public int playerID;
	public Text textBox;
	//The state of whether there is a card in the summon zone
	public bool isOccupied;

    BoxCollider2D zoneCollider;

	// Use this for initialization
	void Start () {
		//sets this zones text box to the first text component it
        //finds in its children (which is the summonZoneTextBox)
        textBox = gameObject.GetComponentInChildren<Text>();
        //player1HealthTextBox.text = "Life: " + 20;
        zoneCollider = GetComponent<BoxCollider2D>();
	}
	
	// Update is called once per frame
	void Update () 
	{
        //if a card is in the summon zone
		if (isOccupied) 
		{
            //disable the zone's collider
            zoneCollider.enabled = false;
		}
        else
        {
            //enable the zone's collider
            zoneCollider.enabled = true;
        }
	}
}
