#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class LineObject : MonoBehaviour
{
	public class Edge
	{
		public int i0;

		public int i1;

		public List<Triangle> shares = new List<Triangle>();

		public Edge(int i0, int i1, Triangle triangle)
		{
			this.i0 = i0;
			this.i1 = i1;
			shares.Add(triangle);
		}
	}

	public class Triangle
	{
		public int i0;

		public int i1;

		public int i2;

		public Vector3 normal = Vector3.up;

		public Triangle(int i0, int i1, int i2, Vector3 normal)
		{
			this.i0 = i0;
			this.i1 = i1;
			this.i2 = i2;
			this.normal = normal;
		}
	}

	public Material lineMaterial;

	public int[] lineIndices;

	public float coPlanarThreshold = 0.707f;

	private Mesh mesh;

	private void Awake()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		mesh = component.mesh;
	}

	private void OnEnable()
	{
		BuildLineObject();
	}

	private void OnDisable()
	{
	}

	private void BuildLineObject()
	{
		Vector3[] normals = mesh.normals;
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			Debug.Log("Submesh: " + i);
			List<Triangle> list = new List<Triangle>();
			int[] triangles = mesh.GetTriangles(i);
			for (int j = 0; j < triangles.Length; j += 3)
			{
				Vector3 normalized = (normals[triangles[j]] + normals[triangles[j + 1]] + normals[triangles[j + 2]]).normalized;
				list.Add(new Triangle(triangles[j], triangles[j + 1], triangles[j + 2], normalized));
			}
			Debug.Log("Triangles: " + list.Count);
			List<Edge> list2 = new List<Edge>();
			for (int k = 0; k < list.Count; k++)
			{
				Triangle triangle = list[k];
				Edge edge = FindExistingEdge(list2, triangle.i0, triangle.i1);
				if (edge != null)
				{
					edge.shares.Add(triangle);
				}
				else
				{
					list2.Add(new Edge(triangle.i0, triangle.i1, triangle));
				}
				edge = FindExistingEdge(list2, triangle.i1, triangle.i2);
				if (edge != null)
				{
					edge.shares.Add(triangle);
				}
				else
				{
					list2.Add(new Edge(triangle.i1, triangle.i2, triangle));
				}
				edge = FindExistingEdge(list2, triangle.i0, triangle.i2);
				if (edge != null)
				{
					edge.shares.Add(triangle);
				}
				else
				{
					list2.Add(new Edge(triangle.i0, triangle.i2, triangle));
				}
			}
			Debug.Log("Edges: " + list2.Count);
			List<int> list3 = new List<int>();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int l = 0; l < list2.Count; l++)
			{
				Edge edge2 = list2[l];
				if (edge2.shares.Count == 1)
				{
					list3.Add(edge2.i0);
					list3.Add(edge2.i1);
					num2++;
					continue;
				}
				num++;
				Vector3 normal = edge2.shares[0].normal;
				for (int m = 1; m < edge2.shares.Count; m++)
				{
					Vector3 normal2 = edge2.shares[m].normal;
					if (Vector3.Dot(normal, normal2) < coPlanarThreshold)
					{
						list3.Add(edge2.i0);
						list3.Add(edge2.i1);
						num3++;
						break;
					}
				}
			}
			Debug.Log("Lines: " + list3.Count / 2 + " used, " + num2 + " on boundary, " + num + " shared, of which " + num3 + " accepted ");
			mesh.SetIndices(list3.ToArray(), MeshTopology.Lines, i);
		}
	}

	private Edge FindExistingEdge(List<Edge> edges, int i0, int i1)
	{
		for (int j = 0; j < edges.Count; j++)
		{
			Edge edge = edges[j];
			if ((edge.i0 == i0 && edge.i1 == i1) || (edge.i0 == i1 && edge.i1 == i0))
			{
				return edge;
			}
		}
		return null;
	}
}
