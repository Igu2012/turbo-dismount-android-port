#pragma warning disable 0618,0619
using System;
using Dismount.LevelEditor;
using UnityEngine;

namespace Dismount
{
	public class Cop : MonoBehaviour
	{
		private const float TargetChangeInterval = 3f;

		public float minSpeed = 5f;

		public float maxSpeed = 70f;

		public float acceleration = 40f;

		public float deceleration = 50f;

		public float lateralAcceleration = 28f;

		public float minSpeedAngularVelocity = 2f;

		public float maxSpeedAngularVelocity = 1f;

		public float maxAngularAcceleration = 5f;

		public float crashVelocityDelta = 80f;

		public float anticipation = 0.25f;

		public float wheelRadius = 0.45f;

		public bool isTrafficCop;

		public bool showDebug;

		private NPCCopData copData;

		private float observeSectorDot = 0.5f;

		private Rigidbody thisRigidbody;

		private Transform thisTransform;

		private Rigidbody[] targets;

		private Rigidbody currentTarget;

		private float currentTargetTime;

		private bool isChasing;

		private bool hasCrashed;

		private Vector3 prevVelocity = Vector3.zero;

		private int characterLayer;

		private float angleIncMul;

		private CopWheel[] wheels;

		private CopLights lights;

		private Transform wheelCenter;

		private float prevWheelAngle;

		public void Start()
		{
			thisRigidbody = base.GetComponent<Rigidbody>();
			thisTransform = base.transform;
			wheels = thisTransform.GetComponentsInChildren<CopWheel>();
			angleIncMul = 1f / ((float)Math.PI * wheelRadius) * Time.fixedDeltaTime * 180f;
			lights = GetComponentInChildren<CopLights>();
			lights.gameObject.SetActive(false);
			copData = GetComponent<NPCCopData>();
			copData.observeSectorAngle = Mathf.Clamp(copData.observeSectorAngle, 0f, 180f);
			observeSectorDot = Mathf.Cos(copData.observeSectorAngle * 0.5f / 180f * (float)Math.PI);
			copData.observeSectorRadius = Mathf.Clamp(copData.observeSectorRadius, 0f, 1000f);
			copData.observeOmniRadius = Mathf.Clamp(copData.observeOmniRadius, 0f, 1000f);
			characterLayer = LayerMask.NameToLayer("Character");
			wheelCenter = base.transform.Find("PhysicalWheels");
			if (isTrafficCop)
			{
				thisRigidbody.isKinematic = true;
				for (int i = 0; i < wheels.Length; i++)
				{
					wheels[i].GetComponent<Rigidbody>().isKinematic = true;
				}
				copData.observeSectorAngle = 90f;
				observeSectorDot = Mathf.Cos(copData.observeSectorAngle * 0.5f / 180f * (float)Math.PI);
				copData.observeSectorRadius = 70f;
				copData.observeOmniRadius = 10f;
			}
			else
			{
				thisRigidbody.isKinematic = false;
				for (int j = 0; j < wheels.Length; j++)
				{
					wheels[j].GetComponent<Rigidbody>().isKinematic = false;
				}
			}
		}

		public void Chase(Rigidbody[] targets)
		{
			this.targets = targets;
			currentTarget = null;
			isChasing = false;
			hasCrashed = false;
			currentTargetTime = 2f;
		}

		public void SetDynamic()
		{
			thisRigidbody.isKinematic = false;
			for (int i = 0; i < wheels.Length; i++)
			{
				wheels[i].GetComponent<Rigidbody>().isKinematic = false;
			}
			MovingObject component = GetComponent<MovingObject>();
			if (component != null)
			{
				component.enabled = false;
			}
			isTrafficCop = false;
		}

