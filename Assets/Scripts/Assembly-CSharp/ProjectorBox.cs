#pragma warning disable 0618,0619
using System;
using UnityEngine;

[ExecuteInEditMode]
public class ProjectorBox : MonoBehaviour
{
	public Projector projector;

	public Vector3 size = new Vector3(2f, 1f, 4f);

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
		Vector3 vector = size;
		Vector3 lossyScale = xform.lossyScale;
		vector.x *= lossyScale.x;
		vector.y *= lossyScale.y;
		vector.z *= lossyScale.z;
		float num = 0.5f * vector.magnitude * farClipMultiplier;
		Vector3 right = xform.right;
		Vector3 lhs = xform.up;
		Vector3 vector2 = xform.forward;
		Vector3 worldUp = vector2;
		float num2 = Mathf.Abs(vector2.y);
		if (num2 > 0.9f)
		{
			worldUp = (vector2 = xform.up);
			lhs = xform.forward;
			float z = vector.z;
			vector.z = vector.y;
			vector.y = z;
		}
		projectorXform.LookAt(projectorXform.position + Vector3.down, worldUp);
		worldUp = projectorXform.up;
		Vector3 right2 = projectorXform.right;
		Vector3 forward = projectorXform.forward;
		if (projector.orthographic)
		{
			float num3 = Mathf.Lerp(vector.y, vector.z, Mathf.Abs(Vector3.Dot(vector2, worldUp)));
			float num4 = Mathf.Lerp(vector.y, vector.x, Mathf.Abs(Vector3.Dot(right, right2)));
			float num5 = Mathf.Lerp(Mathf.Max(vector.x, vector.z), vector.y, Mathf.Abs(Vector3.Dot(lhs, forward)));
			projector.orthographicSize = num3;
			projector.aspectRatio = num4 / num3;
			projector.farClipPlane = num + num5 * 0.5f;
		}
		else
		{
			float t = Mathf.Abs(Vector3.Dot(vector2, worldUp));
			float num6 = Mathf.Lerp(vector.y, vector.z, t);
			float num7 = Mathf.Lerp(vector.y, vector.x, Mathf.Abs(Vector3.Dot(right, right2)));
			float num8 = Mathf.Lerp(Mathf.Max(vector.x, vector.z), vector.y, Mathf.Abs(Vector3.Dot(lhs, forward)));
			float to = Mathf.Atan(vector.z / vector.y);
			float num9 = Mathf.Atan(vector.y / vector.z);
			projector.fieldOfView = 2f * Mathf.Lerp(num9, to, t) * 180f / (float)Math.PI;
			projector.aspectRatio = num7 / num6;
			projector.farClipPlane = num + num8 * 0.5f;
		}
		projector.nearClipPlane = Mathf.Min(vector.x, vector.y, vector.z) * 0.25f;
	}

	private void OnDrawGizmosSelected()
	{
		if (drawGizmo)
		{
			Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawCube(Vector3.zero, size);
		}
	}
}
