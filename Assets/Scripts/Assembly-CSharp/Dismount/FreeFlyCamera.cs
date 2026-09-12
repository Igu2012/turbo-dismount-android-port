#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class FreeFlyCamera : MonoBehaviour
	{
		private const float minFov = 5f;

		private const float maxFov = 90f;

		private const float defaultFov = 50f;

		private const float mouseLookSpeed = 2.5f;

		private const float moveSpeed = 10f;

		private Vector3 targetMove = Vector3.zero;

		private Vector3 move = Vector3.zero;

		private Vector3 targetEulers = Vector3.zero;

		private Vector3 eulers = Vector3.zero;

		private float targetFov = 50f;

		[HideInInspector]
		public bool isMouseLook;

		private bool replayMode;

		private bool setupMode;

		private Transform xform;

		private float orientationAnimationStartTime = -1f;

		private Quaternion startOrientation;

		private Quaternion targetOrientation;

		public Vector3 position
		{
			get
			{
				return xform.position;
			}
			set
			{
				xform.position = value;
			}
		}

		public Vector3 euler
		{
			get
			{
				return eulers;
			}
			set
			{
				eulers = (targetEulers = value);
			}
		}

		public float fov
		{
			get
			{
				return base.GetComponent<Camera>().fieldOfView;
			}
			set
			{
				base.GetComponent<Camera>().fieldOfView = (targetFov = value);
			}
		}

		private void Awake()
		{
			xform = base.transform;
			if (GetComponent<SceneSetupCamera>() != null)
			{
				position = base.transform.position;
				euler = base.transform.rotation.eulerAngles;
				fov = GetComponent<Camera>().fieldOfView;
				setupMode = true;
			}
		}

		private void FlattenZOrientation()
		{
			if (Mathf.Abs(eulers.z) > 0.01f && orientationAnimationStartTime == -1f)
			{
				orientationAnimationStartTime = Time.time;
				startOrientation = base.transform.rotation;
				targetOrientation = default(Quaternion);
				targetOrientation.eulerAngles = new Vector3(eulers.x, eulers.y, 0f);
			}
			else
			{
				orientationAnimationStartTime = -1f;
			}
		}

		private void EnterMouseLook()
		{
			if (!(DismountGame.uiManager == null) && !DismountGame.uiManager.IsMouseObstructedByUI())
			{
				isMouseLook = true;
				Screen.lockCursor = true;
				eulers = xform.eulerAngles;
				if (eulers.x > 180f)
				{
					eulers.x -= 360f;
				}
				FlattenZOrientation();
				targetEulers = eulers;
			}
		}

		private void ExitMouseLook()
		{
			isMouseLook = false;
			if (Screen.lockCursor)
			{
				Screen.lockCursor = false;
			}
		}

		private void Update()
		{
			if (orientationAnimationStartTime != -1f)
			{
				float num = Time.time - orientationAnimationStartTime;
				if (num < 0.25f)
				{
					eulers = Quaternion.Lerp(startOrientation, targetOrientation, num / 0.25f).eulerAngles;
				}
				else
				{
					orientationAnimationStartTime = -1f;
					eulers.z = 0f;
				}
				targetEulers = eulers;
				return;
			}
			if (SXInputManager.GetMouseButtonDown(1) || (SXInputManager.GetMouseButtonDown(0) && !setupMode))
			{
				EnterMouseLook();
			}
			if (SXInputManager.GetMouseButtonUp(1) || (SXInputManager.GetMouseButtonUp(0) && !setupMode))
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
				targetMove += Vector3.forward;
			}
			if (Input.GetKey(KeyCode.A))
			{
				targetMove += Vector3.left;
			}
			if (Input.GetKey(KeyCode.S))
			{
				targetMove += Vector3.back;
			}
			if (Input.GetKey(KeyCode.D))
			{
				targetMove += Vector3.right;
			}
			if (Input.GetKey(KeyCode.Q))
			{
				targetMove += Vector3.down;
			}
			if (Input.GetKey(KeyCode.E))
			{
				targetMove += Vector3.up;
			}
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				targetMove *= 5f;
			}
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				targetMove *= 0.2f;
			}
			if ((bool)DismountGame.uiManager && DismountGame.uiManager.CurrentState != UIManager.State.SetupVehicle)
			{
				targetFov -= SXInputManager.GetMouseScrollWheel() * 5f;
			}
			float threshold = 0.2f;
			float num2 = Time.smoothDeltaTime * 100f;
			float num3 = Time.smoothDeltaTime * 30f;
			if (DismountGame.uiManager != null && DismountGame.uiManager.CurrentState != UIManager.State.LevelSelect && DismountGame.uiManager.CurrentState != UIManager.State.SetupCharacter && DismountGame.uiManager.CurrentState != UIManager.State.SetupVehicle && DismountGame.uiManager.CurrentState != UIManager.State.SetupObstacle && DismountGame.uiManager.CurrentState != UIManager.State.CustomizeCharacter && DismountGame.uiManager.CurrentState != UIManager.State.Paused)
			{
				float num4 = SXInputManager.GetAxisValue(SXInputManager.Axis.LSTICKH, threshold);
				float num5 = SXInputManager.GetAxisValue(SXInputManager.Axis.LSTICKV, threshold);
				if (num4 != 0f || num5 != 0f)
				{
					FlattenZOrientation();
				}
				if (SXInputManager.GetButton(SXInputManager.Button.Y))
				{
					num4 = 0f;
					num5 = 0f;
				}
				targetMove += Vector3.right * num4 * num2;
				targetMove += Vector3.forward * num5 * num2;
			}
			float num6 = Mathf.Clamp01(SXInputManager.GetAxisValue(SXInputManager.Axis.L2, threshold)) - Mathf.Clamp01(SXInputManager.GetAxisValue(SXInputManager.Axis.R2, threshold));
			if (SXInputManager.GetButton(SXInputManager.Button.L3))
			{
				targetMove += Vector3.up * num6 * num2;
			}
			else if (SXInputManager.GetButton(SXInputManager.Button.R3))
			{
				targetFov += num6 * num3;
			}
			float num7 = (0f - SXInputManager.GetAxisValue(SXInputManager.Axis.RSTICKV, threshold)) * num3;
			float num8 = (0f - SXInputManager.GetAxisValue(SXInputManager.Axis.RSTICKH, threshold)) * num3;
			if (num7 != 0f || num8 != 0f)
			{
				FlattenZOrientation();
			}
			if (SXInputManager.GetButton(SXInputManager.Button.Y))
			{
				num7 = 0f;
				num8 = 0f;
			}
			bool flag = num7 != 0f || num8 != 0f;
			targetFov = Mathf.Clamp(targetFov, 5f, 90f);
			if (flag || isMouseLook)
			{
				float num9 = 1f;
				if (targetFov < 50f)
				{
					num9 = 0.7f + (targetFov - 5f) / 45f * 0.3f;
				}
				else if (targetFov > 50f)
				{
					num9 = 1f + (targetFov - 50f) / 40f * 0.5f;
				}
				num9 *= 2.5f;
				Vector3 vector = new Vector3(0f - SXInputManager.GetMouseY(), SXInputManager.GetMouseX(), 0f);
				vector += new Vector3(num7, num8, 0f);
				targetEulers += vector * num9;
				if (targetEulers.x > 89.5f)
				{
					targetEulers.x = 89.5f;
				}
				else if (targetEulers.x < -89.5f)
				{
					targetEulers.x = -89.5f;
				}
			}
		}

		private void EnterReplayMode()
		{
			replayMode = true;
		}

		private void ExitReplayMode()
		{
			replayMode = false;
		}

		private void LateUpdate()
		{
			if (replayMode)
			{
				UpdateCamera(Time.deltaTime);
			}
		}

		private void FixedUpdate()
		{
			if (!replayMode)
			{
				UpdateCamera(Time.fixedDeltaTime);
			}
		}

		private void UpdateCamera(float deltaTime)
		{
			float num = 5f * deltaTime;
			if (num > 1f)
			{
				num = 1f;
			}
			eulers = Vector3.Lerp(eulers, targetEulers, num * 4f);
			xform.eulerAngles = eulers;
			move = Vector3.Lerp(move, targetMove, num * 2f);
			xform.Translate(move * deltaTime * 10f, Space.Self);
			if (xform.position.y < 0.6f)
			{
				xform.position = new Vector3(xform.position.x, 0.6f, xform.position.z);
			}
			base.GetComponent<Camera>().fieldOfView = Mathf.Lerp(base.GetComponent<Camera>().fieldOfView, targetFov, num);
		}
	}
}
