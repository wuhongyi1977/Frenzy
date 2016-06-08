using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    PhotonView photonView;
	// Use this for initialization
	void Start ()
    {
        photonView = GetComponent<PhotonView>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        photonView.RPC("Hey", PhotonTargets.Others);
	}
    [PunRPC]
    void Hey()
    {
        Debug.Log("Hey!!!");
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            
        }
        else
        {
            //Network player, receive data          
           
        }
       
    }
}
