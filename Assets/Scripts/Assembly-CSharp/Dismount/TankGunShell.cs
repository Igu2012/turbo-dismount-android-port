#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;
using reLive;

namespace Dismount
{
	public class TankGunShell : MonoBehaviour
	{
		public Transform visual;

		private Rigidbody thisrb;

		private Transform thisxf;

		public float age = 5f;

		private float dieTime;

		public AudioClip hitSound;

		private AudioSource audioSettings;

		[HideInInspector]
		public TankGun owner;

		private void Awake()
		{
			thisxf = base.transform;
			thisrb = base.GetComponent<Rigidbody>();
			dieTime = Time.fixedTime + age;
			audioSettings = base.GetComponent<AudioSource>();
		}

		private void OnEnable()
		{
			dieTime = Time.fixedTime + age;
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
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			Vector3 vector = ((collision.contacts.Length <= 0) ? thisrb.position : collision.contacts[0].point);
			if ((bool)hitSound && collision.contacts.Length > 0)
			{
				audioSettings.pitch = Random.Range(0.75f, 1.33f);
				OneShotAudioHelper.PlayClipAtPoint(hitSound, vector, 1f, audioSettings);
			}
			DismountGame.particleManager.EmitExplosion(vector, Vector3.up);
			DestroyMe();
			Rigidbody rigidbody = collision.rigidbody;
			if (rigidbody != null && rigidbody.useGravity)
			{
				if (rigidbody.useGravity)
				{
					rigidbody.isKinematic = false;
				}
				rigidbody.AddForceAtPosition(-collision.relativeVelocity * 0.2f * rigidbody.mass, vector, ForceMode.Impulse);
			}
			float num = 15f;
			Collider[] array = Physics.OverlapSphere(vector, num);
			List<Rigidbody> list = new List<Rigidbody>();
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].isTrigger)
				{
					rigidbody = array[i].attachedRigidbody;
					if ((bool)rigidbody && !list.Contains(rigidbody))
					{
						list.Add(rigidbody);
					}
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				rigidbody = list[j];
				if (rigidbody.useGravity)
				{
					rigidbody.isKinematic = false;
				}
				float num2 = Mathf.Min(rigidbody.mass / 5000f, 1f);
				num2 *= num2;
				rigidbody.AddExplosionForce(100f + num2 * 25000f, vector, num, 2f, ForceMode.Impulse);
			}
		}
	}
}
