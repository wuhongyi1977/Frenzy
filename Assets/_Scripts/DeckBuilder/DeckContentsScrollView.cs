﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeckContentsScrollView : MonoBehaviour
{
    public GameObject DeckBuilderManager;
    private DeckBuilderManager builderManagerScript;

    public GameObject cardCollection;
    private CardCollectionScrollView cardCollectionScript;

    public GameObject Button_Template;
    public GameObject scrollContent;
    //this is populated with all of the cards currently in deck at start (item ids)
    //The list is changed when cards are dragged in/ out
    public List<string> DeckContentsList = new List<string>();

    public List<string> deckAtStart;



    // Use this for initialization
    void OnEnable()
    {
        //set this list to be the same as the deck was before changes
        deckAtStart = PlayFabDataStore.cardsInDeck;
        //store script for deck builder manager
        builderManagerScript = DeckBuilderManager.GetComponent<DeckBuilderManager>();
        //store script for card collection
        cardCollectionScript = cardCollection.GetComponent<CardCollectionScrollView>();
        //load card list into scroll view
        StartCoroutine(LoadList());
    }
    //when panel is set inactive
    void OnDisable()
    {
        //for every card button instantiated
        foreach (Transform child in scrollContent.transform)
        {
            //destroy the button
            Destroy(child.gameObject);
        }
    }
    //Loads a list of all decks for this user
    //creates a button for each deck in the scrollview
    public IEnumerator LoadList()
    {
        //wait until collection is loaded
        while(cardCollectionScript.GetLoaded() == false)
        {
            yield return new WaitForSeconds(1f);
        }
        //begin loading deck
        Debug.Log("Loading deck contents");
        foreach (string cardId in PlayFabDataStore.cardsInDeck)
        {
            //get item id of this card
            string itemId = PlayFabDataStore.itemIdCollection[cardId]; 
            
            //get a reference to a button of this card
            GameObject button = cardCollectionScript.GetButton(itemId);
            if(button != null)
            {
                //remove card from collection list
                cardCollectionScript.RemoveCard(itemId);
                //Add the card to this deck's list
                DeckContentsList.Add(cardId);
                //set it active
                button.SetActive(true);
                //store the button's script
                DraggableCard CB = button.GetComponent<DraggableCard>();
                //set the cards scrollview variable to be this script
                CB.SetDeckScrollView(this);
                //set the instance id variable of the button to the items instance id
                CB.SetInstanceId(cardId);
                //set the item id of the button to this item's item id
                CB.SetId(itemId);

                //call the set name function for that button
                //use the name associated with the id key in the dictionary
                CB.SetName(PlayFabDataStore.cardNameList[itemId]);

                //indicate that this card is already in deck
                CB.inDeck = true;
                //set parent to scroll view
                //second argument is worldPositionStays
                //setting to false retain local orientation and scale rather than world orientation and scale
                button.transform.SetParent(scrollContent.transform, false);
            }
            else
            {
                Debug.Log("Error! Cannot create card in deck because it does not exist in collection");
            }
        }
        Debug.Log("Deck Contents Loaded");
        yield return null;
    }
    
  
    public void AddCardToDeck(string newCardId)
    {
        //Add the card to this deck's list
        DeckContentsList.Add(newCardId);
        //DEBUG CODE
        foreach(string output in DeckContentsList)
        {
            Debug.Log(output);
        }
    }
    public void RemoveCardFromDeck(string cardId)
    {
        //Add the card to this deck's list
        DeckContentsList.Remove(cardId);
    }
    //returns the list in array form
    public string[] GetListOfCards()
    {
        //string[] deckContentArray 
           return (string[])DeckContentsList.ToArray();
        //set to strings?
       // return deckContentArray;
    }
    //returns the list in array form
    public string[] GetOldCards()
    {
      
        return (string[])deckAtStart.ToArray();
        
    }


    /*
    public void ReloadList()
    {
        PlayfabApiCalls.RetrieveDecks(PlayFabDataStore.playFabId);
        var children = new List<GameObject>();
        foreach (Transform child in scrollContent.transform) children.Add(child.gameObject);
        children.ForEach(child => Destroy(child));
        LoadList();


    }
    

    public void ButtonClicked(string id, string name)
    {
        //send id and name of deck clicked to deck builder manager
        builderManagerScript.DeckButtonClicked(id, name);

    }
    */
}


