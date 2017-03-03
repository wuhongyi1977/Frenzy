using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardCollectionButton : MonoBehaviour {

    //The itemId of the Card (not unique, used to reference custom data)
    public string cardId;
    //the custom data of the card
    private string customData;
    //The name of the card
    public string cardTitle;


    //CARD COMPONENTS
    //contains all visible components on card
    protected Transform cardLayoutCanvas;
    protected Canvas cardCanvasScript;//< script for canvas that contains all card elements 
    protected Image cardArtImage;//< the image to display for the card art
    protected Text descriptionText;//< the text displayed to the player listing the card's abilities
    protected Text cardTitleTextBox; //< displays the card title
    protected Text castTimeTextBox; //< displays the cast time
    protected Text attackPowerTextBox; //< shows the attack power (creature only)
    protected Text defensePowerTextBox; //< shows the defense power (creature only)
    protected Text rechargeTimeTextBox; //< shows the recharge time (creature only)
    protected Image inactiveFilter; //< makes card grayed out when inactive (e.g. casting)
    protected Image cardBack; //< blocks card info when in opponents hand  

    //STATS 
    //all card stats are here, are assigned by custom data from Playfab
    //name of art asset to use for card art
    public string artName;
    //the type of card classification this card has (creature, spell, etc.)
    public string cardType;
    //The time it takes for the card to be casted
    public float castTime;
    public string castTarget = null;
    //The potential targets for this cards abilities (could be creature, player, all, autocast)
    //does not include types of targets for a creature attack, but DOES include targets for special creature abilites (like direct damage on summon)
    public string target;
    //The faction that the card belongs to. Neutral means it is available to all factions
    public string faction = "Neutral";

    // Use this for initialization
    void Start ()
    {
        //if the layout canvas of this card is found, locate its components
        if (cardLayoutCanvas = transform.FindChild("CardLayoutCanvas"))
        {
            //Get all components
            descriptionText = cardLayoutCanvas.FindChild("DescriptionText").GetComponent<Text>();
            cardTitleTextBox = cardLayoutCanvas.FindChild("CardTitle").GetComponent<Text>();
            castTimeTextBox = cardLayoutCanvas.FindChild("CastTime").GetComponent<Text>();
            attackPowerTextBox = cardLayoutCanvas.FindChild("AttackPower").GetComponent<Text>();
            defensePowerTextBox = cardLayoutCanvas.FindChild("DefensePower").GetComponent<Text>();
            rechargeTimeTextBox = cardLayoutCanvas.FindChild("RechargeTime").GetComponent<Text>();
            cardArtImage = cardLayoutCanvas.FindChild("CardArtImage").GetComponent<Image>();
            inactiveFilter = cardLayoutCanvas.FindChild("InactiveFilter").GetComponent<Image>();
            inactiveFilter.enabled = false;
            cardBack = cardLayoutCanvas.FindChild("CardBack").GetComponent<Image>();
            cardBack.enabled = false;

            cardCanvasScript = cardLayoutCanvas.GetComponent<Canvas>();
        }

        cardTitleTextBox.text = cardTitle;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void InitializeCard(string id)
    {
        SetCustomData(id);
    }
    //takes a string of custom data and stores it
    public void SetCustomData(string id)//string[] data)
    {
        //set the card's id
        cardId = id;
        //set cards name 
        cardTitle = PlayFabDataStore.cardNameList[id];
        cardTitleTextBox.text = cardTitle;
        //set cards description
        descriptionText.text = PlayFabDataStore.cardDescriptions[id];
        // Retrieve the custom data for this card type
        string[] data = PlayFabDataStore.cardCustomData[id];
        //iterate through each string in the split data
        //goes to 1 less than total length because the last variable doesnt need to be checked and nextString will fail
        for (int j = 0; j < data.Length - 1; j++)//splitResultTest.Length -1; j++)
        {
            //stores the current string being viewed
            string currentString = data[j];
            //store the next string in the list
            string nextString = data[j + 1];

            //Assign variables
            switch (currentString)
            {
                case "ArtName":
                    artName = nextString;
                    break;
                case "Faction":
                    faction = nextString;
                    break;
                case "CardType":
                    cardType = nextString;
                    break;
                case "CastTarget":
                    castTarget = nextString;
                    break;
                case "CastTime":
                    castTime = float.Parse(nextString);
                    castTimeTextBox.text = castTime.ToString();
                    break;
                case "RechargeTime":
                    rechargeTimeTextBox.text = nextString;
                    break;
                case "AttackPower":
                    attackPowerTextBox.text = nextString;
                    break;
                case "DefensePower":
                    defensePowerTextBox.text = nextString;
                    break;

                default:
                    break;
            }
        }

        //Set the proper art for the card
        SetArt();
    }

   

    public void SetArt()
    {
        if (artName != null && artName != "temp")
        {
            cardArtImage.overrideSprite = Resources.Load<Sprite>("CardArt/" + artName);
        }
    }
}
