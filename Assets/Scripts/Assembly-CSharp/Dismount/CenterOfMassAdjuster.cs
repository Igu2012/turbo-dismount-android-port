#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(Rigidbody))]
	public class CenterOfMassAdjuster : MonoBehaviour
	{
		public Vector3 centerOfMass = Vector3.zero;

		private void Start()
		{
			base.GetComponent<Rigidbody>().centerOfMass = centerOfMass;
		}

		private void Update()
		{
			if (base.GetComponent<Rigidbody>().centerOfMass != centerOfMass)
			{
				base.GetComponent<Rigidbody>().centerOfMass = centerOfMass;
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawSphere(centerOfMass, 0.25f);
		}
	}
}
