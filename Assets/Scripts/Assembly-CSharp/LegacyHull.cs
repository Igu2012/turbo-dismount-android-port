#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

public class LegacyHull : IHull
{
	private static float smallestValidLength = 0.01f;

	private static float smallestValidRatio = 0.05f;

	private Object key = new Object();

	private IList<Vector3> vertices;

	private IList<Vector3> normals;

	private IList<Vector4> tangents;

	private IList<Vector2> uvs;

	private IList<Point> vertexToPointMap;

	private IList<Point> points;

	private IList<Edge> edges;

	private IList<Triangle> triangles;

	public bool IsEmpty
	{
		get
		{
			lock (key)
			{
				return points.Count < 3 || edges.Count < 3 || triangles.Count < 1;
			}
		}
	}

	public LegacyHull(Mesh mesh)
	{
		vertices = new List<Vector3>(mesh.vertices);
		normals = new List<Vector3>(mesh.normals);
		tangents = new List<Vector4>(mesh.tangents);
		uvs = new List<Vector2>(mesh.uv);
		vertexToPointMap = new List<Point>(vertices.Count);
		points = new List<Point>();
		edges = new List<Edge>();
		triangles = new List<Triangle>();
		for (int i = 0; i < vertices.Count; i++)
		{
			Point uniquePoint;
			AddUniquePoint(vertices[i], out uniquePoint);
			vertexToPointMap.Add(uniquePoint);
		}
		int[] array = mesh.triangles;
		for (int j = 0; j < array.Length / 3; j++)
		{
			int num = j * 3;
			AddTriangle(array[num], array[num + 1], array[num + 2]);
		}
	}

	public LegacyHull(LegacyHull reference)
	{
		int capacity = reference.vertices.Count * 2;
		vertices = new List<Vector3>(capacity);
		normals = new List<Vector3>(capacity);
		tangents = new List<Vector4>(capacity);
		uvs = new List<Vector2>(capacity);
		vertexToPointMap = new List<Point>(capacity);
		points = new List<Point>(reference.points.Count * 2);
		edges = new List<Edge>(reference.edges.Count * 2);
		triangles = new List<Triangle>(reference.triangles.Count * 2);
	}

	private void AddUniquePoint(Vector3 position, out Point uniquePoint)
	{
		foreach (Point point in points)
		{
			if (point.position == position)
			{
				uniquePoint = point;
				return;
			}
		}
		uniquePoint = new Point(position);
		points.Add(uniquePoint);
	}

	private void AddUniqueEdge(Point point0, Point point1, out Edge uniqueEdge)
	{
		foreach (Edge edge in edges)
		{
			if ((edge.point0 == point0 && edge.point1 == point1) || (edge.point0 == point1 && edge.point1 == point0))
			{
				uniqueEdge = edge;
				return;
			}
		}
		uniqueEdge = new Edge(point0, point1);
		edges.Add(uniqueEdge);
	}

	private void AddTriangle(int vertex0, int vertex1, int vertex2)
	{
		Point point = vertexToPointMap[vertex0];
		Point point2 = vertexToPointMap[vertex1];
		Point point3 = vertexToPointMap[vertex2];
		Edge uniqueEdge;
		AddUniqueEdge(point, point2, out uniqueEdge);
		Edge uniqueEdge2;
		AddUniqueEdge(point2, point3, out uniqueEdge2);
		Edge uniqueEdge3;
		AddUniqueEdge(point3, point, out uniqueEdge3);
		Triangle item = new Triangle(vertex0, vertex1, vertex2, point, point2, point3, uniqueEdge, uniqueEdge2, uniqueEdge3);
		triangles.Add(item);
	}

	private void AddVertex(Vector3 vertex, Vector3 normal, Vector4 tangent, Vector2 uv, Point point, out int index)
	{
		index = vertices.Count;
		vertices.Add(vertex);
		normals.Add(normal);
		tangents.Add(tangent);
		uvs.Add(uv);
		vertexToPointMap.Add(point);
	}

