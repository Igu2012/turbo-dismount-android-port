#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

public class FastHull : IHull
{
	private static float smallestValidLength = 0.01f;

	private static float smallestValidRatio = 0.05f;

	private bool isValid = true;

	private List<Vector3> vertices;

	private List<Vector3> normals;

	private List<Vector4> tangents;

	private List<Vector2> uvs;

	private List<int> indices;

	public bool IsEmpty
	{
		get
		{
			return !isValid || vertices.Count < 3 || indices.Count < 3;
		}
	}

	public FastHull(Mesh mesh)
	{
		vertices = new List<Vector3>(mesh.vertices);
		indices = new List<int>(mesh.triangles);
		if (mesh.normals.Length > 0)
		{
			normals = new List<Vector3>(mesh.normals);
		}
		if (mesh.tangents.Length > 0)
		{
			tangents = new List<Vector4>(mesh.tangents);
		}
		if (mesh.uv.Length > 0)
		{
			uvs = new List<Vector2>(mesh.uv);
		}
	}

	public FastHull(FastHull reference)
	{
		vertices = new List<Vector3>(reference.vertices.Count);
		indices = new List<int>(reference.indices.Count);
		if (reference.normals != null)
		{
			normals = new List<Vector3>(reference.normals.Count);
		}
		if (reference.tangents != null)
		{
			tangents = new List<Vector4>(reference.tangents.Count);
		}
		if (reference.uvs != null)
		{
			uvs = new List<Vector2>(reference.uvs.Count);
		}
	}

	public Mesh GetMesh()
	{
		if (isValid)
		{
			Mesh mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = indices.ToArray();
			if (normals != null)
			{
				mesh.normals = normals.ToArray();
			}
			if (tangents != null)
			{
				mesh.tangents = tangents.ToArray();
			}
			if (uvs != null)
			{
				mesh.uv = uvs.ToArray();
			}
			return mesh;
		}
		return null;
	}

	public void Split(Vector3 localPointOnPlane, Vector3 localPlaneNormal, bool fillCut, UvMapper uvMapper, out IHull resultA, out IHull resultB)
	{
		if (localPlaneNormal == Vector3.zero)
		{
			localPlaneNormal = Vector3.up;
		}
		FastHull fastHull = new FastHull(this);
		FastHull fastHull2 = new FastHull(this);
		bool[] vertexAbovePlane;
		int[] oldToNewVertexMap;
		AssignVertices(fastHull, fastHull2, localPointOnPlane, localPlaneNormal, out vertexAbovePlane, out oldToNewVertexMap);
		IList<Vector3> cutEdges;
		AssignTriangles(fastHull, fastHull2, vertexAbovePlane, oldToNewVertexMap, localPointOnPlane, localPlaneNormal, out cutEdges);
		if (fillCut)
		{
			FillCutEdges(fastHull, fastHull2, cutEdges, localPlaneNormal, uvMapper);
		}
		ValidateOutput(fastHull, fastHull2, localPlaneNormal);
		isValid = false;
		resultA = fastHull;
		resultB = fastHull2;
	}

	private void AssignVertices(FastHull a, FastHull b, Vector3 pointOnPlane, Vector3 planeNormal, out bool[] vertexAbovePlane, out int[] oldToNewVertexMap)
	{
		vertexAbovePlane = new bool[vertices.Count];
		oldToNewVertexMap = new int[vertices.Count];
		for (int i = 0; i < vertices.Count; i++)
		{
			Vector3 vector = vertices[i];
			bool flag = Vector3.Dot(vector - pointOnPlane, planeNormal) >= 0f;
			vertexAbovePlane[i] = flag;
			if (flag)
			{
				oldToNewVertexMap[i] = a.vertices.Count;
				a.vertices.Add(vector);
				if (normals != null)
				{
					a.normals.Add(normals[i]);
				}
				if (tangents != null)
				{
					a.tangents.Add(tangents[i]);
				}
				if (uvs != null)
				{
					a.uvs.Add(uvs[i]);
				}
			}
			else
			{
				oldToNewVertexMap[i] = b.vertices.Count;
				b.vertices.Add(vector);
				if (normals != null)
				{
					b.normals.Add(normals[i]);
				}
				if (tangents != null)
				{
					b.tangents.Add(tangents[i]);
				}
				if (uvs != null)
				{
					b.uvs.Add(uvs[i]);
				}
			}
		}
	}

