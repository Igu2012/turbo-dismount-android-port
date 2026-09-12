#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(Camera))]
	public class PinchZoomCamera : MonoBehaviour
	{
		public int speed = 4;

		public int minFov = 15;

		public int maxFov = 90;

		public float minPinchSpeed = 5f;

		public float varianceInDistances = 5f;

		private float touchDelta;

		private Vector2 prevDist = new Vector2(0f, 0f);

		private Vector2 curDist = new Vector2(0f, 0f);

		private float speedTouch0;

		private float speedTouch1;

		private SceneSetupCamera sceneSetupCamera;

		private bool isSetupCamera;

		private void Start()
		{
			sceneSetupCamera = GetComponent<SceneSetupCamera>();
			if (sceneSetupCamera != null)
			{
				isSetupCamera = true;
			}
		}

		private void FixedUpdate()
		{
			if (isSetupCamera)
			{
				return;
			}
			if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
			{
				curDist = Input.GetTouch(0).position - Input.GetTouch(1).position;
				prevDist = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);
				touchDelta = curDist.magnitude - prevDist.magnitude;
				speedTouch0 = Input.GetTouch(0).deltaPosition.magnitude / Input.GetTouch(0).deltaTime;
				speedTouch1 = Input.GetTouch(1).deltaPosition.magnitude / Input.GetTouch(1).deltaTime;
				if (touchDelta + varianceInDistances <= 1f && speedTouch0 > minPinchSpeed && speedTouch1 > minPinchSpeed)
				{
					base.GetComponent<Camera>().fieldOfView = Mathf.Clamp(base.GetComponent<Camera>().fieldOfView + (float)(1 * speed), minFov, maxFov);
				}
				if (touchDelta + varianceInDistances > 1f && speedTouch0 > minPinchSpeed && speedTouch1 > minPinchSpeed)
				{
					base.GetComponent<Camera>().fieldOfView = Mathf.Clamp(base.GetComponent<Camera>().fieldOfView - (float)(1 * speed), minFov, maxFov);
				}
			}
			else if (SXInputManager.GetMouseScrollWheel() < 0f && (bool)DismountGame.uiManager && DismountGame.uiManager.CurrentState != UIManager.State.SetupVehicle)
			{
				base.GetComponent<Camera>().fieldOfView = Mathf.Clamp(base.GetComponent<Camera>().fieldOfView + (float)(1 * speed), minFov, maxFov);
			}
			else if (SXInputManager.GetMouseScrollWheel() > 0f && (bool)DismountGame.uiManager && DismountGame.uiManager.CurrentState != UIManager.State.SetupVehicle)
			{
				base.GetComponent<Camera>().fieldOfView = Mathf.Clamp(base.GetComponent<Camera>().fieldOfView - (float)(1 * speed), minFov, maxFov);
			}
		}
	}
}
