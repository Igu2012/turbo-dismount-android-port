#pragma warning disable 0618,0619
using UnityEngine;

public class SnowBank : MonoBehaviour
{
	public float friction = 0.2f;

	private void OnTriggerStay(Collider other)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (!(attachedRigidbody == null))
		{
			Vector3 center = other.bounds.center;
			Vector3 pointVelocity = attachedRigidbody.GetPointVelocity(center);
			pointVelocity.y -= Mathf.Min(pointVelocity.magnitude, 3f);
			attachedRigidbody.AddForceAtPosition(pointVelocity * (0f - friction), center, ForceMode.Acceleration);
		}
	}
}
