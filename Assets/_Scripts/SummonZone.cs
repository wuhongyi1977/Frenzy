using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SummonZone : MonoBehaviour
{
	//The id of the player that this summon zone belongs to
	public int playerID;

    private int zoneIndex;
	
	//The state of whether there is a card in the summon zone
	public bool isOccupied;

    BoxCollider2D zoneCollider;


	// Use this for initialization
	void Start ()
    {
        //player1HealthTextBox.text = "Life: " + 20;
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneIndex = int.Parse(name.Substring(name.Length-1));
	}
	

    public int GetZoneIndex()
    {
        return zoneIndex;
    }

    //Pass in a card if this zone is becoming occupied (true) or null if unoccupied (false)
    public void SetOccupied(BaseCard card , bool occupy)
    {
        if(card == null)
        {
           Debug.LogError("No card passed to: " + this.gameObject.name);
           return;
        }
        if(occupy)
        {
            //pass this script to the card as its current zone
            card.SetOccupiedZone(this); 
            isOccupied = true;
            //disable the zone's collider
            zoneCollider.enabled = false;                 
        }
        else
        {
            card.SetOccupiedZone(null);
            isOccupied = false;
            //enable the zone's collider
            zoneCollider.enabled = true;
        }
    }
}
