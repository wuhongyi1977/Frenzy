using UnityEngine;
using System.Collections;
using Photon;

public class PhotonCalls : PunBehaviour {

    
    //exits the current room
    public static void LeaveRoom()
    {

        PhotonNetwork.LeaveRoom();
    }
   
    //when the player leaves their current room, reenter the lobby
    public override void OnLeftRoom()
    {
        PhotonNetwork.JoinLobby();
    }
    
    
    
    
    
}
