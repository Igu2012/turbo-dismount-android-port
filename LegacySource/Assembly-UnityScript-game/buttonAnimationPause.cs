using System;
using System.Collections;
using Boo.Lang.Runtime;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
[RequireComponent(typeof(GUITexture))]
public class buttonAnimationPause : MonoBehaviour
{
	public Animation sampleAnimation;

	public float animationSpeed;

	public float animationSpeedTarget;

	public float pauseSpeed;

	public buttonAnimationPause()
	{
		animationSpeed = 1f;
		animationSpeedTarget = 1f;
		pauseSpeed = 10f;
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		if (Input.GetMouseButtonDown(0) && guiTexture.HitTest(Input.mousePosition))
		{
			if (!(animationSpeedTarget <= 0.9f))
			{
				animationSpeedTarget = 0f;
			}
			else if (!(animationSpeedTarget >= 0.1f))
			{
				animationSpeedTarget = 1f;
			}
		}
		animationSpeed = Mathf.Lerp(animationSpeed, animationSpeedTarget, Time.deltaTime * pauseSpeed);
		IEnumerator enumerator = UnityRuntimeServices.GetEnumerator(sampleAnimation);
		while (enumerator.MoveNext())
		{
			object obj = enumerator.Current;
			if (!(obj is AnimationState))
			{
				obj = RuntimeServices.Coerce(obj, typeof(AnimationState));
			}
			AnimationState animationState = (AnimationState)obj;
			animationState.speed = animationSpeed;
			UnityRuntimeServices.Update(enumerator, animationState);
		}
	}

	public virtual void Main()
	{
	}
}
