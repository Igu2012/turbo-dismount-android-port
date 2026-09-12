#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

public class Intersections : MonoBehaviour
{
	private struct Intersection
	{
		public float distance;

		public Vector3 pos;
	}

	public Transform start;

	public Transform end;

	private Vector3 diagUnitZ = new Vector3(0.5f, 0f, 0.5f);

	private Vector3 diagUnitX = new Vector3(0.5f, 0f, -0.5f);

	private Vector3 toDiagonal(Vector3 oVector)
	{
		return new Vector3(Vector3.Dot(diagUnitX, oVector) * 2f, oVector.y, Vector3.Dot(diagUnitZ, oVector) * 2f);
	}

	private Vector3 toOrthogonal(Vector3 dVector)
	{
		return dVector.x * diagUnitX + dVector.y * Vector3.up + dVector.z * diagUnitZ;
	}

	private void OnDrawGizmos()
	{
		Vector3[] array = new Vector3[5];
		int num = 0;
		array[num++] = new Vector3(0f, 0f, 0f);
		array[num++] = new Vector3(0f, 0f, 1f);
		array[num++] = new Vector3(1f, 0f, 0f);
		array[num++] = new Vector3(1f, 0f, 1f);
		array[num++] = new Vector3(0.5f, 0f, 0.5f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Vector3 zero = Vector3.zero;
		Vector3 up = Vector3.up;
		Vector3 right = Vector3.right;
		Vector3 forward = Vector3.forward;
		Color color = new Color(1f, 1f, 1f, 1f);
		Color color2 = new Color(0f, 0f, 0f, 0.25f);
		Color color3 = new Color(0f, 1f, 0.5f, 1f);
		Gizmos.color = color;
		Gizmos.DrawCube(zero + right * 5f + up * 0.005f + forward * 5f, new Vector3(10f, 0.01f, 10f));
		Gizmos.color = color2;
		for (int i = 0; i < 10; i++)
		{
			zero.z = 0f;
			for (int j = 0; j < 10; j++)
			{
				Gizmos.DrawLine(zero + array[0], zero + array[1]);
				Gizmos.DrawLine(zero + array[0], zero + array[2]);
				Gizmos.DrawLine(zero + array[1], zero + array[3]);
				Gizmos.DrawLine(zero + array[2], zero + array[3]);
				Gizmos.DrawLine(zero + array[0], zero + array[3]);
				Gizmos.DrawLine(zero + array[1], zero + array[2]);
				zero.z++;
			}
			zero.x++;
		}
		Vector3 position = start.position;
		Vector3 position2 = end.position;
		Gizmos.color = color3;
		Gizmos.DrawLine(position, position2);
		Gizmos.DrawSphere(position, 0.05f);
		Gizmos.DrawSphere(position2, 0.05f);
		List<Vector3> list = new List<Vector3>();
		Vector3 vector = position2 - position;
		Vector3 normalized = vector.normalized;
		Vector3 vector2 = new Vector3(Mathf.Repeat(position.x, 1f), 0f, Mathf.Repeat(position.z, 1f));
		Vector3 zero2 = Vector3.zero;
		if (normalized.z > float.Epsilon || normalized.z < -float.Epsilon)
		{
			Vector3 vector3 = normalized * (1f / Mathf.Abs(normalized.z));
			for (zero2 = ((!(normalized.z > 0f)) ? (position + vector2.z * vector3) : (position + (1f - vector2.z) * vector3)); (zero2 - position).sqrMagnitude < vector.sqrMagnitude; zero2 += vector3)
			{
				list.Add(zero2);
			}
		}
		if (normalized.x > float.Epsilon || normalized.x < -float.Epsilon)
		{
			Vector3 vector4 = normalized * (1f / Mathf.Abs(normalized.x));
			for (zero2 = ((!(normalized.x > 0f)) ? (position + vector2.x * vector4) : (position + (1f - vector2.x) * vector4)); (zero2 - position).sqrMagnitude < vector.sqrMagnitude; zero2 += vector4)
			{
				list.Add(zero2);
			}
		}
		position = toDiagonal(position);
		position2 = toDiagonal(position2);
		vector = position2 - position;
		normalized = vector.normalized;
		vector2 = new Vector3(Mathf.Repeat(position.x, 1f), 0f, Mathf.Repeat(position.z, 1f));
		if (normalized.z > float.Epsilon || normalized.z < -float.Epsilon)
		{
			Vector3 vector5 = normalized * (1f / Mathf.Abs(normalized.z));
			for (zero2 = ((!(normalized.z > 0f)) ? (position + vector2.z * vector5) : (position + (1f - vector2.z) * vector5)); (zero2 - position).sqrMagnitude < vector.sqrMagnitude; zero2 += vector5)
			{
				list.Add(toOrthogonal(zero2));
			}
		}
		if (normalized.x > float.Epsilon || normalized.x < -float.Epsilon)
		{
			Vector3 vector6 = normalized * (1f / Mathf.Abs(normalized.x));
			for (zero2 = ((!(normalized.x > 0f)) ? (position + vector2.x * vector6) : (position + (1f - vector2.x) * vector6)); (zero2 - position).sqrMagnitude < vector.sqrMagnitude; zero2 += vector6)
			{
				list.Add(toOrthogonal(zero2));
			}
		}
		foreach (Vector3 item in list)
		{
			Gizmos.DrawSphere(item, 0.025f);
		}
	}
}
