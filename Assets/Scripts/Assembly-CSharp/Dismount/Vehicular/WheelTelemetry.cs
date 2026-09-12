#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	public class WheelTelemetry : MonoBehaviour
	{
		public float nominalForce = 2500f;

		public bool hit;

		public float force;

		public Vector3 forwardDir = Vector3.forward;

		public float forwardSlip;

		public Vector3 normal = Vector3.up;

		public Vector3 point = Vector3.zero;

		public Vector3 sidewaysDir = Vector3.right;

		public float sidewaysSlip;
	}
}