	private void AssignTriangles(FastHull a, FastHull b, bool[] vertexAbovePlane, int[] oldToNewVertexMap, Vector3 pointOnPlane, Vector3 planeNormal, out IList<Vector3> cutEdges)
	{
		cutEdges = new List<Vector3>();
		int num = indices.Count / 3;
		for (int i = 0; i < num; i++)
		{
			int num2 = indices[i * 3];
			int num3 = indices[i * 3 + 1];
			int num4 = indices[i * 3 + 2];
			bool flag = vertexAbovePlane[num2];
			bool flag2 = vertexAbovePlane[num3];
			bool flag3 = vertexAbovePlane[num4];
			if (flag && flag2 && flag3)
			{
				a.indices.Add(oldToNewVertexMap[num2]);
				a.indices.Add(oldToNewVertexMap[num3]);
				a.indices.Add(oldToNewVertexMap[num4]);
				continue;
			}
			if (!flag && !flag2 && !flag3)
			{
				b.indices.Add(oldToNewVertexMap[num2]);
				b.indices.Add(oldToNewVertexMap[num3]);
				b.indices.Add(oldToNewVertexMap[num4]);
				continue;
			}
			int num5;
			int cw;
			int ccw;
			if (flag2 == flag3 && flag != flag2)
			{
				num5 = num2;
				cw = num3;
				ccw = num4;
			}
			else if (flag3 == flag && flag2 != flag3)
			{
				num5 = num3;
				cw = num4;
				ccw = num2;
			}
			else
			{
				num5 = num4;
				cw = num2;
				ccw = num3;
			}
			Vector3 ccwIntersection;
			Vector3 cwIntersection;
			if (vertexAbovePlane[num5])
			{
				SplitTriangle(a, b, oldToNewVertexMap, pointOnPlane, planeNormal, num5, cw, ccw, out ccwIntersection, out cwIntersection);
			}
			else
			{
				SplitTriangle(b, a, oldToNewVertexMap, pointOnPlane, planeNormal, num5, cw, ccw, out cwIntersection, out ccwIntersection);
			}
			if (ccwIntersection != cwIntersection)
			{
				cutEdges.Add(ccwIntersection);
				cutEdges.Add(cwIntersection);
			}
		}
	}

