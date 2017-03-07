using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionManager : MonoBehaviour {

    List<GameObject> fullCardList;
    public GameObject cardButton;

    public GameObject collectionScrollView;
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
            if(itemId != "Cardset_Cardname_Holo")
            {
                //instantiate a new button for this deck
                GameObject button = Instantiate(cardButton) as GameObject;
                button.transform.SetParent(collectionScrollView.transform, false);
                button.GetComponent<CardCollectionButton>().itemId = itemId;
                //fullCardList.Add(button);
            }


        }

       
    }
}
