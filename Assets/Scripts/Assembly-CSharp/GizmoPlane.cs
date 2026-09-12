#pragma warning disable 0618,0619
using UnityEngine;

public class GizmoPlane : MonoBehaviour
{
	public GameObject[] disableWhenDragging;

	public bool dragging;

	private Vector3 dragOffset;

	private Plane dragPlane = new Plane(Vector3.up, Vector3.zero);

	private Vector3 originalRight = Vector3.right;

	private Vector3 originalForward = Vector3.forward;

	public Color defaultColor = new Color(0.6f, 0f, 0f, 1f);

	public Color hilightColor = new Color(1f, 0f, 0f, 1f);

	private Material gizmoMaterial;

	private GameObject gizmo;

	private void Start()
	{
		gizmo = base.transform.Find("Gizmo").gameObject;
		gizmoMaterial = gizmo.GetComponent<Renderer>().material;
		gizmoMaterial.renderQueue += 1;
	}

	private void LateUpdate()
	{
	}
}
