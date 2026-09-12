#pragma warning disable 0618,0619
using System;
using Dismount;
using UnityEngine;

public class TestScene : MonoBehaviour
{
	public Transform capsule;

	private void Start()
	{
		Application.targetFrameRate = 120;
	}

	private void Update()
	{
		capsule.position = new Vector3(Mathf.Sin(TimeManager.time) * 3f, 2f, Mathf.Cos(TimeManager.time) * 3f);
		capsule.Rotate(Vector3.right, TimeManager.deltaTime * (float)Math.PI, Space.Self);
	}
}
