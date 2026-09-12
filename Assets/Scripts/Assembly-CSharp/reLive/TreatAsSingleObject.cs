#pragma warning disable 0618,0619
using UnityEngine;

namespace reLive
{
	public class TreatAsSingleObject : MonoBehaviour
	{
		private void Awake()
		{
			Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>(true);
			foreach (Transform transform in componentsInChildren)
			{
				if (!(transform == base.transform) && !transform.gameObject.GetComponent<DontNeedRecorder>())
				{
					transform.gameObject.AddComponent<DontNeedRecorder>();
				}
			}
		}
	}
}
