using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeckContentsScrollView : MonoBehaviour
{
    public GameObject DeckBuilderManager;
    private DeckBuilderManager builderManagerScript;

    public GameObject Button_Template;
    public GameObject scrollContent;
    //this is populated with all of the cards currently in deck at start (item ids)
    //The list is changed when cards are dragged in/ out
    public List<string> DeckContentsList = new List<string>();



    // Use this for initialization
    void Start()
    {
        //store script for deck builder manager
        builderManagerScript = DeckBuilderManager.GetComponent<DeckBuilderManager>();
        //load card list into scroll view
        LoadList();
    }

    //Loads a list of all decks for this user
    //creates a button for each deck in the scrollview
    public void LoadList()
    {
        foreach (string cardId in PlayFabDataStore.cardsInDeck)
        {
            //Add the card to this deck's list
            DeckContentsList.Add(cardId);
            //instantiate a new button for this deck
            GameObject button = Instantiate(Button_Template) as GameObject;
            //set it active
            button.SetActive(true);
            //store the button's script
            DraggableCard CB = button.GetComponent<DraggableCard>();
            //set the cards scrollview variable to be this script
            CB.SetDeckScrollView(this);
            //call the set name function for that button
            //use the name associated with the id key in the dictionary
            CB.SetName(PlayFabDataStore.cardList[cardId]);
            //call the set id function for that button
            //stores item id for later use
            CB.SetId(cardId);
            //indicate that this card is already in deck
            CB.inDeck = true;
            //set parent to scroll view
            //second argument is worldPositionStays
            //setting to false retain local orientation and scale rather than world orientation and scale
            button.transform.SetParent(scrollContent.transform, false);

        }

    }
    
    public void AddCardToDeck(string newCardId)
    {
        //Add the card to this deck's list
        DeckContentsList.Add(newCardId);
    }
    public void RemoveCardFromDeck(string cardId)
    {
        //Add the card to this deck's list
        DeckContentsList.Remove(cardId);
    }
    //returns the list in array form
    public string[] GetListOfCards()
    {
        string[] deckContentArray = null;
        foreach (string cardId in DeckContentsList)
        {
           //save data to array
        }
        return deckContentArray;
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


