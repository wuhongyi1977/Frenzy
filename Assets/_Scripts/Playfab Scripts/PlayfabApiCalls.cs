using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabApiCalls : MonoBehaviour
{

    public static bool cardRetrievalDone = false;
    public static bool deckRetrievalDone = false;

    public static void RetrieveDecks(string playfabId)
    {
        deckRetrievalDone = false;
        var request = new ListUsersCharactersRequest()
        {
            PlayFabId = playfabId
        };

        PlayFabClientAPI.GetAllUsersCharacters(request, (result) =>
        {
            Debug.Log("Retrieving Decks...");
            int numberOfDecks = 0;
            //clear deck info
            PlayFabDataStore.deckIds.Clear();
            PlayFabDataStore.deckList.Clear();
            //retrive results
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
            deckRetrievalDone = true;
            Debug.Log("Decks Retrieved");


        }, (error) =>
        {
            Debug.Log("Can't create deck!");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }
    public static void GrantCardsToUser(string[] cardIdsToAdd)
    {
        var request = new ExecuteCloudScriptRequest()
        {
           FunctionName = "grantItemsToUser",
           FunctionParameter = new { itemsToAdd = cardIdsToAdd }
        };

        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
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

    //Fill a deck with cards
    public static void FillDeck(string deck, string[] cardIdsToAdd)
    {
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "fillDeck",
            FunctionParameter = new { deckId = deck,items = cardIdsToAdd }
        };

        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            Debug.Log("Cards Granted To Deck");

        },
        (error) =>
        {
            Debug.Log("Cards Not Granted To Deck");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }
    //remove a card from a deck
    public static void RemoveCardFromDeck(string deck, string cardIdToRemove)
    {
        Debug.Log("Trying to remove: "+ cardIdToRemove+ " from: "+deck);
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "removeCardFromDeck",
            FunctionParameter = new { deckId = deck, item = cardIdToRemove }
        };

        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            Debug.Log("Card Removed From Deck");

        },
        (error) =>
        {
            Debug.Log("Card Not Removed From Deck");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }


    //Retrieve all cards in user's inventory
    public static void RetrieveCardCollection()
    {
        
        var request = new GetUserInventoryRequest()
        {
        };

        PlayFabClientAPI.GetUserInventory(request, (result) =>
        {
            Debug.Log("Inventory Retrieved");
            //clear the card collection list to prevent duplicates
            PlayFabDataStore.cardCollection.Clear();
            //get all items
            foreach(var item in result.Inventory)
            {
                //if the item is a card
                if(item.ItemClass == "Card")
                {
                    //add the card's item instance id to the collection list
                    //PlayFabDataStore.cardCollection.Add(item.ItemId);
                    PlayFabDataStore.cardCollection.Add(item.ItemInstanceId);
                    if (!PlayFabDataStore.itemIdCollection.ContainsKey(item.ItemInstanceId))
                    {
                        //allow each cards item id to be referenced from its item instancde id
                        PlayFabDataStore.itemIdCollection.Add(item.ItemInstanceId, item.ItemId);
                    }
                    //add a reference to the name associated with that id if it hasnt been added
                    if(!PlayFabDataStore.cardNameList.ContainsKey(item.ItemId))
                    {
                        PlayFabDataStore.cardNameList.Add(item.ItemId, item.DisplayName);
                    }
                    
                    Debug.Log("Found: "+ item.ItemId +" -> "+item.DisplayName);
                }
                
            }
           
            
        },
        (error) =>
        {
            Debug.Log("Inventory was not retrieved");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }

    //Retrieve all cards in a particular deck
    public static void RetrieveCardsInDeck(string deckId)
    {
        Debug.Log("Trying to retrieve cards for: "+ deckId);
        cardRetrievalDone = false;
        var request = new GetCharacterInventoryRequest()
        {
            CharacterId = deckId
        };

        PlayFabClientAPI.GetCharacterInventory(request, (result) =>
        {
            Debug.Log("Deck Inventory Retrieved");
            //clear the cards in deck (in case deck has changed, list is being repopulated)
            PlayFabDataStore.cardsInDeck.Clear();
            //get all items
            foreach (var item in result.Inventory)
            {
               
                //if the item is a card
                if (item.ItemClass == "Card")
                {


                    //add the card's item id to the collection list
                    //PlayFabDataStore.cardsInDeck.Add(item.ItemId);
                    PlayFabDataStore.cardsInDeck.Add(item.ItemInstanceId);
                    //allow each cards item id to be referenced from its item instancde id
                    if (!PlayFabDataStore.itemIdCollection.ContainsKey(item.ItemInstanceId))
                    {
                        PlayFabDataStore.itemIdCollection.Add(item.ItemInstanceId, item.ItemId);
                    }
                    //add a reference to the name associated with that id if it hasnt been added
                    //All cards the player owns should be able to be referenced here
                    if (!PlayFabDataStore.cardNameList.ContainsKey(item.ItemId))
                    {
                        PlayFabDataStore.cardNameList.Add(item.ItemId, item.DisplayName);
                    }

                    Debug.Log("Found: " + item.ItemId + " -> " + item.DisplayName);
                }

            }
            cardRetrievalDone = true;

        },
        (error) =>
        {
            Debug.Log("Deck Inventory was not retrieved");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }

    //Delete a deck
    public static void DeleteDeck(string deck)
    {
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "deleteDeck",
            FunctionParameter = new { deckId = deck }
        };

        PlayFabClientAPI.ExecuteCloudScript(request, (result) =>
        {
            Debug.Log("Deck Deleted");
            //store the number of saved decks
            PlayFabDataStore.numberOfDecks -=1;
            //Remove all references to deck
            if (PlayFabDataStore.deckIds.Contains(deck))
            {
                //Remove deck id from list
                PlayFabDataStore.deckIds.Remove(deck);
                //remove deck id and associated name
                PlayFabDataStore.deckList.Remove(deck);
            }

        },
        (error) =>
        {
            Debug.Log("Deck Not Deleted");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }

    //store all custom data for all cards (every card in existance)
    public static void RetrieveCatalogData()
    {
        var request = new GetCatalogItemsRequest()
        {
        };

        PlayFabClientAPI.GetCatalogItems(request, (result) =>
        {
            Debug.Log("Card data retrieved");
            
            foreach (var item in result.Catalog)
            {
                //only store info if this is a card
                if(item.ItemClass == "Card")
                {
                    PlayFabDataStore.fullGameCardList.Add(item.ItemId);
                    Debug.Log("Item id of this is :" + item.ItemId);
                    //store custom data for this card
                    string customData = item.CustomData;
                    Debug.Log("Heres custom data: " + customData);
                    //split the string to get prefab name
                    string[] splitResult =customData.Split(':', '"', '"', '}');
                    //assign this to the prefab name from split
                    string prefabName = splitResult[4];
                    Debug.Log("Prefab name is: "+prefabName);
                    
                    //define where to split the custom data to retrive variables
                    string[] stringSeparators = new string[] { "{\"", "\":\"", "\",\"", "\"}" };
                    //split custom data using seperators
                    string[] splitResults = customData.Split(stringSeparators, System.StringSplitOptions.None);

                    //store all custom data to data store
                    if (!PlayFabDataStore.cardDescriptions.ContainsKey(item.ItemId))
                    {
                        PlayFabDataStore.cardDescriptions.Add(item.ItemId, item.Description);//item.CustomData);
                    }


                    //store all custom data to data store
                    if (!PlayFabDataStore.cardCustomData.ContainsKey(item.ItemId))
                    {
                        PlayFabDataStore.cardCustomData.Add(item.ItemId, splitResults);//item.CustomData);
                    }

                   
                    //stores display names for all cards
                    if (!PlayFabDataStore.cardNameList.ContainsKey(item.ItemId))
                    {
                        PlayFabDataStore.cardNameList.Add(item.ItemId, item.DisplayName);
                    }
                }
               
            }

        },
        (error) =>
        {
            Debug.Log("Card Data Not Retrieved");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }


}
