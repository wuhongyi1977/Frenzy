using UnityEngine;
using System.Collections;

public class PlayerDeck : MonoBehaviour
{
    //the maximum number of cards a player can hold
    int maxHandSize = 7;
    //the number of cards to draw at the start of the game
    int startingHandSize = 5;
    //the number of cards in a player's deck
    int deckSize = 60;
    //the next card to draw from the deck (index of decksize array)
    int currentCardIndex = 0;

    //the player's deck, formed of an array of card objects
    Card[] cardDeck;
    Card[] playerHand;

	// Use this for initialization
	void Start ()
    {
        //initialize player hand array to the hand size
        playerHand = new Card[maxHandSize];
        cardDeck = new Card[deckSize];

       //draw a hand of (startingHandSize) cards
       for(int i = 0; i < startingHandSize; i++)
       {
            //set the first card (starting at index 0) of the players hand to
            //the first card (starting at index 0) in the players deck
            playerHand[i] = cardDeck[currentCardIndex];
            //increment the index of the deck (since a card has now been taken)
            currentCardIndex++;
       }
       //after this resolves, the player's hand should have 5 cards and the current card index 
       //should be at 5 (since indecies 0-4 have been taken)
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
