#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount
{
	public class RockerForce : MonoBehaviour
	{
		public Rigidbody forceBody;

		public Vector3 direction = Vector3.down;

		public float frequency = 1f;

		public float amplitude = 1000f;

		private Transform thisTransform;

		private void Awake()
		{
			thisTransform = base.transform;
		}

		private void FixedUpdate()
		{
			if (!(forceBody == null))
			{
				Vector3 vector = thisTransform.TransformDirection(direction);
				float num = Mathf.Sin((float)Math.PI * 2f * frequency * Time.fixedTime) * amplitude;
				forceBody.AddForceAtPosition(num * vector, base.transform.position, ForceMode.Force);
			}
		}
	}
}
