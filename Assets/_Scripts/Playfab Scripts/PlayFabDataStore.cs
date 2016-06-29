using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This script stores data that is retieved from playfab
/// </summary>
public class PlayFabDataStore : MonoBehaviour
{

    public static string sessionTicket;
    public static string playFabId;
    public static string userName;
    public static string email;
   
    //number of decks a player has stored
    public static int numberOfDecks;
    //all deck ids
    public static List<string> deckIds = new List<string>();
    //all decks organized by name and id associated with it
    public static Dictionary<string, string> deckList = new Dictionary<string, string>();

    //All data for catalog cards on playfab (cardId, string of custom data )
    public static Dictionary<string, string> cardCustomData = new Dictionary<string, string>();

    //all cards owned
    public static List<string> cardCollection = new List<string>();
    //all card names stored by id
    public static Dictionary<string, string> cardList = new Dictionary<string, string>();

    //current selected deck id
    public static string currentDeck;
    //all cards in current deck
    public static List<string> cardsInDeck = new List<string>();
    //all prefab names for the current deck
    public static Dictionary<string, string> cardPrefabs = new Dictionary<string, string>();
    


    //user name for opponent
    //when not in game, stores last played opponent
    public static string opponentUserName;
 
   
}
