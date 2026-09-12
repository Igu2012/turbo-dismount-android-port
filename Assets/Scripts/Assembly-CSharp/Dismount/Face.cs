#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class Face : MonoBehaviour
	{
		public Transform target;

		public Vector3 position;

		public GameObject realPositionTracker;

		private Transform headTransform;

		private int tick;

		private void Awake()
		{
			headTransform = base.transform.parent;
		}

		private void LateUpdate()
		{
			tick++;
			if ((bool)DismountGame.cameraManager.currentCamera)
			{
				target = DismountGame.cameraManager.currentCamera.transform;
				Vector3 up = target.up;
				up = target.up * Mathf.Sign(Vector3.Dot(headTransform.up, target.up));
				base.transform.LookAt(target, up);
			}
		}
	}
}
