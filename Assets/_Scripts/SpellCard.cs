using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SpellCard : BaseCard
{
    /*
    //positive values for healing, negative values for damage
    public int ownerHealth = 0;
    public int opponentHealth = 0;

    public bool isSelectable = true;

    private bool canTarget = false;
    //checks to see if the card is ready for a target to be selected
    private bool waitingForTarget = false;
    //checks to see if the card has been given a target yet
    private bool targetSelected = false;
    //checks if the cards target type has been assigned
    private bool targetAssignment = false;

    private GameObject currentTarget;

    public Text[] textBoxes;

    LineRenderer targetLine;
    */

    protected override void Awake()                //Abstract method for start
    {
        base.Awake();
    }
    public override void Start()                //Abstract method for start
    {
        base.Start();    
    }
    public override void Update()               //Abstract method for Update
    {
        base.Update();
        /*
        //get references to player objects if not assigned
        GetPlayers();

        //If the card is Not in the graveyard and is in the summon zone
        if (!inGraveyard && inSummonZone)
        {
            if (summonZoneTextBox == null)
            {
                //GetSummonZoneText();
            }
            if (castTimeTextBox != null)
            {
                castTimeTextBox.text = castTime.ToString();
            }
            //IF the current time is larger than or equal to the cast time
            isDraggable = false;
            //allow player to choose target
            canTarget = true;
            //assign this cards target based on CustomData
            SetTarget();

            if (!playedCardInSpellSlotSound)
            {
                playedCardInSpellSlotSound = true;
                audioManager.playCardInSpellSlot();
            }


            //if the target has been selected, begin countdown
            if (targetSelected == true)
            {

                //Decrement the current Time
                currentTime -= Time.deltaTime;

                //set summon zone text to current time
                summonZoneTextBox.text = currentTime.ToString("F1");
                //play card builduup sound when current time is near 0
                if (currentTime <= 3.25f && !playedCardBuildupSound)
                {
                    playedCardBuildupSound = true;
                    audioManager.playCardBuildUp();
                }
                //if the cast time completes
                if (currentTime <= 0)
                {
                    //run this card's OnCast function
                    OnCast();

                    //send to graveyard
                    SendToGraveyard();

                }
            }

        }
        */

    }
    /*
    [PunRPC]
    public override void Cast()
    {
        base.Cast();
    }
    */

    protected override void PutIntoPlay()
    {
        base.PutIntoPlay();
        OnPlay();
    }

    //handles card's function upon casting
    protected override void OnPlay()
    {
        foreach(string ability in castAbilities)
        {
            cardAbilityList.UseCardAbility(ability);
        }

        //photonView.RPC("SendToGraveyard", PhotonTargets.All);
    }

    [PunRPC]
    public void DrawTargetLine(int viewId, string targetTag)
    {
        /*
        GameObject objectToTarget;
        targetLine.enabled = true;
        //set line color based on if this is your card or opponents
        if (photonView.isMine)
        {
            objectToTarget = currentTarget;
            targetLine.SetColors(Color.blue, Color.green);
        }
        else
        {
            //if the casting player targeted their opponent
            if (targetTag == "Player2")
            {
                //show the line drawn to self on opponents screen
                objectToTarget = GameObject.FindGameObjectWithTag("Player1");
            }
            //if the casting player targeted themself
            else if (targetTag == "Player1")
            {
                //show the line drawn to opponents opponent 
                objectToTarget = GameObject.FindGameObjectWithTag("Player2");
            }
            else
            {
                objectToTarget = PhotonView.Find(viewId).gameObject;
            }

            targetLine.SetColors(Color.yellow, Color.red);
        }

        //set the first component of the line renderer to the position of the card
        targetLine.SetPosition(0, transform.position);

        //set the second component of the line renderer to the position of the target
        targetLine.SetPosition(1, objectToTarget.transform.position);
        */
    }
    //Photon Serialize View
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
        /*
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(target);
            stream.SendNext(targetSelected);


        }
        else
        {
            //Network player, receive data   
            target = (string)stream.ReceiveNext();
            targetSelected = (bool)stream.ReceiveNext();
            //cardTitleTextBox.text = cardTitle;
        }
        */

    }

}