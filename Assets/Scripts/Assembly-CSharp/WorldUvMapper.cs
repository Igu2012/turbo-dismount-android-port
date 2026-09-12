#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

public class WorldUvMapper : UvMapper
{
	public Vector2 scale = Vector2.one;

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
		for (int j = 0; j < points.Count; j++)
		{
			Vector3 lhs = points[j];
			array[j].x = Vector3.Dot(lhs, vector);
			array[j].y = Vector3.Dot(lhs, rhs);
			vector4 = ((j != 0) ? Vector2.Min(array[j], vector4) : array[j]);
		}
		for (int k = 0; k < points.Count; k++)
		{
			array[k] -= vector4;
			array[k].x *= scale.x;
			array[k].y *= scale.y;
		}
		uvsA = array;
		uvsB = array;
	}
}
