using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class CreatureCard : DamageCard {
	public int health;
	public int attackSpeed;

	//IF the card is a creature card
	public bool isCreature = true;
	//The bool variable that turns off the summoning timer after the creature has been summoned
	private bool stopCastingTimer = false;
	public bool creatureCanAttack = false;
	private float creatureAttackSpeedTimer;
	public Text[] textBoxes;
	public override void Start()
	{
		p1Manager = GameObject.Find ("Player1Manager");
		p2Manager = GameObject.Find ("Player2Manager");
		doneAddingToGraveyard = false;
		currentTime = castTime;
		inSummonZone = false;
		summonZoneTextBox = null;
		isDraggable = true;
		textBoxes = gameObject.GetComponentsInChildren<Text>();
		for (int i = 0; i < textBoxes.Length; i++) {
			if (textBoxes [i].name == "CardTitle")
				textBoxes [i].text = cardTitle;
			if (textBoxes [i].name == "Stats")
				textBoxes [i].text = damageToDeal + "/" + health + "/" + attackSpeed;
		}
		creatureAttackSpeedTimer = attackSpeed;
	}
	public override void Update()
	{
		//If the card is Not in the graveyard and is in the summon zone
		if (!inGraveyard && inSummonZone) {
			
			if (!stopCastingTimer) {
				//Increment the current Time
				isDraggable = false;
				currentTime -= Time.deltaTime;
				summonZoneTextBox.text = currentTime.ToString ("F1");
			}
			//IF the current time is larger than or equal to the cast time
			if (currentTime <= 0) 
			{
				if (!stopCastingTimer) {
					stopCastingTimer = true;
					summonZoneTextBox.text = "";
					creatureCanAttack = true;
				}

				if (creatureCanAttack == false) {
					creatureAttackSpeedTimer -= Time.deltaTime;
					summonZoneTextBox.text = creatureAttackSpeedTimer.ToString ("F1");
					if (creatureAttackSpeedTimer < 0) {
						summonZoneTextBox.text = "";
						creatureCanAttack = true;
						creatureAttackSpeedTimer = attackSpeed;
					}
				}

				//Add code here to deal damage to creature or player
				if (health == 0 && !inGraveyard) {
					summonZoneTextBox.text = "";
					inGraveyard = true;
					inSummonZone = false;
					if (playerID == 1) {
						p1Manager.GetComponent<Player1Manager> ().sendToGraveyard (gameObject);
					} 
					else 
					{
						p2Manager.GetComponent<Player2Manager> ().sendToGraveyard (gameObject);
					}
				}
			}
		}
	}
	public override void OnMouseDown()			
	{
		if (isDraggable == true) 
		{
			cardHandPos = gameObject.transform.position;
			screenPoint = Camera.main.WorldToScreenPoint (gameObject.transform.position);
			offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
		}
	}
	public override void OnMouseUp()			
	{
		p1Manager.GetComponent<Player1Manager> ().makeLineInvisible ();
		p1Manager.GetComponent<Player1Manager> ().drawLineOff ();
		dropped = true;
		if (playerID == 1) 
		{
			
			if (creatureCanAttack) 
			{
				Debug.Log ("HERE");
				p1Manager.GetComponent<Player1Manager> ().creatureCardIsDropped (gameObject, cardHandPos);
			}
			else
				p1Manager.GetComponent<Player1Manager> ().cardIsDropped (gameObject, cardHandPos);

		} 
		else 
		{
			p2Manager.GetComponent<Player2Manager> ().cardIsDropped (gameObject, cardHandPos);
			//p2Manager.GetComponent<Player2Manager> ().creatureCardIsDropped (gameObject, cardHandPos);
		}
		//finds the text box that corresponds to the summon zone
		if (summonZoneTextBox == null) 
		{
			if (playerID == 1)
				summonZoneTextBox = p1Manager.GetComponent<Player1Manager> ().getSummonZone (gameObject);
			else
				summonZoneTextBox = p2Manager.GetComponent<Player2Manager> ().getSummonZone (gameObject);
		}

	}
	public override void OnMouseOver()
	{
		if (creatureCanAttack) {
			if (Input.GetMouseButtonDown (0))
				p1Manager.GetComponent<Player1Manager> ().drawLineOn ();
		}
		Debug.Log (gameObject.name);
		if (playerID == 1) 
		{
			p1Manager.GetComponent<Player1Manager> ().setMousedOverCard (gameObject);
		} 
		else 
		{
			p2Manager.GetComponent<Player2Manager> ().setMousedOverCard (gameObject);
		}
		if(creatureCanAttack)
		{
			if (playerID == 1) {
				p1Manager.GetComponent<Player1Manager> ().drawLineOn ();
			}
		}
	}/*
	public virtual void OnMouseExit()
	{
			if (playerID == 1) {
				//p1Manager.GetComponent<Player1Manager> ().drawLineOff ();
			}

	}*/
}