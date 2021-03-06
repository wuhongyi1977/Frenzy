﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeckBuilderScrollView : MonoBehaviour
{
    public GameObject DeckBuilderManager;
    private DeckBuilderManager builderManagerScript;

    public GameObject Button_Template;
    public GameObject scrollContent;
    private List<string> NameList = new List<string>();

    bool decksLoaded = false;

    //all decks organized by id and button associated with it
    public static Dictionary<string, GameObject> deckButtonList = new Dictionary<string, GameObject>();


    // Use this for initialization
    void Start()
    {
        //store script for deck builder manager
        builderManagerScript = DeckBuilderManager.GetComponent<DeckBuilderManager>();
        //load deck list into scroll view
        StartCoroutine(LoadList());
    }

    //Loads a list of all decks for this user
    //creates a button for each deck in the scrollview
    public IEnumerator LoadList()
    {
        decksLoaded = false;
        //clear list of decks to reload
        deckButtonList.Clear();
        yield return new WaitForSeconds(2f);
        while(!PlayfabApiCalls.deckRetrievalDone)
        {
            yield return new WaitForSeconds(1f);
        }
        foreach (string deckId in PlayFabDataStore.deckIds)
        {
            //instantiate a new button for this deck
            GameObject button = Instantiate(Button_Template) as GameObject;
            //store a reference to this button
            deckButtonList.Add(deckId, button);
            //set it active
            button.SetActive(true);
            //store the button's script
            DeckButtonScript DB = button.GetComponent<DeckButtonScript>();
            //call the set name function for that button
            //use the name associated with the id key in the dictionary
            DB.SetName(PlayFabDataStore.deckList[deckId]);
            //call the set id function for that button
            //stores item id for later use
            DB.SetId(deckId);
            //set parent to scroll view
            //second argument is worldPositionStays
            //setting to false retain local orientation and scale rather than world orientation and scale
            button.transform.SetParent(scrollContent.transform, false);

        }
        yield return null;
        decksLoaded = true;

    }
    public void ReloadList()
    {
        //for every deck button instantiated
        foreach (Transform child in scrollContent.transform)
        {
            //destroy the button
            Destroy(child.gameObject);
        }
        PlayfabApiCalls.RetrieveDecks(PlayFabDataStore.playFabId);
        //var children = new List<GameObject>();
        //foreach (Transform child in scrollContent.transform) children.Add(child.gameObject);
        //children.ForEach(child => Destroy(child));
        StartCoroutine(LoadList());


    }
    public void CreateButton(string deckId)
    {
        //instantiate a new button for this deck
        GameObject button = Instantiate(Button_Template) as GameObject;
        //store a reference to this button
        deckButtonList.Add(deckId, button);
        //set it active
        button.SetActive(true);
        //store the button's script
        DeckButtonScript DB = button.GetComponent<DeckButtonScript>();
        //call the set name function for that button
        //use the name associated with the id key in the dictionary
        DB.SetName(PlayFabDataStore.deckList[deckId]);
        //call the set id function for that button
        //stores item id for later use
        DB.SetId(deckId);
        //set parent to scroll view
        //second argument is worldPositionStays
        //setting to false retain local orientation and scale rather than world orientation and scale
        button.transform.SetParent(scrollContent.transform, false);
    }
    public void ButtonClicked(string id, string name)
    {
        if(builderManagerScript.GetDelete() == true)
        {
            Debug.Log("Deleting Deck");
            //delete deck
            PlayfabApiCalls.DeleteDeck(id);
            //delete deck button associated with this id
            Destroy(deckButtonList[id]);
            //remove deck from dictionary
            deckButtonList.Remove(id);

            //reload decks
            PlayfabApiCalls.RetrieveDecks(PlayFabDataStore.playFabId);
        }
        else
        {
            //send id and name of deck clicked to deck builder manager
            builderManagerScript.DeckButtonClicked(id, name);
        }
       

    }

    public bool GetDecksLoaded()
    {
        return decksLoaded;
    }
}
