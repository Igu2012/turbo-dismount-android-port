#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class PinballPaddle : MonoBehaviour
	{
		private const float thresholdVelocity = 2f;

		private const float thresholdMass = 2f;

		private const float actDuration = 0.15f;

		private const float stayDuration = 0.1f;

		private const float returnDuration = 0.2f;

		private const float totalDuration = 0.45f;

		private const float targetAngle = 60f;

		private float actTime;

		private Quaternion originalRotation = Quaternion.identity;

		private Quaternion targetRotation = Quaternion.identity;

		private void Awake()
		{
			originalRotation = base.transform.rotation;
		}

		public void OnDismountStarted()
		{
			originalRotation = base.transform.rotation;
		}

		private void FixedUpdate()
		{
			if (actTime <= 0f)
			{
				return;
			}
			if (Time.fixedTime > actTime + 0.45f)
			{
				actTime = 0f;
				base.GetComponent<Rigidbody>().MoveRotation(originalRotation);
				return;
			}
			float num = Time.fixedTime - actTime;
			if (num < 0.15f)
			{
				base.GetComponent<Rigidbody>().MoveRotation(Quaternion.Lerp(originalRotation, targetRotation, num / 0.15f));
			}
			else if (num < 0.25f)
			{
				base.GetComponent<Rigidbody>().MoveRotation(targetRotation);
			}
			else
			{
				base.GetComponent<Rigidbody>().MoveRotation(Quaternion.Lerp(targetRotation, originalRotation, (num - 0.25f) / 0.2f));
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (actTime > 0f && Time.fixedTime < actTime + 0.45f)
			{
				return;
			}
			Collider collider = collision.collider;
			if (!collider.isTrigger && collision.contacts.Length != 0)
			{
				Rigidbody attachedRigidbody = collider.attachedRigidbody;
				if ((!(attachedRigidbody == null) || !(attachedRigidbody.mass <= 2f)) && !(collision.relativeVelocity.sqrMagnitude < 4f))
				{
					float num = Mathf.Sign(Vector3.Dot(collision.contacts[0].point - base.transform.position, base.transform.forward));
					targetRotation = Quaternion.AngleAxis(num * 60f, base.transform.up) * originalRotation;
					actTime = Time.fixedTime;
					base.GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
					base.GetComponent<AudioSource>().Play();
				}
			}
		}
	}
}
