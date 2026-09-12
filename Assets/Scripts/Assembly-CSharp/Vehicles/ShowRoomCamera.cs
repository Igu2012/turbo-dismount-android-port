#pragma warning disable 0618,0619
using UnityEngine;

namespace Vehicles
{
	public class ShowRoomCamera : MonoBehaviour
	{
		private float targetDistance = 10f;

		private float distance = 10f;

		private float originalDistance = 10f;

		private Vector3 orbitOrigin = new Vector3(0f, 1f, 0f);

		private float originalY;

		private void Start()
		{
			originalY = base.transform.position.y;
			targetDistance = (distance = (originalDistance = (orbitOrigin - base.transform.position).magnitude));
		}

		private void Update()
		{
			if (SXInputManager.GetMouseScrollWheel() > 0f)
			{
				targetDistance--;
			}
			else if (SXInputManager.GetMouseScrollWheel() < 0f)
			{
				targetDistance++;
			}
			targetDistance = Mathf.Clamp(targetDistance, 5f, 30f);
			float f = -1f + Time.time * 0.5f;
			Vector3 position = new Vector3(Mathf.Sin(f) * distance, distance / originalDistance * originalY, Mathf.Cos(f) * distance);
			base.transform.position = position;
			base.transform.LookAt(orbitOrigin);
		}

		private void FixedUpdate()
		{
			distance = distance * 0.975f + targetDistance * 0.025f;
		}
	}
}
