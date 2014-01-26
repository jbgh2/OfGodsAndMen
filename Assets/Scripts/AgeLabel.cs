using UnityEngine;
using System.Collections;

public class AgeLabel : MonoBehaviour 
{
	private int mAge;
	public int age {
		get { return mAge; }
		set {
			mAge = value;
			textMesh.text = mAge.ToString();
		}
	}

	private Color mColor;
	public Color color {
		get { return mColor; }
		set {
			mColor = value;
			textMesh.color = mColor;
		}
	}

	public TextMesh textMesh;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.LookAt(Camera.main.transform.position);
	}
}
