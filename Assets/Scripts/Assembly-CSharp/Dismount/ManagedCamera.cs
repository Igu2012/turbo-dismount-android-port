#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class ManagedCamera : MonoBehaviour
	{
		public string groupId;

		private void Awake()
		{
			base.GetComponent<Camera>().enabled = false;
		}
	}
}
