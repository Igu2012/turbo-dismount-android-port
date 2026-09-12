#pragma warning disable 0618,0619
using UnityEngine;
using reLive;

namespace Dismount
{
	public class TurboLaserShot : MonoBehaviour
	{
		public Transform visual;

		public Light boltLight;

		private Rigidbody thisrb;

		private Transform thisxf;

		public int maxBounces;

		public float age = 5f;

		private int bounces;

		private float dieTime;

		public AudioClip hitSound;

		private AudioSource audioSettings;

		[HideInInspector]
		public TurboLaserGun owner;

		private void Awake()
		{
			thisxf = base.transform;
			thisrb = base.GetComponent<Rigidbody>();
			bounces = maxBounces;
			dieTime = Time.fixedTime + age;
			audioSettings = base.GetComponent<AudioSource>();
		}

		private void OnEnable()
		{
			bounces = maxBounces;
			dieTime = Time.fixedTime + age;
			if (visual != null)
			{
				visual.LookAt(visual.position + thisxf.forward, visual.up);
				Vector3 localScale = visual.localScale;
				localScale.z = 5f;
				visual.localScale = localScale;
			}
		}

		private void DestroyMe()
		{
			if ((bool)owner)
			{
				owner.DestroyShot(base.gameObject);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}

		private void FixedUpdate()
		{
			if (Time.fixedTime > dieTime)
			{
				DestroyMe();
			}
			else if (visual != null)
			{
				visual.LookAt(visual.position + thisrb.velocity, visual.up);
				Vector3 localScale = visual.localScale;
				localScale.z = Mathf.Clamp(thisrb.velocity.magnitude * 0.05f, 1f, 10f);
				visual.localScale = localScale;
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			Vector3 position = ((collision.contacts.Length <= 0) ? thisrb.position : collision.contacts[0].point);
			if ((bool)hitSound && collision.contacts.Length > 0)
			{
				audioSettings.pitch = Random.Range(0.75f, 1.33f);
				OneShotAudioHelper.PlayClipAtPoint(hitSound, position, 1f, audioSettings);
			}
			Rigidbody rigidbody = collision.rigidbody;
			if (rigidbody == null || !rigidbody.useGravity)
			{
				if (bounces-- <= 0)
				{
					DismountGame.particleManager.EmitLaserImpact(position, collision.relativeVelocity.normalized * 3f);
					DestroyMe();
				}
				return;
			}
			if (rigidbody.useGravity)
			{
				rigidbody.isKinematic = false;
			}
			rigidbody.AddForceAtPosition(-collision.relativeVelocity * 0.2f * rigidbody.mass, position, ForceMode.Impulse);
			if (bounces-- <= 0)
			{
				DismountGame.particleManager.EmitLaserImpact(position, collision.relativeVelocity.normalized * 3f);
				DestroyMe();
			}
		}
	}
}
