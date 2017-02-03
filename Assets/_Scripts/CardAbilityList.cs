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
    private void ChangeTargetHealth()
    {
        GameObject targetObj = cardScript.targetObject;
        if(targetObj.tag == "Creature")
        {
            //creature health change
        }
        else if (targetObj.tag == "Player1") //< local player
        {
            cardScript.localPlayerController.ChangeHealth(cardScript.targetHealthChange);
        }
        else if (targetObj.tag == "Player2") //< opponent
        {
            cardScript.opponentPlayerController.ChangeHealth(cardScript.targetHealthChange);          
        }
    }
}
