using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Population : MonoBehaviour 
{
	public Dictionary<int, float> sizeForAge = new Dictionary<int, float>() 
	{
		{1, 0.75f},
		{2, 1.1f},
		{3, 1.1f},
		{4, 1.0f},
		{5, 0.9f},
		{6, 0.8f},
		{7, 0.1f} //Dead
	};

	private int mAge;
	public int age {
		get { return mAge; }
		set { 
			mAge = value; 
			label.age = mAge; 
			var scale = sizeForAge[mAge];
			body.transform.localScale = new Vector3(scale, scale, scale);
		}
	}
	
	private God mOwner;
	public God owner {
		get { return mOwner; }
		set {
			if(mOwner != value)
			{
				mOwner = value;
				//label.color = mOwner.color;
				body.renderer.material.color = mOwner.color;
			}
		}
	}

	//Block used to track migration
	private LandscapeBlock mCurrentBlock;
	public LandscapeBlock currentBlock {
		get { return mCurrentBlock; }
		set {
			mCurrentBlock = value;
		}
	}

	private LandscapeBlock mTargetBlock;
	public LandscapeBlock targetBlock {
		get { return mTargetBlock; }
		set {
			mTargetBlock = value;
		}
	}

	//Graphical stuff
	public GameObject body;
	public AgeLabel label;

}
