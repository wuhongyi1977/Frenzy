using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAvatar : MonoBehaviour {

    PhotonView photonView;
    Text playerName;
    // Use this for initialization
    void Start ()
    {
        photonView = GetComponent<PhotonView>();
        playerName = transform.GetComponentInChildren<Text>();

        if (photonView.isMine)
        {
            gameObject.name = "LocalPlayerAvatar";
            gameObject.tag = "Player1";
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0.2f, 0.1f, 5));
            playerName.text = PlayFabDataStore.userName;

        }
        else
        {
            gameObject.name = "OpponentAvatar";
            gameObject.tag = "Player2";
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0.2f, 0.9f, 5));
            playerName.text = PlayFabDataStore.opponentUserName;
        }
    }

    //Photon Serialize View
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       
    }
}
