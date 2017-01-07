using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTypeSelector : MonoBehaviour
{
    // TODO remove this later
    public void Awake()
    {    
        gameObject.AddComponent<SpellCard>();

    }

    public void SetCardType(string cardType)
    {
        switch (cardType)
        {
            case "Creature":
                gameObject.AddComponent<CreatureCard>();
                break;
            case "Spell":
                gameObject.AddComponent<SpellCard>();
                break;
            default:
                Debug.LogError("Card type does not exist!");
                break;
        }
       
    }
	
}
