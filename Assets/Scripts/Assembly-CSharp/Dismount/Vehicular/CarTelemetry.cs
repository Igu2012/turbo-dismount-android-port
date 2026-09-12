#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	public class CarTelemetry : MonoBehaviour
	{
		public Vector2[] outputTorqueCurve;

		public float frictionCoefficient;

		public float maxRpm = 1000f;

		public float maxTorque = 100f;

		public bool overrideGear;

		public float steer;

		public float throttle;

		public float brake;

		public float clutch;

		public int gear;

		public float rpm;

		public float outputTorque;

		public float frictionTorque;
	}
}
