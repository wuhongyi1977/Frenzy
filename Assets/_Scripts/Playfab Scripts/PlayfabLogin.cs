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


    void Awake()
    {
        PlayFabSettings.TitleId = "57F4";
        //make sure login panel is the only active panel
        registerPanel.SetActive(false);

        //check playerpref storage for rapid login
        if(PlayerPrefs.HasKey("SavedLogin"))
        {
            //login automatically, skip login page
            //offer option to log out later
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
        PlayFabApiCalls.PlayFabLogin(loginEmailField.text, loginPasswordField.text);
    }

    //calls the PlayfabRegister function in PlayFabApiCalls
    //Sends data from username,password, and email input fields
    public void Register()
    {
        PlayFabApiCalls.PlayFabRegister(registerUsernameField.text, registerPasswordField.text, registerEmailField.text);
    }
}
