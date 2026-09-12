#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount.PrefabBuilder
{
	[RequireComponent(typeof(Camera))]
	public class FreeCamera : MonoBehaviour
	{
		private const float minFov = 5f;

		private const float maxFov = 120f;

		private const float mouseLookSpeed = 2.5f;

		private const float moveSpeed = 10f;

		private const float defaultFov = 50f;

		[Range(0f, 0.99f)]
		public float smoothing = 0.9f;

		private Vector3 targetMove = Vector3.zero;

		private Vector3 move = Vector3.zero;

		private Vector3 targetEulers = Vector3.zero;

		private Vector3 eulers = Vector3.zero;

		private float targetFov = 50f;

		private bool isMouseLook;

		private Camera thisCamera;

		private void Awake()
		{
			thisCamera = GetComponent<Camera>();
			move = (targetMove = Vector3.zero);
			eulers = (targetEulers = base.transform.eulerAngles);
			targetFov = thisCamera.fieldOfView;
		}

		private void EnterMouseLook()
		{
			isMouseLook = true;
			eulers = base.transform.eulerAngles;
			if (eulers.x > 180f)
			{
				eulers.x -= 360f;
			}
			targetEulers = eulers;
		}

		private void ExitMouseLook()
		{
			isMouseLook = false;
		}

		private void Update()
		{
			if (SXInputManager.GetMouseButtonDown(1))
			{
				EnterMouseLook();
			}
			if (SXInputManager.GetMouseButtonUp(1))
			{
				ExitMouseLook();
			}
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				ExitMouseLook();
			}
			targetMove = Vector3.zero;
			if (Input.GetKey(KeyCode.W))
			{
				targetMove.z++;
			}
			if (Input.GetKey(KeyCode.S))
			{
				targetMove.z--;
			}
			if (Input.GetKey(KeyCode.A))
			{
				targetMove.x--;
			}
			if (Input.GetKey(KeyCode.D))
			{
				targetMove.x++;
			}
			if (Input.GetKey(KeyCode.Q))
			{
				targetMove.y--;
			}
			if (Input.GetKey(KeyCode.E))
			{
				targetMove.y++;
			}
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				targetMove *= 5f;
			}
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				targetMove *= 0.2f;
			}
			float mouseScrollWheel = SXInputManager.GetMouseScrollWheel();
			if (mouseScrollWheel != 0f)
			{
				targetFov -= Mathf.Sign(mouseScrollWheel) * 5f;
				targetFov = Mathf.Clamp(targetFov, 5f, 120f);
			}
			if (isMouseLook)
			{
				float num = Mathf.Sin(targetFov * ((float)Math.PI / 180f) * 0.5f) / Mathf.Sin(0.43633232f);
				num *= 2.5f;
				Vector3 vector = new Vector3(0f - SXInputManager.GetMouseY(), SXInputManager.GetMouseX(), 0f);
				targetEulers += vector * num;
				if (targetEulers.x > 89.5f)
				{
					targetEulers.x = 89.5f;
				}
				else if (targetEulers.x < -89.5f)
				{
					targetEulers.x = -89.5f;
				}
			}
			SmoothUpdate(Time.deltaTime);
		}

		private void SmoothUpdate(float dt)
		{
			float geometricRatio = GetGeometricRatio(1f - smoothing, 0.01f, dt);
			eulers = Vector3.Lerp(eulers, targetEulers, geometricRatio);
			base.transform.eulerAngles = eulers;
			move = Vector3.Lerp(move, targetMove, geometricRatio);
			base.transform.Translate(move * dt * 10f, Space.Self);
			thisCamera.fieldOfView = Mathf.Lerp(thisCamera.fieldOfView, targetFov, geometricRatio * 0.5f);
		}

		private float GetGeometricRatio(float referenceRatio, float referenceDt, float targetDt)
		{
			return 1f - Mathf.Pow(1f - referenceRatio, targetDt / referenceDt);
		}
	}
}
