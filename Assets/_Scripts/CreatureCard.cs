using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class CreatureCard : BaseCard
{
    public enum creatureState
    { 
       Recharging, Idle
    };
    public creatureState currentCreatureState = creatureState.Idle;

    //the time it takes for this card to recharge after a use (ex: time it takes a creature before it can attack again)
    public float rechargeTime;
    //the attack power of this card (if its a creature)
    public int attackPower;
    //the defense power of this card (if its a creature)
    public int defensePower;

    private float rechargeCountdown = 0;

    private float frozenCountdown = 0;
    private Color defaultInactiveFilterColor;

    // creature stats are AttackPower, DefensePower, RechargeTime

    // called right after retrieving custom data in base card class
    protected override void InitializeStats()
    {
        rechargeTime = float.Parse(creatureStatValues["RechargeTime"]);
        attackPower = int.Parse(creatureStatValues["AttackPower"]);
        defensePower = int.Parse(creatureStatValues["DefensePower"]);
        attackPowerTextBox.text = attackPower.ToString(); 
        defensePowerTextBox.text = defensePower.ToString();
        rechargeTimeTextBox.text = rechargeTime.ToString();
    }

    //resets variables if card is returned to hand
    [PunRPC]
    public override void Reset()
    {
        base.Reset();
        InitializeStats();
    }

    public override void Update()                //Abstract method for Update
    {
        base.Update();

        // if frozen, handle frozen countdown but dont allow other code to run
        if(frozenCountdown > 0)
        {
            frozenCountdown -= Time.deltaTime;
            summonZoneTextBox.text = frozenCountdown.ToString("F1");
            if(photonView.isMine && frozenCountdown <= 0)
            {
                photonView.RPC("EndFreeze", PhotonTargets.All);      
            }
        }
        //handle recharge countdown
        else if (currentCreatureState == creatureState.Recharging)
        {
            rechargeCountdown -= Time.deltaTime;
            summonZoneTextBox.text = rechargeCountdown.ToString("F1");
            if (rechargeCountdown <= 0)
            {
                currentCreatureState = creatureState.Idle;
                currentCardState = cardState.InPlay;
                //clear summon zone text
                summonZoneTextBox.text = "";
                if(photonView.isMine)
                {
                    PrepareToAttack();
                }               
            }
        }
        else if(currentCardState == cardState.WaitForTarget)
        {
            if(targetObject != null)
            {
                currentCardState = cardState.InPlay;
                Attack();
                photonView.RPC("StartRecharging", PhotonTargets.All);
            }
        }
    }

    protected override void CardEntersPlayTrigger(int viewId)
    {
        //override in sub class for proper behavior
    }
    protected override void CardLeavesPlayTrigger(int viewId)
    {
        if(photonView.isMine)
        {
            //if this creature has consume and the creature that left play belonged to this player
            if (creatureAbilities.Contains("Consume") && (currentCardState == cardState.InPlay || currentCardState == cardState.WaitForTarget))
            {
                PhotonView view = PhotonView.Find(viewId);
                if(view.isMine && view.GetComponent<CreatureCard>())
                {
                    //get value of consume
                    int consumeAmount = int.Parse(abilityValues["Consume"]);
                    //increase attack and defense by consume amount
                    photonView.RPC("ModifyCreatureStats", PhotonTargets.All, consumeAmount, consumeAmount, 0.0f);
                }
            }
        }
        
    }

    //handles card's function upon casting
    protected override void OnPlay()
    {      
        foreach (string ability in castAbilities)
        {
            cardAbilityList.UseCardAbility(ability);
        }
        targetLine.enabled = false;
        if (!creatureAbilities.Contains("Rush"))
        {
            photonView.RPC("StartRecharging", PhotonTargets.All);
        }
        else
        {
            PrepareToAttack();
        }
    }

    [PunRPC]
    void StartRecharging()
    {
        rechargeCountdown = rechargeTime;
        summonZoneTextBox.color = Color.red;
        currentCreatureState = creatureState.Recharging;
    }

    void PrepareToAttack()
    {
        //handle target selection
        targetReticle.SetActive(true);
        targetObject = null;        
        MoveReticle(transform.position);
        currentCardState = cardState.WaitForTarget;
    }

    void Attack()
    {
        if(targetObject == this.gameObject)
        {return;}

        //StartCoroutine(AttackAnimation(targetObject));
        if (targetObject.tag == "Player1") //< local player
        {
            localPlayerController.ChangeHealth(-attackPower);
        }
        else if (targetObject.tag == "Player2") //< opponent
        {
            opponentPlayerController.photonView.RPC("ChangeHealth", PhotonTargets.Others, -attackPower);
        }
        
        else if (targetObject.GetComponent<CreatureCard>() != null)
        {
            //get damage dealt by defending creature
            int damageToTake = targetObject.GetComponent<CreatureCard>().attackPower;
            // deal damage to defending creature
            if(!targetObject.GetComponent<CreatureCard>().creatureAbilities.Contains("Elusive"))
            {
                targetObject.GetPhotonView().RPC("TakeDamage", PhotonTargets.Others, attackPower);
            }          
            // take damage from defending creature
            if (!creatureAbilities.Contains("Elusive"))
            {
                TakeDamage(damageToTake);
            }

        }
        
        targetObject = null;
        targetReticle.SetActive(false);
    }

    // moves card to target and back (UNTESTED)
    IEnumerator AttackAnimation(GameObject targetObj)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = targetObj.transform.position;
        float i = 0.0f;
        float time = 3.0f;
        float rate = 1.0f / time;
        while (i < 1.0)
        {
            i += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(startPos, endPos, i);
        }
        while (i > 0.0)
        {
            i -= Time.deltaTime * rate;
            transform.position = Vector3.Lerp(endPos, startPos, i);
        }
        transform.position = startPos;
        yield return null;
    }


    [PunRPC]
    void TakeDamage(int damageAmount)
    {
        photonView.RPC("ModifyCreatureStats", PhotonTargets.All, 0, -damageAmount, 0.0f);
    }


    [PunRPC]
    public void ModifyCreatureStats(int changeToAttack, int changeToDefense, float changeToRecharge)
    {
        attackPower += changeToAttack;
        defensePower += changeToDefense;
        rechargeTime += changeToRecharge;
        if(rechargeTime < 0)
        { rechargeTime = 0; }
        //update text objects 
        attackPowerTextBox.text = attackPower.ToString();
        defensePowerTextBox.text = defensePower.ToString();
        rechargeTimeTextBox.text = rechargeTime.ToString();
        //kill creature if either stat is 0 or below
        if (attackPower <= 0  || defensePower <= 0)
        {
            NotifyFieldManagerExit();
        }
        
    }

    [PunRPC]
    void GrantAbility(string ability)
    {
        creatureAbilities.Add(ability);
    }

    [PunRPC]
    void Freeze(float duration)
    {
        inactiveFilter.enabled = true;
        defaultInactiveFilterColor = inactiveFilter.color;
        Color freezeColor = new Color(0, 211, 200, 189);
        inactiveFilter.color = freezeColor;
        frozenCountdown = duration;       
    }

    [PunRPC]
    void EndFreeze()
    {
        summonZoneTextBox.text = "";
        inactiveFilter.color = defaultInactiveFilterColor;
        inactiveFilter.enabled = false;
        frozenCountdown = 0;
    }

    //Photon Serialize View
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
        /*
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(attackPower);
            stream.SendNext(defensePower);


        }
        else
        {
            //Network player, receive data   
            attackPower = (int)stream.ReceiveNext();
            defensePower = (int)stream.ReceiveNext();
        }
        */
    }
}