#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.LevelEditor
{
	public class NPCVehicleData : MonoBehaviour
	{
		public float velocity;

		public float wheelRadius = 0.3f;

		public Vector3 centerOfMassOffset = Vector3.zero;
	}
}
