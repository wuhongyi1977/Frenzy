using UnityEngine;
using System.Collections;

public class DeckBuilderManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void BackToMenu()
    {
        PhotonNetwork.LoadLevel("Login");
    }
}
