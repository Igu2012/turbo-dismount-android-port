#pragma warning disable 0618,0619
using UnityEngine;
using reLive;

namespace Dismount
{
	public class ProximityTrigger : MonoBehaviour
	{
		private Transform thisTransform;

		private Rigidbody thisRigidbody;

		private BoxCollider thisCollider;

		private Cop thisCop;

		private Vector3 originalCenter;

		private Vector3 originalSize;

		public bool isPlayer;

		private float previousAdjustmentSqrSpeed;

		private float previousAdjustmentDot;

		private int init = 10;

		private void Awake()
		{
			thisTransform = base.transform;
			thisRigidbody = base.transform.parent.GetComponent<Rigidbody>();
			thisCollider = base.GetComponent<Collider>() as BoxCollider;
			thisCop = base.transform.parent.GetComponent<Cop>();
			originalCenter = thisCollider.center;
			originalSize = thisCollider.size;
			base.gameObject.AddComponent<DoNotRecordMeInReplay>();
		}

		private void FixedUpdate()
		{
			if (thisRigidbody.isKinematic && --init < 0)
			{
				return;
			}
			Vector3 vector = thisRigidbody.velocity;
			float num = Vector3.Dot(vector, thisTransform.forward);
			float sqrMagnitude = vector.sqrMagnitude;
			if (!(Mathf.Abs(num - previousAdjustmentDot) < 2f) || !(Mathf.Abs(sqrMagnitude - previousAdjustmentSqrSpeed) < 9f))
			{
				previousAdjustmentDot = num;
				previousAdjustmentSqrSpeed = sqrMagnitude;
				if (sqrMagnitude > 2500f)
				{
					vector = 50f * vector.normalized;
				}
				Vector3 direction = vector * 2f * Time.fixedDeltaTime;
				direction = thisTransform.InverseTransformDirection(direction);
				thisCollider.center = originalCenter + direction * 0.5f;
				direction.x = Mathf.Abs(direction.x);
				direction.y = Mathf.Abs(direction.y);
				direction.z = Mathf.Abs(direction.z);
				thisCollider.size = originalSize + direction;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			Rigidbody attachedRigidbody = other.attachedRigidbody;
			if (attachedRigidbody == null || attachedRigidbody == thisRigidbody || !attachedRigidbody.useGravity)
			{
				return;
			}
			if (isPlayer)
			{
				if (attachedRigidbody.isKinematic)
				{
					Cop component = attachedRigidbody.GetComponent<Cop>();
					if (component != null && component.isTrafficCop)
					{
						component.SetDynamic();
					}
					else
					{
						attachedRigidbody.isKinematic = false;
					}
				}
			}
			else if (!attachedRigidbody.isKinematic && thisRigidbody.isKinematic)
			{
				if (thisCop != null && thisCop.isTrafficCop)
				{
					thisCop.SetDynamic();
				}
				else
				{
					thisRigidbody.isKinematic = false;
				}
			}
		}

		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				DrawGizmo();
			}
		}

		private void OnDrawGizmosSelected()
		{
			DrawGizmo();
		}

		private void DrawGizmo()
		{
			BoxCollider boxCollider = base.GetComponent<Collider>() as BoxCollider;
			if (!(boxCollider == null))
			{
				if (thisRigidbody == null || !thisRigidbody.isKinematic)
				{
					Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
				}
				else
				{
					Gizmos.color = new Color(1f, 0f, 1f, 0.15f);
				}
				Gizmos.matrix = base.transform.localToWorldMatrix;
				Gizmos.DrawCube(boxCollider.center, boxCollider.size);
			}
		}
	}
}
