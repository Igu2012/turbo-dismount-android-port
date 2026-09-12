#pragma warning disable 0618,0619
using UnityEngine;

public interface IHull
{
	bool IsEmpty { get; }

	Mesh GetMesh();

	void Split(Vector3 localPointOnPlane, Vector3 localPlaneNormal, bool fillCut, UvMapper uvMapper, out IHull resultA, out IHull resultB);
}
