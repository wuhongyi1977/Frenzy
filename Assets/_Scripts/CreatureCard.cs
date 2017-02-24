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

    //handles card's function upon casting
    protected override void OnPlay()
    {      
        foreach (string ability in castAbilities)
        {
            cardAbilityList.UseCardAbility(ability);
        }

        if(!creatureAbilities.Contains("Rush"))
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
        targetObject = null;
        targetReticle.SetActive(true);
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
        defensePower -= damageAmount;
        if(defensePower <= 0)
        {
            photonView.RPC("SendToGraveyard", PhotonTargets.All);
        }
        photonView.RPC("UpdateCreatureStatsIndicators", PhotonTargets.All);
    }

    [PunRPC]
    void UpdateCreatureStatsIndicators()
    {
        //update text objects 
        attackPowerTextBox.text = attackPower.ToString();
        defensePowerTextBox.text = defensePower.ToString();
    }

    [PunRPC]
    void ModifyCreatureStats(int changeToAttack, int changeToDefense)
    {
        attackPower += changeToAttack;
        defensePower += changeToDefense;
        //update text objects 
        attackPowerTextBox.text = attackPower.ToString();
        defensePowerTextBox.text = defensePower.ToString();
        //kill creature if either stat is 0 or below
        if (attackPower <= 0  || defensePower <= 0)
        {
            photonView.RPC("SendToGraveyard", PhotonTargets.All);
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
        inactiveFilter.color = new Color(0,211,200,189);
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
    }
}