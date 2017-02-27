using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public delegate void GameNotification();
    public static event GameNotification GameStart;

    PhotonView photonView;

    bool gameStarts = false;

    //child of GameManager, assigned in inspector
    public Canvas FieldUICanvas;

    //bool tracks if player is a network opponent or AI opponent
    public bool versusAi;
    //stored name for AI player
    string computerAiName = "Jarvis";
 
	//The text box that the enemie's health is placed
	private Text player2HealthTextBox;
	private Text player1HealthTextBox;
	//The health of the players
	int player1Health = 20;
	int player2Health = 20;

    //The panel and text box for when a player wins or loses
    public GameObject gameNotifyPanel;
    public Text gameNotifyText;

    int playersInScene = 0;

    // Use this for initialization
    void Start ()
    {

        photonView = GetComponent<PhotonView>();
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.InstantiateSceneObject("FieldManager", transform.position, Quaternion.identity, 0, null);
        }


        Debug.Log("Running start function!!!!");
        Debug.Log("Spawning player object");

        gameNotifyPanel.SetActive(true);
        gameNotifyText.text = "Preparing Field...";
 
        //if there is another player in the room
        if (PhotonNetwork.room != null && PhotonNetwork.room.maxPlayers == 2)
        {
            //set bool to indicate real player
            versusAi = false;
          
        }
        else // if the opponent is a computer controlled player
        {
            //set bool to indicate computer player
            versusAi = true;
        }

        //spawn player
        if(!PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.Instantiate("PlayerController", Vector3.zero, Quaternion.identity, 0);
            photonView.RPC("SpawnPlayer", PhotonTargets.Others);
        }
        
        
    }

    private void Update()
    {
        
        if( gameStarts == false && GameObject.Find("NetworkOpponent"))
        {
            gameStarts = true;
            StartCoroutine(BeginGame());
        }
    }


    // only master will call this
    [PunRPC]
    void SpawnPlayer()
    {
        PhotonNetwork.Instantiate("PlayerController", Vector3.zero, Quaternion.identity, 0);
    }

    IEnumerator BeginGame()
    {
        yield return new WaitForSeconds(3);
        gameNotifyText.text = "Fight!";
        yield return new WaitForSeconds(0.5f);
        gameNotifyPanel.SetActive(false);
        //display begin
        GameStart();
        yield return null;
    }
    //Method for when a damage card is done casting
    public void dealDamage(int damage, int playerID)
	{
		//If the card belongs to player 1
		if (playerID == 1) 
		{
			//Subtract the health of player 2
			player2Health -= damage;
		} 
		//If the card belongs to player 2
		else 
		{
			player1Health -= damage;
		}
	}

	public void healPlayer(int healAmount, int playerID)
	{
		if (playerID == 1) 
		{
			player1Health += healAmount;
		} 
		else 
		{
			player2Health += healAmount;
		}

	}
    
    public void Win()
    {
        Debug.Log("You Win!!");
        gameNotifyPanel.SetActive(true);
        gameNotifyText.text = "You Win! \n Returning To Menu";
        StartCoroutine("WaitForExit");
    }

    public void Lose()
    {
        Debug.Log("You Lost");
        gameNotifyPanel.SetActive(true);
        gameNotifyText.text = "You Lost \n Returning To Menu";
        StartCoroutine("WaitForExit");
      
    }
    public IEnumerator WaitForExit()
    {
        yield return new WaitForSeconds(4);
        ExitGame();
        yield return null;
    }
    public void ExitGame()
    {
        //leave the current room
        if(PhotonNetwork.inRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        PhotonNetwork.LoadLevel("Login");
    }
    //Handle the player quitting or crashing mid game
    void OnApplicationQuit()
    {
        //notify other player that they won
        //photonView.RPC("Win", PhotonTargets.Others);

        //if the player is connected to the network
        if (PhotonNetwork.connected)
        {
            //leave the current room
            PhotonNetwork.LeaveRoom();
            //disconnect from the server
            PhotonNetwork.Disconnect();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            

        }
        else
        {
            
        }

    }

}