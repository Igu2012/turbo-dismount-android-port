#pragma warning disable 0618,0619
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("SuperSplines/Spline Mesh")]
[ExecuteInEditMode]
public class SplineMesh : MonoBehaviour
{
	private class MeshData
	{
		public Vector3[] vertices;

		public Vector2[] uvCoord;

		public Vector3[] normals;

		public Vector4[] tangents;

		public int[] triangles;

		public Bounds bounds;

		public int currentTriangleIndex;

		public int currentVertexIndex;

		public bool HasNormals;

		public bool HasTangents;

		public Mesh referencedMesh;

		public int VertexCount
		{
			get
			{
				return vertices.Length;
			}
		}

		public int TriangleCount
		{
			get
			{
				return triangles.Length;
			}
		}

		public MeshData(Mesh mesh)
		{
			referencedMesh = mesh;
			currentTriangleIndex = 0;
			currentVertexIndex = 0;
			if (mesh == null)
			{
				vertices = new Vector3[0];
				normals = new Vector3[0];
				tangents = new Vector4[0];
				uvCoord = new Vector2[0];
				triangles = new int[0];
				bounds = new Bounds(Vector3.zero, Vector3.zero);
				HasNormals = normals.Length > 0;
				HasTangents = tangents.Length > 0;
			}
			else
			{
				vertices = mesh.vertices;
				normals = mesh.normals;
				tangents = mesh.tangents;
				uvCoord = mesh.uv;
				triangles = mesh.triangles;
				bounds = mesh.bounds;
				HasNormals = normals.Length > 0;
				HasTangents = tangents.Length > 0;
			}
		}

		public MeshData(MeshData mData, int segmentCount, MeshData[] additionalMeshes)
		{
			int num = mData.vertices.Length * segmentCount;
			int num2 = mData.uvCoord.Length * segmentCount;
			int num3 = mData.normals.Length * segmentCount;
			int num4 = mData.tangents.Length * segmentCount;
			int num5 = mData.triangles.Length * segmentCount;
			foreach (MeshData meshData in additionalMeshes)
			{
				if (meshData != null)
				{
					num += meshData.vertices.Length;
					num2 += meshData.uvCoord.Length;
					num3 += meshData.normals.Length;
					num4 += meshData.tangents.Length;
					num5 += meshData.triangles.Length;
				}
			}
			vertices = new Vector3[num];
			uvCoord = new Vector2[num2];
			normals = new Vector3[num3];
			tangents = new Vector4[num4];
			triangles = new int[num5];
			HasNormals = normals.Length > 0;
			HasTangents = tangents.Length > 0;
		}

		public bool Suits(MeshData mData, int segmentCount, MeshData[] additionalMeshes)
		{
			int num = mData.vertices.Length * segmentCount;
			int num2 = mData.uvCoord.Length * segmentCount;
			int num3 = mData.normals.Length * segmentCount;
			int num4 = mData.tangents.Length * segmentCount;
			int num5 = mData.triangles.Length * segmentCount;
			foreach (MeshData meshData in additionalMeshes)
			{
				if (meshData != null)
				{
					num += meshData.vertices.Length;
					num2 += meshData.uvCoord.Length;
					num3 += meshData.normals.Length;
					num4 += meshData.tangents.Length;
					num5 += meshData.triangles.Length;
				}
			}
			if (num != vertices.Length)
			{
				return false;
			}
			if (num2 != uvCoord.Length)
			{
				return false;
			}
			if (num3 != normals.Length)
			{
				return false;
			}
			if (num4 != tangents.Length)
			{
				return false;
			}
			if (num5 != triangles.Length)
			{
				return false;
			}
			return true;
		}

		public bool ReferencesMesh(Mesh mesh)
		{
			return referencedMesh == mesh;
		}

		public void Reset()
		{
			currentTriangleIndex = 0;
			currentVertexIndex = 0;
		}

		public void AssignToMesh(Mesh mesh)
		{
			mesh.vertices = vertices;
			mesh.uv = uvCoord;
			if (HasNormals)
			{
				mesh.normals = normals;
			}
			if (HasTangents)
			{
				mesh.tangents = tangents;
			}
			mesh.triangles = triangles;
		}
	}

	public enum UVMode
	{
		InterpolateV = 0,
		InterpolateU = 1,
		DontInterpolate = 2
	}

	public enum SplitMode
	{
		DontSplit = 0,
		BySplineSegment = 1,
		BySplineParameter = 2
	}

	public enum UpdateMode
	{
		DontUpdate = 0,
		EveryFrame = 1,
		EveryXFrames = 2,
		EveryXSeconds = 3,
		WhenSplineChanged = 4
	}

	public Spline spline;

	public UpdateMode updateMode;

	public int deltaFrames = 1;

	public float deltaTime = 0.1f;

	private int updateFrame;

	private float updateTime;

	public Mesh startBaseMesh;

	public Mesh baseMesh;

	public Mesh endBaseMesh;

	public int segmentCount = 50;

