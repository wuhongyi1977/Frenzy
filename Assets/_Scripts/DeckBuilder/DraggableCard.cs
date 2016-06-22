using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DraggableCard : MonoBehaviour {

    private string Name;
    private string itemId;

    bool isDraggable = false;
    bool dropped = false;
    Vector3 origPos;
    Vector3 screenPoint;
    Vector3 offset;

    public Text nameText;
	// Use this for initialization
	void Start ()
    {
        isDraggable = true;
        origPos = gameObject.transform.position;
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }
	
	// Update is called once per frame
	void Update ()
    {
	
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
    //Registers that the player has clicked on the card
    public void OnMouseDown()
    {
        if (isDraggable == true)
        {
            origPos = gameObject.transform.position;
            screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

        }
    }
    //Registers that the player has let go of the card
    public void OnMouseUp()
    {
        dropped = true;
        
    }

    //Registers that the card is being dragged
    public void OnMouseDrag()
    {
        if (isDraggable == true)
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            transform.position = curPosition;
        }
    }
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
}
