using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {
	//Variables for the numerous audio clips
	public AudioClip cardBuildUp;
	public AudioClip cardInSpellSlot;
	public AudioClip cardPickup;
	public AudioClip cardRelease;
	public AudioClip cardSelect;
	private AudioSource audioSource;
	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource> ();
	}
	/*
	// Update is called once per frame
	void Update () {

	}*/
	//functions to call to play sounds
	public void playCardBuildUp()
	{
		audioSource.clip = cardBuildUp;
		audioSource.Play ();
	}
	public void playCardInSpellSlot()
	{
		audioSource.clip = cardInSpellSlot;
		audioSource.Play ();
	}
	public void playCardPickup()
	{
		audioSource.clip = cardPickup;
		audioSource.Play ();
	}
	public void playCardRelease()
	{
		audioSource.clip = cardRelease;
		audioSource.Play ();
	}
	public void playCardSelect()
	{
		audioSource.clip = cardSelect;
		audioSource.Play ();
	}
}
