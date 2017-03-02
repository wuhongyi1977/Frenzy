using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class holds static functions to handle all types of card abilities
/// Any cards can call these functions and pass in proper variables to use ability
/// </summary>
public class CardAbilityList : MonoBehaviour
{
    public delegate void FieldHandlerCounter(BaseCard counterCard); 
    public static event FieldHandlerCounter NotifyCounter;

    //TEST
    public FieldManager fieldManager;

    BaseCard cardScript;
    //public FieldManager fieldManager;


    private void OnEnable()
    {
        FieldManager.SendFieldManagerScript += GetFieldManager;
    }

    private void OnDisable()
    {
        FieldManager.SendFieldManagerScript -= GetFieldManager;
    }

    public void GetFieldManager(FieldManager managerScript)
    {
        fieldManager = managerScript;
    }

    private void Start()
    {
        cardScript = GetComponent<BaseCard>();
       
    }

    public void UseCardAbility(string abilityToCall)
    {
        Invoke(abilityToCall, 0);
    }

    



/// <summary>
/// All Card Abilities below
/// </summary>

    // discards the card
    private void DiscardOnCast()
    {
        cardScript.NotifyFieldManagerExit();//photonView.RPC("SendToGraveyard", PhotonTargets.All);
    }

    // damages or heals a player (positive values for healing, negative for damage)
    private void TargetHealthChange()
    {
        GameObject targetObj = cardScript.targetObject;
        int healthChange = int.Parse(cardScript.abilityValues["TargetHealthChange"]);             
        if (targetObj.tag == "Player1") //< local player
        {cardScript.localPlayerController.ChangeHealth(healthChange);}
        else if (targetObj.tag == "Player2") //< opponent
        {cardScript.opponentPlayerController.photonView.RPC("ChangeHealth", PhotonTargets.Others, healthChange);}
        //creature health change (must be negative because creature damage expects a positive value for damage) 
        else if (targetObj.tag == "Card" && targetObj.GetComponent<CreatureCard>())
        { targetObj.GetPhotonView().RPC("TakeDamage", PhotonTargets.Others, -healthChange); }
    }

    // damages or heals the local player (positive values for healing, negative for damage)
    private void OwnerHealthChange()
    {     
        int healthChange = int.Parse(cardScript.abilityValues["OwnerHealthChange"]);
        cardScript.localPlayerController.ChangeHealth(healthChange);  
    }

    // grants an ability to target creature
    private void GrantCreatureAbility()
    {
        GameObject targetObj = cardScript.targetObject;
        string ability = cardScript.abilityValues["GrantCreatureAbility"];
        if (targetObj.GetComponent<CreatureCard>() != null)
        {targetObj.GetPhotonView().RPC("GrantAbility", PhotonTargets.All, ability);}
    }

    // grants an ability to target creature
    private void ReturnCardToHand()
    {
        GameObject targetObj = cardScript.targetObject;
        targetObj.GetComponent<BaseCard>().ReturnToHand();
    }

    private void FreezeCreature()
    {
        GameObject targetObj = cardScript.targetObject;
        float freezeDuration = float.Parse(cardScript.abilityValues["FreezeCreature"]);
        //creature health change (must be negative because creature damage expects a positive value for damage)
        if (targetObj.tag == "Card" && targetObj.GetComponent<CreatureCard>() != null)
        { targetObj.GetPhotonView().RPC("Freeze", PhotonTargets.All, freezeDuration); }
    }

    private void ChangeCreatureAttackPower()
    {
        GameObject targetObj = cardScript.targetObject;
        int amountToChangeAttack = int.Parse(cardScript.abilityValues["ChangeCreatureAttackPower"]);
        //creature health change (must be negative because creature damage expects a positive value for damage)
        if (targetObj.tag == "Card" && targetObj.GetComponent<CreatureCard>() != null)
        { targetObj.GetPhotonView().RPC("ModifyCreatureStats", PhotonTargets.All, amountToChangeAttack, 0, 0.0f); }
    }

    private void ChangeAllOwnCreatureRecharge()
    {
        List<GameObject> allOwnedCreatures = fieldManager.GetOwnCreatures();
        float amountToChangeRecharge = float.Parse(cardScript.abilityValues["ChangeAllOwnCreatureRecharge"]);
        foreach (GameObject creatureCard in allOwnedCreatures)
        {
            creatureCard.GetPhotonView().RPC("ModifyCreatureStats", PhotonTargets.All, 0, 0, amountToChangeRecharge);
        }
    } 

    private void Counter()
    {
        NotifyCounter(GetComponent<BaseCard>());
    }


}
