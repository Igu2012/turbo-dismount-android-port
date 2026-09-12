#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(Camera))]
	public class SceneSetupCamera : MonoBehaviour
	{
		private static string setupLayer = "Setup";

		private int layerMask;

		private Vector3 defaultPosition = Vector3.zero;

		private Quaternion defaultRotation = Quaternion.identity;

		private AnimatorVector3 cameraPosition = new AnimatorVector3();

		private AnimatorQuaternion cameraRotation = new AnimatorQuaternion();

		private SelectionHotspot targetHotspot;

		private float cameraReturnTime = -1f;

		private SelectionHotspot hoverHotspot;

		private float hoverStartTime = -1f;

		private FreeFlyCamera freeFlyCamera;

		private void Awake()
		{
			layerMask = 1 << LayerMask.NameToLayer(setupLayer);
			defaultPosition = base.transform.position;
			defaultRotation = base.transform.rotation;
			cameraPosition.interpolator = AnimatorBase.Interpolator.SmoothStep;
			cameraPosition.duration = 0.4f;
			cameraPosition.Reset(defaultPosition);
			cameraRotation.interpolator = AnimatorBase.Interpolator.SmoothStep;
			cameraRotation.duration = 0.4f;
			cameraRotation.Reset(defaultRotation);
		}

		public void FocusHotspot(SelectionHotspot hotspot)
		{
		}

		private void Update()
		{
			if (freeFlyCamera == null)
			{
				freeFlyCamera = GetComponent<FreeFlyCamera>();
			}
			if (DismountGame.uiManager == null)
			{
				return;
			}
			float time = Time.time;
			if (freeFlyCamera != null && freeFlyCamera.isMouseLook)
			{
				return;
			}
			bool flag = GizmoManager.currentlyActiveGizmo != null && GizmoManager.currentlyActiveGizmo.IsDragging();
			if (flag)
			{
				cameraReturnTime = -1f;
				if (targetHotspot != null)
				{
					targetHotspot.Hover = true;
				}
			}
			if (DismountGame.uiManager.IsMouseObstructedByUI())
			{
				return;
			}
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Gizmo")))
			{
				SelectionHotspot selectionHotspot = Utils.FindFirstComponentUpInHierarchy<SelectionHotspot>(hitInfo.transform);
				if (selectionHotspot != null)
				{
					selectionHotspot.Hover = true;
					if (selectionHotspot != hoverHotspot)
					{
						hoverStartTime = time;
						hoverHotspot = selectionHotspot;
					}
					if (!flag && hoverHotspot == selectionHotspot && hoverStartTime > 0f && time > hoverStartTime + 0.2f)
					{
						hoverStartTime = -1f;
						hoverHotspot = null;
						FocusHotspot(selectionHotspot);
					}
				}
			}
			else
			{
				if (!Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, layerMask))
				{
					return;
				}
				SelectionHotspot component = hitInfo.transform.GetComponent<SelectionHotspot>();
				if (!(component != null))
				{
					return;
				}
				if (SXInputManager.GetMouseButtonDown(0))
				{
					Utils.SendMessage("SetupScene", "OnOpenSelection", component);
					return;
				}
				component.Hover = true;
				if (component != hoverHotspot)
				{
					hoverStartTime = time;
					hoverHotspot = component;
				}
				if (!flag && hoverHotspot == component && hoverStartTime > 0f && time > hoverStartTime + 0.2f)
				{
					hoverStartTime = -1f;
					hoverHotspot = null;
					FocusHotspot(component);
				}
			}
		}
	}
}
