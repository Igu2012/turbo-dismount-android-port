#pragma warning disable 0618,0619
using UnityEngine;

[AddComponentMenu("SuperSplines/Other/Spline Mesh Modifiers/Twist Modifier")]
public class SplineTwistModifier : SplineMeshModifier
{
	public float twistCount = 10f;

	public float twistOffset;

	private Quaternion rotationQuaternion;

	public override Vector3 ModifyVertex(SplineMesh splineMesh, Vector3 vertex, float splineParam)
	{
		rotationQuaternion = Quaternion.Euler(Vector3.forward * (splineParam - twistOffset) * 360f * twistCount);
		return rotationQuaternion * vertex;
	}

	public override Vector2 ModifyUV(SplineMesh splineMesh, Vector2 uvCoord, float splineParam)
	{
		return uvCoord;
	}

	public override Vector3 ModifyNormal(SplineMesh splineMesh, Vector3 normal, float splineParam)
	{
		return rotationQuaternion * normal;
	}

	public override Vector4 ModifyTangent(SplineMesh splineMesh, Vector4 tangent, float splineParam)
	{
		return rotationQuaternion * tangent;
	}
}
