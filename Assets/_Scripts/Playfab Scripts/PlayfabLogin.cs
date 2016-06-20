using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabLogin : MonoBehaviour
{
   
    //input field where the user inputs their email address
    public InputField loginEmailField;
    //input field where the user inputs their password
    public InputField loginPasswordField;
    //input field where the user inputs their email address for registration
    public InputField registerEmailField;
    //input field where the user inputs their username for registration
    public InputField registerUsernameField;
    //input field where the user inputs their password for registration
    public InputField registerPasswordField;

    //panel for login UI
    public GameObject loginPanel;
    //Error Text for Login
    public Text loginError;

    //panel for registration UI
    public GameObject registerPanel;
    //Error Text for Registration
    public Text registerError;

    //panel to notify player that game is connecting
    public GameObject connectPanel;
    //panel for main menu UI
    public GameObject mainMenuPanel;
    //Text for the name of the user currently logged in
    public Text accountName;
    //Panel for searching for game
    public GameObject searchPanel;
    //Text for searching progress
    public Text searchText;
    //The number of seconds to wait for an opponent before returning to main menu
    int searchingTime = 120;
    //check if search has been cancelled
    bool searchCancelled = false;

    void Awake()
    {
        PlayFabSettings.TitleId = "57F4";
        
        //make sure login panel is the only active panel
        registerPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        searchPanel.SetActive(false);
        //check playerpref storage for rapid login
        if(PlayerPrefs.HasKey("SavedEmail") && PlayerPrefs.HasKey("SavedPassword"))
        {
            //login automatically, skip login page
            //offer option to log out later
            loginPanel.SetActive(false);
            //display connecting panel
            connectPanel.SetActive(true);
            PlayFabLogin(PlayerPrefs.GetString("SavedEmail"), PlayerPrefs.GetString("SavedPassword"));
        }
        else
        {
            loginPanel.SetActive(true);
        }
        

    }
    void Update()
    {
        
    }
  
    //called when player clicks the new account button
    //sets login panel inactive and activates registration panel
    public void NewAccount()
    {
        //set error text to blank
        registerError.text = "";
        loginError.text = "";
        //switch panels
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }
    public void BackToLogin()
    {
        //set error text to blank
        registerError.text = "";
        loginError.text = "";
        //switch panels
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
    }
    //calls the PlayfabLogin function in PlayFabApiCalls
    //Sends data from username and password input fields
    public void Login()
    {
        PlayFabLogin(loginEmailField.text, loginPasswordField.text);
    }

    //calls the PlayfabRegister function in PlayFabApiCalls
    //Sends data from username,password, and email input fields
    public void Register()
    {
       PlayFabRegister(registerUsernameField.text, registerPasswordField.text, registerEmailField.text);
    }
    /// <summary>
    /// Returns the player from the menu to the login page
    /// WIP: does not currently log you out
    /// </summary>
    public void Logout()
    {
        //Disconnect from Photon Server
        PhotonNetwork.Disconnect();

        //Delete all saved login data
        PlayerPrefs.DeleteKey("SavedEmail");
        PlayerPrefs.DeleteKey("SavedPassword");
        //set error text to blank
        registerError.text = "";
        loginError.text = "";
        //set other panels inactive
        mainMenuPanel.SetActive(false);
        registerPanel.SetActive(false);
        //reactivate login panel 
        //to allow login as other user/register a new user
        loginPanel.SetActive(true);
    }
    /// <summary>
    /// Call to load main menu panel on successful login
    /// </summary>
    public void MainMenu()
    {
        //set other panels inactive
        registerPanel.SetActive(false);
        loginPanel.SetActive(false);
        //display connecting panel
        connectPanel.SetActive(true);
        //wait until network is connected
        StartCoroutine(WaitForConnection());
       
        /*
        //hide connecting panel
        connectPanel.SetActive(false);
        //set main menu panel active
        mainMenuPanel.SetActive(true);
        */
       
    }
    //Wait for a set number of seconds in a room for an opponent before leaving
    IEnumerator WaitForConnection()
    {
        //wait here until player has joined their own room
        while (PhotonNetwork.connected == false)
        {
            if (PhotonNetwork.connected == true)
            { break; }
            yield return null;
        }
        //set main menu panel active
        connectPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        //Set photon nickname to player username
        PhotonNetwork.player.name = PlayFabDataStore.userName;
        //set main menu account name to the player's username
        accountName.text = "User: " + PlayFabDataStore.userName;
        Debug.Log("PlayFabDataStore Username: "+PlayFabDataStore.userName);
        Debug.Log("Photon Username: " + PhotonNetwork.player.name);
        yield return null;
    }
    //begin searching for an opponent
    public void SearchForOpponent()
    {
        //set search panel active
        mainMenuPanel.SetActive(false);
        searchPanel.SetActive(true);
        //Indicate that search is happening
        searchText.text = "Searching For Opponent...";
        //search for a random room to join, fails if room is not found
        //jumps to OnPhotonRandomJoinFailed()
        Debug.Log("Attempting to Join Room");
        PhotonNetwork.JoinRandomRoom();    
    }
    public void ComputerOpponent()
    {
        //set search panel active
        mainMenuPanel.SetActive(false);
        searchPanel.SetActive(true);
        //Indicate that search is happening
        searchText.text = "Preparing AI Opponent...";

        //set up room options
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.maxPlayers = 1;//set max players to 1
        roomOptions.isVisible = false;//make the room unjoinable by other players
        //create a room with random name, the previous options, and no specification for typed lobby
        PhotonNetwork.CreateRoom(null, roomOptions, null);// this can make room name same as username(PlayFabDataStore.userName);
    }
    //when player presses deck builder button on main menu
    public void DeckBuilder()
    {
        PhotonNetwork.LoadLevel("DeckBuilder");
    }
    //if the player fails to join a random room
    public void OnPhotonRandomJoinFailed()
    {
        Debug.Log("No Room Found");
        Debug.Log("Creating a new room");
        //set up room options
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.maxPlayers = 2;//set max players to 2
        //create a room with random name, the previous options, and no specification for typed lobby
        PhotonNetwork.CreateRoom(null, roomOptions, null);// this can make room name same as username(PlayFabDataStore.userName);
        //Start Waiting for user to join
        StartCoroutine(WaitForOpponent());

    }
    //upon  this player joining a new room, output the room name
    public void OnJoinedRoom()
    {
        Debug.Log("Join Room Successfully!");
        Debug.Log("Room name is: " + PhotonNetwork.room);
        //if this player is the room owner and this is a single player room
        if (PhotonNetwork.isMasterClient && PhotonNetwork.room.maxPlayers == 1)
        {
            //jump to gameplay scene immediately
            StartCoroutine(BeginGame());
        }
        //else if this is the joining player
        else if(!PhotonNetwork.isMasterClient)
        {
            searchText.text = "Opponent Found! \n "+ PhotonNetwork.otherPlayers[0].name;
            Debug.Log(PhotonNetwork.otherPlayers);
            Debug.Log("Opponent is: " + PhotonNetwork.otherPlayers[0].name);
            PlayFabDataStore.opponentUserName = PhotonNetwork.otherPlayers[0].name;
            //jump to gameplay scene
            StartCoroutine(BeginGame());


        }
       
    }
    //on another player joining this room
    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        PlayFabDataStore.opponentUserName = newPlayer.name;
       Debug.Log("New Player Joined Room: " + newPlayer.name);
        
    }
    IEnumerator BeginGame()
    {
        //wait so user can read success indication
        yield return new WaitForSeconds(2);
        PhotonNetwork.LoadLevel("Test");
      
        yield return null;
    }
    //Wait for a set number of seconds in a room for an opponent before leaving
    IEnumerator WaitForOpponent()
    {
        //wait here until player has joined their own room
        while(PhotonNetwork.room == null)
        {
            if(PhotonNetwork.room != null)
            { break; }
            yield return null;
        }
        Debug.Log(PhotonNetwork.room);
        //begin timer for searching
        Debug.Log("Waiting for an opponent");
        for(int i = 0; i <= searchingTime; i++)
        {
            //if search is cancelled, end the coroutine
            if(searchCancelled == true)
            {
                searchCancelled = false;
                yield break;
            }
            Debug.Log("Time Elapsed: "+i);
            
            //if you are in a room and another player has joined
            if(PhotonNetwork.room != null && (PhotonNetwork.room.playerCount > 1))
            {
                Debug.Log("Player Found");
                searchText.text = "Opponent Found!\n " + PhotonNetwork.otherPlayers[0].name;
               
                //if this player is the room owner
                if (PhotonNetwork.isMasterClient)
                {
                    //jump to gameplay scene
                    StartCoroutine(BeginGame());
                }
                //exit this function, level will load
                yield break;
            }
            //wait 1 second each iteration
            yield return new WaitForSeconds(1);
        }
        Debug.Log("Failed to find opponent");
        //leave current room (room is empty because no one joined)
        PhotonNetwork.LeaveRoom();
        //indicate that a player was not found 
        searchText.text = "Opponent Not Found";
        //wait so user can read failure indication
        yield return new WaitForSeconds(3);
        //set main menu panel active
        searchPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        yield return null;
    }
    public void EndSearch()
    {
        searchCancelled = true;
        Debug.Log("Cancelled search");
        //leave current room (room is empty because no one joined)
        if (PhotonNetwork.inRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        //indicate that a player was not found 
        searchText.text = "Opponent Not Found";
        //wait so user can read failure indication
        
        //set main menu panel active
        searchPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    /// <summary>
    /// Registers a new user on playfab website
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="email"></param>
    public void PlayFabRegister(string username, string password, string email)
    {
        var request = new RegisterPlayFabUserRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Username = username,
            Password = password,
            Email = email,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, (result) =>
        {
            PlayFabDataStore.playFabId = result.PlayFabId;
            PlayFabDataStore.sessionTicket = result.SessionTicket;
            PlayFabDataStore.userName = username;
            PlayFabDataStore.email = email;
            Debug.Log("New Account Created!");
            //save user data for rapid login
            ChangeSavedLogin(email, password);
            //Initialize cloud script
            PlayfabApiCalls.PlayFabInitialize();
            //get photon token
            GetPhotonToken();
            //set main menu active on successful login
            MainMenu();

        }, (error) =>
        {
            Debug.Log("New Account Creation Failed!");
            Debug.Log(error.ErrorMessage.ToString());
            //output error to error text
            registerError.text = error.ErrorMessage.ToString();

        });
    }


    /// <summary>
    /// Login to a pre registered user on playfab website
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    public void PlayFabLogin(string email, string password)
    {
        var request = new LoginWithEmailAddressRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Email = email,
            Password = password
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, (result) =>
        {
            Debug.Log("Login Successful!");

            PlayFabDataStore.playFabId = result.PlayFabId;
            PlayFabDataStore.email = email;
            
           
            PlayFabDataStore.sessionTicket = result.SessionTicket;
            //save user data for rapid login
            ChangeSavedLogin(email, password);
            //Initialize cloud script
            PlayfabApiCalls.PlayFabInitialize();
            //get photon token
            GetPhotonToken();

            //get account info (username) to save in PlayFabDataStore
            //jumps to main menu in GetAccountInfo to avoid jumping before retrieving data
            GetAccountInfo(email);

            //Retrieve player decks
            PlayfabApiCalls.RetrieveDecks(PlayFabDataStore.playFabId);
            
        }, (error) =>
        {
            Debug.Log("Login Failed!");
            Debug.Log(error.ErrorMessage.ToString());
            //output error to error text
            loginError.text = error.ErrorMessage.ToString();
        });
    }

    public void GetAccountInfo(string email)
    {
        var request = new GetAccountInfoRequest()
        {
            PlayFabId = PlayFabDataStore.playFabId,
            Email = email,

        };

        PlayFabClientAPI.GetAccountInfo(request, (result) =>
        {
            Debug.Log("Info Retrieval Successful!");
            PlayFabDataStore.userName = result.AccountInfo.Username.ToString();
            //set main menu active on successful login
            MainMenu();


        }, (error) =>
        {
            Debug.Log("Retrieval Failed!");
            Debug.Log(error.ErrorMessage.ToString());

        });
    }
    //modify saved login info
    public static void ChangeSavedLogin(string email, string password)
    {
        //store email and password into playerprefs
        PlayerPrefs.SetString("SavedEmail", email);
        PlayerPrefs.SetString("SavedPassword", password);
    }

    //Get Photon Token from playfab
    public static void GetPhotonToken()
    {

        var request = new GetPhotonAuthenticationTokenRequest();
        {
            request.PhotonApplicationId = "ea176371-3496-4c4d-b800-e7ff315cd30e".Trim();
        }

        PlayFabClientAPI.GetPhotonAuthenticationToken(request, (result) =>
        {
            string photonToken = result.PhotonCustomAuthenticationToken;
            Debug.Log(string.Format("logged in in session token: {0}", photonToken));
            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
            PhotonNetwork.AuthValues.AddAuthParameter("username", PlayFabDataStore.playFabId);
            PhotonNetwork.AuthValues.AddAuthParameter("Token", result.PhotonCustomAuthenticationToken);
            PhotonNetwork.AuthValues.UserId = PlayFabDataStore.playFabId;
            PhotonNetwork.ConnectUsingSettings("1.0");
            //make sure all players are  NOT synced to same scene in same room
            PhotonNetwork.automaticallySyncScene = false;
            //Set photon nickname to player username
            PhotonNetwork.player.name = PlayFabDataStore.userName;
          

        }, (error) =>
        {
            Debug.Log("Photon Connection Failed! " + error.ErrorMessage.ToString());
        });

    }




}
