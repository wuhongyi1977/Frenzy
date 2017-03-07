using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGameManager : MonoBehaviour
{

    private void Awake()
    {
        if(PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.InstantiateSceneObject("GameManager", transform.position, Quaternion.identity, 0, null);
        }
        
    }
}
