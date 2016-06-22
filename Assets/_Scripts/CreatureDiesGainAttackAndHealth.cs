using UnityEngine;
using System.Collections;

/// <summary>
/// This script is the receiver of the event for when a creature dies
/// The creature this script is attached too will gain +1 damage and +1 health 
/// </summary>

public class CreatureDiesGainAttackAndHealth : MonoBehaviour {
	//The bonus amount of attack and health that the creature will get
	public int attackBonus, healthBonus;
	//The creature script that is attached to the same creature as this script
	private CreatureCard creatureCardScript;

	// Use this for initialization
	void Start () {
		creatureCardScript = gameObject.GetComponent<CreatureCard> ();
	}
	/*
	// Update is called once per frame
	void Update () {
	}*/

	void OnEnable()
	{
		PlayerController.creatureHasDied += increaseAttkHealth;
	}
	void OnDisable()
	{
		PlayerController.creatureHasDied += increaseAttkHealth;
	}
	void increaseAttkHealth()
	{
		if (creatureCardScript.inSummonZone) {
			creatureCardScript.damageToDeal += attackBonus;
			creatureCardScript.health += healthBonus;
		}
	}
}