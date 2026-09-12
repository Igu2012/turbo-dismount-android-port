#pragma warning disable 0618,0619
using UnityEngine;

public class RotateRigid : MonoBehaviour
{
	public Vector3 angularVelocity;

	private void Start()
	{
		base.GetComponent<Rigidbody>().angularVelocity = angularVelocity;
	}
}