	private void SplitTriangle(FastHull topHull, FastHull bottomHull, int[] oldToNewVertexMap, Vector3 pointOnPlane, Vector3 planeNormal, int top, int cw, int ccw, out Vector3 cwIntersection, out Vector3 ccwIntersection)
	{
		Vector3 vector = vertices[top];
		Vector3 vector2 = vertices[cw];
		Vector3 vector3 = vertices[ccw];
		float num = Vector3.Dot(vector2 - vector, planeNormal);
		float num2 = Mathf.Clamp01(Vector3.Dot(pointOnPlane - vector, planeNormal) / num);
		float num3 = Vector3.Dot(vector3 - vector, planeNormal);
		float num4 = Mathf.Clamp01(Vector3.Dot(pointOnPlane - vector, planeNormal) / num3);
		Vector3 vector4 = new Vector3
		{
			x = vector.x + (vector2.x - vector.x) * num2,
			y = vector.y + (vector2.y - vector.y) * num2,
			z = vector.z + (vector2.z - vector.z) * num2
		};
		Vector3 vector5 = new Vector3
		{
			x = vector.x + (vector3.x - vector.x) * num4,
			y = vector.y + (vector3.y - vector.y) * num4,
			z = vector.z + (vector3.z - vector.z) * num4
		};
		int count = topHull.vertices.Count;
		topHull.vertices.Add(vector4);
		int count2 = topHull.vertices.Count;
		topHull.vertices.Add(vector5);
		topHull.indices.Add(oldToNewVertexMap[top]);
		topHull.indices.Add(count);
		topHull.indices.Add(count2);
		int count3 = bottomHull.vertices.Count;
		bottomHull.vertices.Add(vector4);
		int count4 = bottomHull.vertices.Count;
		bottomHull.vertices.Add(vector5);
		bottomHull.indices.Add(oldToNewVertexMap[cw]);
		bottomHull.indices.Add(oldToNewVertexMap[ccw]);
		bottomHull.indices.Add(count4);
		bottomHull.indices.Add(oldToNewVertexMap[cw]);
		bottomHull.indices.Add(count4);
		bottomHull.indices.Add(count3);
		if (normals != null)
		{
			Vector3 vector6 = normals[top];
			Vector3 vector7 = normals[cw];
			Vector3 vector8 = normals[ccw];
			Vector3 item = default(Vector3);
			item.x = vector6.x + (vector7.x - vector6.x) * num2;
			item.y = vector6.y + (vector7.y - vector6.y) * num2;
			item.z = vector6.z + (vector7.z - vector6.z) * num2;
			item.Normalize();
			Vector3 item2 = default(Vector3);
			item2.x = vector6.x + (vector8.x - vector6.x) * num4;
			item2.y = vector6.y + (vector8.y - vector6.y) * num4;
			item2.z = vector6.z + (vector8.z - vector6.z) * num4;
			item2.Normalize();
			topHull.normals.Add(item);
			topHull.normals.Add(item2);
			bottomHull.normals.Add(item);
			bottomHull.normals.Add(item2);
		}
		if (tangents != null)
		{
			Vector4 vector9 = tangents[top];
			Vector4 vector10 = tangents[cw];
			Vector4 vector11 = tangents[ccw];
			Vector4 item3 = default(Vector4);
			item3.x = vector9.x + (vector10.x - vector9.x) * num2;
			item3.y = vector9.y + (vector10.y - vector9.y) * num2;
			item3.z = vector9.z + (vector10.z - vector9.z) * num2;
			item3.Normalize();
			item3.w = vector10.w;
			Vector4 item4 = default(Vector4);
			item4.x = vector9.x + (vector11.x - vector9.x) * num4;
			item4.y = vector9.y + (vector11.y - vector9.y) * num4;
			item4.z = vector9.z + (vector11.z - vector9.z) * num4;
			item4.Normalize();
			item4.w = vector11.w;
			topHull.tangents.Add(item3);
			topHull.tangents.Add(item4);
			bottomHull.tangents.Add(item3);
			bottomHull.tangents.Add(item4);
		}
		if (uvs != null)
		{
			Vector2 vector12 = uvs[top];
			Vector2 vector13 = uvs[cw];
			Vector2 vector14 = uvs[ccw];
			Vector2 item5 = new Vector2
			{
				x = vector12.x + (vector13.x - vector12.x) * num2,
				y = vector12.y + (vector13.y - vector12.y) * num2
			};
			Vector2 item6 = new Vector2
			{
				x = vector12.x + (vector14.x - vector12.x) * num4,
				y = vector12.y + (vector14.y - vector12.y) * num4
			};
			topHull.uvs.Add(item5);
			topHull.uvs.Add(item6);
			bottomHull.uvs.Add(item5);
			bottomHull.uvs.Add(item6);
		}
		cwIntersection = vector4;
		ccwIntersection = vector5;
	}

