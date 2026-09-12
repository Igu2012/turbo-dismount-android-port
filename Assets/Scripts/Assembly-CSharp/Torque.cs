#pragma warning disable 0618,0619
using UnityEngine;

public class Torque : MonoBehaviour
{
	public float torque;

	private void Awake()
	{
		base.GetComponent<Rigidbody>().maxAngularVelocity = 100f;
	}

	private void FixedUpdate()
	{
		base.GetComponent<Rigidbody>().AddTorque(base.transform.right * torque, ForceMode.Force);
	}
}
