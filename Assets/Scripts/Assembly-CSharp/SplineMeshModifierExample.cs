#pragma warning disable 0618,0619
using UnityEngine;

[AddComponentMenu("SuperSplines/Other/Spline Mesh Modifiers/Mesh Modifier Template")]
public class SplineMeshModifierExample : SplineMeshModifier
{
	public override Vector3 ModifyVertex(SplineMesh splineMesh, Vector3 vertex, float splineParam)
	{
		return vertex;
	}

	public override Vector3 ModifyNormal(SplineMesh splineMesh, Vector3 normal, float splineParam)
	{
		return normal;
	}

	public override Vector4 ModifyTangent(SplineMesh splineMesh, Vector4 tangent, float splineParam)
	{
		return tangent;
	}

	public override Vector2 ModifyUV(SplineMesh splineMesh, Vector2 uvCoord, float splineParam)
	{
		return uvCoord;
	}
}
