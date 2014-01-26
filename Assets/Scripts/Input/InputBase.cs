using UnityEngine;
using System.Collections;

public class InputBase : MonoBehaviour {

    public float dragSpeed = 2f;
    private Vector3 dragOrigin;

	void Translate()
	{
		if (Input.GetMouseButtonDown(0))
		{
			dragOrigin = Input.mousePosition;
			return;
		}
		
		if (!Input.GetMouseButton(0)) return;
		
		Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
		Vector3 move = new Vector3(pos.x * -dragSpeed, 0, pos.y * -dragSpeed);
		
		transform.Translate(move, Space.World);  
	}

	void Zoom()
	{
		var scroll = Input.GetAxis("Mouse ScrollWheel");

		if(scroll != 0f)
		{
			Debug.Log("Scroll: " + scroll.ToString("F"));

			Vector3 move = Camera.main.transform.forward;
			move *= scroll;

			transform.Translate(move, Space.World);

		}
	}

	void Update()
	{
		Translate();
		Zoom();
		
	}
	
	
}