	public UVMode uvMode;

	public Vector2 uvScale = Vector2.one;

	public Vector2 xyScale = Vector2.one;

	public bool highAccuracy;

	public SplitMode splitMode;

	public float segmentStart;

	public float segmentEnd = 1f;

	public int splineSegment;

	private MeshData meshDataStart;

	private MeshData meshDataBase;

	private MeshData meshDataEnd;

	private MeshData meshDataNew;

	private Mesh bentMesh;

	public Mesh BentMesh
	{
		get
		{
			return ReturnMeshReference();
		}
	}

	public bool IsSubSegment
	{
		get
		{
			return splineSegment != -1;
		}
	}

	private void Start()
	{
		if (spline != null)
		{
			spline.UpdateSpline();
		}
		UpdateMesh();
	}

	private void OnEnable()
	{
		if (spline != null)
		{
			spline.UpdateSpline();
		}
		UpdateMesh();
	}

	private void LateUpdate()
	{
		switch (updateMode)
		{
		default:
			return;
		case UpdateMode.DontUpdate:
			return;
		case UpdateMode.EveryXFrames:
			if (Time.frameCount % deltaFrames == 0)
			{
				break;
			}
			return;
		case UpdateMode.EveryXSeconds:
			if (deltaTime < Time.realtimeSinceStartup - updateTime)
			{
				updateTime = Time.realtimeSinceStartup;
				break;
			}
			return;
		case UpdateMode.WhenSplineChanged:
			if (updateFrame != spline.UpdateFrame)
			{
				updateFrame = spline.UpdateFrame;
				break;
			}
			return;
		case UpdateMode.EveryFrame:
			break;
		}
		UpdateMesh();
	}

	public void UpdateMesh()
	{
		SetupMesh();
		bentMesh.Clear();
		if (!(baseMesh == null) && !(spline == null) && segmentCount > 0)
		{
			SetupMeshBuffers();
			float num;
			float num2;
			switch (splitMode)
			{
			case SplitMode.BySplineSegment:
			{
				SplineSegment[] splineSegments = spline.SplineSegments;
				this.splineSegment = Mathf.Clamp(this.splineSegment, 0, splineSegments.Length - 1);
				SplineSegment splineSegment = splineSegments[this.splineSegment];
				num = (float)splineSegment.StartNode.Parameters[spline].position;
				num2 = num + splineSegment.NormalizedLength;
				break;
			}
			case SplitMode.BySplineParameter:
				num = segmentStart;
				num2 = segmentEnd;
				break;
			default:
				num = 0f;
				num2 = 1f;
				break;
			}
			float num3 = num2 - num;
			float num4 = 0f;
			float num5 = 0f;
			SplineMeshModifier[] componentsInChildren = GetComponentsInChildren<SplineMeshModifier>();
			for (int i = 0; i < segmentCount; i++)
			{
				MeshData meshData = ((i == 0 && startBaseMesh != null) ? meshDataStart : ((i != segmentCount - 1 || !(endBaseMesh != null)) ? meshDataBase : meshDataEnd));
				num4 = num + num3 * (float)i / (float)segmentCount;
				num5 = num + num3 * (float)(i + 1) / (float)segmentCount;
				BendMesh(num4, num5, meshData, meshDataNew, componentsInChildren);
			}
			meshDataNew.AssignToMesh(bentMesh);
		}
	}

