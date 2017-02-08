using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class holds static functions to handle all types of card abilities
/// Any cards can call these functions and pass in proper variables to use ability
/// </summary>
public class CardAbilityList : MonoBehaviour
{
    BaseCard cardScript;

    private void Start()
    {
        cardScript = GetComponent<BaseCard>();
    }

    public void UseCardAbility(string abilityToCall)
    {
        Invoke(abilityToCall, 0);
    }

    // damages or heals a player (positive values for healing, negative for damage)
    private void TargetHealthChange()
    {
        GameObject targetObj = cardScript.targetObject;
        int healthChange = int.Parse(cardScript.abilityValues["TargetHealthChange"]);

        if (targetObj.tag == "Creature")
        {
            //creature health change
        }
        else if (targetObj.tag == "Player1") //< local player
        {
            cardScript.localPlayerController.ChangeHealth(healthChange);
        }
        else if (targetObj.tag == "Player2") //< opponent
        {
            cardScript.opponentPlayerController.photonView.RPC("ChangeHealth", PhotonTargets.Others, healthChange);
        }
    }

    // damages or heals the local player (positive values for healing, negative for damage)
    private void OwnerHealthChange()
    {     
        int healthChange = int.Parse(cardScript.abilityValues["OwnerHealthChange"]);
        cardScript.localPlayerController.ChangeHealth(healthChange);  
    }

    // discards the card
    private void DiscardOnCast()
    {
        cardScript.photonView.RPC("SendToGraveyard", PhotonTargets.All);
    }
}
