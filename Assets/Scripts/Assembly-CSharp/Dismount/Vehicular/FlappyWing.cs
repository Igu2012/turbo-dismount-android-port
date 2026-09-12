#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	[RequireComponent(typeof(HingeJoint))]
	public class FlappyWing : MonoBehaviour
	{
		private Wing wing;

		private Vehicle vehicle;

		private Rigidbody vehicleRigidbody;

		private HingeJoint hinge;

		private bool hasJoint = true;

		private float normalSpring = 10f;

		public float speedLimit = 5f;

		public float flapSpringAmplitude = 10f;

		public float flapSpringFrequency = 1f;

		private void Start()
		{
			vehicle = Utils.FindFirstComponentUpInHierarchy<Vehicle>(base.transform);
			if (vehicle == null)
			{
				Debug.LogWarning("Flappy wing didn't find a parent vehicle");
			}
			else
			{
				vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
			}
			wing = GetComponent<Wing>();
			hinge = GetComponent<HingeJoint>();
			normalSpring = hinge.spring.spring;
		}

		private void Update()
		{
			bool flag = false;
			bool flag2 = hasJoint && wing != null;
			if (hasJoint && vehicle != null && vehicleRigidbody != null)
			{
				flag = vehicleRigidbody.velocity.sqrMagnitude > speedLimit * speedLimit && vehicle.airTime < 0.5f;
			}
			wing.enabled = flag2;
			if (flag)
			{
				JointSpring spring = hinge.spring;
				spring.spring = normalSpring + flapSpringAmplitude * Mathf.Abs(Mathf.Sin(flapSpringFrequency * 6.283f * Time.time));
				hinge.spring = spring;
			}
			else if (hasJoint)
			{
				JointSpring spring2 = hinge.spring;
				spring2.spring = normalSpring;
				hinge.spring = spring2;
			}
		}

		private void OnJointBreak()
		{
			hasJoint = false;
		}
	}
}
