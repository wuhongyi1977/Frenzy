using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;

public class DeckBuilderManager : MonoBehaviour {

    int maxDecks = 4;
    int currentDecks = 0;

    public GameObject deckButton;

    //UI Panels
    public GameObject deckSelectPanel;
    public GameObject deckBuildPanel;

 
	// Use this for initialization
	void Start ()
    {
        //retrieve all decks the user has made
        RetrieveDecks(PlayFabDataStore.playFabId);
        //create buttons for all decks
        //PUT CODE HERE

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
    }
  
    //Creates a new deck for the player
    //Deck is a "character" on playfab granted to user
    public static void CreateDeck(string name)
    {
        var request = new RunCloudScriptRequest()
        {
            ActionId = "newDeck",
            Params = new { characterName = name, characterType = "Deck" }
        };
        PlayFabClientAPI.RunCloudScript(request, (result) =>
        {
            Debug.Log("Deck Created");
            //string[] splitResult = result.ResultsEncoded.Split('"'); //19th element is the itemInstanceId
            //Debug.Log("Split Result " + splitResult[59]); // 63th element is the itemId of the item granted from the drop table
           // Debug.Log("Split Result " + splitResult[63]); // 63th element is the itemInstanceId of the item granted from the drop table
           // Debug.Log("Split Result " + splitResult[67]); // 67st element is the item class  

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
    //Creates a new deck for the player
    //Deck is a "character" on playfab granted to user
    public static void RetrieveDecks(string playfabId)
    {
        var request = new ListUsersCharactersRequest()
        {
            PlayFabId = playfabId
        };

        PlayFabClientAPI.GetAllUsersCharacters(request, (result) =>
        {
            int numberOfDecks = 0;
            foreach (var deck in result.Characters)
            {
                numberOfDecks++;
                //store all retrieved deck ids in the playfabdatastore list
                if (!PlayFabDataStore.deckIds.Contains(deck.CharacterId))
                {
                    PlayFabDataStore.deckIds.Add(deck.CharacterId);
                }
            }
            //store the number of saved decks
            PlayFabDataStore.numberOfDecks = numberOfDecks;

        }, (error) =>
        {
            Debug.Log("Can't create deck!");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }
    //load buttons for each deck that has been created already
    public void LoadDeckButtons()
    {
        foreach (string deckId in PlayFabDataStore.deckIds)
        {
            //instantiate a new button for this deck
            GameObject button = Instantiate(deckButton) as GameObject;
            //set it active
            button.SetActive(true);
            //store the button's script
            DeckButtonScript DB = button.GetComponent<DeckButtonScript>();
            //call the set name function for that button
            DB.SetName(deckId);
            //set 
            button.transform.SetParent(deckButton.transform.parent);

        }
    }
    public void DeckButtonClicked(string name)
    {
       
        deckSelectPanel.SetActive(false);
        deckBuildPanel.SetActive(true);
        //retrieve deck info and populate build panel
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
