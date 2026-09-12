#pragma warning disable 0618,0619
using UnityEngine;

public abstract class SplineMeshModifier : MonoBehaviour
{
	public abstract Vector3 ModifyVertex(SplineMesh splineMesh, Vector3 vertex, float splineParam);

	public abstract Vector2 ModifyUV(SplineMesh splineMesh, Vector2 uvCoord, float splineParam);

	public abstract Vector3 ModifyNormal(SplineMesh splineMesh, Vector3 normal, float splineParam);

	public abstract Vector4 ModifyTangent(SplineMesh splineMesh, Vector4 tangent, float splineParam);
}
