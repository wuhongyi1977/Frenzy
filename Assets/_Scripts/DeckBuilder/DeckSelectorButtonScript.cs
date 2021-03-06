﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeckSelectorButtonScript : MonoBehaviour
{

    private string Name;
    private string itemId;
    public Text ButtonText;
    public DeckSelectionScrollView scrollView;

    public void SetName(string name)
    {
        Name = name;
        ButtonText.text = name;
    }
    public void SetId(string id)
    {
        itemId = id;
    }
    public void ButtonClick()
    {
        scrollView.ButtonClicked(itemId, Name);
    }
}