	public void Clear()
	{
		lock (key)
		{
			vertices.Clear();
			normals.Clear();
			tangents.Clear();
			uvs.Clear();
			vertexToPointMap.Clear();
			points.Clear();
			edges.Clear();
			triangles.Clear();
		}
	}

	public Mesh GetMesh()
	{
		lock (key)
		{
			if (!IsEmpty)
			{
				Vector3[] array = new Vector3[vertices.Count];
				Vector3[] array2 = new Vector3[normals.Count];
				Vector4[] array3 = new Vector4[tangents.Count];
				Vector2[] array4 = new Vector2[uvs.Count];
				vertices.CopyTo(array, 0);
				normals.CopyTo(array2, 0);
				tangents.CopyTo(array3, 0);
				uvs.CopyTo(array4, 0);
				int[] array5 = new int[triangles.Count * 3];
				int num = 0;
				foreach (Triangle triangle in triangles)
				{
					array5[num++] = triangle.vertex0;
					array5[num++] = triangle.vertex1;
					array5[num++] = triangle.vertex2;
				}
				Mesh mesh = new Mesh();
				mesh.vertices = array;
				mesh.normals = array2;
				mesh.tangents = array3;
				mesh.uv = array4;
				mesh.triangles = array5;
				return mesh;
			}
			return null;
		}
	}

	public void Split(Vector3 localPointOnPlane, Vector3 localPlaneNormal, bool fillCut, UvMapper uvMapper, out IHull resultA, out IHull resultB)
	{
		lock (key)
		{
			if (localPlaneNormal == Vector3.zero)
			{
				localPlaneNormal = Vector3.up;
			}
			LegacyHull legacyHull = new LegacyHull(this);
			LegacyHull legacyHull2 = new LegacyHull(this);
			SetIndices();
			bool[] pointAbovePlane;
			AssignPoints(legacyHull, legacyHull2, localPointOnPlane, localPlaneNormal, out pointAbovePlane);
			int[] oldToNewVertex;
			AssignVertices(legacyHull, legacyHull2, pointAbovePlane, out oldToNewVertex);
			bool[] edgeIntersectsPlane;
			EdgeHit[] edgeHits;
			AssignEdges(legacyHull, legacyHull2, pointAbovePlane, localPointOnPlane, localPlaneNormal, out edgeIntersectsPlane, out edgeHits);
			IList<Edge> cutEdgesA;
			IList<Edge> cutEdgesB;
			AssignTriangles(legacyHull, legacyHull2, pointAbovePlane, edgeIntersectsPlane, edgeHits, oldToNewVertex, out cutEdgesA, out cutEdgesB);
			if (fillCut)
			{
				SortCutEdges(cutEdgesA, cutEdgesB);
				FillCutEdges(legacyHull, legacyHull2, cutEdgesA, cutEdgesB, localPlaneNormal, uvMapper);
			}
			ValidateOutput(legacyHull, legacyHull2, localPlaneNormal);
			Clear();
			resultA = legacyHull;
			resultB = legacyHull2;
		}
	}

	private void SetIndices()
	{
		int num = 0;
		foreach (Point point in points)
		{
			point.index = num++;
		}
		int num2 = 0;
		foreach (Edge edge in edges)
		{
			edge.index = num2++;
		}
	}

	private void AssignPoints(LegacyHull a, LegacyHull b, Vector3 pointOnPlane, Vector3 planeNormal, out bool[] pointAbovePlane)
	{
		pointAbovePlane = new bool[points.Count];
		foreach (Point point in points)
		{
			bool flag = Vector3.Dot(point.position - pointOnPlane, planeNormal) >= 0f;
			pointAbovePlane[point.index] = flag;
			if (flag)
			{
				a.points.Add(point);
			}
			else
			{
				b.points.Add(point);
			}
		}
	}

