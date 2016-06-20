using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabApiCalls : MonoBehaviour {

	
    //Access the newest version of cloud script
    public static void PlayFabInitialize()
    {
        var cloudRequest = new GetCloudScriptUrlRequest()
        {
            Testing = false
        };

        PlayFabClientAPI.GetCloudScriptUrl(cloudRequest, (result) =>
        {
            Debug.Log("URL is set");

        },
        (error) =>
        {
            Debug.Log("Failed to retrieve Cloud Script URL");
        });
    }


    public static void RetrieveDecks(string playfabId)
    {
        var request = new ListUsersCharactersRequest()
        {
            PlayFabId = playfabId
        };

        PlayFabClientAPI.GetAllUsersCharacters(request, (result) =>
        {
            Debug.Log("Retrieving Decks...");
            int numberOfDecks = 0;
            foreach (var deck in result.Characters)
            {
                numberOfDecks++;
                //store all retrieved deck ids in the playfabdatastore list
                if (!PlayFabDataStore.deckIds.Contains(deck.CharacterId))
                {
                    //add all deck ids to a list
                    PlayFabDataStore.deckIds.Add(deck.CharacterId);
                    //associate each id with a name
                    PlayFabDataStore.deckList.Add(deck.CharacterId, deck.CharacterName);
                    Debug.Log(deck.CharacterName + "  " + deck.CharacterId);

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
    public static void GrantCardsToUser(string[] cardIdsToAdd)
    {
        var request = new RunCloudScriptRequest()
        {
            ActionId = "grantItemsToUser",
            Params = new { itemsToAdd = cardIdsToAdd }
        };

        PlayFabClientAPI.RunCloudScript(request, (result) =>
        {
            Debug.Log("Cards Granted To User");

        },
        (error) =>
        {
            Debug.Log("Cards Not Granted To User");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }
}
