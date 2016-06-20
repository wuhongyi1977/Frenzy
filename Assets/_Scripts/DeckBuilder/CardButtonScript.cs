using UnityEngine;
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
       
       

    }
    
}