	private void AssignVertices(LegacyHull a, LegacyHull b, bool[] pointAbovePlane, out int[] oldToNewVertex)
	{
		oldToNewVertex = new int[vertices.Count];
		for (int i = 0; i < vertices.Count; i++)
		{
			Point point = vertexToPointMap[i];
			if (pointAbovePlane[point.index])
			{
				a.AddVertex(vertices[i], normals[i], tangents[i], uvs[i], point, out oldToNewVertex[i]);
			}
			else
			{
				b.AddVertex(vertices[i], normals[i], tangents[i], uvs[i], point, out oldToNewVertex[i]);
			}
		}
	}

	private void AssignEdges(LegacyHull a, LegacyHull b, bool[] pointAbovePlane, Vector3 pointOnPlane, Vector3 planeNormal, out bool[] edgeIntersectsPlane, out EdgeHit[] edgeHits)
	{
		edgeIntersectsPlane = new bool[edges.Count];
		edgeHits = new EdgeHit[edges.Count];
		foreach (Edge edge3 in edges)
		{
			bool flag = pointAbovePlane[edge3.point0.index];
			bool flag2 = pointAbovePlane[edge3.point1.index];
			if (flag && flag2)
			{
				a.edges.Add(edge3);
				continue;
			}
			if (!flag && !flag2)
			{
				b.edges.Add(edge3);
				continue;
			}
			float num = Vector3.Dot(edge3.line, planeNormal);
			float num2 = Mathf.Clamp01(Vector3.Dot(pointOnPlane - edge3.point0.position, planeNormal) / num);
			Vector3 position = edge3.point0.position + edge3.line * num2;
			Point point = new Point(position);
			Point point2 = new Point(position);
			a.points.Add(point);
			b.points.Add(point2);
			Edge edge;
			Edge edge2;
			if (pointAbovePlane[edge3.point0.index])
			{
				edge = new Edge(point, edge3.point0);
				edge2 = new Edge(point2, edge3.point1);
			}
			else
			{
				edge = new Edge(point, edge3.point1);
				edge2 = new Edge(point2, edge3.point0);
			}
			a.edges.Add(edge);
			b.edges.Add(edge2);
			edgeIntersectsPlane[edge3.index] = true;
			edgeHits[edge3.index] = default(EdgeHit);
			edgeHits[edge3.index].scalar = num2;
			edgeHits[edge3.index].splitA = edge;
			edgeHits[edge3.index].splitB = edge2;
		}
	}

