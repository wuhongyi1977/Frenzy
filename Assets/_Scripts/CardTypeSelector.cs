using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTypeSelector : MonoBehaviour
{
    // finds the card type in the custom data, then calls AddClassAndInitialize
    public void SetCardType(int ownerId, string id)
    {
        string[] data = PlayFabDataStore.cardCustomData[id];
        string cardType = null;
        for (int j = 0; j < data.Length - 1; j++)//splitResultTest.Length -1; j++)
        {
            if (data[j] == "CardType")
            {
                cardType = data[j + 1];
                if (cardType == "Spell" || cardType == "Creature")
                {
                    GetComponent<PhotonView>().RPC("AddClassAndInitialize", PhotonTargets.All, cardType, ownerId, id);
                    return;
                }
                else
                { Debug.LogError(id + " does not have a recognized card type"); }
                return;
            }
        }
    }

    // adds the proper card component to this card, then call initialize on the local copy
    [PunRPC]
    public void AddClassAndInitialize(string cardType, int ownerId, string id)
    {
        if (cardType == "Spell")
        {
            gameObject.AddComponent<SpellCard>();
        }
        else if (cardType == "Creature")
        {
            gameObject.AddComponent<CreatureCard>();
        }
        if (GetComponent<PhotonView>().isMine)
        {
            GetComponent<BaseCard>().InitializeCard(ownerId, id);
        }
    }
	
}
