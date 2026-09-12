#pragma warning disable 0618,0619
using UnityEngine;

public class Bobber : MonoBehaviour
{
	private Vector3 originalEulers;

	private Vector3 originalPosition;

	public Vector3 angularSpeed = Vector3.zero;

	public Vector3 bob = Vector3.zero;

	private float birth;

	private void Awake()
	{
		originalEulers = base.transform.eulerAngles;
		originalPosition = base.transform.position;
		birth = Time.time;
	}

	private void Update()
	{
		base.transform.eulerAngles = originalEulers + angularSpeed * (Time.time - birth);
		base.transform.position = originalPosition + bob * Mathf.Sin(Time.time - birth);
	}
}
