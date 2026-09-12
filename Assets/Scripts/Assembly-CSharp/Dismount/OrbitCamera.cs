#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount
{
	public class OrbitCamera : MonoBehaviour
	{
		public Transform target;

		public float yOffset;

		public float minDistance = 3f;

		public float maxDistance = 50f;

		public float minXAngle = -80f;

		public float maxXAngle = 80f;

		public float minYPosition = 2f;

		public float mouseZoomSpeed = 3f;

		public Vector3 startAngles = Vector3.zero;

		public float startDistance = 8f;

		private float targetDistance;

		private float currDistance;

		private Vector3 dragStartEuler;

		private Vector3 targetEuler;

		private Vector3 currEuler;

		private Vector3 dragStartPosition;

		private bool dragging;

		private bool pinching;

		private float pinchReferenceSize = 1f;

		private float pinchReferenceDistance = 10f;

		private LayerMask avoidLayers;

		private bool replayMode;

		private float smoothingWeight = 1f;

		private bool requestedChaseMode;

		private Vector3 chaseCamCurrPosition = Vector3.zero;

		private Vector3 chaseCamTargetPosition = Vector3.zero;

		private Transform customCameraTarget;

		public bool chaseMode
		{
			get
			{
				return requestedChaseMode && !replayMode;
			}
			set
			{
				requestedChaseMode = value;
			}
		}

		public float distance
		{
			get
			{
				return targetDistance;
			}
			set
			{
				currDistance = (targetDistance = value);
			}
		}

		public Vector3 euler
		{
			get
			{
				return targetEuler;
			}
			set
			{
				currEuler = (targetEuler = value);
			}
		}

		public void SetChaseCameraDistanceAndAngle(float chaseDistance, float chaseAngle)
		{
			targetDistance = chaseDistance;
			targetEuler.x = chaseAngle;
		}

		private void Awake()
		{
			targetDistance = (currDistance = startDistance);
			currEuler = (targetEuler = (dragStartEuler = startAngles));
			avoidLayers = (1 << LayerMask.NameToLayer("AvoidCamera")) | (1 << LayerMask.NameToLayer("Ground"));
		}

		private float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
			return Mathf.Clamp(angle, min, max);
		}

		private void Update()
		{
			if (DismountGame.uiManager == null)
			{
				return;
			}
			if ((SXInputManager.GetMouseButtonDown(0) || SXInputManager.GetMouseButtonDown(1)) && !DismountGame.uiManager.IsMouseObstructedByUI())
			{
				dragging = true;
				dragStartPosition = Input.mousePosition;
				dragStartEuler = targetEuler;
			}
			if (SXInputManager.GetMouseButtonUp(0) || SXInputManager.GetMouseButtonUp(1))
			{
				dragging = false;
			}
			bool flag = pinching;
			if (Input.touchCount == 2)
			{
				dragging = false;
				pinching = true;
			}
			else
			{
				pinching = false;
			}
			if (pinching)
			{
				Camera camera = DismountGame.cameraManager.globalCamera.GetComponent<Camera>();
				Vector3 vector = camera.ViewportToWorldPoint(new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, camera.nearClipPlane));
				Vector3 vector2 = camera.ViewportToWorldPoint(new Vector3(Input.touches[1].position.x, Input.touches[1].position.y, camera.nearClipPlane));
				float magnitude = (vector2 - vector).magnitude;
				if (!flag)
				{
					pinchReferenceSize = magnitude;
					pinchReferenceDistance = currDistance;
				}
				targetDistance = pinchReferenceSize / magnitude * pinchReferenceDistance;
				if (targetDistance < minDistance)
				{
					targetDistance = minDistance;
				}
				if (targetDistance > maxDistance)
				{
					targetDistance = maxDistance;
				}
			}
			if (SXInputManager.GetMouseScrollWheel() < 0f && (bool)DismountGame.uiManager && DismountGame.uiManager.CurrentState != UIManager.State.SetupVehicle)
			{
				targetDistance += mouseZoomSpeed;
			}
			if (SXInputManager.GetMouseScrollWheel() > 0f && (bool)DismountGame.uiManager && DismountGame.uiManager.CurrentState != UIManager.State.SetupVehicle)
			{
				targetDistance -= mouseZoomSpeed;
			}
			float num = 0f;
			float num2 = 0f;
			float threshold = 0.2f;
			float num3 = Time.smoothDeltaTime * 25f;
			float num4 = Time.smoothDeltaTime * 100f;
			UIManager.State currentState = DismountGame.uiManager.CurrentState;
			if (currentState != UIManager.State.LevelSelect && currentState != UIManager.State.SetupCharacter && currentState != UIManager.State.SetupVehicle && currentState != UIManager.State.SetupObstacle && currentState != UIManager.State.CustomizeCharacter && currentState != UIManager.State.Paused)
			{
				num = (0f - SXInputManager.GetAxisValue(SXInputManager.Axis.RSTICKV, threshold)) * num4;
				num2 = (0f - SXInputManager.GetAxisValue(SXInputManager.Axis.RSTICKH, threshold)) * num4;
				if (!DismountGame.playerState.manualControls)
				{
					float axisValue = SXInputManager.GetAxisValue(SXInputManager.Axis.LSTICKV, threshold);
					targetDistance -= axisValue * num3;
				}
			}
			if (dragging)
			{
				Vector3 vector3 = Input.mousePosition - dragStartPosition;
				float num5 = 1f / (float)Mathf.Min(Screen.width, Screen.height);
				targetEuler.x = dragStartEuler.x + vector3.y * num5 * 180f;
				if (!chaseMode)
				{
					targetEuler.y = dragStartEuler.y + vector3.x * num5 * 180f;
				}
			}
			targetEuler.x += num;
			if (!chaseMode)
			{
				targetEuler.y += num2;
			}
			targetEuler.x = ClampAngle(targetEuler.x, minXAngle, maxXAngle);
			targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
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
				smoothingWeight = 5f * Time.deltaTime;
				if (smoothingWeight > 1f)
				{
					smoothingWeight = 1f;
				}
				UpdateCamera();
			}
		}

		private void FixedUpdate()
		{
			if (!replayMode)
			{
				smoothingWeight = 5f * Time.fixedDeltaTime;
				UpdateCamera();
			}
		}

		private void UpdateCamera()
		{
			if (!target)
			{
				return;
			}
			float num = smoothingWeight;
			if (chaseMode)
			{
				num = smoothingWeight * 3f;
			}
			if (dragging || pinching)
			{
				smoothingWeight *= 2f;
			}
			if (!chaseMode)
			{
				num = smoothingWeight;
			}
			if (smoothingWeight > 1f)
			{
				smoothingWeight = 1f;
			}
			if (num > 1f)
			{
				num = 1f;
			}
			if (chaseMode)
			{
				Vector3 forward = target.forward;
				Vector3 position = target.position;
				forward.y = 0f;
				chaseCamTargetPosition = position + forward.normalized * 20f;
				chaseCamCurrPosition = Vector3.Lerp(chaseCamCurrPosition, chaseCamTargetPosition, num);
				Vector3 to = chaseCamCurrPosition - position;
				targetEuler.y = Vector3.Angle(Vector3.back, to);
				if (to.x > 0f)
				{
					targetEuler.y *= -1f;
				}
			}
			currEuler.x = Mathf.LerpAngle(currEuler.x, targetEuler.x, smoothingWeight);
			currEuler.y = Mathf.LerpAngle(currEuler.y, targetEuler.y, num);
			Quaternion quaternion = Quaternion.Euler(currEuler);
			currDistance = Mathf.Lerp(currDistance, targetDistance, smoothingWeight);
			Vector3 vector = quaternion * new Vector3(0f, 0f, currDistance) + target.position;
			if (vector.y < minYPosition)
			{
				float num2 = target.position.y - minYPosition;
				float num3 = Mathf.Asin(Mathf.Clamp(Mathf.Abs(num2) / currDistance, -1f, 1f));
				float num4 = num2 / Mathf.Tan(num3);
				if (num2 < 0f)
				{
					num4 = 0f - num4;
					num3 = 0f - num3;
				}
				Vector3 vector2 = new Vector3(vector.x, minYPosition, vector.z);
				Vector3 vector3 = new Vector3(target.position.x, minYPosition, target.position.z);
				Vector3 normalized = (vector2 - vector3).normalized;
				vector = vector3 + normalized * num4;
				currEuler.x = (targetEuler.x = num3 / (float)Math.PI * 180f);
				quaternion = Quaternion.Euler(currEuler);
				if (dragging)
				{
					dragStartEuler.x = targetEuler.x;
					dragStartPosition.y = Input.mousePosition.y;
				}
			}
			float num5 = minYPosition;
			float num6 = -1f;
			float num7 = -1f;
			Vector3 vector4 = vector + Vector3.up * 2f;
			Vector3 vector5 = vector + Vector3.down * 2f;
			RaycastHit hitInfo;
			if (Physics.SphereCast(vector4, num5, Vector3.down, out hitInfo, 2f + num5, avoidLayers))
			{
				num6 = hitInfo.distance;
			}
			if (vector5.y > minYPosition && Physics.SphereCast(vector5, num5, Vector3.up, out hitInfo, 2f + num5, avoidLayers))
			{
				num7 = hitInfo.distance;
			}
			if (num6 > num7 && num6 > 0f)
			{
				vector = vector4 + (num6 - num5) * Vector3.down;
				if (vector.y < minYPosition)
				{
					vector.y = minYPosition;
				}
			}
			else if (num7 > num6 && num7 > 0f)
			{
				vector = vector5 + (num7 - num5) * Vector3.up;
				if (vector.y < minYPosition)
				{
					vector.y = minYPosition;
				}
			}
			base.transform.position = vector;
			base.transform.LookAt(target.position + yOffset * Vector3.up, Vector3.up);
		}
	}
}
