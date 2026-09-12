using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(GUITexture))]
public class buttonSetAnimation : MonoBehaviour
{
	public Animation animationChange;

	public string animationName;

	public string currentAnimation;

	public bool matchNormalizedTime;

	public buttonSetAnimation()
	{
		currentAnimation = "tPose";
		matchNormalizedTime = true;
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		if (Input.GetMouseButtonDown(0) && guiTexture.HitTest(Input.mousePosition))
		{
			animationChange.CrossFade(animationName);
			if (matchNormalizedTime)
			{
				animationChange[animationName].normalizedTime = animationChange[currentAnimation].normalizedTime;
			}
			transform.parent.BroadcastMessage("SetCurrentAnimation", animationName);
		}
	}

	public virtual void SetCurrentAnimation(string newAnimationName)
	{
		currentAnimation = newAnimationName;
	}

	public virtual void Main()
	{
	}
}
