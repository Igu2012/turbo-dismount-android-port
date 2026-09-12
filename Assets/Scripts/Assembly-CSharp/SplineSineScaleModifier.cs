#pragma warning disable 0618,0619
using UnityEngine;

[AddComponentMenu("SuperSplines/Other/Spline Mesh Modifiers/Sine Scale Modifier (scale periodically)")]
public class SplineSineScaleModifier : SplineMeshModifier
{
	public float frequency = 10f;

	public float offset;

	public float sinMultiplicator = 1f;

	public float sinOffset = 0.25f;

	public override Vector3 ModifyVertex(SplineMesh splineMesh, Vector3 vertex, float splineParam)
	{
		return vertex * (Mathf.Pow(Mathf.Sin(splineParam * frequency + offset), 2f) * sinMultiplicator + sinOffset);
	}

	public override Vector2 ModifyUV(SplineMesh splineMesh, Vector2 uvCoord, float splineParam)
	{
		return uvCoord;
	}

	public override Vector3 ModifyNormal(SplineMesh splineMesh, Vector3 normal, float splineParam)
	{
		return normal;
	}

	public override Vector4 ModifyTangent(SplineMesh splineMesh, Vector4 tangent, float splineParam)
	{
		return tangent;
	}
}
