using UnityEngine;
using System.Collections;

public class DamageCard : Card
{
	private float currentTime;
	private Vector3 graveyardPosition;
	private bool inGraveyard;
	private bool inSummonZone;
	private GameObject gameManager;
	public float castTime;
	public int damageToDeal;

	public override void Start()
	{
		//Debug.Log ("DAMAGE CARD START");
		currentTime = 0;
		//gameManager = GameObject.Find ("GameManager");
		//graveyardPosition = GameObject.Find ("Player1Graveyard").transform.position;
	}

	public override void Update()
	{
		//Debug.Log ("DAMAGE CARD UPDATE");
		if (!inGraveyard && inSummonZone) {
			currentTime += Time.deltaTime;
			Debug.Log (currentTime);
			if (currentTime > castTime) {
				//gameManager.GetComponent<GameManager> ().attackEnemy (damageToDeal);
				gameObject.transform.position = graveyardPosition;
				currentTime = 0;
				inGraveyard = true;
			}
		}
	}
}