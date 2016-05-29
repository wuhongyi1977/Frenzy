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
    //panel for registration UI
    public GameObject registerPanel;
    //panel for main menu UI
    public GameObject mainMenuPanel;


    void Awake()
    {
        PlayFabSettings.TitleId = "57F4";
        //make sure login panel is the only active panel
        registerPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        //check playerpref storage for rapid login
        if(PlayerPrefs.HasKey("SavedEmail") && PlayerPrefs.HasKey("SavedPassword"))
        {
            //login automatically, skip login page
            //offer option to log out later
            loginPanel.SetActive(false);
            PlayFabLogin(PlayerPrefs.GetString("SavedEmail"), PlayerPrefs.GetString("SavedPassword"));
        }
        else
        {
            loginPanel.SetActive(true);
        }
        

    }
   
    //called when player clicks the new account button
    //sets login panel inactive and activates registration panel
    public void NewAccount()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }
    public void BackToLogin()
    {
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
        //PlayFabLogin(loginEmailField.text, loginPasswordField.text);
        mainMenuPanel.SetActive(false);
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
       

    }
    /// <summary>
    /// Call to load main menu panel on successful login
    /// </summary>
    public void MainMenu()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(false);
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
            Email = email
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
            //get photon token
            GetPhotonToken();
            //set main menu active on successful login
            MainMenu();

        }, (error) =>
        {
            Debug.Log("New Account Creation Failed!");

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
            //get photon token
            GetPhotonToken();
            //set main menu active on successful login
            MainMenu();
            

        }, (error) =>
        {
            Debug.Log("Login Failed!");
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
            //make sure all players are synced to same scene in same room
            PhotonNetwork.automaticallySyncScene = true;

        }, (error) =>
        {
            Debug.Log("Photon Connection Failed! " + error.ErrorMessage.ToString());
        });

    }




}
