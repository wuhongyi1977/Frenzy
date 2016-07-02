using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DraggableCard : MonoBehaviour {

    private string Name;
    private string itemId;
    private string instanceId;

    bool isOverScrollView = false;
    bool isDraggable = false;
    bool dropped = false;
    Vector3 origPos;
    Vector3 screenPoint;
    Vector3 offset;

    public GameObject sceneCanvas;
    DeckContentsScrollView deckContents;
    CardCollectionScrollView collectionContents;

    public Text nameText;
    //bool to check if the card was in the list of cards within a deck
    public bool inDeck = false;

	// Use this for initialization
	void Start ()
    {
        sceneCanvas = GameObject.Find("Canvas");
        isDraggable = true;
        origPos = gameObject.transform.position;
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        //find whatever scroll contents havent been assigned
        if (deckContents == null)
        {
            SetDeckScrollView(GameObject.Find("DeckScrollView").GetComponent<DeckContentsScrollView>());
        }
        if(collectionContents == null)
        {
            SetCollectScrollView(GameObject.Find("CollectionScrollView").GetComponent<CardCollectionScrollView>());
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        //check if card's position is near deck list
        //since position of scrollview is from top right corner, offset added (-5 goes to edge of list)
        if(transform.position.x > deckContents.transform.position.x - 5f )//change this to check position over scroll view
        {
            isOverScrollView = true;
           // Debug.Log("Over scroll view");
           
        }
        else
        {
            isOverScrollView = false;
           // Debug.Log("NOT over view");
            
        }
	
	}
    public void SetName(string name)
    {
        Name = name;
        nameText.text = name;
    }
    public void SetId(string id)
    {
        itemId = id;
    }
    public string GetId()
    {
        return itemId;
    }
    public void SetInstanceId(string id)
    {
        instanceId = id;
    }
    public string GetInstanceId()
    {
        return instanceId;
    }
    //set this object's deck scroll view 
    public void SetDeckScrollView(DeckContentsScrollView scrollView)
    {
        deckContents = scrollView;
    }
    //set this object's collection scroll view 
    public void SetCollectScrollView(CardCollectionScrollView scrollView)
    {
        collectionContents = scrollView;
    }

    public void FollowMouse()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
       
    }
   
    //Registers that the player has clicked on the card
    public void OnMouseDown()
    {
        origPos = gameObject.transform.position;
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        //remove from scrollview
        transform.SetParent(sceneCanvas.transform, false);
        //if the card was in the deck list already
        if (inDeck)
        {
            //Remove card from list while dragging 
            deckContents.RemoveCardFromDeck(itemId);
        }

    }
    //Registers that the player has let go of the card
    public void OnMouseUp()
    {
        
        //if the card is dropped off of the scroll contents
        if(!isOverScrollView)
        {
            //indicate that this card has now been dropped into deck
            inDeck = false;
            //destroy the card object
            //Destroy(this.gameObject);
            //set the parent of this card to be the deck content scroll list
            transform.SetParent(collectionContents.scrollContent.transform, false);
            gameObject.transform.position = origPos;
            

        }
        //if the card is dropped over the scroll contents, set position and add it to list 
        else
        {
            //indicate that this card has now been dropped into deck
            inDeck = true;
            //set the parent of this card to be the deck content scroll list
            transform.SetParent(deckContents.scrollContent.transform, false);
            //gameObject.transform.position = origPos;
            //Add card to list when dropped
            deckContents.AddCardToDeck(itemId);
        }
        
    }

    //Registers that the card is being dragged
    public void OnMouseDrag()
    {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            transform.position = curPosition;
    }

    
    
    //Registers what card is under the mouse
    public virtual void OnMouseOver()
    {
        //show a large, detailed version of card after a few seconds
        //Debug.Log("Show Details of: "+gameObject.name);
       
    }
    //Registers what card is under the mouse
    public virtual void OnMouseExit()
    {
        //hide large, detailed version of card
        //Debug.Log("Hide Details of: "+ gameObject.name);
    }
    
}
