#pragma warning disable 0618,0619
using UnityEngine;

public class ComboGizmo : MonoBehaviour
{
	public Transform target;

	public float objectScale = 1f;

	private Vector3 initialScale;

	public GizmoAxis gizmoX;

	public GizmoAxis gizmoZ;

	public GizmoRotation gizmoRotation;

	public GizmoPlane gizmoPlane;

	private void Start()
	{
		initialScale = base.transform.localScale * 0.03f;
	}

	private void LateUpdate()
	{
		target.position = base.transform.position;
		target.rotation = base.transform.rotation;
		base.transform.position = target.position;
		base.transform.rotation = target.rotation;
		Camera main = Camera.main;
		float distanceToPoint = new Plane(main.transform.forward, main.transform.position).GetDistanceToPoint(base.transform.position);
		base.transform.localScale = initialScale * distanceToPoint * objectScale;
	}

	public bool IsDragging()
	{
		return gizmoX.dragging | gizmoZ.dragging | gizmoRotation.dragging | gizmoPlane.dragging;
	}
}
