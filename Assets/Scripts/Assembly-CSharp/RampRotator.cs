#pragma warning disable 0618,0619
using System;
using UnityEngine;

public class RampRotator : MonoBehaviour
{
	public Vector3 axis = Vector3.right;

	public float angleAmplitude = -20f;

	public float cycleLength = 2f;

	public float cycleOffset;

	private float cycleFrequency = 0.5f;

	private Vector3 originalLocalEulers = Vector3.zero;

	private void Awake()
	{
		originalLocalEulers = base.transform.localEulerAngles;
		if (cycleLength > 0f)
		{
			cycleFrequency = 1f / cycleLength;
		}
	}

	private void FixedUpdate()
	{
		float num = Mathf.Abs(Mathf.Sin((cycleOffset + cycleFrequency * Time.fixedTime) * (float)Math.PI)) * angleAmplitude;
		base.transform.localEulerAngles = originalLocalEulers + num * axis;
	}
}
