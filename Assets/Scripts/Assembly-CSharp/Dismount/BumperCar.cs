#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class BumperCar : MonoBehaviour
	{
		private const float superBumpWindow = 0.3f;

		public AudioSource boostSound;

		public Transform electricBolt;

		public AudioSource superBounceSound;

		public Transform bumpForceTarget;

		private bool turboBoost;

		private float prevBoltScaleTime;

		private Rigidbody bumperCarRigidbody;

		private float prevSuperBumpTime;

		private Vector3 bumpForce = Vector3.zero;

		private Vector3 bumpForcePos = Vector3.zero;

		private void Awake()
		{
			bumperCarRigidbody = base.GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			if (turboBoost)
			{
				if (Time.fixedTime > prevBoltScaleTime + 0.1f)
				{
					electricBolt.localRotation = Quaternion.Euler(Random.insideUnitSphere * 360f);
					prevBoltScaleTime = Time.fixedTime;
				}
				float num = (Time.fixedTime - prevBoltScaleTime) / 0.1f;
				electricBolt.localScale = (0.2f + num * 0.8f) * Vector3.one;
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (Time.fixedTime < prevSuperBumpTime + 0.3f)
			{
				return;
			}
			Collider collider = collision.collider;
			if (collider.isTrigger || collision.contacts.Length == 0 || (!collision.contacts[0].thisCollider.name.Equals("BColl") && !collision.contacts[0].otherCollider.name.Equals("BColl")))
			{
				return;
			}
			float num = 2000f;
			Rigidbody attachedRigidbody = collider.attachedRigidbody;
			if (attachedRigidbody != null)
			{
				num = attachedRigidbody.mass;
			}
			if (!(num < 50f))
			{
				num = Mathf.Min(num, 2000f);
				float num2 = Mathf.InverseLerp(50f, 2000f, num);
				float magnitude = collision.relativeVelocity.magnitude;
				if (!(magnitude < 5f))
				{
					num2 *= Mathf.InverseLerp(5f, 30f, magnitude);
					Vector3 point = collision.contacts[0].point;
					Vector3 vector = bumpForceTarget.position - point;
					vector = new Vector3(vector.x, vector.y * 0.5f, vector.z).normalized;
					bumpForce = vector * (1500f + num2 * 1500f);
					bumpForcePos = point;
					bumperCarRigidbody.AddForceAtPosition(bumpForce, bumpForcePos, ForceMode.Impulse);
					superBounceSound.volume = 0.8f + num2 * 0.2f;
					superBounceSound.pitch = Random.Range(0.8f, 1.1f);
					superBounceSound.Play();
					prevSuperBumpTime = Time.fixedTime;
				}
			}
		}

		private void TurboBoostStarted()
		{
			turboBoost = true;
			boostSound.Play();
			electricBolt.gameObject.SetActive(true);
		}

		private void TurboBoostEnded()
		{
			turboBoost = false;
			boostSound.Stop();
			electricBolt.gameObject.SetActive(false);
		}
	}
}
