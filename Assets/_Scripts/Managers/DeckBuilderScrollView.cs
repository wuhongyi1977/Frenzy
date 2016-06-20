using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeckBuilderScrollView : MonoBehaviour
{

    public GameObject Button_Template;
    public GameObject scrollContent;
    private List<string> NameList = new List<string>();


    // Use this for initialization
    void Start()
    {

        LoadList();
    }

   
    public void LoadList()
    {
        foreach (string deckId in PlayFabDataStore.deckIds)
        {
            //instantiate a new button for this deck
            GameObject button = Instantiate(Button_Template) as GameObject;
            //set it active
            button.SetActive(true);
            //store the button's script
            DeckButtonScript DB = button.GetComponent<DeckButtonScript>();
            //call the set name function for that button
            //use the name associated with the id key in the dictionary
            DB.SetName(PlayFabDataStore.deckList[deckId]);
            //set parent to scroll view
            //second argument is worldPositionStays
            //setting to false retain local orientation and scale rather than world orientation and scale
            button.transform.SetParent(scrollContent.transform, false);

        }

    }
    public void ReloadList()
    {
        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => Destroy(child));

        LoadList();
    }
    public void ButtonClicked(string str)
    {
        Debug.Log(str + " button clicked.");

    }
}
