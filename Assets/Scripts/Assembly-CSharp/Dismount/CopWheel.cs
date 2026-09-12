#pragma warning disable 0618,0619
using UnityEngine;
using reLive;

namespace Dismount
{
	public class CopWheel : MonoBehaviour
	{
		private const float initialSuspensionOffset = 0.11f;

		private const float suspensionTravel = 0.15f;

		[HideInInspector]
		public Transform thisTransform;

		[HideInInspector]
		public ExtendedLocalEulers extendedLocalEulers;

		public Transform visualWheel;

		public bool isFrontWheel;

		private float suspensionRestPosition;

		public float vel;

		public float visualPosition
		{
			get
			{
				float y = thisTransform.localPosition.y;
				return Mathf.Clamp(y, suspensionRestPosition - 0.3f, suspensionRestPosition + 0.3f);
			}
		}

		public float load
		{
			get
			{
				float value = thisTransform.localPosition.y - suspensionRestPosition;
				value = Mathf.Clamp(value, 0f, 0.15f);
				value /= 0.15f;
				return value * value;
			}
		}

		private void Awake()
		{
			thisTransform = base.transform;
			extendedLocalEulers = visualWheel.GetComponent<ExtendedLocalEulers>();
			suspensionRestPosition = thisTransform.localPosition.y - 0.11f;
		}

		private void OnDrawGizmosSelected()
		{
			if ((bool)thisTransform)
			{
				Color color = new Color(0f, 0f, 0f, 0.5f);
				color.r = Mathf.Min(1f, load * 2f);
				color.g = Mathf.Min(1f, (1f - load) * 2f);
				Gizmos.color = color;
				Gizmos.DrawSphere(base.transform.position, (base.GetComponent<Collider>() as SphereCollider).radius);
			}
		}

		public void DebugDraw()
		{
			GLDraw.Begin();
			GL.MultMatrix(base.transform.localToWorldMatrix);
			Color color = new Color(0f, 0f, 0f, 0.5f);
			color.r = Mathf.Min(1f, load * 2f);
			color.g = Mathf.Min(1f, (1f - load) * 2f);
			GLDraw.Sphere(Vector3.zero, (base.GetComponent<Collider>() as SphereCollider).radius, color, true);
			GLDraw.End();
		}
	}
}
