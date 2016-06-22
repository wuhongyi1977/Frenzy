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
    //current selected deck id
    public static string currentDeck;
    //number of decks a player has stored
    public static int numberOfDecks;
    //all deck ids
    public static List<string> deckIds = new List<string>();
    //all decks organized by name and id associated with it
    public static Dictionary<string, string> deckList = new Dictionary<string, string>();

    //all cards owned
    public static List<string> cardCollection = new List<string>();
    //all card names stored by id
    public static Dictionary<string, string> cardList = new Dictionary<string, string>();

    //all cards in current deck
    public static List<string> cardsInDeck = new List<string>();
    


    //user name for opponent
    //when not in game, stores last played opponent
    public static string opponentUserName;
 
   
}