	private void AssignTriangles(LegacyHull a, LegacyHull b, bool[] pointAbovePlane, bool[] edgeIntersectsPlane, EdgeHit[] edgeHits, int[] oldToNewVertex, out IList<Edge> cutEdgesA, out IList<Edge> cutEdgesB)
	{
		cutEdgesA = new List<Edge>();
		cutEdgesB = new List<Edge>();
		foreach (Triangle triangle in triangles)
		{
			bool flag = pointAbovePlane[triangle.point0.index];
			bool flag2 = pointAbovePlane[triangle.point1.index];
			bool flag3 = pointAbovePlane[triangle.point2.index];
			if (flag && flag2 && flag3)
			{
				triangle.vertex0 = oldToNewVertex[triangle.vertex0];
				triangle.vertex1 = oldToNewVertex[triangle.vertex1];
				triangle.vertex2 = oldToNewVertex[triangle.vertex2];
				a.triangles.Add(triangle);
				continue;
			}
			if (!flag && !flag2 && !flag3)
			{
				triangle.vertex0 = oldToNewVertex[triangle.vertex0];
				triangle.vertex1 = oldToNewVertex[triangle.vertex1];
				triangle.vertex2 = oldToNewVertex[triangle.vertex2];
				b.triangles.Add(triangle);
				continue;
			}
			Point point;
			Edge edge;
			Edge edge2;
			Edge bottomEdge;
			int vertex;
			int vertex2;
			int vertex3;
			if (edgeIntersectsPlane[triangle.edge0.index] && edgeIntersectsPlane[triangle.edge1.index])
			{
				point = triangle.point1;
				edge = triangle.edge0;
				edge2 = triangle.edge1;
				bottomEdge = triangle.edge2;
				vertex = triangle.vertex0;
				vertex2 = triangle.vertex1;
				vertex3 = triangle.vertex2;
			}
			else if (edgeIntersectsPlane[triangle.edge1.index] && edgeIntersectsPlane[triangle.edge2.index])
			{
				point = triangle.point2;
				edge = triangle.edge1;
				edge2 = triangle.edge2;
				bottomEdge = triangle.edge0;
				vertex = triangle.vertex1;
				vertex2 = triangle.vertex2;
				vertex3 = triangle.vertex0;
			}
			else
			{
				point = triangle.point0;
				edge = triangle.edge2;
				edge2 = triangle.edge0;
				bottomEdge = triangle.edge1;
				vertex = triangle.vertex2;
				vertex2 = triangle.vertex0;
				vertex3 = triangle.vertex1;
			}
			EdgeHit edgeHit = edgeHits[edge.index];
			EdgeHit edgeHit2 = edgeHits[edge2.index];
			float scalar = ((point != edge.point1) ? (1f - edgeHit.scalar) : edgeHit.scalar);
			float scalar2 = ((point != edge2.point0) ? (1f - edgeHit2.scalar) : edgeHit2.scalar);
			Edge edge3;
			Edge edge4;
			if (pointAbovePlane[point.index])
			{
				edge3 = new Edge(edgeHit2.splitA.point0, edgeHit.splitA.point0);
				edge4 = new Edge(edgeHit2.splitB.point0, edgeHit.splitB.point0);
				a.edges.Add(edge3);
				b.edges.Add(edge4);
				SplitTriangle(a, b, edgeHit.splitA, edgeHit2.splitA, edge3, edgeHit.splitB, edgeHit2.splitB, edge4, bottomEdge, vertex, vertex2, vertex3, scalar, scalar2, oldToNewVertex);
			}
			else
			{
				edge3 = new Edge(edgeHit.splitA.point0, edgeHit2.splitA.point0);
				edge4 = new Edge(edgeHit.splitB.point0, edgeHit2.splitB.point0);
				a.edges.Add(edge3);
				b.edges.Add(edge4);
				SplitTriangle(b, a, edgeHit.splitB, edgeHit2.splitB, edge4, edgeHit.splitA, edgeHit2.splitA, edge3, bottomEdge, vertex, vertex2, vertex3, scalar, scalar2, oldToNewVertex);
			}
			cutEdgesA.Add(edge3);
			cutEdgesB.Add(edge4);
		}
	}

