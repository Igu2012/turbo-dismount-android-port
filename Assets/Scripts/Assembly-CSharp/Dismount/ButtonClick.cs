#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class ButtonClick : MonoBehaviour
	{
		public string targetGameObjectName = string.Empty;

		public string message = string.Empty;

		public string releaseMessage = string.Empty;

		public bool broadcast;

		public bool sendPressAndRelease;

		public void OnPress(bool isPressed)
		{
			if (isPressed)
			{
				if (broadcast)
				{
					Utils.BroadcastMessage(targetGameObjectName, message, base.gameObject);
				}
				else
				{
					Utils.SendMessage(targetGameObjectName, message, base.gameObject);
				}
			}
			else if (sendPressAndRelease)
			{
				if (broadcast)
				{
					Utils.BroadcastMessage(targetGameObjectName, releaseMessage, base.gameObject);
				}
				else
				{
					Utils.SendMessage(targetGameObjectName, releaseMessage, base.gameObject);
				}
			}
		}

		public void OnClickFoo()
		{
			OnPress(true);
		}
	}
}
