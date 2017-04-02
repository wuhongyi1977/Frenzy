using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class BoosterManager : MonoBehaviour {

    public Text goldText;

    public Text unopenedPacks;

    public Text boosterContents;

    //all boosters owned with booster type as key and number owned as value
    public static Dictionary<string, int> boostersOwned = new Dictionary<string, int>();

    // Use this for initialization
    void Start ()
    {
        RetrieveOwnedPacks();
        goldText.text = "Gold: " + PlayFabDataStore.userGold.ToString();
       
	}
	
	// Update is called once per frame
	void Update ()
    {
        // TODO remove this
        //temp code to grant gold
		if(Input.GetKeyDown(KeyCode.G))
        {
            AddGold(100);
            
        }
	}

    public void PurchasePack()
    {
        //temp hard coded packs to add
        string[] packsToAdd = new string[] { "BoosterPack_Classic" };


        SubtractGold(100);
        PlayfabApiCalls.GrantPacksToUser(packsToAdd);
    }

    public void OpenPack()
    {
        //temp hard coded pack to open
        string packType = "BoosterPack_Classic";

        PlayfabOpenPack(packType);
    }

    public void BackToMenu()
    {
        PhotonNetwork.LoadLevel("Login");
    }

    private void PlayfabOpenPack(string packItemId)
    {
        //clear contents text
        boosterContents.text = "";

        var request = new UnlockContainerItemRequest()
        {
            ContainerItemId = packItemId
        };

        PlayFabClientAPI.UnlockContainerItem(request, (result) =>
        {
            Debug.Log("Pack opened");
            unopenedPacks.text = "Unopened packs: ";
            string[] grantedCards = new string[7];
            foreach (ItemInstance item in result.GrantedItems)
            {
                boosterContents.text += item.DisplayName+"\n" ;
                Debug.Log(item.DisplayName + " received from pack");
            }
        },
        (error) =>
        {
            Debug.Log("Pack not opened ");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }

    public void AddGold(int amountToAdd)
    {
        var request = new AddUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "GD",
            Amount = amountToAdd
        };

        PlayFabClientAPI.AddUserVirtualCurrency(request, (result) =>
        {
            Debug.Log(amountToAdd + " gold granted to local player");

            PlayFabDataStore.userGold = result.Balance;
            goldText.text = "Gold: " + result.Balance;
        },
        (error) =>
        {
            Debug.Log("Gold not granted");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }

    public void SubtractGold(int amountToSubtract)
    {
        var request = new SubtractUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "GD",
            Amount = amountToSubtract
        };

        PlayFabClientAPI.SubtractUserVirtualCurrency(request, (result) =>
        {
            Debug.Log(amountToSubtract + " gold taken from local player");

            PlayFabDataStore.userGold = result.Balance;
            goldText.text = "Gold: " + result.Balance;
        },
        (error) =>
        {
            Debug.Log("Gold not removed");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }


    //Retrieve all boosters in user's inventory
    public void RetrieveOwnedPacks()
    {
        boostersOwned.Clear();
        var request = new GetUserInventoryRequest()
        {
        };

        PlayFabClientAPI.GetUserInventory(request, (result) =>
        {
            Debug.Log("Boosters Retrieved");         
            //get all items
            foreach (var item in result.Inventory)
            {
                //if the item is a card
                if (item.ItemClass == "Booster")
                {
                    if(!boostersOwned.ContainsKey(item.DisplayName))
                    {
                        boostersOwned.Add(item.DisplayName, 1);
                    }
                    else
                    {
                        boostersOwned[item.DisplayName] += 1;
                    }
                }
            }

            foreach (KeyValuePair<string, int> entry in boostersOwned)
            {
                unopenedPacks.text = entry.Key+": "+entry.Value+"\n";
            }
           

        },
        (error) =>
        {
            Debug.Log("Boosters not retrieved");
            Debug.Log(error.ErrorMessage);
            Debug.Log(error.ErrorDetails);
        });
    }
}
