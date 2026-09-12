#pragma warning disable 0618,0619
using UnityEngine;

public static class ShatterTools
{
	public static bool IsPointInsideTriangle(ref Vector3 point, ref Vector3 triangle0, ref Vector3 triangle1, ref Vector3 triangle2)
	{
		Vector3 triangleNormal = Vector3.Cross(triangle1 - triangle0, triangle2 - triangle0);
		return IsPointInsideTriangle(ref point, ref triangle0, ref triangle1, ref triangle2, ref triangleNormal);
	}

	public static bool IsPointInsideTriangle(ref Vector3 point, ref Vector3 triangle0, ref Vector3 triangle1, ref Vector3 triangle2, ref Vector3 triangleNormal)
	{
		if (Vector3.Cross(triangle1 - triangle0, triangle2 - triangle0) == Vector3.zero)
		{
			return false;
		}
		Vector3 vector = triangle0 - point;
		Vector3 vector2 = triangle1 - point;
		Vector3 vector3 = triangle2 - point;
		if (Vector3.Dot(Vector3.Cross(vector, vector2), triangleNormal) < 0f || Vector3.Dot(Vector3.Cross(vector2, vector3), triangleNormal) < 0f || Vector3.Dot(Vector3.Cross(vector3, vector), triangleNormal) < 0f)
		{
			return false;
		}
		return true;
	}
}
