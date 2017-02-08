using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SpellCard : BaseCard
{
   /*
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
    */
    
    
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
    }

}