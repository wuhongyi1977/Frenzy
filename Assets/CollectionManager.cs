using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionManager : MonoBehaviour {

    List<GameObject> fullCardList;
    public GameObject cardButton;
    // Use this for initialization
    void Start ()
    {
        LoadCollection();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void LoadCollection()
    {
        foreach (string itemId in PlayFabDataStore.fullGameCardList)
        {

            //instantiate a new button for this deck
            GameObject button = Instantiate(cardButton) as GameObject;
            fullCardList.Add(button);

        }
    }
}
