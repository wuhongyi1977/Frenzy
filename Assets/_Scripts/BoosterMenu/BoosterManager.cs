using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BoosterManager : MonoBehaviour {

    public Text goldText;

	// Use this for initialization
	void Start ()
    {
        goldText.text = "Gold: " + PlayFabDataStore.userGold.ToString();
	}
	
	// Update is called once per frame
	void Update ()
    {
        //temp code to grant gold
		if(Input.GetKeyDown(KeyCode.G))
        {
            PlayfabApiCalls.AddGold(100);
        }
	}

    public void PurchasePack()
    {
        //temp hard coded packs to add
        string[] packsToAdd = new string[] { "BoosterPack_Classic" };


        PlayfabApiCalls.SubtractGold(100);
        PlayfabApiCalls.GrantPacksToUser(packsToAdd);
    }

    public void OpenPack()
    {
        //temp hard coded pack to open
        string packType = "BoosterPack_Classic";

        PlayfabApiCalls.OpenPack(packType);
    }

    public void BackToMenu()
    {

    }
}
