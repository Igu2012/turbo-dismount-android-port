#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(BodyPart))]
	[RequireComponent(typeof(Rigidbody))]
	public class JointFriction : MonoBehaviour
	{
		public float dampingTime = 0.1f;

		public float velocityThreshold = 0.001f;

		private CharacterJoint joint;

		private void Awake()
		{
			joint = GetComponent<CharacterJoint>();
		}

		private void FixedUpdate()
		{
			Vector3 vector = base.transform.localToWorldMatrix.MultiplyVector(joint.axis);
			float num = Vector3.Dot(base.GetComponent<Rigidbody>().angularVelocity, vector);
			float num2 = Vector3.Dot(joint.connectedBody.angularVelocity, vector);
			float num3 = num2 - num;
			if (!(num3 > 0f - velocityThreshold) || !(num3 < velocityThreshold))
			{
				float num4 = Time.fixedDeltaTime / dampingTime;
				if (num4 > 1f)
				{
					num4 = 1f;
				}
				base.GetComponent<Rigidbody>().AddTorque(vector * num3 * num4, ForceMode.VelocityChange);
			}
		}
	}
}
