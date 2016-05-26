using UnityEngine;
using System.Collections;

public class PlayFabManager : MonoBehaviour {
    /*
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
            PlayFabUserLogin.playFabUserLogin.login.gameObject.SetActive(false);
            PlayFabUserLogin.playFabUserLogin.mainMenu.gameObject.SetActive(true);
            PlayFabDataStore.playFabId = result.PlayFabId;
            PlayFabDataStore.userName = username;
            PlayFabDataStore.sessionTicket = result.SessionTicket;
            UpdateUserStatistic("AllTime");
            GetPhotonToken();
        }, (error) =>
        {
            Debug.Log("Login Failed!");
            PlayFabUserLogin.playFabUserLogin.errorText.text = error.ErrorMessage.ToString();
        });
    }
    */
    // Use this for initialization
    void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
