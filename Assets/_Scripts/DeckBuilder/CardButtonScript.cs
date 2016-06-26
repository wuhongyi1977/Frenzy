﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardButtonScript : MonoBehaviour
{

    private string Name;
    private string itemId;
    public Text ButtonText;
    public CardCollectionScrollView cardScrollView;
    public DeckContentsScrollView deckScrollView;

    public GameObject draggableCard;
   
   
    public void SetName(string name)
    {
        Name = name;
        ButtonText.text = name;
    }
    public void SetId(string id)
    {
        itemId = id;
    }
    public void OnMouseDown()
    {
        /*
        //instantiate a new button for this deck
        GameObject card = Instantiate(draggableCard) as GameObject;
        //set it active
        card.SetActive(true);
        //store the button's script
        DraggableCard DC = card.GetComponent<DraggableCard>();
        //call the set name function for that button
        DC.SetName(Name);
        DC.SetId(itemId);
        //remove from scrollview (set the scene canvas as parent)
        DC.transform.SetParent(cardScrollView.sceneCanvas.transform, false);
        //set to mouse position
        DC.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, DC.transform.position.z));
        //needs to follow mouse!!!

        */
        


    }
    /*
    public void ButtonClick()
    {
        //instantiate a new button for this deck
        GameObject card = Instantiate(draggableCard) as GameObject;
        //set it active
        card.SetActive(true);
        //store the button's script
        DraggableCard DC = card.GetComponent<DraggableCard>();
        //call the set name function for that button
        DC.SetName(Name);
        DC.SetId(itemId);
        //remove from scrollview
        DC.transform.SetParent(cardScrollView.sceneCanvas.transform, false);



    }
    */
    
}