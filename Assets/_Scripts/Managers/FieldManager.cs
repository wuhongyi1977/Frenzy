using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    // Event for field handler
    public delegate void FieldEvent(int viewId, bool success); //< receives the photon view from the event callback
    public static event FieldEvent OnEnter;
    public static event FieldEvent OnExit;

    bool cancelEntrance = false;
    bool cancelExit = false;
    

    private int numberOfSummonZones = 3;
    GameObject[] localSummonZones;
    GameObject[] opponentSummonZones; 

	// Use this for initialization
	void Start ()
    {
        localSummonZones = new GameObject[3];
        opponentSummonZones = new GameObject[3];
    }

    void OnEnable()
    {
        BaseCard.NotifyEnter += CardEnteredField;
        BaseCard.NotifyExit += CardLeftField;
    }

    void OnDisable()
    {
        BaseCard.NotifyEnter -= CardEnteredField;
        BaseCard.NotifyExit -= CardLeftField;
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void CardEnteredField(int viewId, int zoneIndex)
    {       
        GameObject card = PhotonView.Find(viewId).gameObject;
        if(card.GetPhotonView().isMine)
        {
            Debug.Log("Photon view id " + viewId + " entered field");
        }
        bool success = true;
        // check if this card was cancelled by something else
        if(cancelEntrance)
        {
            cancelEntrance = false;
            success = false;
        }
        else 
        {
            //add to zone index
            localSummonZones[zoneIndex] = card;
           
        }
        OnEnter(viewId, success);

    }

    void CardLeftField(int viewId, int zoneIndex)
    {
        GameObject card = PhotonView.Find(viewId).gameObject;
        if (card.GetPhotonView().isMine)
        {
            Debug.Log("Photon view id " + viewId + " left field");
        }
        bool success = true;
        if(cancelExit)
        {
            cancelExit = false;
            success = false;
        }
        else
        {
            //add to zone index
            localSummonZones[zoneIndex] = null;
        }
        OnExit(viewId, success);
    }

    void CancelNextEntrance()
    {
        cancelEntrance = true;
    }

    void CancelNextExit()
    {
        cancelExit = true;
    }
}
