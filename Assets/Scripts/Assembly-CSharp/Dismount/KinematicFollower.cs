#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(Rigidbody))]
	public class KinematicFollower : MonoBehaviour
	{
		public Transform followTarget;

		private void FixedUpdate()
		{
			base.GetComponent<Rigidbody>().MovePosition(followTarget.position);
			base.GetComponent<Rigidbody>().MoveRotation(followTarget.rotation);
		}
	}
}
