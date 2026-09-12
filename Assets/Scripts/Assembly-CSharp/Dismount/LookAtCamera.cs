#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class LookAtCamera : MonoBehaviour
	{
		private void LateUpdate()
		{
			Transform transform = DismountGame.cameraManager.currentCamera.transform;
			base.transform.LookAt(transform, transform.up);
		}
	}
}
