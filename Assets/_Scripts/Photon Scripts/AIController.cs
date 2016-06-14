using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AIController : MonoBehaviour
{

   
    GameManager gameManager;
    //PLAYER STATS
    int startingHealth = 20;
    int health;

    //The text box that the controller's health is placed
    private Text healthTextBox;


    // Use this for initialization
    void Start()
    {
       
        health = startingHealth;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
      
       
        //this controller belongs to the AI opponent
        gameObject.name = "NetworkOpponent";
        //set the health text box of this controller to the player2healthbox
        healthTextBox = GameObject.Find("Player2HealthBox").GetComponent<Text>();
        
        //set initial text for health text box
        healthTextBox.text = "Life: " + startingHealth;
    }

    // Update is called once per frame
    void Update()
    {
        //keep health text updated to current health
        healthTextBox.text = "Life: " + health;


        //if health is 0 or less
        if (health <= 0)
        {
            //the NPC loses
             Lose();   
        }

    }
    void Lose()
    {
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
   
    public void ChangeHealth(int amount)
    {
        //this should never happen, but if it does, return immediately
        if (amount == 0)
        { return; }


        //HEALING
        //if the amount is positive (if this is healing)
        if (amount > 0)
        {
            
                health += amount;
            
        }
        //DAMAGE
        //if the amount is negative (if this is damage)
        else if (amount < 0)
        {

                health += amount;
           
        }

    }



   
}
