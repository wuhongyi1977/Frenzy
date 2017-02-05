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
        
    }
    
    [PunRPC]
    protected override void PutIntoPlay()
    {
        base.PutIntoPlay();
        if(photonView.isMine)
        {
            OnPlay();
        }
       
    }

    //handles card's function upon casting
    protected override void OnPlay()
    {
        foreach (string ability in castAbilities)
        {
            cardAbilityList.UseCardAbility(ability);
        }
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