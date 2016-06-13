using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    //bool tracks if player is a network opponent or AI opponent
    public bool versusAi;
    //stored name for AI player
    string computerAiName = "Jarvis";
    //The text box to display the player's name
    public Text player1Username;
    public Text player2Username;
	//The text box that the enemie's health is placed
	private Text player2HealthTextBox;
	private Text player1HealthTextBox;
	//The health of the players
	int player1Health = 20;
	int player2Health = 20;

    //The panel and text box for when a player wins or loses
    public GameObject gameEndPanel;
    public Text gameEndText;
    //NETWORK COMPONENTS
    PhotonView photonView;


    // Use this for initialization
    void Start ()
    {
        Debug.Log("Running start function!!!!");
        Debug.Log("Spawning player object");
       
       

        //get this manager's photon view
        photonView = GetComponent<PhotonView>();
        StartCoroutine("TimedCall");
        //if this is the 2nd player joining
        /*
        if (!PhotonNetwork.isMasterClient)
        {
            photonView.RPC("SpawnPlayers", PhotonTargets.All);
        }
        */
        //PhotonNetwork.Instantiate("PlayerController", Vector3.zero, Quaternion.identity, 0);
        ///COMMENTED OUT TEMPORARILY SINCE PLAYERCONTROLLERS WILL HANDLE THIS
        /*
        player1HealthTextBox = GameObject.Find("Player1HealthBox").GetComponent<Text>();
        player1HealthTextBox.text = "Life: " + 20;
        player2HealthTextBox = GameObject.Find("Player2HealthBox").GetComponent<Text>();
        player2HealthTextBox.text = "Enemy Life: " + 20;
        */

        //set your username to the stored username on playfabdatastore
        player1Username.text = PlayFabDataStore.userName;
        //if there is another player in the room
        if (PhotonNetwork.room != null && PhotonNetwork.room.maxPlayers == 2)
        {
            //set bool to indicate real player
            versusAi = false;
            //set username to opponents username           
           player2Username.text = PlayFabDataStore.opponentUserName;
        }
        else // if the opponent is a computer controlled player
        {
            //set bool to indicate computer player
            versusAi = true;
            //set username to AI username
            player2Username.text = computerAiName;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        
    }
    //TEMP CALL TO SPAWN PLAYERS
    IEnumerator TimedCall()
    {
        
        yield return new WaitForSeconds(2);
        SpawnPlayers();
      
        yield return null;
    }
    ///////////////////////////
    public void SpawnPlayers()
    {
        PhotonNetwork.Instantiate("PlayerController", Vector3.zero, Quaternion.identity, 0);
    }
	//Method for when a damage card is done casting
	public void dealDamage(int damage, int playerID)
	{
		//If the card belongs to player 1
		if (playerID == 1) 
		{
			//Subtract the health of player 2
			player2Health -= damage;
			Debug.Log ("Player 2 Health: " + player2Health);
            //COMMENTED OUT TEMPORARILY
			//Update the text box
			//player2HealthTextBox.text = "Enemy Life: " + player2Health;
		} 
		//If the card belongs to player 2
		else 
		{
			player1Health -= damage;
			Debug.Log ("Player 1 Health: " + player1Health);
            //COMMENTED OUT TEMPORARILY
           // player1HealthTextBox.text = "Life: " + player1Health;
		}
	}

	public void healPlayer(int healAmount, int playerID)
	{
		if (playerID == 1) 
		{
			player1Health += healAmount;
			player1HealthTextBox.text = "Life: " + player1Health;
		} 
		else 
		{
			player2Health += healAmount;
			player2HealthTextBox.text = "Enemy Life: " + player2Health;
		}

	}
    
    public void Win()
    {
        Debug.Log("You Win!!");
        gameEndPanel.SetActive(true);
        gameEndText.text = "You Win!";
        
    }

    public void Lose()
    {
        Debug.Log("You Lost");
        gameEndPanel.SetActive(true);
        gameEndText.text = "You Lost";
      
    }

    //Handle the player quitting or crashing mid game
    void OnApplicationQuit()
    {
        //notify other player that they won
        photonView.RPC("Win", PhotonTargets.Others);

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

	}
    
}