	private void FillCutEdges(FastHull a, FastHull b, IList<Vector3> edges, Vector3 planeNormal, UvMapper uvMapper)
	{
		int num = edges.Count / 2;
		List<Vector3> list = new List<Vector3>(num);
		List<int> list2 = new List<int>(num * 2);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			int num3 = i + 1;
			int num4 = num2;
			float num5 = (edges[i * 2 + 1] - edges[num2 * 2]).sqrMagnitude;
			for (int j = num3; j < num; j++)
			{
				float sqrMagnitude = (edges[i * 2 + 1] - edges[j * 2]).sqrMagnitude;
				if (sqrMagnitude < num5)
				{
					num4 = j;
					num5 = sqrMagnitude;
				}
			}
			if (num4 == num2 && i > num2)
			{
				int count = list.Count;
				int item = count;
				for (int k = num2; k < i; k++)
				{
					list.Add(edges[k * 2]);
					list2.Add(item++);
					list2.Add(item);
				}
				list.Add(edges[i * 2]);
				list2.Add(item);
				list2.Add(count);
				num2 = num3;
			}
			else if (num3 < num)
			{
				Vector3 value = edges[num3 * 2];
				Vector3 value2 = edges[num3 * 2 + 1];
				edges[num3 * 2] = edges[num4 * 2];
				edges[num3 * 2 + 1] = edges[num4 * 2 + 1];
				edges[num4 * 2] = value;
				edges[num4 * 2 + 1] = value2;
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		ITriangulator triangulator = new Triangulator(list, list2, planeNormal);
		int[] newEdges;
		int[] newTriangles;
		int[] newTriangleEdges;
		triangulator.Fill(out newEdges, out newTriangles, out newTriangleEdges);
		Vector3 item2 = -planeNormal;
		Vector4[] tangentsA;
		Vector4[] tangentsB;
		Vector2[] uvsA;
		Vector2[] uvsB;
		uvMapper.Map(list, planeNormal, out tangentsA, out tangentsB, out uvsA, out uvsB);
		int count2 = a.vertices.Count;
		int count3 = b.vertices.Count;
		for (int l = 0; l < list.Count; l++)
		{
			a.vertices.Add(list[l]);
			b.vertices.Add(list[l]);
		}
		if (normals != null)
		{
			for (int m = 0; m < list.Count; m++)
			{
				a.normals.Add(item2);
				b.normals.Add(planeNormal);
			}
		}
		if (tangents != null)
		{
			for (int n = 0; n < list.Count; n++)
			{
				a.tangents.Add(tangentsA[n]);
				b.tangents.Add(tangentsB[n]);
			}
		}
		if (uvs != null)
		{
			for (int num6 = 0; num6 < list.Count; num6++)
			{
				a.uvs.Add(uvsA[num6]);
				b.uvs.Add(uvsB[num6]);
			}
		}
		int num7 = newTriangles.Length / 3;
		for (int num8 = 0; num8 < num7; num8++)
		{
			a.indices.Add(count2 + newTriangles[num8 * 3]);
			a.indices.Add(count2 + newTriangles[num8 * 3 + 2]);
			a.indices.Add(count2 + newTriangles[num8 * 3 + 1]);
			b.indices.Add(count3 + newTriangles[num8 * 3]);
			b.indices.Add(count3 + newTriangles[num8 * 3 + 1]);
			b.indices.Add(count3 + newTriangles[num8 * 3 + 2]);
		}
	}

	private void ValidateOutput(FastHull a, FastHull b, Vector3 planeNormal)
	{
		float num = a.LengthAlongAxis(planeNormal);
		float num2 = b.LengthAlongAxis(planeNormal);
		float num3 = num + num2;
		if (num3 < smallestValidLength)
		{
			a.isValid = false;
			b.isValid = false;
		}
		else if (num / num3 < smallestValidRatio)
		{
			a.isValid = false;
		}
		else if (num2 / num3 < smallestValidRatio)
		{
			b.isValid = false;
		}
	}

	private float LengthAlongAxis(Vector3 axis)
	{
		if (vertices.Count > 0)
		{
			float num = Vector3.Dot(vertices[0], axis);
			float num2 = num;
			foreach (Vector3 vertex in vertices)
			{
				float a = Vector3.Dot(vertex, axis);
				num = Mathf.Min(a, num);
				num2 = Mathf.Max(a, num2);
			}
			return num2 - num;
		}
		return 0f;
	}
}
