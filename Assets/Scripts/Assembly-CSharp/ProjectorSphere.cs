#pragma warning disable 0618,0619
using UnityEngine;

[ExecuteInEditMode]
public class ProjectorSphere : MonoBehaviour
{
	public Projector projector;

	public float radius = 1f;

	public float farClipMultiplier = 1f;

	public bool drawGizmo = true;

	private Transform xform;

	private Transform projectorXform;

	private void Awake()
	{
		xform = base.transform;
		projectorXform = projector.transform;
	}

	private void LateUpdate()
	{
		if (xform == null)
		{
			xform = base.transform;
			projectorXform = projector.transform;
		}
		float num = radius;
		Vector3 lossyScale = xform.lossyScale;
		float num2 = Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
		num *= num2;
		float num3 = num * farClipMultiplier;
		projectorXform.LookAt(projectorXform.position + Vector3.down, xform.forward);
		if (projector.orthographic)
		{
			projector.orthographicSize = num * 1.414f;
			projector.aspectRatio = 1f;
			projector.farClipPlane = num3 + num;
		}
		else
		{
			projector.fieldOfView = 109.5f;
			projector.aspectRatio = 1f;
			projector.farClipPlane = num3 + num;
		}
		projector.nearClipPlane = num * 0.25f;
	}

	private void OnDrawGizmosSelected()
	{
		if (drawGizmo)
		{
			Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawSphere(Vector3.zero, radius);
		}
	}
}
