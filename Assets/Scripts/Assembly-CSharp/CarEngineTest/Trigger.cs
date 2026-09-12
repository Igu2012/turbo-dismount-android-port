#pragma warning disable 0618,0619
using UnityEngine;

namespace CarEngineTest
{
	public class Trigger : MonoBehaviour
	{
		public GameObject messageTarget;

		public string message = string.Empty;

		private void OnTriggerEnter(Collider other)
		{
			messageTarget.SendMessage(message, SendMessageOptions.DontRequireReceiver);
		}
	}
}
