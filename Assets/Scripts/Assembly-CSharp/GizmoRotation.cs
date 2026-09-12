#pragma warning disable 0618,0619
using UnityEngine;

public class GizmoRotation : MonoBehaviour
{
	public GameObject[] disableWhenDragging;

	public bool dragging;

	private Vector3 mouseStartPos;

	private Vector3 dragAxisScreenSpace;

	private Vector3 originalForward;

	private Vector3 originalRight;

	public Color defaultColor = new Color(0.6f, 0f, 0f, 1f);

	public Color hilightColor = new Color(1f, 0f, 0f, 1f);

	private Material gizmoMaterial;

	private GameObject gizmo;

	private void Start()
	{
		gizmo = base.transform.Find("Gizmo").gameObject;
		gizmoMaterial = gizmo.GetComponent<Renderer>().material;
	}

	private void LateUpdate()
	{
	}
}
