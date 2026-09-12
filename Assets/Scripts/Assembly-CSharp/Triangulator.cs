#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

public class Triangulator : ITriangulator
{
	private Object key = new Object();

	private List<Vector3> points;

	private List<int> edges;

	private List<int> triangles;

	private List<int> triangleEdges;

	private List<List<int>> loops;

	private List<List<bool>> concavities;

	private List<int> duplicateEdges;

	private Vector3 planeNormal;

	private int originalEdgeCount;

	public Triangulator(IList<Vector3> points, IList<int> edges, Vector3 planeNormal)
	{
		this.points = new List<Vector3>(points);
		this.edges = new List<int>(edges);
		triangles = new List<int>();
		triangleEdges = new List<int>();
		this.planeNormal = planeNormal;
		originalEdgeCount = this.edges.Count;
	}

	public void Fill(out int[] newEdges, out int[] newTriangles, out int[] newTriangleEdges)
	{
		lock (key)
		{
			FindLoops();
			FindConcavities();
			PrepareDuplicateEdges();
			for (int i = 0; i < loops.Count; i++)
			{
				List<int> list = loops[i];
				List<bool> list2 = concavities[i];
				int num = 0;
				int num2 = 0;
				while (list.Count >= 3)
				{
					int zero = ((num != 0) ? (num - 1) : (list.Count - 1));
					int num3 = num;
					int second = (num + 1) % list.Count;
					int third = (num + 2) % list.Count;
					if (list2[num3] || IsTriangleOverlappingLoop(num3, second, third, list, list2))
					{
						num++;
						num2++;
					}
					else
					{
						int swallowedLoopIndex;
						if (MergeLoops(num3, second, third, list, list2, out swallowedLoopIndex))
						{
							if (swallowedLoopIndex < i)
							{
								i--;
							}
						}
						else
						{
							FillTriangle(zero, num3, second, third, list, list2);
						}
						num2 = 0;
					}
					if (num2 >= list.Count)
					{
						break;
					}
					if (num >= list.Count)
					{
						num = 0;
						num2 = 0;
					}
				}
				if (list.Count <= 2)
				{
					loops.RemoveAt(i);
					concavities.RemoveAt(i);
					i--;
				}
			}
			for (int j = 0; j < loops.Count; j++)
			{
				List<int> list3 = loops[j];
				List<bool> concavity = concavities[j];
				while (list3.Count >= 3)
				{
					FillTriangle(0, 1, 2, 3 % list3.Count, list3, concavity);
				}
			}
			RemoveDuplicateEdges();
			SetOutput(out newEdges, out newTriangles, out newTriangleEdges);
		}
	}

	private void FindLoops()
	{
		loops = new List<List<int>>();
		List<int> list = new List<int>(edges.Count / 2);
		for (int i = 0; i < edges.Count / 2; i++)
		{
			int num = i * 2;
			int num2 = edges[num];
			int num3 = edges[num + 1];
			if (list.Count >= 1)
			{
				int num4 = edges[num - 1];
				if (num2 != num4)
				{
					Debug.LogError("The edges do not form an edge loop!");
				}
			}
			list.Add(num);
			if (num3 == edges[list[0]])
			{
				loops.Add(list);
				list = new List<int>();
			}
		}
	}

	private void FindConcavities()
	{
		concavities = new List<List<bool>>();
		foreach (List<int> loop in loops)
		{
			List<bool> list = new List<bool>(loop.Count);
			for (int i = 0; i < loop.Count; i++)
			{
				int index = edges[loop[i]];
				int index2 = edges[loop[(i + 1) % loop.Count]];
				int index3 = edges[loop[(i + 2) % loop.Count]];
				Vector3 line = points[index2] - points[index];
				Vector3 line2 = points[index3] - points[index2];
				list.Add(IsLinePairConcave(ref line, ref line2));
			}
			concavities.Add(list);
		}
	}

	private void PrepareDuplicateEdges()
	{
		duplicateEdges = new List<int>();
	}

