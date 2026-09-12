#pragma warning disable 0618,0619
using UnityEngine;

namespace reLive
{
	public class ExtendedLocalEulers : MonoBehaviour
	{
		public Vector3 localEulers;

		[HideInInspector]
		public Transform thisTransform;

		private void Awake()
		{
			thisTransform = base.transform;
		}
	}
}
