using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(buttonPosition))]
public class buttonSize : MonoBehaviour
{
	private buttonPosition buttonPositionScript;

	public float buttonSize;

	public buttonSize()
	{
		buttonSize = 1f;
	}

	public virtual void Start()
	{
		buttonPositionScript = (buttonPosition)GetComponent(typeof(buttonPosition));
	}

	public virtual void Update()
	{
		buttonPositionScript.buttonSize = buttonSize;
	}

	public virtual void Main()
	{
	}
}
