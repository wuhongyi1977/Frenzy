using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeckButtonScript : MonoBehaviour
{

    private string Name;
    public Text ButtonText;
    public DeckBuilderScrollView scrollView;

    public void SetName(string name)
    {
        Name = name;
        ButtonText.text = name;
    }
    public void ButtonClick()
    {
       scrollView.ButtonClicked(Name);

    }
}