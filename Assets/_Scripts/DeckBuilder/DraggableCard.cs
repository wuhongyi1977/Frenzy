using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DraggableCard : MonoBehaviour {

    private string Name;
    private string itemId;

    bool isOverScrollView = false;
    bool isDraggable = false;
    bool dropped = false;
    Vector3 origPos;
    Vector3 screenPoint;
    Vector3 offset;

    public GameObject sceneCanvas;
    DeckContentsScrollView deckContents;

    public Text nameText;
	// Use this for initialization
	void Start ()
    {
        sceneCanvas = GameObject.Find("Canvas");
        isDraggable = true;
        origPos = gameObject.transform.position;
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        if(deckContents == null)
        {
            SetScrollView(GameObject.Find("ScrollDeckContent").GetComponent<DeckContentsScrollView>());
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(false)//change this to check position over scroll view
        {
            isOverScrollView = true;
        }
        else
        {
            isOverScrollView = false;
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
    public void SetScrollView(DeckContentsScrollView scrollView)
    {
        deckContents = scrollView;
    }
    

   
    //Registers that the player has clicked on the card
    public void OnMouseDown()
    {
        origPos = gameObject.transform.position;
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        //remove from scrollview
        transform.SetParent(sceneCanvas.transform, false);
        //Remove card from list while dragging 
        deckContents.RemoveCardFromDeck(itemId);

    }
    //Registers that the player has let go of the card
    public void OnMouseUp()
    {
        
        //if the card is dropped off of the scroll contents
        if(!isOverScrollView)
        {
            Destroy(this.gameObject);
        }
        //if the card is dropped over the scroll contents, set position and add it to list 
        else
        {
            //set the parent of this card to be the deck content scroll list
            transform.SetParent(deckContents.scrollContent.transform, false);
            gameObject.transform.position = origPos;
            //Remove card from list while dragging 
            deckContents.AddCardToDeck(itemId);
        }
        
    }

    //Registers that the card is being dragged
    public void OnMouseDrag()
    {
       // if (isDraggable == true)
       // {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            transform.position = curPosition;
       // }
    }
    /*
    //Registers what card is under the mouse
    public virtual void OnMouseOver()
    {
        Debug.Log(gameObject.name);
       
    }
    //Registers what card is under the mouse
    public virtual void OnMouseExit()
    {
        Debug.Log("test");
    }
    */
}