	private void ValidateConcavities()
	{
		for (int i = 0; i < loops.Count; i++)
		{
			IList<int> list = loops[i];
			IList<bool> list2 = concavities[i];
			for (int j = 0; j < list.Count; j++)
			{
				int index = edges[list[j]];
				int index2 = edges[list[(j + 1) % list.Count]];
				int index3 = edges[list[(j + 2) % list.Count]];
				Vector3 line = points[index2] - points[index];
				Vector3 line2 = points[index3] - points[index2];
				if (list2[j] != IsLinePairConcave(ref line, ref line2))
				{
					Debug.LogError("Concavity not valid!");
				}
			}
		}
	}

	private void UpdateConcavity(int index, List<int> loop, List<bool> concavity)
	{
		int num = loop[index];
		int num2 = loop[(index + 1) % loop.Count];
		Vector3 line = points[edges[num + 1]] - points[edges[num]];
		Vector3 line2 = points[edges[num2 + 1]] - points[edges[num2]];
		concavity[index] = IsLinePairConcave(ref line, ref line2);
	}

	private bool IsLinePairConcave(ref Vector3 line0, ref Vector3 line1)
	{
		Vector3 rhs = Vector3.Cross(line0, planeNormal);
		return Vector3.Dot(line1, rhs) > 0f;
	}

