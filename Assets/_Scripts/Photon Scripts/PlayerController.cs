using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    PhotonView photonView;
    GameManager gameManager;
    //PLAYER STATS
    int startingHealth = 20;
    int health;

	// Use this for initialization
	void Start ()
    {
        photonView = GetComponent<PhotonView>();
        health = startingHealth;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //if this controller belongs to the local player
        if(photonView.isMine)
        {
            gameObject.name = "LocalPlayer";
        }
        else //if this controller belongs to the opponent
        {
            gameObject.name = "NetworkOpponent";
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        //if this is the local player
        if(photonView.isMine && health <= 0)
        {
            Lose();
            //photonView.RPC("ChatMessage", PhotonTargets.All, "jup", "and jup!");
        }
        else if(!photonView.isMine && health <= 0) // if this is the opponent
        {
            Win();
        }
	}
    void Lose()
    {
        Debug.Log("YOU LOST!!!");
        //call lose function on gamemanager
        //displays ending panel and message
        gameManager.Lose();
        //wait for a set number of seconds before returning to menu
        StartCoroutine("FinishingGame");
    }
    void Win()
    {
        Debug.Log("YOU WIN!!!");
        //call lose function on gamemanager
        //displays ending panel and message
        gameManager.Win();
        //wait for a set number of seconds before returning to menu
        StartCoroutine("FinishingGame");
    }

    IEnumerator FinishingGame()
    {
        yield return new WaitForSeconds(3);
        //leave the current room
        PhotonNetwork.LeaveRoom();

    }
    /*
    [PunRPC]
    void ChangeHealth(int amount)
    {
        health += amount;
    }
    
   
    [PunRPC]
    void Win(int amount)
    {
        health += amount;
    }
    */
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            //sync health
            stream.SendNext(health);

        }
        else
        {
            //Network player, receive data          
           health = (int)stream.ReceiveNext();
        }
       
    }
}
