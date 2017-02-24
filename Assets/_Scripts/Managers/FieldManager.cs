using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    private int numberOfSummonZones = 3;
    GameObject[] localSummonZones;
    GameObject[] opponentSummonZones; 

	// Use this for initialization
	void Start ()
    {
        localSummonZones = new GameObject[3];
        opponentSummonZones = new GameObject[3];
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
