using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {
    //Variables for the numerous audio clips
    public AudioClip levelMusic;
	public AudioClip cardBuildUp;
	public AudioClip cardInSpellSlot;
	public AudioClip cardPickup;
	public AudioClip cardRelease;
	public AudioClip cardSelect;
	private AudioSource soundEffectSource;
    private AudioSource levelMusicSource;

	// Use this for initialization
	void Start ()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        soundEffectSource = sources[0];
        levelMusicSource = sources[1];

        StartLevelMusic();

    }

    private void OnEnable()
    {
        BaseCard.PlaySound += PlaySoundByName;
    }

    private void OnDisable()
    {
        BaseCard.PlaySound -= PlaySoundByName;
    }

    //calls the function with the same name passed
    public void PlaySoundByName(string name)
    {
        Invoke(name, 0f);
    }

    public void StartLevelMusic()
    {
        levelMusicSource.clip = levelMusic;
        levelMusicSource.loop = true;
        levelMusicSource.Play();
    }
	//functions to call to play sounds
	public void playCardBuildUp()
	{
		soundEffectSource.clip = cardBuildUp;
		soundEffectSource.Play ();
	}
	public void playCardInSpellSlot()
	{
		soundEffectSource.clip = cardInSpellSlot;
		soundEffectSource.Play ();
	}
	public void playCardPickup()
	{
		soundEffectSource.clip = cardPickup;
		soundEffectSource.Play ();
	}
	public void playCardRelease()
	{
		soundEffectSource.clip = cardRelease;
		soundEffectSource.Play ();
	}
	public void playCardSelect()
	{
		soundEffectSource.clip = cardSelect;
		soundEffectSource.Play ();
	}
}
