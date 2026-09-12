#pragma warning disable 0618,0619
using System;
using Dismount.LevelEditor;
using UnityEngine;
using reLive;

namespace Dismount
{
	public class MovingObject : MonoBehaviour
	{
		public Spline followPath;

		private float followPathDistance;

		private Vector3 startPosition;

		private ObjectSpawner spawner;

		private int characterLayer;

		private float angleIncMul;

		private ExtendedLocalEulers[] wheelEulers;

		private NPCVehicleData vehicleData;

		private NPCWheelRotation npcWheelRotation;

		private Rigidbody thisRigidbody;

		private Transform thisTransform;

		public void SetSpawner(ObjectSpawner spawner)
		{
			this.spawner = spawner;
		}

		public void SetFollowPath(Spline followPath)
		{
			this.followPath = followPath;
			followPathDistance = 0f;
		}

		public void StartMoving(float velocity)
		{
			vehicleData.velocity = velocity;
			thisRigidbody.isKinematic = true;
		}

		private void Awake()
		{
			thisRigidbody = base.GetComponent<Rigidbody>();
			thisTransform = base.transform;
			wheelEulers = thisTransform.GetComponentsInChildren<ExtendedLocalEulers>();
			characterLayer = LayerMask.NameToLayer("Character");
			npcWheelRotation = GetComponent<NPCWheelRotation>();
			SetFollowPath(followPath);
		}

		private void Start()
		{
			vehicleData = GetComponent<NPCVehicleData>();
			startPosition = thisTransform.position;
			thisRigidbody.centerOfMass += vehicleData.centerOfMassOffset;
			angleIncMul = 1f / ((float)Math.PI * vehicleData.wheelRadius) * Time.fixedDeltaTime * 180f;
		}

		private void FixedUpdate()
		{
			float velocity = vehicleData.velocity;
			if (thisRigidbody.isKinematic)
			{
				if ((bool)followPath)
				{
					followPathDistance += velocity * Time.fixedDeltaTime;
					float num = followPathDistance / followPath.Length;
					if (followPath.AutoClose)
					{
						num = Mathf.Repeat(num, 1f);
					}
					thisRigidbody.MovePosition(followPath.GetPositionOnSplineFast(num));
					thisRigidbody.MoveRotation(followPath.GetOrientationOnSplineFast(num));
				}
				else
				{
					thisRigidbody.MovePosition(thisRigidbody.position + thisTransform.forward * velocity * Time.fixedDeltaTime);
				}
			}
			velocity = Vector3.Dot(thisRigidbody.velocity, thisTransform.forward);
			float num2 = velocity * angleIncMul;
			if (npcWheelRotation != null)
			{
				npcWheelRotation.angle += num2;
			}
			else if (wheelEulers != null)
			{
				for (int i = 0; i < wheelEulers.Length; i++)
				{
					wheelEulers[i].localEulers.x += num2;
					wheelEulers[i].thisTransform.localEulerAngles = wheelEulers[i].localEulers;
				}
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			int num = collision.contacts.Length;
			if (num <= 0)
			{
				return;
			}
			if (collision.gameObject.layer == characterLayer)
			{
				DismountGame.playerState.statistics.NPCVehicleHit(GetInstanceID());
			}
			float num2 = Vector3.Dot(collision.relativeVelocity, collision.contacts[0].normal);
			float num3 = num2 * num2;
			if (num3 > 40f)
			{
				float volumeMultiplier = Mathf.Max(1f, (num3 - 40f) / 40f) * 0.3f + 0.7f;
				string text = Utils.RandomPick("AutoBumpHard", "AutoCrash") as string;
				DismountGame.audioManager.PlaySoundEffect(text, thisTransform, volumeMultiplier);
			}
			else if (num3 > 4f)
			{
				string text2 = Utils.RandomPick("AutoBumpSoft", "AutoCrashSoft") as string;
				DismountGame.audioManager.PlaySoundEffect(text2, thisTransform);
			}
			if (thisRigidbody.isKinematic)
			{
				Cop component = thisRigidbody.GetComponent<Cop>();
				if (component != null && component.isTrafficCop)
				{
					component.SetDynamic();
				}
				else
				{
					thisRigidbody.isKinematic = false;
				}
				float num4 = 1f / (float)num;
				float num5 = collision.rigidbody.mass / (collision.rigidbody.mass + thisRigidbody.mass);
				for (int i = 0; i < num; i++)
				{
					float num6 = Mathf.Abs(Vector3.Dot(collision.relativeVelocity.normalized, collision.contacts[i].normal));
					thisRigidbody.AddForceAtPosition(num4 * collision.relativeVelocity * num6 * num5, collision.contacts[i].point, ForceMode.VelocityChange);
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (thisRigidbody.isKinematic && other.tag == "Respawn")
			{
				if ((bool)spawner)
				{
					spawner.Respawn(base.gameObject);
				}
				else
				{
					thisTransform.position = startPosition;
				}
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.75f);
			Gizmos.DrawSphere(base.GetComponent<Rigidbody>().worldCenterOfMass, 0.25f);
		}
	}
}
