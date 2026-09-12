#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class MoveToDynamicRoot : MonoBehaviour
	{
		private void OnDismountStarted()
		{
			GameObject gameObject = GameObject.Find("SceneDynamic");
			if ((bool)gameObject)
			{
				base.transform.parent = gameObject.transform;
			}
		}
	}
}
