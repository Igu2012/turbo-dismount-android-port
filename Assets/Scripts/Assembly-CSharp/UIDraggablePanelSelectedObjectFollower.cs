#pragma warning disable 0618,0619
using UnityEngine;

[RequireComponent(typeof(UIDraggablePanel))]
public class UIDraggablePanelSelectedObjectFollower : MonoBehaviour
{
	private UIDraggablePanel draggablePanel;

	private Transform mTrans;

	private GameObject mGO;

	private GameObject lastFollowed;

	private void Awake()
	{
		mGO = base.gameObject;
		mTrans = base.transform;
		draggablePanel = GetComponent<UIDraggablePanel>();
	}

	private void Update()
	{
		if (base.enabled && mGO.active && UICamera.selectedObject != null && UICamera.selectedObject != lastFollowed && UICamera.selectedObject.transform.IsChildOf(draggablePanel.transform))
		{
			Bounds bounds = draggablePanel.bounds;
			Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(draggablePanel.transform, UICamera.selectedObject.transform);
			UIPanel component = GetComponent<UIPanel>();
			Vector3 vector = component.CalculateConstrainOffset(bounds2.min, bounds2.max);
			if (vector.sqrMagnitude > 1E-06f)
			{
				SpringPanel.Begin(component.gameObject, mTrans.localPosition + vector, 13f);
				lastFollowed = UICamera.selectedObject;
			}
		}
	}
}
