#pragma warning disable 0618,0619
using UnityEngine;

namespace CarEngineTest
{
	public class CameraTrigger : MonoBehaviour
	{
		public GameObject triggerTarget;

		private void OnTriggerEnter(Collider other)
		{
			if ((bool)triggerTarget)
			{
				triggerTarget.SendMessage("ChangeCamera", other.transform, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				SendMessageUpwards("ChangeCamera", other.transform, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
