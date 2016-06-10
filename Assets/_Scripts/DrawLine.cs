using UnityEngine;
using System.Collections;

public class DrawLine : MonoBehaviour {
	private Vector3 pos1, pos2;
	private float objectHeight = 2.0f;
	private Vector3 localScale;
	public bool isDrawing;
	// Use this for initialization
	void Start () {
		isDrawing = false;
	}

	// Update is called once per frame
	void Update () {
		if (isDrawing) {
			if (Input.GetMouseButtonDown (0)) {
			
				pos1 = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 0.5f);
				pos1 = Camera.main.ScreenToWorldPoint (pos1);
				pos2 = pos1;
			}


			if (Input.GetMouseButton (0)) {
				pos2 = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 0.5f);
				pos2 = Camera.main.ScreenToWorldPoint (pos2);
			}


			if (pos2 != pos1) {
				gameObject.GetComponent<Renderer> ().material.color = Color.black;
				Vector3 v3 = pos2 - pos1;
				gameObject.transform.position = pos1 + (v3) / 2.0f;
				localScale = gameObject.transform.localScale;
				localScale.y = v3.magnitude / objectHeight;
				gameObject.transform.localScale = localScale;
				gameObject.transform.rotation = Quaternion.FromToRotation (Vector3.up, v3);
			}
			if (!Input.GetMouseButton (0))
				gameObject.GetComponent<Renderer> ().material.color = Color.clear;
		}
	}
	public void makeLineInvisible()
	{
		gameObject.GetComponent<Renderer> ().material.color = Color.clear;
	}
}