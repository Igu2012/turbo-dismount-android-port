#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class ReplayCamera : MonoBehaviour
	{
		public Transform target;

		public float minDistance = 1f;

		public float maxDistance = 100f;

		public float yMinLimit = -20f;

		public float yMaxLimit = 80f;

		public float zoomSpeed = 5f;

		public float minY = 3f;

		private Vector3 dragStartPosition;

		private bool dragging;

		private bool pinching;

		private float x = 180f;

		private float y = 20f;

		public float startAngle;

		public float distance = 3f;

		private float targetDistance;

		private float zoomDistance;

		private bool cameraObstructed;

		private void Awake()
		{
			zoomDistance = (targetDistance = distance);
		}

		private void Start()
		{
			if ((bool)target)
			{
				x = target.transform.rotation.eulerAngles.y + startAngle;
			}
		}

		private void OnEnable()
		{
			if ((bool)target)
			{
				Quaternion quaternion = Quaternion.Euler(y, x, 0f);
				Vector3 position = quaternion * new Vector3(0f, 0f, 0f - distance) + target.position;
				base.transform.rotation = quaternion;
				base.transform.position = position;
			}
			else
			{
				Debug.LogError("No camera target set!");
			}
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

		private void LateUpdate()
		{
			if (SXInputManager.GetMouseButtonDown(0) && Input.mousePosition.y > (float)Screen.height * 0.3f)
			{
				dragging = true;
				dragStartPosition = Input.mousePosition;
			}
			if (SXInputManager.GetMouseButtonUp(0))
			{
				dragging = false;
			}
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
				Vector3 vector = Camera.main.ViewportToWorldPoint(new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, Camera.main.nearClipPlane));
				Vector3 vector2 = Camera.main.ViewportToWorldPoint(new Vector3(Input.touches[0].position.x - Input.touches[0].deltaPosition.x, Input.touches[0].position.y - Input.touches[0].deltaPosition.y, Camera.main.nearClipPlane));
				Vector3 vector3 = Camera.main.ViewportToWorldPoint(new Vector3(Input.touches[1].position.x, Input.touches[1].position.y, Camera.main.nearClipPlane));
				Vector3 vector4 = Camera.main.ViewportToWorldPoint(new Vector3(Input.touches[1].position.x - Input.touches[1].deltaPosition.x, Input.touches[1].position.y - Input.touches[1].deltaPosition.y, Camera.main.nearClipPlane));
				Vector3 vector5 = vector - vector3;
				Vector3 vector6 = vector2 - vector4;
				float num = vector5.magnitude - vector6.magnitude;
				zoomDistance -= num * 0.2f;
				if (zoomDistance < minDistance)
				{
					zoomDistance = minDistance;
				}
				if (zoomDistance > maxDistance)
				{
					zoomDistance = maxDistance;
				}
			}
			if (SXInputManager.GetMouseScrollWheel() < 0f && zoomDistance < maxDistance)
			{
				zoomDistance += zoomSpeed;
			}
			if (SXInputManager.GetMouseScrollWheel() > 0f && zoomDistance > minDistance)
			{
				zoomDistance -= zoomSpeed;
			}
			if (dragging)
			{
				Vector3 vector7 = Input.mousePosition - dragStartPosition;
				x += vector7.x;
				y -= vector7.y;
			}
			if (!cameraObstructed)
			{
				targetDistance = zoomDistance;
			}
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			Quaternion quaternion = Quaternion.Euler(y, x, 0f);
			if ((bool)target && Camera.main == base.GetComponent<Camera>())
			{
				Vector3 position = quaternion * new Vector3(0f, 0f, 0f - targetDistance) + target.position;
				base.transform.rotation = quaternion;
				base.transform.position = position;
				dragStartPosition = Input.mousePosition;
				int layerMask = 4096;
				float num2 = targetDistance - 0.5f;
				RaycastHit hitInfo;
				if (Physics.SphereCast(target.position - base.transform.forward * 0.5f, 0.25f, -base.transform.forward, out hitInfo, num2, layerMask))
				{
					float num3 = hitInfo.distance;
					targetDistance = num3 + 0.5f + 0.125f;
					cameraObstructed = true;
				}
				else
				{
					cameraObstructed = false;
				}
			}
			if (cameraObstructed)
			{
				distance = distance * 0.7f + targetDistance * 0.3f;
			}
			else
			{
				distance = distance * 0.9f + targetDistance * 0.1f;
			}
			Vector3 position2 = quaternion * new Vector3(0f, 0f, 0f - distance) + target.position;
			base.transform.position = position2;
			if (position2.y < minY)
			{
				position2 = new Vector3(position2.x, minY, position2.z);
				base.transform.position = position2;
				base.transform.LookAt(target.position, Vector3.up);
				y = base.transform.rotation.eulerAngles.x;
			}
		}
	}
}
