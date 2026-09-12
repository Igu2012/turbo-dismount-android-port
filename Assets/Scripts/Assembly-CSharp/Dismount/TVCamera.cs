#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount
{
	public class TVCamera : MonoBehaviour
	{
		public Transform target;

		public float targetSize = 10f;

		public float minFov = 5f;

		public float maxFov = 120f;

		public float targetLookAhead = 3f;

		public float velocityWeight = 0.99f;

		public float targetPositionWeight = 0.125f;

		private Vector3 previousTargetPosition = Vector3.zero;

		private Vector3 previousTargetVelocity = Vector3.zero;

		private Vector3 lookAtPoint = Vector3.zero;

		private Vector3 lookAtVelocity = Vector3.zero;

		public void SetTarget(Transform newTarget)
		{
			target = newTarget;
			lookAtPoint = (previousTargetPosition = target.position);
			lookAtVelocity = (previousTargetVelocity = Vector3.zero);
			base.transform.LookAt(lookAtPoint);
		}

		private void OnEnable()
		{
			if ((bool)target)
			{
				SetTarget(target);
			}
		}

		private void FixedUpdate()
		{
			if ((bool)target)
			{
				Vector3 vector = previousTargetPosition + (1f + targetLookAhead) * previousTargetVelocity * Time.fixedDeltaTime;
				Vector3 vector2 = (vector - lookAtPoint) / Time.fixedDeltaTime;
				lookAtVelocity = lookAtVelocity * velocityWeight + vector2 * (1f - velocityWeight);
				lookAtPoint += lookAtVelocity * Time.fixedDeltaTime;
				lookAtPoint = (1f - targetPositionWeight) * lookAtPoint + targetPositionWeight * target.position;
				previousTargetVelocity = (target.position - previousTargetPosition) / Time.fixedDeltaTime;
				previousTargetPosition = target.position;
				float magnitude = (base.transform.position - lookAtPoint).magnitude;
				float num = Mathf.Atan(targetSize * 0.5f / magnitude);
				base.GetComponent<Camera>().fieldOfView = Mathf.Clamp(num * 2f * 180f / (float)Math.PI, minFov, maxFov);
				base.transform.LookAt(lookAtPoint);
			}
		}

		private void ChangeCamera(Transform other)
		{
			if (other == target || other.IsChildOf(target))
			{
				Camera main = Camera.main;
				if ((bool)main)
				{
					main.enabled = false;
					main.GetComponent<AudioListener>().enabled = false;
				}
				base.GetComponent<Camera>().enabled = true;
				base.GetComponent<Camera>().GetComponent<AudioListener>().enabled = true;
			}
		}
	}
}
