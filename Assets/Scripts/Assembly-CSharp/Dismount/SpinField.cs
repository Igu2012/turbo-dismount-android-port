#pragma warning disable 0618,0619
using Dismount.Vehicular;
using UnityEngine;

namespace Dismount
{
	public class SpinField : MonoBehaviour
	{
		public float spinAmount = 15f;

		public bool randomDirection = true;

		private Vehicle vehicle;

		private Rigidbody vehicleRigidBody;

		private bool vehicleOnOil;

		private void Start()
		{
			spinAmount *= Random.Range(0.8f, 1.25f);
			if (randomDirection && Utils.Maybe())
			{
				spinAmount *= -1f;
			}
		}

		private void FixedUpdate()
		{
			if (vehicleOnOil && !(vehicle.lastGroundContactTime <= 0f))
			{
				float num = Mathf.Clamp(vehicleRigidBody.velocity.magnitude, 2f, 50f);
				float num2 = Mathf.Clamp((vehicleRigidBody.position - base.transform.position).magnitude, 0f, 10f);
				float num3 = (num - 2f) / 48f * ((10f - num2) / 10f) * spinAmount;
				vehicleRigidBody.AddTorque(base.transform.up * num3, ForceMode.Acceleration);
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (vehicle == null)
			{
				GameObject currentVehicleInstance = DismountGame.playerState.currentVehicleInstance;
				if ((bool)currentVehicleInstance)
				{
					vehicle = currentVehicleInstance.GetComponent<Vehicle>();
					vehicleRigidBody = vehicle.GetComponent<Rigidbody>();
				}
			}
			if (!vehicleOnOil && other.isTrigger && other.attachedRigidbody == vehicleRigidBody)
			{
				vehicleOnOil = true;
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (vehicleOnOil && other.isTrigger && other.attachedRigidbody == vehicleRigidBody)
			{
				vehicleOnOil = false;
			}
		}
	}
}