		private void FixedUpdate()
		{
			if (thisRigidbody == null)
			{
				return;
			}
			currentTargetTime += Time.fixedDeltaTime;
			Vector3 velocity = thisRigidbody.velocity;
			for (int i = 0; i < wheels.Length; i++)
			{
				CopWheel copWheel = wheels[i];
				Vector3 localPosition = copWheel.visualWheel.localPosition;
				localPosition.y = copWheel.visualPosition;
				copWheel.visualWheel.localPosition = localPosition;
			}
			if (targets != null && targets.Length > 0 && !hasCrashed && currentTargetTime > 3f)
			{
				float num = float.MaxValue;
				Rigidbody rigidbody = null;
				for (int j = 0; j < targets.Length; j++)
				{
					if (targets[j] == null)
					{
						prevVelocity = velocity;
						return;
					}
					float sqrMagnitude = (targets[j].position - thisRigidbody.position).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						rigidbody = targets[j];
					}
				}
				if (!isChasing)
				{
					float num2 = Vector3.Dot((rigidbody.position - thisRigidbody.position).normalized, thisTransform.forward);
					if (num <= copData.observeOmniRadius * copData.observeOmniRadius || (num2 >= observeSectorDot && num <= copData.observeSectorRadius * copData.observeSectorRadius))
					{
						currentTarget = rigidbody;
						currentTargetTime = 0f;
						isChasing = true;
						thisRigidbody.WakeUp();
						base.GetComponent<AudioSource>().Play();
						lights.gameObject.SetActive(true);
						copData.enabled = false;
						if (isTrafficCop)
						{
							SetDynamic();
						}
					}
				}
				else
				{
					currentTarget = rigidbody;
					currentTargetTime = 0f;
				}
			}
			float num3 = Vector3.Dot(thisTransform.up, Vector3.up);
			if (isChasing && !hasCrashed)
			{
				if (num3 < 0f)
				{
					hasCrashed = true;
				}
				else
				{
					float sqrMagnitude2 = (velocity - prevVelocity).sqrMagnitude;
					if (sqrMagnitude2 > crashVelocityDelta * crashVelocityDelta * Time.fixedDeltaTime)
					{
						hasCrashed = true;
					}
				}
			}
			prevVelocity = velocity;
			if (hasCrashed && base.GetComponent<AudioSource>().isPlaying)
			{
				base.GetComponent<AudioSource>().pitch *= 0.99f;
				base.GetComponent<AudioSource>().volume *= 0.99f;
				lights.speed *= 0.99f;
				if (base.GetComponent<AudioSource>().volume < 0.1f)
				{
					base.GetComponent<AudioSource>().Stop();
					lights.gameObject.SetActive(false);
				}
			}
			float num4 = Vector3.Dot(velocity, thisTransform.forward);
			if (thisRigidbody.isKinematic && !isChasing && !hasCrashed)
			{
				for (int k = 0; k < wheels.Length; k++)
				{
					CopWheel copWheel2 = wheels[k];
					copWheel2.vel = num4;
				}
				return;
			}
			float num5 = 0f;
			Vector3 vector = wheelCenter.position;
			Vector3 zero = Vector3.zero;
			for (int l = 0; l < wheels.Length; l++)
			{
				CopWheel copWheel3 = wheels[l];
				float load = copWheel3.load;
				if (load > 0f)
				{
					num5 += load;
					zero += (copWheel3.thisTransform.position + Vector3.down * wheelRadius - vector) * load;
				}
			}
			if (num5 > 0f)
			{
				vector = Vector3.Lerp(vector, base.GetComponent<Rigidbody>().worldCenterOfMass, 0.825f);
			}
			bool flag = num3 > 0.5f && num5 > 0f;
			if (!flag || hasCrashed || currentTarget == null)
			{
				for (int m = 0; m < wheels.Length; m++)
				{
					CopWheel copWheel4 = wheels[m];
					float num6 = copWheel4.load;
					if (num3 < 0f)
					{
						num6 = 0.05f;
						copWheel4.vel = Mathf.Lerp(copWheel4.vel, 0f, num6 * num6);
					}
					else if (num6 < 0.05f)
					{
						num6 = 0.05f;
						copWheel4.vel = Mathf.Lerp(copWheel4.vel, 0f, num6 * num6);
					}
					else
					{
						if (num6 < 0.3f)
						{
							num6 = 0.3f;
						}
						copWheel4.vel = Mathf.Lerp(copWheel4.vel, num4, num6 * num6);
					}
					float num7 = copWheel4.vel * angleIncMul;
					copWheel4.extendedLocalEulers.localEulers.x += num7;
					copWheel4.visualWheel.localEulerAngles = copWheel4.extendedLocalEulers.localEulers;
				}
			}
			float num8 = num5 / (float)wheels.Length * num3;
			if (hasCrashed)
			{
				thisRigidbody.AddForceAtPosition(num8 * (0f - lateralAcceleration) * 0.5f * thisRigidbody.velocity.normalized, vector, ForceMode.Acceleration);
			}
			else
			{
				if (!flag || currentTarget == null)
				{
					return;
				}
				Vector3 normalized = (currentTarget.position + currentTarget.velocity * anticipation - thisRigidbody.position).normalized;
				float num9 = Vector3.Dot(normalized, thisTransform.forward);
				float num10 = Vector3.Dot(velocity, thisTransform.right);
				if (Mathf.Abs(num10) > 1f)
				{
					thisRigidbody.AddForceAtPosition(num8 * Mathf.Sign(num10) * (0f - lateralAcceleration) * thisTransform.right, vector, ForceMode.Acceleration);
				}
				else
				{
					thisRigidbody.AddForceAtPosition(num8 * num10 * (0f - lateralAcceleration) * thisTransform.right, vector, ForceMode.Acceleration);
				}
				float f = Vector3.Dot(normalized, thisTransform.right);
				float num11 = 0f;
				float num12 = 0f;
				if (num9 > 0f || num4 < minSpeed)
				{
					num11 = Mathf.Abs(num9);
					num11 = ((num4 < minSpeed) ? 1f : ((!(num11 > 0.75f)) ? (0.25f + num11) : 1f));
					num11 = num11 * (1f - num4 / maxSpeed) * acceleration;
					thisRigidbody.AddForceAtPosition(num8 * num11 * thisTransform.forward, vector, ForceMode.Acceleration);
				}
				else if (num9 < 0f && num4 > minSpeed)
				{
					num12 = num9 * deceleration;
					thisRigidbody.AddForceAtPosition(num8 * num12 * thisTransform.forward, vector, ForceMode.Acceleration);
				}
				float num13 = 0f;
				num13 = ((num4 < minSpeed) ? (num4 / minSpeed * minSpeedAngularVelocity) : ((!(num4 > maxSpeed)) ? Mathf.Lerp(minSpeedAngularVelocity, maxSpeedAngularVelocity, Mathf.Lerp(minSpeed, maxSpeed, num4)) : maxSpeedAngularVelocity));
				float num14 = Mathf.Abs(f);
				num14 = ((num9 < 0f) ? 1f : ((!(num14 > 0.25f)) ? (0.2f + num14 * 3.2f) : 1f));
				num14 *= Mathf.Sign(f);
				num13 *= num14;
				float num15 = Vector3.Dot(thisRigidbody.angularVelocity, thisTransform.up);
				float num16 = (num13 - num15) / Time.fixedDeltaTime;
				if (Mathf.Abs(num16) > maxAngularAcceleration)
				{
					num16 = Mathf.Sign(num16) * maxAngularAcceleration;
				}
				thisRigidbody.AddRelativeTorque(num8 * new Vector3(0f, num16, 0f), ForceMode.Acceleration);
				float num17 = num13 / minSpeedAngularVelocity * 40f;
				float num18 = Mathf.Clamp01(Mathf.Abs(num10) * 0.05f);
				float to = Mathf.Sign(num10) * num18 * 40f;
				float num19 = Mathf.Lerp(num17, to, num18 * num18);
				num19 = (prevWheelAngle = Mathf.Lerp(num19, prevWheelAngle, 0.975f));
				for (int n = 0; n < wheels.Length; n++)
				{
					CopWheel copWheel5 = wheels[n];
					float num20 = copWheel5.load;
					bool flag2 = false;
					if (!copWheel5.isFrontWheel & (num11 > 0f))
					{
						if (num20 > 0.3f)
						{
							num20 = 0.3f;
						}
						copWheel5.vel = Mathf.Lerp(copWheel5.vel, num4, num20 * num20);
						flag2 = true;
						copWheel5.vel += num11 * 1.25f * Time.fixedDeltaTime;
						if (copWheel5.vel > maxSpeed)
						{
							copWheel5.vel = maxSpeed;
						}
					}
					if (num12 < 0f)
					{
						if (num20 > 0.3f)
						{
							num20 = 0.3f;
						}
						copWheel5.vel = Mathf.Lerp(copWheel5.vel, num4, num20 * num20);
						flag2 = true;
						copWheel5.vel += num12 * 2f * Time.fixedDeltaTime;
						if (copWheel5.vel < 0f)
						{
							copWheel5.vel = 0f;
						}
					}
					if (!flag2)
					{
						if (num20 < 0.05f)
						{
							num20 = 0.05f;
							copWheel5.vel = Mathf.Lerp(copWheel5.vel, 0f, num20 * num20);
						}
						else
						{
							if (num20 < 0.5f)
							{
								num20 = 0.5f;
							}
							copWheel5.vel = Mathf.Lerp(copWheel5.vel, num4, num20 * num20);
						}
					}
					float num21 = copWheel5.vel * angleIncMul;
					copWheel5.extendedLocalEulers.localEulers.x += num21;
					copWheel5.extendedLocalEulers.localEulers.z = 0f;
					if (copWheel5.isFrontWheel)
					{
						copWheel5.extendedLocalEulers.localEulers.y = num19;
					}
					copWheel5.visualWheel.localEulerAngles = copWheel5.extendedLocalEulers.localEulers;
				}
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			int num = collision.contacts.Length;
			if (num > 0)
			{
				if (collision.gameObject.layer == characterLayer)
				{
					DismountGame.playerState.statistics.NPCVehicleHit(GetInstanceID());
				}
				float num2 = Vector3.Dot(collision.relativeVelocity, collision.contacts[0].normal);
				if (num2 > 15f)
				{
					float volumeMultiplier = Mathf.Max(1f, (num2 - 15f) / 15f) * 0.3f + 0.7f;
					string text = Utils.RandomPick("AutoBumpHard", "AutoCrash") as string;
					DismountGame.audioManager.PlaySoundEffect(text, thisTransform, volumeMultiplier);
				}
				else if (num2 > 4f)
				{
					string text2 = Utils.RandomPick("AutoBumpSoft", "AutoCrashSoft") as string;
					DismountGame.audioManager.PlaySoundEffect(text2, thisTransform);
				}
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (isChasing && !hasCrashed && currentTarget != null)
			{
				Gizmos.color = new Color(1f, 0.2f, 0.4f, (3f - currentTargetTime) / 3f);
				Gizmos.DrawLine(base.GetComponent<Rigidbody>().position, currentTarget.position);
			}
		}
	}
}