	private void SplitTriangle(LegacyHull topHull, LegacyHull bottomHull, Edge topEdge0, Edge topEdge1, Edge topCutEdge, Edge bottomEdge0, Edge bottomEdge1, Edge bottomCutEdge, Edge bottomEdge2, int vertex0, int vertex1, int vertex2, float scalar0, float scalar1, int[] oldToNewVertex)
	{
		Vector3 vector = normals[vertex0];
		Vector3 vector2 = normals[vertex1];
		Vector3 vector3 = normals[vertex2];
		Vector4 vector4 = tangents[vertex0];
		Vector4 vector5 = tangents[vertex1];
		Vector4 vector6 = tangents[vertex2];
		Vector2 vector7 = uvs[vertex0];
		Vector2 vector8 = uvs[vertex1];
		Vector2 vector9 = uvs[vertex2];
		Vector3 normal = default(Vector3);
		normal.x = vector.x + (vector2.x - vector.x) * scalar0;
		normal.y = vector.y + (vector2.y - vector.y) * scalar0;
		normal.z = vector.z + (vector2.z - vector.z) * scalar0;
		normal.Normalize();
		Vector3 normal2 = default(Vector3);
		normal2.x = vector2.x + (vector3.x - vector2.x) * scalar1;
		normal2.y = vector2.y + (vector3.y - vector2.y) * scalar1;
		normal2.z = vector2.z + (vector3.z - vector2.z) * scalar1;
		normal2.Normalize();
		Vector4 tangent = default(Vector4);
		tangent.x = vector4.x + (vector5.x - vector4.x) * scalar0;
		tangent.y = vector4.y + (vector5.y - vector4.y) * scalar0;
		tangent.z = vector4.z + (vector5.z - vector4.z) * scalar0;
		tangent.Normalize();
		tangent.w = vector4.w;
		Vector4 tangent2 = default(Vector4);
		tangent2.x = vector5.x + (vector6.x - vector5.x) * scalar1;
		tangent2.y = vector5.y + (vector6.y - vector5.y) * scalar1;
		tangent2.z = vector5.z + (vector6.z - vector5.z) * scalar1;
		tangent2.Normalize();
		tangent2.w = vector5.w;
		Vector2 uv = new Vector2
		{
			x = vector7.x + (vector8.x - vector7.x) * scalar0,
			y = vector7.y + (vector8.y - vector7.y) * scalar0
		};
		Vector2 uv2 = new Vector2
		{
			x = vector8.x + (vector9.x - vector8.x) * scalar1,
			y = vector8.y + (vector9.y - vector8.y) * scalar1
		};
		int index;
		topHull.AddVertex(topEdge0.point0.position, normal, tangent, uv, topEdge0.point0, out index);
		int index2;
		topHull.AddVertex(topEdge1.point0.position, normal2, tangent2, uv2, topEdge1.point0, out index2);
		int index3;
		bottomHull.AddVertex(bottomEdge0.point0.position, normal, tangent, uv, bottomEdge0.point0, out index3);
		int index4;
		bottomHull.AddVertex(bottomEdge1.point0.position, normal2, tangent2, uv2, bottomEdge1.point0, out index4);
		Triangle item = new Triangle(index, oldToNewVertex[vertex1], index2, topEdge0.point0, topEdge0.point1, topEdge1.point0, topEdge0, topEdge1, topCutEdge);
		topHull.triangles.Add(item);
		Edge edge = new Edge(bottomEdge0.point1, bottomEdge1.point0);
		Triangle item2 = new Triangle(oldToNewVertex[vertex0], index3, index4, bottomEdge0.point1, bottomEdge0.point0, bottomEdge1.point0, bottomEdge0, bottomCutEdge, edge);
		Triangle item3 = new Triangle(oldToNewVertex[vertex0], index4, oldToNewVertex[vertex2], bottomEdge0.point1, bottomEdge1.point0, bottomEdge1.point1, edge, bottomEdge1, bottomEdge2);
		bottomHull.edges.Add(edge);
		bottomHull.triangles.Add(item2);
		bottomHull.triangles.Add(item3);
	}

	private void SortCutEdges(IList<Edge> edgesA, IList<Edge> edgesB)
	{
		Edge edge = null;
		for (int i = 0; i < edgesA.Count; i++)
		{
			if (edge == null)
			{
				edge = edgesA[i];
				continue;
			}
			Edge edge2 = edgesA[i - 1];
			for (int j = i; j < edgesA.Count; j++)
			{
				Edge edge3 = edgesA[j];
				if (edge2.point1 == edge3.point0)
				{
					Edge value = edgesA[i];
					edgesA[i] = edge3;
					edgesA[j] = value;
					Edge value2 = edgesB[i];
					edgesB[i] = edgesB[j];
					edgesB[j] = value2;
					if (edge3.point1 == edge.point0)
					{
						edge = null;
					}
					break;
				}
			}
		}
	}

