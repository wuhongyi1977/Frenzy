﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CardCollectionScrollView : MonoBehaviour
{
    public GameObject DeckBuilderManager;
    private DeckBuilderManager builderManagerScript;

    public GameObject Button_Template;
    public GameObject scrollContent;
    private List<string> NameList = new List<string>();

    //dictionary to check how many of a card exist by id
    public Dictionary<string, int> cardCount = new Dictionary<string, int>();
   

    //bool to indicate that collection has been loaded
    private bool isLoaded = false;

    // Use this for initialization
    void OnEnable()
    {
        isLoaded = false;
        //store script for deck builder manager
        builderManagerScript = DeckBuilderManager.GetComponent<DeckBuilderManager>();
        //load deck list into scroll view
        LoadList();
    }
    //when panel is set inactive
    void OnDisable()
    {
        isLoaded = false;
        //for every card button instantiated
        foreach (Transform child in scrollContent.transform)
        {
            //destroy the button
            Destroy(child.gameObject);
        }
        
    }

    //Loads a list of all decks for this user
    //creates a button for each deck in the scrollview
    public void LoadList()
    {
        isLoaded = false;
        foreach (string cardId in PlayFabDataStore.cardCollection)
        {

            //instantiate a new button for this deck
            GameObject button = Instantiate(Button_Template) as GameObject;
            //set it active
            button.SetActive(true);

            //store the button's script
            //CardButtonScript CB = button.GetComponent<CardButtonScript>();
            DraggableCard CB = button.GetComponent<DraggableCard>();
            //set the cards scrollview variable to be this script
            CB.SetCollectScrollView(this);
           
            //call the set id function for that button
            //stores instance id for later use
            CB.SetInstanceId(cardId);
            //sets item id based on instance id
            string itemId = PlayFabDataStore.itemIdCollection[cardId];
            CB.SetId(itemId);

            //call the set name function for that button
            //use the name associated with the id key in the dictionary
            CB.SetName(PlayFabDataStore.cardNameList[itemId]);
            //CB.SetId(cardId);

            //if the card is already in the count list, increase the count
            if (cardCount.ContainsKey(itemId))
            {
                cardCount[itemId] = (cardCount[itemId] + 1);
            }
            else //if the card isnt in the count list, add it and set to 1
            {
                cardCount.Add(itemId, 1);
            }
           
            //indicate that this card is already in deck
            CB.inDeck = false;
            //set parent to scroll view
            //second argument is worldPositionStays
            //setting to false retain local orientation and scale rather than world orientation and scale
            button.transform.SetParent(scrollContent.transform, false);

        }
        isLoaded = true;

    }
    public bool GetLoaded()
    {
        return isLoaded;
    }
    public void RemoveCard(string id)
    {
        //if the card is in the list
        if(cardCount.ContainsKey(id))
        {
            //reduce the number of available cards of that type by one
            cardCount[id] -= 1;
        }
        else
        {
            Debug.Log("Error! Cant remove card because there are none in collection");
        }
        
    }
    public GameObject GetButton(string id)
    {
        GameObject tempButton = null;
        foreach (Transform child in scrollContent.transform)
        {
            //if this button has the id of the card we are looking for
            if(child.GetComponent<DraggableCard>().GetId() == id)
            {
                //return this object
                tempButton = child.gameObject;
                //stop looking
                break;
            }
        }
        return tempButton;
    }
    //prevent the scrollview from moving
    public void FreezeScrolling()
    {
        ScrollRect scrollRect = GetComponent<ScrollRect>();
        scrollRect.StopMovement();
        scrollRect.enabled = false;
    }
    //allow the scrollview to move
    public void AllowScrolling()
    {
        ScrollRect scrollRect = GetComponent<ScrollRect>();
        scrollRect.enabled = true;
    }
}

