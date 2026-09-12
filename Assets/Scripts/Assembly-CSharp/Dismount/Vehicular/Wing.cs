#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	public class Wing : MonoBehaviour
	{
		public Rigidbody connectedBody;

		public float lift;

		public float frontalDrag;

		public float surfaceDrag;

		private void Start()
		{
			if (!connectedBody)
			{
				connectedBody = base.GetComponent<Rigidbody>();
			}
			if (!connectedBody)
			{
				Debug.LogWarning("Wing has no connected rigidbody");
			}
		}

		private void FixedUpdate()
		{
			if ((bool)connectedBody)
			{
				Vector3 pointVelocity = connectedBody.GetPointVelocity(base.transform.position);
				float num = Vector3.Dot(pointVelocity, base.transform.forward);
				float num2 = Vector3.Dot(pointVelocity, base.transform.right);
				float num3 = Vector3.Dot(pointVelocity, base.transform.up);
				if (num > 0f)
				{
					connectedBody.AddForceAtPosition(base.transform.up * num * lift, base.transform.position, ForceMode.Force);
				}
				connectedBody.AddForceAtPosition(base.transform.forward * (0f - num) * frontalDrag, base.transform.position, ForceMode.Force);
				connectedBody.AddForceAtPosition(base.transform.right * (0f - num2) * frontalDrag, base.transform.position, ForceMode.Force);
				connectedBody.AddForceAtPosition(base.transform.up * (0f - num3) * surfaceDrag, base.transform.position, ForceMode.Force);
			}
		}
	}
}
