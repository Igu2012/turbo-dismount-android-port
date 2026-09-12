#pragma warning disable 0618,0619
using UnityEngine;

namespace reLive
{
	public class RetainHierarchy : MonoBehaviour
	{
		private void Awake()
		{
			Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>(true);
			foreach (Transform transform in componentsInChildren)
			{
				if (!(transform == base.transform) && !transform.gameObject.GetComponent<RecordLocalTransformation>())
				{
					transform.gameObject.AddComponent<RecordLocalTransformation>();
				}
			}
		}
	}
}