	private void BendMesh(float param0, float param1, MeshData meshDataBase, MeshData meshDataNew, SplineMeshModifier[] meshModiefiers)
	{
		float num = param1 - param0;
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		Quaternion quaternion = Quaternion.identity;
		Quaternion to = Quaternion.identity;
		Quaternion quaternion2 = Quaternion.Inverse(spline.transform.rotation);
		int currentVertexIndex = meshDataNew.currentVertexIndex;
		if (!highAccuracy)
		{
			vector = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline(param0));
			vector2 = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline(param1));
			quaternion = spline.GetOrientationOnSpline(param0) * quaternion2;
			to = spline.GetOrientationOnSpline(param1) * quaternion2;
		}
		int num2 = 0;
		while (num2 < meshDataBase.VertexCount)
		{
			Vector3 vector3 = meshDataBase.vertices[num2];
			Vector2 vector4 = meshDataBase.uvCoord[num2];
			float num3 = vector3.z + 0.5f;
			float num4 = param0 + num * num3;
			switch (uvMode)
			{
			case UVMode.InterpolateU:
				vector4.x = num4;
				break;
			case UVMode.InterpolateV:
				vector4.y = num4;
				break;
			}
			vector4.x *= uvScale.x;
			vector4.y *= uvScale.y;
			Quaternion quaternion3;
			Vector3 vector5;
			if (highAccuracy)
			{
				quaternion3 = spline.GetOrientationOnSpline(num4) * quaternion2;
				vector5 = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline(num4));
			}
			else
			{
				quaternion3 = Quaternion.Lerp(quaternion, to, num3);
				vector5 = new Vector3(vector.x + (vector2.x - vector.x) * num3, vector.y + (vector2.y - vector.y) * num3, vector.z + (vector2.z - vector.z) * num3);
			}
			vector3.x *= xyScale.x;
			vector3.y *= xyScale.y;
			vector3.z = 0f;
			foreach (SplineMeshModifier splineMeshModifier in meshModiefiers)
			{
				vector3 = splineMeshModifier.ModifyVertex(this, vector3, num4);
			}
			meshDataNew.vertices[meshDataNew.currentVertexIndex] = FastRotation(quaternion3, vector3) + vector5;
			if (meshDataBase.HasNormals)
			{
				Vector3 vector6 = meshDataBase.normals[num2];
				foreach (SplineMeshModifier splineMeshModifier2 in meshModiefiers)
				{
					vector6 = splineMeshModifier2.ModifyNormal(this, vector6, num4);
				}
				meshDataNew.normals[meshDataNew.currentVertexIndex] = quaternion3 * vector6;
			}
			if (meshDataBase.HasTangents)
			{
				Vector4 vector7 = meshDataBase.tangents[num2];
				foreach (SplineMeshModifier splineMeshModifier3 in meshModiefiers)
				{
					vector7 = splineMeshModifier3.ModifyTangent(this, vector7, num4);
				}
				meshDataNew.tangents[meshDataNew.currentVertexIndex] = quaternion3 * vector7;
			}
			foreach (SplineMeshModifier splineMeshModifier4 in meshModiefiers)
			{
				vector4 = splineMeshModifier4.ModifyUV(this, vector4, num4);
			}
			meshDataNew.uvCoord[meshDataNew.currentVertexIndex] = vector4;
			num2++;
			meshDataNew.currentVertexIndex++;
		}
		int num5 = 0;
		while (num5 < meshDataBase.TriangleCount)
		{
			meshDataNew.triangles[meshDataNew.currentTriangleIndex] = meshDataBase.triangles[num5] + currentVertexIndex;
			num5++;
			meshDataNew.currentTriangleIndex++;
		}
	}

	private Vector3 FastRotation(Quaternion rotation, Vector3 point)
	{
		float num = rotation.x * 2f;
		float num2 = rotation.y * 2f;
		float num3 = rotation.z * 2f;
		float num4 = rotation.x * num;
		float num5 = rotation.y * num2;
		float num6 = rotation.z * num3;
		float num7 = rotation.x * num2;
		float num8 = rotation.x * num3;
		float num9 = rotation.y * num3;
		float num10 = rotation.w * num;
		float num11 = rotation.w * num2;
		float num12 = rotation.w * num3;
		Vector3 result = default(Vector3);
		result.x = (1f - (num5 + num6)) * point.x + (num7 - num12) * point.y;
		result.y = (num7 + num12) * point.x + (1f - (num4 + num6)) * point.y;
		result.z = (num8 - num11) * point.x + (num9 + num10) * point.y;
		return result;
	}

	private void SetupMesh()
	{
		if (bentMesh == null)
		{
			bentMesh = new Mesh();
			bentMesh.name = "BentMesh";
			bentMesh.hideFlags = HideFlags.HideAndDontSave;
		}
		MeshFilter component = GetComponent<MeshFilter>();
		if (component.sharedMesh != bentMesh)
		{
			component.sharedMesh = bentMesh;
		}
		MeshCollider component2 = GetComponent<MeshCollider>();
		if (component2 != null)
		{
			component2.sharedMesh = null;
			component2.sharedMesh = bentMesh;
		}
	}

	private void SetupMeshBuffers()
	{
		if (meshDataStart == null)
		{
			meshDataStart = new MeshData(null);
		}
		if (meshDataBase == null)
		{
			meshDataBase = new MeshData(null);
		}
		if (meshDataEnd == null)
		{
			meshDataEnd = new MeshData(null);
		}
		if (meshDataNew == null)
		{
			meshDataNew = new MeshData(null);
		}
		if (!meshDataStart.ReferencesMesh(startBaseMesh))
		{
			meshDataStart = new MeshData(startBaseMesh);
		}
		if (!meshDataBase.ReferencesMesh(baseMesh))
		{
			meshDataBase = new MeshData(baseMesh);
		}
		if (!meshDataEnd.ReferencesMesh(endBaseMesh))
		{
			meshDataEnd = new MeshData(endBaseMesh);
		}
		MeshData[] additionalMeshes = new MeshData[2] { meshDataStart, meshDataEnd };
		int num = segmentCount;
		if (startBaseMesh != null)
		{
			num--;
		}
		if (endBaseMesh != null)
		{
			num--;
		}
		if (!meshDataNew.Suits(meshDataBase, num, additionalMeshes))
		{
			meshDataNew = new MeshData(meshDataBase, num, additionalMeshes);
		}
		else
		{
			meshDataNew.Reset();
		}
	}

	private Mesh ReturnMeshReference()
	{
		return bentMesh;
	}
}
