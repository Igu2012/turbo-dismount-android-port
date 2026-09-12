#pragma warning disable 0618,0619
using UnityEngine;

public class GizmoAxis : MonoBehaviour
{
	public GameObject[] disableWhenDragging;

	public bool dragging;

	private Vector3 startPos;

	private Vector3 originalForward = Vector3.forward;

	private Vector3 originalRight = Vector3.right;

	private Vector3 groundCastDirection = Vector3.down;

	private float groundCastHeight = 10f;

	private Ray axisRay;

	private float mouseStartPosRayDistance;

	public Color defaultColor = new Color(0.6f, 0f, 0f, 1f);

	public Color hilightColor = new Color(1f, 0f, 0f, 1f);

	private Material gizmoMaterial;

	private GameObject gizmo;

	private void Start()
	{
		gizmo = base.transform.Find("Gizmo").gameObject;
		gizmoMaterial = gizmo.GetComponent<Renderer>().material;
	}

	private bool RayClosestPoint(Ray ray1, Ray ray2, out float ray2Distance)
	{
		Vector3 direction = ray1.direction;
		Vector3 direction2 = ray2.direction;
		Vector3 rhs = ray1.origin - ray2.origin;
		float num = Vector3.Dot(direction, direction);
		float num2 = Vector3.Dot(direction, direction2);
		float num3 = Vector3.Dot(direction2, direction2);
		float num4 = Vector3.Dot(direction, rhs);
		float num5 = Vector3.Dot(direction2, rhs);
		float num6 = num * num3 - num2 * num2;
		float num7;
		if (num6 < 0.0001f)
		{
			num7 = ((!(num2 > num3)) ? (num5 / num3) : (num4 / num2));
			ray2Distance = num7;
			return false;
		}
		num7 = (num * num5 - num2 * num4) / num6;
		ray2Distance = num7;
		return true;
	}

	private void LateUpdate()
	{
	}
}
