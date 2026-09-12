#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.PrefabBuilder
{
	public class DismountMessages : MonoBehaviour
	{
		public GameObject target;

		public string[] messages;

		private void OnGUI()
		{
			if (messages == null)
			{
				return;
			}
			if (target == null)
			{
				target = base.gameObject;
			}
			Rect position = new Rect(20f, 20f, 240f, 20f);
			string empty = string.Empty;
			for (int i = 0; i < messages.Length; i++)
			{
				empty = messages[i];
				if (GUI.Button(position, empty))
				{
					target.BroadcastMessage(empty, SendMessageOptions.DontRequireReceiver);
				}
				position.y += 30f;
			}
			if (GUI.Button(position, "Reload level"))
			{
				Application.LoadLevel(Application.loadedLevel);
			}
		}
	}
}
