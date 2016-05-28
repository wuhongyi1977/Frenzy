using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabApiCalls : MonoBehaviour {


    public static void PlayFabRegister(string username, string password, string email)
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
            Debug.Log("New Account Created!");
            //save user data for rapid login
            ChangeSavedLogin(email, password);
            //get photon token
            GetPhotonToken();
        }, (error) =>
        {
            Debug.Log("New Account Creation Failed!");
            
        });
    }


    //Login to playfab/game
    //RIGHT NOW, USERNAME IS BEING PASSED THE EMAIL. mAKE SURE THIS MATCHES UP PROPERLY
    public static void PlayFabLogin(string username, string password)
    {
        var request = new LoginWithPlayFabRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            //need an email, not a username
            Username = username,
            Password = password
        };

        PlayFabClientAPI.LoginWithPlayFab(request, (result) =>
        {
            Debug.Log("Login Successful!");

            PlayFabDataStore.playFabId = result.PlayFabId;
            PlayFabDataStore.userName = username;
            PlayFabDataStore.sessionTicket = result.SessionTicket;
            //save user data for rapid login
            ChangeSavedLogin(username, password);
            //get photon token
            GetPhotonToken();
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
