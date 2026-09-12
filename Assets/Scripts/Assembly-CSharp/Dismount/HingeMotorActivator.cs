#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount
{
	public class HingeMotorActivator : MonoBehaviour
	{
		private Rigidbody hingeRigidBody;

		public HingeJoint hinge;

		public float duration = 5f;

		public bool randomDirection;

		private float targetVelocity;

		private float deactivateTime = -1f;

		private void Awake()
		{
			hingeRigidBody = hinge.GetComponent<Rigidbody>();
			hinge.useMotor = false;
			deactivateTime = Time.fixedTime + 0.5f;
		}

		private void Activate()
		{
			targetVelocity = hinge.motor.targetVelocity;
			float num = Mathf.Abs(targetVelocity) / 180f * (float)Math.PI;
			if (hingeRigidBody.maxAngularVelocity < num)
			{
				hingeRigidBody.maxAngularVelocity = num;
			}
			if (randomDirection)
			{
				JointMotor motor = hinge.motor;
				motor.targetVelocity = (float)(-1 + 2 * UnityEngine.Random.Range(0, 2)) * targetVelocity;
				hinge.motor = motor;
			}
			hinge.useMotor = true;
			deactivateTime = Time.fixedTime + duration;
		}

		private void Deactivate()
		{
			hinge.useMotor = false;
			deactivateTime = -1f;
		}

		private void Update()
		{
			if (deactivateTime > 0f && Time.fixedTime > deactivateTime)
			{
				Deactivate();
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!(deactivateTime > 0f) && !other.isTrigger)
			{
				Rigidbody attachedRigidbody = other.attachedRigidbody;
				if (!(attachedRigidbody == null) && !attachedRigidbody.isKinematic && attachedRigidbody.useGravity)
				{
					Debug.Log(other.name);
					Activate();
				}
			}
		}
	}
}
