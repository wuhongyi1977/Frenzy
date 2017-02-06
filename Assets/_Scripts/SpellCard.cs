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