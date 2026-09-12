#pragma warning disable 0618,0619
using UnityEngine;

public class Plotter : MonoBehaviour
{
	[Range(0f, 5f)]
	public float s;

	[Range(0f, 1f)]
	public float x;

	private float sCurve(float t, float s)
	{
		float num = t * 2f;
		if (num < 1f)
		{
			return 0.5f * Mathf.Pow(num, 1f + 2f * s);
		}
		return 0.5f * (2f - Mathf.Pow(2f - num, 1f + 2f * s));
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Vector3 zero = Vector3.zero;
		Vector3 up = Vector3.up;
		Vector3 right = Vector3.right;
		Vector3 forward = Vector3.forward;
		Color color = new Color(1f, 1f, 1f, 1f);
		Color color2 = new Color(0f, 0f, 0f, 1f);
		Color color3 = new Color(0f, 0f, 0f, 0.25f);
		Color color4 = new Color(0f, 0f, 0f, 1f);
		Color color5 = new Color(0f, 0f, 0f, 0.5f);
		Gizmos.color = color;
		Gizmos.DrawCube(zero + right * 0.5f + up * 0.5f + forward * 0.005f, new Vector3(1f, 1f, 0.01f));
		Gizmos.color = color2;
		Gizmos.DrawLine(zero, zero + up);
		Gizmos.DrawLine(zero, zero + right);
		Gizmos.DrawLine(zero + right, zero + right + Vector3.up);
		Gizmos.DrawLine(zero + up, zero + up + Vector3.right);
		Gizmos.DrawLine(zero + right * 0.5f, zero + right * 0.5f + up);
		Gizmos.DrawLine(zero + up * 0.5f, zero + up * 0.5f + right);
		float num = 0.1f;
		Gizmos.color = color3;
		for (float num2 = num; num2 < 1f - num * 0.5f; num2 += num)
		{
			Gizmos.DrawLine(zero + right * num2, zero + right * num2 + up);
			Gizmos.DrawLine(zero + up * num2, zero + up * num2 + right);
		}
		Gizmos.color = color5;
		Gizmos.DrawLine(zero, zero + up + right);
		Gizmos.color = color4;
		for (float num3 = 0f; num3 < 0.999f; num3 += 0.01f)
		{
			float num4 = num3;
			float num5 = num3 + 0.01f;
			Gizmos.DrawLine(zero + num4 * right + sCurve(num4, s) * up, zero + num5 * right + sCurve(num5, s) * up);
		}
		Gizmos.color = color2;
		Gizmos.DrawSphere(zero + x * right, 0.01f);
		Gizmos.color = color5;
		Gizmos.DrawSphere(zero + right + x * up, 0.01f);
		Gizmos.color = color4;
		Gizmos.DrawSphere(zero + right + sCurve(x, s) * up, 0.01f);
	}
}
