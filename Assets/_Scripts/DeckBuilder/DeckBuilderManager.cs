﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class DeckBuilderManager : MonoBehaviour {

    int maxDecks = 4;
    int currentDecks = 0;

    //UI Panels
    public GameObject deckSelectPanel;
    public GameObject deckBuildPanel;

    //components for deck selector
    public GameObject scrollView;
    DeckBuilderScrollView scrollViewScript;
    public GameObject deckButton;

    //Components for deck builder
    public InputField deckNameField;
    private string currentDeckId;

    // Use this for initialization
    void Start ()
    {
        deckBuildPanel.SetActive(false);
        deckSelectPanel.SetActive(true);
        scrollViewScript = scrollView.GetComponent<DeckBuilderScrollView>();
        //retrieve all decks the user has made
        //NO LONGER NECESSARY, DONE AT START
        //RetrieveDecks(PlayFabDataStore.playFabId);

        //Get all owned cards from player
        PlayfabApiCalls.RetrieveCardCollection();

    }
	
	// Update is called once per frame
	void Update ()
    {
        string[] cards = { "Cardset_Cardname_Holo" };
	    if(Input.GetKeyDown(KeyCode.P))
        {
            ConstructDeck("963057C83DDE4583", cards);
        }
	}

    
    public void NewDeck()
    {
        //check how many decks the user has

        //if the user has less than the max decks, make a new one
        if(PlayFabDataStore.numberOfDecks < maxDecks)
        {
            string deckName = "Deck " + (PlayFabDataStore.numberOfDecks + 1);
            //send the default name of the deck for the name
            Debug.Log(deckName);
            CreateDeck(deckName); 
        }
        else
        {
            Debug.Log("Maximum Deck Count Reached! Cannot Create New Deck");
        }
    }
  
   
    //Creates a new deck for the player
    //Deck is a "character" on playfab granted to user
    public void CreateDeck(string name)
    {
        var request = new RunCloudScriptRequest()
        {
            ActionId = "newDeck",
            Params = new { characterName = name, characterType = "Deck" }
        };
        PlayFabClientAPI.RunCloudScript(request, (result) =>
        {
            Debug.Log("Deck Created");
            Debug.Log(result);
            //string[] splitResult = result.ResultsEncoded.Split('"'); //19th element is the itemInstanceId
            //Debug.Log("Split Result " + splitResult[59]); // 63th element is the itemId of the item granted from the drop table
            //Debug.Log("Split Result " + splitResult[63]); // 63th element is the itemInstanceId of the item granted from the drop table
            //Debug.Log("Split Result " + splitResult[67]); // 67st element is the item class  

            //delete all deck buttons and reload list
            scrollViewScript.ReloadList();

        },
        (error) =>
        {
            Debug.Log("Deck Not Created!");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }


    public static void ConstructDeck(string deckIdNum, string[] itemsToAdd)
    {
        var request = new RunCloudScriptRequest()
        {
            ActionId = "fillDeck",
            Params = new { deckId = deckIdNum, items = itemsToAdd }
        };
        PlayFabClientAPI.RunCloudScript(request, (result) =>
        {
            Debug.Log("Cards Added To Deck");
            //string[] splitResult = result.ResultsEncoded.Split('"'); //19th element is the itemInstanceId
            //Debug.Log("Split Result " + splitResult[59]); // 63th element is the itemId of the item granted from the drop table
            // Debug.Log("Split Result " + splitResult[63]); // 63th element is the itemInstanceId of the item granted from the drop table
            // Debug.Log("Split Result " + splitResult[67]); // 67st element is the item class  

        },
        (error) =>
        {
            Debug.Log("Cards Not Added To Deck");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }
    public static void UpdateDeckName(string deckIdNum, string newName)
    {
        var request = new RunCloudScriptRequest()
        {
            ActionId = "changeDeckName",
            Params = new { deckId = deckIdNum, name = newName }
        };
        PlayFabClientAPI.RunCloudScript(request, (result) =>
        {
            Debug.Log("Name Changed");
            Debug.Log(result);
            //string[] splitResult = result.ResultsEncoded.Split('"'); //19th element is the itemInstanceId
            //Debug.Log("Split Result " + splitResult[59]); // 63th element is the itemId of the item granted from the drop table
            // Debug.Log("Split Result " + splitResult[63]); // 63th element is the itemInstanceId of the item granted from the drop table
            // Debug.Log("Split Result " + splitResult[67]); // 67st element is the item class  

        },
        (error) =>
        {
            Debug.Log("Name Not Changed");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }
    public void DeckButtonClicked(string id, string name)
    {
       
        deckSelectPanel.SetActive(false);
        deckBuildPanel.SetActive(true);
        //retrieve deck info and populate build panel
        currentDeckId = id;
        deckNameField.text = name;

    }
    //saves cards in deck and deck name
    public void SaveDeck()
    {
        //NONE OF THIS WORKS YET//////////////
        //store new name from input text field
        string newName = deckNameField.text;
        //update deck name
        UpdateDeckName(currentDeckId, newName);
        ////////////////////////////////////////////


    }
    public void BackToDeckSelect()
    {
        deckBuildPanel.SetActive(false);
        deckSelectPanel.SetActive(true);
    }
    public void BackToMenu()
    {
        PhotonNetwork.LoadLevel("Login");
    }
}