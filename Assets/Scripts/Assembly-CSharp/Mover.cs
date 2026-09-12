#pragma warning disable 0618,0619
using UnityEngine;

public class Mover : MonoBehaviour
{
	public Vector3 velocity = Vector3.forward;

	public Space velocitySpace;

	public Vector3 angularVelocity = Vector3.zero;

	public Space angularSpace;

	private void FixedUpdate()
	{
		base.transform.Rotate(angularVelocity * Time.fixedDeltaTime, angularSpace);
		base.transform.Translate(velocity * Time.fixedDeltaTime, velocitySpace);
	}
}
