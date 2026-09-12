#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	public class Support : MonoBehaviour
	{
		public float spring = 5000f;

		public float distance = 0.5f;

		private float hitDistance;

		private Vehicle owner;

		private LayerMask groundHitLayerMask = 512;

		private void Awake()
		{
			groundHitLayerMask = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Default"));
			Transform parent = base.transform;
			while (!owner && (bool)parent)
			{
				Vehicle component = parent.GetComponent<Vehicle>();
				if ((bool)component)
				{
					owner = component;
					break;
				}
				parent = parent.parent;
			}
			if (!owner)
			{
				Debug.LogError("Support needs to be under a Vehicle");
			}
		}

		private void FixedUpdate()
		{
			if (owner.isAwake)
			{
				hitDistance = -1f;
				RaycastHit hitInfo;
				if (Physics.Raycast(base.transform.position, Vector3.down, out hitInfo, distance, groundHitLayerMask) && !hitInfo.collider.isTrigger)
				{
					hitDistance = hitInfo.distance;
				}
				if (hitDistance > 0f)
				{
					float num = distance - hitDistance;
					float num2 = num * spring;
					float x = base.transform.localPosition.x;
					Vector3 position = base.transform.parent.position;
					owner.GetComponent<Rigidbody>().AddForceAtPosition(num2 * base.transform.up, position + Vector3.right * x, ForceMode.Force);
					owner.GetComponent<Rigidbody>().AddForceAtPosition((0f - num2) * base.transform.up, position - Vector3.right * x, ForceMode.Force);
				}
			}
		}

		private void OnDrawGizmosSelected()
		{
			Color green = Color.green;
			float num = hitDistance;
			if (num < 0f)
			{
				num = distance;
			}
			if (num < distance * 0.5f)
			{
				float g = num / (distance * 0.5f);
				green = new Color(1f, g, 0f, 1f);
			}
			else
			{
				float r = 1f - (num - distance * 0.5f) / (distance * 0.5f);
				green = new Color(r, 1f, 0f, 1f);
			}
			Gizmos.color = green;
			Gizmos.DrawLine(base.transform.position, base.transform.position + Vector3.down * hitDistance);
			Gizmos.color = Color.white;
		}
	}
}
