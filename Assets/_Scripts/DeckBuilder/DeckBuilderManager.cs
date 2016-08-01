using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class DeckBuilderManager : MonoBehaviour {

    int maxDecks = 4;
    int currentDecks = 0;
    int deckSize;

    //UI Panels
    public GameObject deckSelectPanel;
    public GameObject deckBuildPanel;
    public GameObject loadingPanel;

    //components for deck selector
    public GameObject scrollView;
    DeckBuilderScrollView scrollViewScript;
    public GameObject deckButton;
    private bool delete = false;

    //Components for deck builder
    public InputField deckNameField;
    private string currentDeckId;
    public DeckContentsScrollView deckContentScript;

    public GameObject notePanel;
    public Text noteText;
   
    void Awake()
    {
        deckSize = PlayFabDataStore.deckSize;
        loadingPanel.SetActive(true);
        deckBuildPanel.SetActive(false);
        deckSelectPanel.SetActive(false);
        StartCoroutine(WaitForLoad());
    }
    public IEnumerator WaitForLoad()
    {
        deckSelectPanel.SetActive(true);
        yield return new WaitForSeconds(1f);
        //wait until decks are loaded to continue
        while (!scrollViewScript.GetDecksLoaded())
        {
            yield return new WaitForSeconds(1f);
        }
        loadingPanel.SetActive(false);
        
    }
    // Use this for initialization
    void Start ()
    {
       
       
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
            //CreateDeck(deckName); 
            CreateDeck("NewDeck");
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
            PlayFabDataStore.numberOfDecks += 1;
            //Debug.Log(result.ResultsEncoded.Split);
            string[] splitResult = result.ResultsEncoded.Split(':','"','"', '}');
            //grab the data at position 2 (which is the character id)
            string deckId = splitResult[4];
            Debug.Log(deckId);
            //store this deck in the decklist
            PlayFabDataStore.deckIds.Add(deckId);
            PlayFabDataStore.deckList.Add(deckId, name);
            
           //create a new button for this deck
            scrollViewScript.CreateButton(deckId);
            //delete all deck buttons and reload list
            //scrollViewScript.ReloadList();

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
        StartCoroutine(LoadDeckBuilder(id, name));
    }
    IEnumerator LoadDeckBuilder(string id, string name)
    {
        //disable deck select panel
        deckSelectPanel.SetActive(false);
        //display loading screen
        loadingPanel.SetActive(true);
        //retrieve deck info and populate build panel
        PlayFabDataStore.currentDeck = id;
        currentDeckId = id;
        deckNameField.text = name;
        //retrieve all cards in the selected deck
        PlayfabApiCalls.RetrieveCardsInDeck(id);
        //wait until retrieval is done
        while(!PlayfabApiCalls.cardRetrievalDone)
        {
            yield return new WaitForSeconds(1f);
        }
        //when cards are retrieved, activate build panel
        loadingPanel.SetActive(false);
        deckBuildPanel.SetActive(true);
    }
   
       

       
    //saves cards in deck and deck name
    public void SaveDeck()
    {
        //NONE OF THIS WORKS YET//////////////
        //store new name from input text field
        string newName = deckNameField.text;
        //update deck name
        //UpdateDeckName(currentDeckId, newName);
        ////////////////////////////////////////////
        //retrieve all cards in the deck content list
        Debug.Log("Deck script is: "+deckContentScript);
        //get the list of cards to add as an array
        string[] contentsList = deckContentScript.GetListOfCards();
        
        //get the list of the original deck as an array
        /*
        string[] oldContents = deckContentScript.GetOldCards();
        //empty deck first
        
        foreach(string id in oldContents)
        {
            PlayfabApiCalls.RemoveCardFromDeck(currentDeckId, id);
        }
        */
        
       
        
        //store all of the cards
        PlayfabApiCalls.FillDeck(currentDeckId, contentsList);

        //display a popup to show deck saved
        notePanel.SetActive(true);
       //if there are not enough cards to complete a deck
        if (contentsList.Length < deckSize)
        {
            //store a variable in the deck that it cannot be played until full
            noteText.text = "Deck Saved \n (Unfinished)";
        }
        else
        {
            noteText.text = "Deck Saved";
        }
        deckBuildPanel.SetActive(false);  
    }
    //changes value of delete bool
    public void ToggleDelete()
    {
        delete = !delete;
        Debug.Log("Delete is: " + delete);
    }
    //returns whether delete button is active
    public bool GetDelete()
    {
        return delete;
    }
    public void BackToDeckSelect()
    {
        notePanel.SetActive(false);
        deckBuildPanel.SetActive(false);
        deckSelectPanel.SetActive(true);
    }
    public void BackToMenu()
    {
        PhotonNetwork.LoadLevel("Login");
    }
}
