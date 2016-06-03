using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
/// <summary>
/// Handles tab presses to switch input boxes
/// </summary>
public class InputNavigator : MonoBehaviour
{
    EventSystem system;

    void Start()
    {
        system = EventSystem.current;

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

            if (next != null)
            {

                InputField inputfield = next.GetComponent<InputField>();
                if (inputfield != null)
                    inputfield.OnPointerClick(new PointerEventData(system));  

                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }
            

        }
    }
}
