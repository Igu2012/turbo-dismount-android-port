#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount.Vehicular
{
	public class WheelColliderData : MonoBehaviour
	{
		[Serializable]
		public class SuspensionSpring
		{
			public float spring = 10000f;

			public float damper = 1000f;

			public float targetPosition;
		}

		[Serializable]
		public class FrictionCurve
		{
			public float extremumSlip = 1f;

			public float extremumValue = 1000f;

			public float asymptoteSlip = 2f;

			public float asymptoteValue = 500f;

			public float stiffness;
		}

		public float mass = 1f;

		public float radius = 0.5f;

		public float suspensionDistance = 0.2f;

		[Space(10f)]
		public Vector3 center = Vector3.zero;

		[Space(10f)]
		public SuspensionSpring suspensionSpring;

		[Space(10f)]
		public FrictionCurve forwardFriction;

		[Space(10f)]
		public FrictionCurve sidewaysFriction;
	}
}
