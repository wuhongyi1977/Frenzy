using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabLogin : MonoBehaviour
{
    //input field where the user inputs their email address
    public InputField loginUsernameField;
    //input field where the user inputs their password
    public InputField loginPasswordField;

    void Awake()
    {
        PlayFabSettings.TitleId = "57F4";
       
    }
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    //Login to playfab/game
    public static void PlayFabLogin(string username, string password)
    {
        var request = new LoginWithPlayFabRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Username = username,
            Password = password
        };

        PlayFabClientAPI.LoginWithPlayFab(request, (result) =>
        {
            Debug.Log("Login Successful!");
          
            PlayFabDataStore.playFabId = result.PlayFabId;
            PlayFabDataStore.userName = username;
            PlayFabDataStore.sessionTicket = result.SessionTicket;
           
            GetPhotonToken();
        }, (error) =>
        {
            Debug.Log("Login Failed!");
            Debug.Log(error.ErrorMessage.ToString());
        });
    }

    //Get Photon Token from playfab
    public static void GetPhotonToken()
    {
        /*
        var request = new GetPhotonAuthenticationTokenRequest();
        {
            request.PhotonApplicationId = "ee6881e0-06d0-4b7c-b552-a4087cd14926".Trim();
        }

        PlayFabClientAPI.GetPhotonAuthenticationToken(request, (result) =>
        {
            string photonToken = result.PhotonCustomAuthenticationToken;
            Debug.Log(string.Format("Yay, logged in in session token: {0}", photonToken));
            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
            PhotonNetwork.AuthValues.AddAuthParameter("username", PlayFabDataStore.playFabId);
            PhotonNetwork.AuthValues.AddAuthParameter("Token", result.PhotonCustomAuthenticationToken);
            PhotonNetwork.AuthValues.UserId = PlayFabDataStore.playFabId;
            PhotonNetwork.ConnectUsingSettings("1.0");
            //make sure all players are synced to same scene in same room
            PhotonNetwork.automaticallySyncScene = true;
            GetUserStatistic();
        }, (error) =>
        {
            Debug.Log("Photon Connection Failed! " + error.ErrorMessage.ToString());
        });
        */
    }
}
