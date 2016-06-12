using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    PhotonView photonView;
    GameManager gameManager;
    //PLAYER STATS
    int startingHealth = 20;
    int health;

    //The text box that the controller's health is placed
    private Text healthTextBox;
    

    // Use this for initialization
    void Start ()
    {
        photonView = GetComponent<PhotonView>();
        health = startingHealth;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //if this controller belongs to the local player
        if(photonView.isMine)
        {
            //name this gameobject localplayer
            gameObject.name = "LocalPlayer";
            //set the health text box of this controller to the player1healthbox
            healthTextBox = GameObject.Find("Player1HealthBox").GetComponent<Text>();

        }
        else //if this controller belongs to the opponent
        {
            //name this gameobject networkopponent
            gameObject.name = "NetworkOpponent";
            //set the health text box of this controller to the player2healthbox
            healthTextBox = GameObject.Find("Player2HealthBox").GetComponent<Text>();
        }
        //set initial text for health text box
        healthTextBox.text = "Life: " + startingHealth;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //keep health text updated to current health
        healthTextBox.text = "Life: " + health;


        //if health is 0 or less
        if(health <= 0)
        {
            if (photonView.isMine)
            {
                Lose();
               
            }
            else if (!photonView.isMine) // if this is the opponent
            {
                Win();
            }
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
    //HANDLE DAMAGE OVER NETWORK
    [PunRPC]
    void ChangeHealth(int amount)
    {
        //this should never happen, but if it does, return immediately
        if(amount == 0)
        { return; }

        //HEALING
        //if the amount is positive (if this is healing)
        if(amount > 0)
        {
            //if this is the local player, add health
            if(photonView.isMine)
            {
                health += amount;
            }
        }
        //DAMAGE
        //if the amount is negative (if this is damage)
        else if(amount < 0)
        {
            //if this is the local player, add health
            //since damage is negative, it will be subtracted
            if (photonView.isMine)
            {
                health += amount;
            }
            //if this is the network opponent
            else if(!photonView.isMine)
            {
                //call the damage rpc on the opponent's local player
                photonView.RPC("ChangeHealth",PhotonTargets.Others,amount);
            }
        }
      
    }
    
   
   
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
