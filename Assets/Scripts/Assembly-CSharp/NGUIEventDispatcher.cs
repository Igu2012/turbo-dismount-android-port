#pragma warning disable 0618,0619
using UnityEngine;

public class NGUIEventDispatcher : MonoBehaviour
{
	public struct EventData
	{
		public GameObject source;

		public string parameter;
	}

	public GameObject Receiver;

	public string Parameter;

	public void OnClick()
	{
		if ((bool)Receiver)
		{
			EventData eventData = default(EventData);
			eventData.source = base.gameObject;
			eventData.parameter = Parameter;
			Receiver.SendMessage("OnClick", eventData, SendMessageOptions.RequireReceiver);
		}
	}
}
