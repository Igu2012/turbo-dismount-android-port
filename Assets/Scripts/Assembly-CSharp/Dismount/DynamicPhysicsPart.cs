#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class DynamicPhysicsPart : MonoBehaviour
	{
		public void Start()
		{
			if (base.GetComponent<Rigidbody>() == null)
			{
				Debug.LogError("DynamicPhysicsPart requires a rigidbody: " + base.name);
			}
		}

		public void WakeUp()
		{
		}

		private void OnDrawGizmosSelected()
		{
			Joint[] components = GetComponents<Joint>();
			Joint[] array = components;
			foreach (Joint joint in array)
			{
				Gizmos.color = new Color(1f, 1f, 0f);
				Gizmos.DrawSphere(joint.connectedBody.gameObject.transform.position, 0.2f);
				Gizmos.DrawLine(base.transform.position, joint.connectedBody.gameObject.transform.position);
			}
		}
	}
}
