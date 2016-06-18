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
    //user name for opponent
    //when not in game, stores last played opponent
    public static string opponentUserName;
 
   
}
