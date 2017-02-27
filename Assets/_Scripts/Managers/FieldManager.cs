using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{

    PhotonView photonView;

    // Event for field handler
    public delegate void FieldEvent(int viewId, bool success); //< receives the photon view from the event callback
    public static event FieldEvent OnEnter;
    public static event FieldEvent OnExit;

    /*
    public delegate void CounterEvent(int viewId, bool success); 
    public static event CounterEvent OnCounter;
    */

    private int opponentCounters = 0;
    private Queue<BaseCard> counterQueue = new Queue<BaseCard>();

    private int numberOfSummonZones = 3;
    int[] localSummonZonesIds;
    GameObject[] localSummonZonesCards;
    int[] opponentSummonZonesIds;  
    GameObject[] opponentSummonZonesCards;

    // Use this for initialization
    void Start ()
    {
        photonView = GetComponent<PhotonView>();
        localSummonZonesIds = new int[numberOfSummonZones];
        opponentSummonZonesIds = new int[numberOfSummonZones];
        localSummonZonesCards = new GameObject[numberOfSummonZones];
        opponentSummonZonesCards = new GameObject[numberOfSummonZones];
    }

    void OnEnable()
    {
        BaseCard.NotifyEnter += CardEnteredField;
        BaseCard.NotifyExit += CardLeftField;
        CardAbilityList.NotifyCounter += CounterCardPlayed;

    }

    void OnDisable()
    {
        BaseCard.NotifyEnter -= CardEnteredField;
        BaseCard.NotifyExit -= CardLeftField;
        CardAbilityList.NotifyCounter -= CounterCardPlayed;
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
    
    public void CounterCardPlayed(BaseCard counterCard)
    {
        //adds current counter to queue
        counterQueue.Enqueue(counterCard);
        photonView.RPC("IncreaseCounter", PhotonTargets.Others);
    }

    [PunRPC]
    public void IncreaseCounter()
    {
        opponentCounters += 1;
    }

    [PunRPC]
    public void CounterHasCast()
    {
        BaseCard successfulCounter = null;
        foreach(BaseCard counterCard in counterQueue)
        {
            successfulCounter = counterQueue.Dequeue();
            if (successfulCounter.currentCardState == BaseCard.cardState.InPlay)
            {
                successfulCounter.NotifyFieldManagerExit();
                return;
            }
        }
        
    }

    void CardEnteredField(int viewId, int zoneIndex)
    {
        GameObject card = PhotonView.Find(viewId).gameObject;
        // check if this card was cancelled by something else
        if(card.gameObject.GetPhotonView().isMine && opponentCounters > 0)
        {
            opponentCounters -= 1;
            photonView.RPC("CounterHasCast", PhotonTargets.Others);
            OnEnter(viewId, false);
        }
        else 
        {
            if (card.gameObject.GetPhotonView().isMine)
            {
                //add to zone index
                localSummonZonesIds[zoneIndex] = viewId;
                localSummonZonesCards[zoneIndex] = card;
            }
            else
            {
                //add to zone index
                opponentSummonZonesIds[zoneIndex] = viewId;
                opponentSummonZonesCards[zoneIndex] = card;
            }
            OnEnter(viewId, true);

        }
       

    }

    void CardLeftField(int viewId, int zoneIndex)
    {
        GameObject card = PhotonView.Find(viewId).gameObject;
        bool success = true;
       
        if (card.gameObject.GetPhotonView().isMine)
        {
            //add to zone index
            localSummonZonesIds[zoneIndex] = -1;
            localSummonZonesCards[zoneIndex] = null;
        }
        else
        {
            //add to zone index
            opponentSummonZonesIds[zoneIndex] = -1;
            opponentSummonZonesCards[zoneIndex] = null;
        }
        
        OnExit(viewId, success);
    }

    

    //returns all cards in play
    public List<int> GetAllCards()
    {
        List<int> allCardViewIds = new List<int>();
        for(int i = 0; i < numberOfSummonZones; i++)
        {
            if(localSummonZonesIds[i] != -1)
            {
                allCardViewIds.Add(localSummonZonesIds[i]);
            }
            if (opponentSummonZonesIds[i] != -1)
            {
                allCardViewIds.Add(localSummonZonesIds[i]);
            }
        }
        return allCardViewIds;
    }

    //returns all creature cards in play
    public List<int> GetAllCreatures()
    {
        List<int> allCardViewIds = new List<int>();
        for (int i = 0; i < numberOfSummonZones; i++)
        {
            if (localSummonZonesIds[i] != -1 && localSummonZonesCards[i].GetComponent<CreatureCard>() != null)
            {
                allCardViewIds.Add(localSummonZonesIds[i]);
            }
            if (opponentSummonZonesIds[i] != -1 && opponentSummonZonesCards[i].GetComponent<CreatureCard>() != null)
            {
                allCardViewIds.Add(localSummonZonesIds[i]);
            }
        }
        return allCardViewIds;
    }

    //returns all creature cards in play, owned by local player
    public List<GameObject> GetOwnCreatures()
    {
        List<GameObject> allCardGameObjects = new List<GameObject>();
        for (int i = 0; i < numberOfSummonZones; i++)
        {
            if (localSummonZonesCards[i] != null && !localSummonZonesCards[i].GetComponent<CreatureCard>().Equals(null))
            {
                allCardGameObjects.Add(localSummonZonesCards[i]);
            }
        }
        return allCardGameObjects;
    }

    //returns all creature cards in play, owned by opponent
    public List<int> GetOpponentCreatures()
    {
        List<int> allCardViewIds = new List<int>();
        for (int i = 0; i < numberOfSummonZones; i++)
        {
            if (opponentSummonZonesIds[i] != -1 && opponentSummonZonesCards[i].GetComponent<CreatureCard>() != null)
            {
                allCardViewIds.Add(localSummonZonesIds[i]);
            }
        }
        return allCardViewIds;
    }

    //Photon Serialize View
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       

    }
}