	private bool IsTriangleOverlappingLoop(int first, int second, int third, List<int> loop, List<bool> concavity)
	{
		int num = edges[loop[first]];
		int num2 = edges[loop[second]];
		int num3 = edges[loop[third]];
		Vector3 triangle = points[num];
		Vector3 triangle2 = points[num2];
		Vector3 triangle3 = points[num3];
		for (int i = 0; i < loop.Count; i++)
		{
			if (!concavity[i])
			{
				continue;
			}
			int num4 = edges[loop[i] + 1];
			if (num4 != num && num4 != num2 && num4 != num3)
			{
				Vector3 point = points[num4];
				if (ShatterTools.IsPointInsideTriangle(ref point, ref triangle, ref triangle2, ref triangle3, ref planeNormal))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool MergeLoops(int first, int second, int third, List<int> loop, List<bool> concavity, out int swallowedLoopIndex)
	{
		int loopIndex;
		int loopLocation;
		if (FindClosestPointInTriangle(first, second, third, loop, out loopIndex, out loopLocation))
		{
			InsertLoop(first, loop, concavity, loopLocation, loops[loopIndex], concavities[loopIndex]);
			loops.RemoveAt(loopIndex);
			concavities.RemoveAt(loopIndex);
			swallowedLoopIndex = loopIndex;
			return true;
		}
		swallowedLoopIndex = -1;
		return false;
	}

	private bool FindClosestPointInTriangle(int first, int second, int third, List<int> loop, out int loopIndex, out int loopLocation)
	{
		Vector3 triangle = points[edges[loop[first]]];
		Vector3 triangle2 = points[edges[loop[second]]];
		Vector3 triangle3 = points[edges[loop[third]]];
		Vector3 rhs = Vector3.Cross(planeNormal, triangle2 - triangle);
		int num = -1;
		int num2 = 0;
		float num3 = 0f;
		for (int i = 0; i < loops.Count; i++)
		{
			IList<int> list = loops[i];
			IList<bool> list2 = concavities[i];
			if (list == loop)
			{
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (!list2[j])
				{
					continue;
				}
				Vector3 point = points[edges[list[j] + 1]];
				if (ShatterTools.IsPointInsideTriangle(ref point, ref triangle, ref triangle2, ref triangle3, ref planeNormal))
				{
					float num4 = Vector3.Dot(point - triangle, rhs);
					if (num4 < num3 || num == -1)
					{
						num = i;
						num2 = (j + 1) % list.Count;
						num3 = num4;
					}
				}
			}
		}
		loopIndex = num;
		loopLocation = num2;
		return num != -1;
	}

	private void InsertLoop(int insertLocation, List<int> loop, List<bool> concavity, int otherAnchorLocation, List<int> otherLoop, List<bool> otherConcavity)
	{
		int item = edges[loop[insertLocation]];
		int item2 = edges[otherLoop[otherAnchorLocation]];
		int count = edges.Count;
		edges.Add(item2);
		edges.Add(item);
		int count2 = edges.Count;
		edges.Add(item);
		edges.Add(item2);
		duplicateEdges.Add(count2);
		int[] array = new int[otherLoop.Count + 2];
		bool[] array2 = new bool[otherConcavity.Count + 2];
		int num = 0;
		array[num] = count2;
		array2[num++] = false;
		for (int i = otherAnchorLocation; i < otherLoop.Count; i++)
		{
			array[num] = otherLoop[i];
			array2[num++] = otherConcavity[i];
		}
		for (int j = 0; j < otherAnchorLocation; j++)
		{
			array[num] = otherLoop[j];
			array2[num++] = otherConcavity[j];
		}
		array[num] = count;
		array2[num] = false;
		loop.InsertRange(insertLocation, array);
		concavity.InsertRange(insertLocation, array2);
		int index = ((insertLocation != 0) ? (insertLocation - 1) : (loop.Count - 1));
		UpdateConcavity(index, loop, concavity);
		UpdateConcavity(insertLocation, loop, concavity);
		UpdateConcavity(insertLocation + otherLoop.Count, loop, concavity);
		UpdateConcavity(insertLocation + otherLoop.Count + 1, loop, concavity);
	}

	private void FillTriangle(int zero, int first, int second, int third, List<int> loop, List<bool> concavity)
	{
		int index = loop[zero];
		int num = loop[first];
		int num2 = loop[second];
		int num3 = loop[third];
		int num4 = edges[num];
		int item = edges[num2];
		int num5 = edges[num3];
		int num6;
		if (loop.Count != 3)
		{
			num6 = edges.Count;
			edges.Add(num4);
			edges.Add(num5);
		}
		else
		{
			num6 = num3;
		}
		triangles.Add(num4);
		triangles.Add(item);
		triangles.Add(num5);
		triangleEdges.Add(num);
		triangleEdges.Add(num2);
		triangleEdges.Add(num6);
		loop[second] = num6;
		loop.RemoveAt(first);
		Vector3 line = points[num4] - points[edges[index]];
		Vector3 line2 = points[num5] - points[num4];
		Vector3 line3 = points[edges[num3 + 1]] - points[num5];
		concavity[zero] = IsLinePairConcave(ref line, ref line2);
		concavity[second] = IsLinePairConcave(ref line2, ref line3);
		concavity.RemoveAt(first);
	}

	private void RemoveDuplicateEdges()
	{
		for (int i = 0; i < duplicateEdges.Count; i++)
		{
			int num = duplicateEdges[i];
			edges.RemoveRange(num, 2);
			for (int j = 0; j < triangleEdges.Count; j++)
			{
				if (triangleEdges[j] >= num)
				{
					List<int> list2;
					List<int> list = (list2 = triangleEdges);
					int index2;
					int index = (index2 = j);
					index2 = list2[index2];
					list[index] = index2 - 2;
				}
			}
			for (int k = i + 1; k < duplicateEdges.Count; k++)
			{
				if (duplicateEdges[k] >= num)
				{
					List<int> list4;
					List<int> list3 = (list4 = duplicateEdges);
					int index2;
					int index3 = (index2 = k);
					index2 = list4[index2];
					list3[index3] = index2 - 2;
				}
			}
		}
	}

	private void SetOutput(out int[] newEdges, out int[] newTriangles, out int[] newTriangleEdges)
	{
		int num = edges.Count - originalEdgeCount;
		if (num > 0)
		{
			newEdges = new int[num];
			edges.CopyTo(originalEdgeCount, newEdges, 0, num);
		}
		else
		{
			newEdges = new int[0];
		}
		newTriangles = triangles.ToArray();
		newTriangleEdges = new int[triangleEdges.Count];
		for (int i = 0; i < triangleEdges.Count; i++)
		{
			newTriangleEdges[i] = triangleEdges[i] / 2;
		}
	}
}
