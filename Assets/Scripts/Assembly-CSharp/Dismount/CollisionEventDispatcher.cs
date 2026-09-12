#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class CollisionEventDispatcher : MonoBehaviour
	{
		public GameObject target;

		private void OnCollisionEnter(Collision collision)
		{
			if ((bool)target)
			{
				target.SendMessage("OnCollisionEnter", collision, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
