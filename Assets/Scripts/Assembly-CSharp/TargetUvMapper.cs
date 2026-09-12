#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

public class TargetUvMapper : UvMapper
{
	public Vector2 targetStart = Vector2.zero;

	public Vector2 targetSize = Vector2.one;

	public bool square;

	public bool centerMeshOrigo;

	public override void Map(IList<Vector3> points, Vector3 planeNormal, out Vector4[] tangentsA, out Vector4[] tangentsB, out Vector2[] uvsA, out Vector2[] uvsB)
	{
		Vector3 vector = Vector3.Cross(planeNormal, Vector3.up);
		if (vector == Vector3.zero)
		{
			vector = Vector3.Cross(planeNormal, Vector3.forward);
		}
		Vector3 rhs = Vector3.Cross(vector, planeNormal);
		vector.Normalize();
		rhs.Normalize();
		Vector4 vector2 = new Vector4(vector.x, vector.y, vector.z, 1f);
		Vector4 vector3 = new Vector4(vector.x, vector.y, vector.z, -1f);
		tangentsA = new Vector4[points.Count];
		tangentsB = new Vector4[points.Count];
		for (int i = 0; i < points.Count; i++)
		{
			tangentsA[i] = vector2;
			tangentsB[i] = vector3;
		}
		Vector2[] array = new Vector2[points.Count];
		Vector2 vector4 = Vector2.zero;
		Vector2 vector5 = Vector2.zero;
		for (int j = 0; j < points.Count; j++)
		{
			Vector3 lhs = points[j];
			array[j].x = Vector3.Dot(lhs, vector);
			array[j].y = Vector3.Dot(lhs, rhs);
			if (j == 0)
			{
				vector4 = array[j];
				vector5 = array[j];
			}
			else
			{
				vector4 = Vector2.Min(array[j], vector4);
				vector5 = Vector2.Max(array[j], vector5);
			}
		}
		Vector2 vector6 = vector5 - vector4;
		if (square)
		{
			float num = Mathf.Max(vector6.x, vector6.y);
			Vector2 vector7 = new Vector2
			{
				x = (num - vector6.x) * 0.5f,
				y = (num - vector6.y) * 0.5f
			};
			vector4 -= vector7;
			vector5 += vector7;
		}
		if (centerMeshOrigo)
		{
			Vector2 vector8 = new Vector2
			{
				x = Mathf.Max(Mathf.Abs(vector4.x), Mathf.Abs(vector5.x)),
				y = Mathf.Max(Mathf.Abs(vector4.y), Mathf.Abs(vector5.y))
			};
			vector4 = -vector8;
			vector5 = vector8;
		}
		Vector2 vector9 = vector5 - vector4;
		Vector2 vector10 = new Vector2(1f / vector9.x, 1f / vector9.y);
		for (int k = 0; k < points.Count; k++)
		{
			array[k].x = (array[k].x - vector4.x) * vector10.x;
			array[k].y = (array[k].y - vector4.y) * vector10.y;
			array[k].x = targetStart.x + targetSize.x * array[k].x;
			array[k].y = targetStart.y + targetSize.y * array[k].y;
		}
		uvsA = array;
		uvsB = array;
	}
}
