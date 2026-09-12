#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount.Vehicular
{
	public class Skidmark : MonoBehaviour
	{
		private const int maxSegmentCount = 4096;

		private const int maxVertexCount = 8192;

		private const int maxIndexCount = 24576;

		[HideInInspector]
		public Vector3[] vertices;

		[HideInInspector]
		public Vector2[] uvs;

		[HideInInspector]
		public Color32[] colors;

		[HideInInspector]
		public int[] triangles;

		private int[] extraSlabTriangles = new int[6];

		private int segmentCount;

		private int vertexCount;

		private int indexCount;

		private int currentSegmentStartIndex = -1;

		[HideInInspector]
		public int slabCount;

		private bool needsUpdate;

		private bool extraSlab;

		[HideInInspector]
		public bool isReplayClone;

		private Mesh mesh;

		private bool initiated;

		private void Awake()
		{
			if (!initiated)
			{
				vertices = new Vector3[8192];
				uvs = new Vector2[8192];
				colors = new Color32[8192];
				triangles = new int[24576];
				mesh = GetComponent<MeshFilter>().mesh;
				mesh.MarkDynamic();
				mesh.Clear();
				for (int i = 0; i < 8192; i++)
				{
					vertices[i] = Vector3.zero;
					uvs[i] = Vector2.zero;
					colors[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				}
				for (int j = 0; j < 24576; j++)
				{
					triangles[j] = 0;
				}
				mesh.vertices = vertices;
				mesh.uv = uvs;
				mesh.colors32 = colors;
				int[] array = new int[3] { 0, 1, 2 };
				mesh.triangles = array;
				int num = 0;
				extraSlabTriangles[num++] = 8188;
				extraSlabTriangles[num++] = 8189;
				extraSlabTriangles[num++] = 8190;
				extraSlabTriangles[num++] = 8190;
				extraSlabTriangles[num++] = 8189;
				extraSlabTriangles[num++] = 8191;
				initiated = true;
			}
		}

		private void OnEnable()
		{
			mesh.Clear();
			segmentCount = 0;
			vertexCount = 0;
			indexCount = 0;
			currentSegmentStartIndex = -1;
			if (!isReplayClone)
			{
				slabCount = 0;
			}
		}

		private void LateUpdate()
		{
			UpdateMesh();
		}

		private void UpdateMesh()
		{
			if (needsUpdate)
			{
				mesh.Clear();
				mesh.vertices = vertices;
				mesh.uv = uvs;
				mesh.colors32 = colors;
				int[] destinationArray;
				if (extraSlab)
				{
					destinationArray = new int[indexCount + 6];
					Array.Copy(triangles, destinationArray, indexCount);
					Array.Copy(extraSlabTriangles, 0, destinationArray, indexCount, 6);
				}
				else
				{
					destinationArray = new int[indexCount];
					Array.Copy(triangles, destinationArray, indexCount);
				}
				mesh.triangles = destinationArray;
				needsUpdate = false;
				extraSlab = false;
			}
		}

		public void AddSegment(Vector3 leftEdge, Vector3 rightEdge, Vector3 up, float intensity, float width)
		{
			if (segmentCount > 4093 || (intensity < 0.001f && currentSegmentStartIndex == -1))
			{
				return;
			}
			if (currentSegmentStartIndex == -1)
			{
				currentSegmentStartIndex = segmentCount;
			}
			byte a = (byte)(255f * intensity);
			vertices[vertexCount] = leftEdge;
			uvs[vertexCount] = new Vector2(0.5f - width * 0.5f, 0.5f);
			colors[vertexCount].a = a;
			vertexCount++;
			vertices[vertexCount] = rightEdge;
			uvs[vertexCount] = new Vector2(0.5f + width * 0.5f, 0.5f);
			colors[vertexCount].a = a;
			vertexCount++;
			segmentCount++;
			if (segmentCount - currentSegmentStartIndex >= 2)
			{
				triangles[indexCount++] = vertexCount - 4;
				triangles[indexCount++] = vertexCount - 3;
				triangles[indexCount++] = vertexCount - 2;
				triangles[indexCount++] = vertexCount - 2;
				triangles[indexCount++] = vertexCount - 3;
				triangles[indexCount++] = vertexCount - 1;
				slabCount++;
				if (intensity < 0.001f)
				{
					currentSegmentStartIndex = -1;
				}
				needsUpdate = true;
			}
		}

		public void RenderUpTo(float slab)
		{
			int num = Mathf.FloorToInt(slab);
			float t = slab - (float)num;
			indexCount = num * 6;
			needsUpdate = true;
			if (num < slabCount)
			{
				extraSlab = true;
				int num2 = triangles[indexCount];
				int num3 = triangles[indexCount + 1];
				int num4 = triangles[indexCount + 3];
				int num5 = triangles[indexCount + 5];
				int num6 = 8188;
				vertices[num6] = vertices[num2];
				uvs[num6] = uvs[num2];
				colors[num6] = colors[num2];
				num6++;
				vertices[num6] = vertices[num3];
				uvs[num6] = uvs[num3];
				colors[num6] = colors[num3];
				num6++;
				vertices[num6] = Vector3.Lerp(vertices[num2], vertices[num4], t);
				uvs[num6] = Vector2.Lerp(uvs[num2], uvs[num4], t);
				colors[num6] = Color.Lerp(colors[num2], colors[num4], t);
				num6++;
				vertices[num6] = Vector3.Lerp(vertices[num3], vertices[num5], t);
				uvs[num6] = Vector2.Lerp(uvs[num3], uvs[num5], t);
				colors[num6] = Color.Lerp(colors[num3], colors[num5], t);
			}
		}

		private void SetPool(ObjectPool pool)
		{
		}
	}
}
