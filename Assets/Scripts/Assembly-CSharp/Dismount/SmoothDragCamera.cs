#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(Camera))]
	public class SmoothDragCamera : MonoBehaviour
	{
		public float dragSpeed = 0.1f;

		public float maxMoveDistance = 10f;

		private Vector3 dragOrigin;

		private Vector3 relativeStartPos;

		private Vector3 lastGoodPosition;

		private bool canMoveLeft = true;

		private bool canMoveRight = true;

		private int groundLayerMask;

		public string groundLayerName = "Ground";

		public string setupLayerName = "Setup";

		private bool maskLayerHit;

		private SceneSetupCamera sceneSetupCamera;

		private bool isSetupCamera;

		private void Start()
		{
			sceneSetupCamera = GetComponent<SceneSetupCamera>();
			if (sceneSetupCamera != null)
			{
				isSetupCamera = true;
			}
			relativeStartPos = base.transform.InverseTransformDirection(base.transform.position);
			lastGoodPosition = relativeStartPos;
			groundLayerMask = 1 << LayerMask.NameToLayer(groundLayerName);
		}

		private void Update()
		{
			if (isSetupCamera)
			{
				return;
			}
			if (SXInputManager.GetMouseButtonDown(0))
			{
				dragOrigin = Input.mousePosition;
			}
			else
			{
				if (!SXInputManager.GetMouseButton(0))
				{
					return;
				}
				if (SXInputManager.GetMouseX() < 0f && canMoveRight)
				{
					MoveCamera(Input.mousePosition, dragOrigin);
				}
				else if (SXInputManager.GetMouseX() > 0f && canMoveLeft)
				{
					MoveCamera(Input.mousePosition, dragOrigin);
				}
				Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, groundLayerMask))
				{
					canMoveLeft = true;
					canMoveRight = true;
					lastGoodPosition = base.transform.position;
					return;
				}
				Vector3 vector = base.transform.InverseTransformDirection(base.transform.position);
				if (vector.x < relativeStartPos.x)
				{
					canMoveLeft = false;
				}
				else if (vector.x > relativeStartPos.x)
				{
					canMoveRight = false;
				}
				base.transform.position = lastGoodPosition;
			}
		}

		private void MoveCamera(Vector3 mousePos, Vector3 dragOrigin)
		{
			Vector3 translation = new Vector3((0f - base.GetComponent<Camera>().ScreenToViewportPoint(mousePos - dragOrigin).x) * (base.GetComponent<Camera>().fieldOfView / dragSpeed * 0.2f), 0f, 0f);
			base.transform.Translate(translation, Space.Self);
		}
	}
}
