#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	[RequireComponent(typeof(HingeJoint))]
	public class AttackWing : MonoBehaviour
	{
		private HingeJoint hinge;

		private bool hasJoint = true;

		private float normalPosition;

		public float attackPosition;

		private void Start()
		{
			hinge = GetComponent<HingeJoint>();
			normalPosition = hinge.spring.targetPosition;
		}

		private void OnJointBreak()
		{
			hasJoint = false;
		}

		public void AttackPosition()
		{
			if (hasJoint)
			{
				JointSpring spring = hinge.spring;
				spring.targetPosition = attackPosition;
				hinge.spring = spring;
			}
		}

		public void NormalPosition()
		{
			if (hasJoint)
			{
				JointSpring spring = hinge.spring;
				spring.targetPosition = normalPosition;
				hinge.spring = spring;
			}
		}
	}
}
