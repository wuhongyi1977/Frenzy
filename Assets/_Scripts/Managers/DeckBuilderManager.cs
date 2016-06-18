using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;

public class DeckBuilderManager : MonoBehaviour {

    int maxDecks = 4;
    int currentDecks = 0;

    public GameObject deckButton;

 
	// Use this for initialization
	void Start ()
    {
        //retrieve all decks the user has made
        RetrieveDecks(PlayFabDataStore.playFabId);
        //create buttons for all decks
        //PUT CODE HERE

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    
    public void NewDeck()
    {
        //check how many decks the user has

        //if the user has less than the max decks, make a new one
        if(PlayFabDataStore.numberOfDecks < maxDecks)
        {
            string deckName = "Deck " + (PlayFabDataStore.numberOfDecks + 1);
            //send the default name of the deck for the name
            CreateDeck(deckName);
        }
    }
  
    //Creates a new deck for the player
    //Deck is a "character" on playfab granted to user
    public static void CreateDeck(string name)
    {
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "newDeck", // Arbitrary function name (must exist in your uploaded cloud.js file)
            FunctionParameter = new { deckName = name, characterType = "Deck" }, // The parameter provided to your function
            //ActionId = "newDeck",
            //Params = new { deckName = name, characterType = "Deck" }//set to whatever default class is
        };

        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            Debug.Log("Deck Created");
            //Debug.Log(result.FunctionResult);
            //PlayFabDataStore.currentDeck = result.characterID;

            // Cloudscript returns arbitrary results, so you have to evaluate them one step and one parameter at a time
            Debug.Log(PlayFab.SimpleJson.SerializeObject(result));
            Debug.Log(PlayFab.SimpleJson.SerializeObject(result.FunctionResult));
            Debug.Log(result.FunctionResult);
            /*
            JsonObject jsonResult = (JsonObject)result.FunctionResult;
            object messageValue;
            jsonResult.TryGetValue("messageValue", out messageValue); // note how "messageValue" directly corresponds to the JSON values set in Cloud Script

            Debug.Log((string)messageValue);
            */

        }, (error) =>
        {
           
            Debug.Log("Can't create deck!");
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

    }
    public void BackToMenu()
    {
        PhotonNetwork.LoadLevel("Login");
    }
}
