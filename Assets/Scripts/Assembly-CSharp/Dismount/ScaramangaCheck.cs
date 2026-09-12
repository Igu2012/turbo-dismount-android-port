#pragma warning disable 0618,0619
using Dismount.Vehicular;
using UnityEngine;

namespace Dismount
{
	public class ScaramangaCheck : MonoBehaviour
	{
		public ScaramangaCheck nextCheck;

		public Vector3 desiredUpVector = Vector3.up;

		private void CheckComplete()
		{
			if (nextCheck != null)
			{
				nextCheck.gameObject.SetActive(true);
			}
			else
			{
				DismountGame.playerState.achievements.ReportComplete("com.secretexit.turbodismount.Scaramanga");
			}
			base.gameObject.SetActive(false);
		}

		private void CheckVehicleUpright(Collider other)
		{
			if (!(other.GetComponent<Vehicle>() == null))
			{
				Transform transform = other.transform;
				if (Vector3.Dot(transform.up, desiredUpVector) > 0.707f)
				{
					CheckComplete();
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			CheckVehicleUpright(other);
		}

		private void OnTriggerStay(Collider other)
		{
			CheckVehicleUpright(other);
		}
	}
}
