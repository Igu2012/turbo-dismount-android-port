#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class KeepInMainCamera : MonoBehaviour
	{
		private Transform thisTransform;

		private Transform mainCameraTransform;

		private void Start()
		{
			thisTransform = base.transform;
			mainCameraTransform = Camera.main.transform;
		}

		private void LateUpdate()
		{
			thisTransform.position = mainCameraTransform.position + mainCameraTransform.forward * 10f;
		}
	}
}
