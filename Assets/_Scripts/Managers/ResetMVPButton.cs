using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class ResetMVPButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void resetMVP()
	{
		SceneManager.LoadScene (0);
	}
}
