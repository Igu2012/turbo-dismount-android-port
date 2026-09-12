#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class FirstPersonCamera : MonoBehaviour
	{
		private Vector3 forwardDir;

		private Vector3 upDir;

		private Vector3 mouseDownPos;

		private Vector3 mouseOffset;

		private Vector3 lookOffset;

		public Vector3 eyeOffset = Vector3.zero;

		public float maxLookAngle = 60f;

		private bool isDragging;

		public void SetTarget(Transform target)
		{
			base.transform.parent = target;
			base.transform.localPosition = eyeOffset;
			forwardDir = target.forward;
			upDir = target.up;
		}

		private void Update()
		{
			if ((SXInputManager.GetMouseButtonDown(0) || SXInputManager.GetMouseButtonDown(1)) && DismountGame.uiManager != null && !DismountGame.uiManager.IsMouseObstructedByUI())
			{
				isDragging = true;
				mouseDownPos = Input.mousePosition;
			}
			if (SXInputManager.GetMouseButtonUp(0) || SXInputManager.GetMouseButtonUp(1))
			{
				isDragging = false;
			}
			mouseOffset = Vector3.zero;
			if (isDragging)
			{
				float num = 1f / (float)Mathf.Min(Screen.width, Screen.height);
				mouseOffset = Input.mousePosition - mouseDownPos;
				mouseOffset.x *= num;
				mouseOffset.y *= num;
			}
		}

		private void LateUpdate()
		{
			float threshold = 0.2f;
			float num = 0f - SXInputManager.GetAxisValue(SXInputManager.Axis.RSTICKH, threshold);
			float axisValue = SXInputManager.GetAxisValue(SXInputManager.Axis.RSTICKV, threshold);
			if (num != 0f || axisValue != 0f)
			{
				lookOffset.x = num;
				lookOffset.y = axisValue;
			}
			else if (isDragging)
			{
				lookOffset.x = Mathf.Clamp(mouseOffset.x, -0.5f, 0.5f);
				lookOffset.x *= 2f;
				lookOffset.y = Mathf.Clamp(mouseOffset.y, -0.5f, 0.5f);
				lookOffset.y *= 2f;
			}
			else
			{
				float num2 = Time.deltaTime * 5f;
				if (num2 > 1f)
				{
					num2 = 1f;
				}
				lookOffset.x *= 1f - num2;
				lookOffset.y *= 1f - num2;
			}
			if ((bool)base.transform.parent)
			{
				float num3 = Time.deltaTime * 5f;
				if (num3 > 1f)
				{
					num3 = 1f;
				}
				forwardDir = Vector3.Lerp(forwardDir, base.transform.parent.forward, num3);
				upDir = Vector3.Lerp(upDir, base.transform.parent.up, num3);
				base.transform.LookAt(base.transform.position + forwardDir, upDir);
			}
			base.transform.Rotate(lookOffset.y * (0f - maxLookAngle), lookOffset.x * maxLookAngle, 0f, Space.Self);
		}
	}
}