	private void FillCutEdges(LegacyHull a, LegacyHull b, IList<Edge> edgesA, IList<Edge> edgesB, Vector3 planeNormal, UvMapper uvMapper)
	{
		int count = edgesA.Count;
		Vector3[] array = new Vector3[count];
		int[] array2 = new int[count * 2];
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			int num2 = i;
			int num3 = (i + 1) % count;
			Edge edge = edgesA[num2];
			Edge edge2 = edgesA[num3];
			array[i] = edge.point0.position;
			array2[i * 2] = num2;
			if (edge.point1 == edge2.point0)
			{
				array2[i * 2 + 1] = num3;
				continue;
			}
			array2[i * 2 + 1] = num;
			num = num3;
		}
		ITriangulator triangulator = new Triangulator(array, array2, planeNormal);
		int[] newEdges;
		int[] newTriangles;
		int[] newTriangleEdges;
		triangulator.Fill(out newEdges, out newTriangles, out newTriangleEdges);
		Vector3 normal = -planeNormal;
		Vector4[] tangentsA;
		Vector4[] tangentsB;
		Vector2[] uvsA;
		Vector2[] uvsB;
		uvMapper.Map(array, planeNormal, out tangentsA, out tangentsB, out uvsA, out uvsB);
		int[] array3 = new int[count];
		int[] array4 = new int[count];
		for (int j = 0; j < count; j++)
		{
			a.AddVertex(array[j], normal, tangentsA[j], uvsA[j], edgesA[j].point0, out array3[j]);
			b.AddVertex(array[j], planeNormal, tangentsB[j], uvsB[j], edgesB[j].point0, out array4[j]);
		}
		for (int k = 0; k < newEdges.Length / 2; k++)
		{
			int index = newEdges[k * 2];
			int index2 = newEdges[k * 2 + 1];
			Edge item = new Edge(edgesA[index].point0, edgesA[index2].point0);
			Edge item2 = new Edge(edgesB[index].point0, edgesB[index2].point0);
			edgesA.Add(item);
			edgesB.Add(item2);
			a.edges.Add(item);
			b.edges.Add(item2);
		}
		for (int l = 0; l < newTriangles.Length / 3; l++)
		{
			int num4 = newTriangles[l * 3];
			int num5 = newTriangles[l * 3 + 1];
			int num6 = newTriangles[l * 3 + 2];
			int index3 = newTriangleEdges[l * 3];
			int index4 = newTriangleEdges[l * 3 + 1];
			int index5 = newTriangleEdges[l * 3 + 2];
			Triangle item3 = new Triangle(array3[num4], array3[num6], array3[num5], edgesA[num4].point0, edgesA[num6].point0, edgesA[num5].point0, edgesA[index5], edgesA[index4], edgesA[index3]);
			Triangle item4 = new Triangle(array4[num4], array4[num5], array4[num6], edgesB[num4].point0, edgesB[num5].point0, edgesB[num6].point0, edgesB[index3], edgesB[index4], edgesB[index5]);
			a.triangles.Add(item3);
			b.triangles.Add(item4);
		}
	}

	private void ValidateOutput(LegacyHull a, LegacyHull b, Vector3 planeNormal)
	{
		float num = a.LengthAlongAxis(planeNormal);
		float num2 = b.LengthAlongAxis(planeNormal);
		float num3 = num + num2;
		if (num3 < smallestValidLength)
		{
			a.Clear();
			b.Clear();
		}
		else if (num / num3 < smallestValidRatio)
		{
			a.Clear();
		}
		else if (num2 / num3 < smallestValidRatio)
		{
			b.Clear();
		}
	}

	private float LengthAlongAxis(Vector3 axis)
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < points.Count; i++)
		{
			float num3 = Vector3.Dot(points[i].position, axis);
			if (i == 0)
			{
				num = num3;
				num2 = num3;
			}
			else
			{
				num = Mathf.Min(num3, num);
				num2 = Mathf.Max(num3, num2);
			}
		}
		return num2 - num;
	}
}
