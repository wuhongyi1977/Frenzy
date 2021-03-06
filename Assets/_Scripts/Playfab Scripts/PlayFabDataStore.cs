﻿using UnityEngine;
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

    //the number of cards in a deck
    public static int deckSize = 60;
   
    //number of decks a player has stored
    public static int numberOfDecks;
    //all deck ids
    public static List<string> deckIds = new List<string>();
    //all decks organized by name and id associated with it
    public static Dictionary<string, string> deckList = new Dictionary<string, string>();

    //All data for catalog cards on playfab (cardId, string of custom data )
    public static Dictionary<string, string[]> cardCustomData = new Dictionary<string, string[]>();
    //all prefab names for all cards
    public static Dictionary<string, string> cardPrefabs = new Dictionary<string, string>();
    //all description text for all cards
    public static Dictionary<string, string> cardDescriptions = new Dictionary<string, string>();

    //all cards owned by instance id (unique to every object)
    public static List<string> cardCollection = new List<string>();
    //all cards item id <itemInstanceId, itemId> (instance id is unique, can be duplicate item id's if it is the same type of card)
    public static Dictionary<string, string> itemIdCollection = new Dictionary<string, string>();
    //all card names stored by item id (to reference actual name from item id)
    public static Dictionary<string, string> cardNameList = new Dictionary<string, string>();

    //current selected deck id
    public static string currentDeck;
    //all cards in current deck 
    public static List<string> cardsInDeck = new List<string>();
    
    


    //user name for opponent
    //when not in game, stores last played opponent
    public static string opponentUserName;
 
   
}
