#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SuperSplines/Other/Branching Spline")]
public class BranchingSpline : MonoBehaviour
{
	public delegate BranchingSplinePath BranchingController(BranchingSplineParameter currentParameter, List<BranchingSplinePath> possiblePaths);

	public List<Spline> splines = new List<Spline>();

	private int recoursionCounter;

	private bool SplinesAvailable
	{
		get
		{
			if (splines == null)
			{
				return false;
			}
			if (splines.Count <= 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool Advance(BranchingSplineParameter bParam, float distanceOffset, BranchingController bController)
	{
		bool flag = false;
		if (!SplinesAvailable)
		{
			return false;
		}
		if (++recoursionCounter > 12)
		{
			recoursionCounter = 0;
			return false;
		}
		CheckParameter(bParam);
		Spline spline = bParam.spline;
		SplineNode splineNode = IsOnSplineNode(bParam.parameter, spline);
		if (splineNode != null)
		{
			BranchingSplinePath branchingSplinePath = ChoseSpline(splineNode, bParam, bController, distanceOffset > 0f);
			bParam.spline = branchingSplinePath.spline;
			bParam.direction = branchingSplinePath.direction;
			bParam.parameter = splineNode.Parameters[bParam.spline].PosInSpline;
			SplineNode[] adjacentSegmentNodes = GetAdjacentSegmentNodes(branchingSplinePath.spline, splineNode);
			SplineNode splineNode2 = adjacentSegmentNodes[ForwardOnSpline(branchingSplinePath.direction, distanceOffset) ? 1 : 0];
			if (splineNode2 != null)
			{
				bParam.parameter += (splineNode2.Parameters[bParam.spline].PosInSpline - splineNode.Parameters[bParam.spline].PosInSpline) * 0.001f;
				Advance(bParam, distanceOffset, bController);
				flag = false;
			}
			else
			{
				flag = false;
			}
		}
		else
		{
			SplineSegment splineSegment = spline.GetSplineSegment(bParam.parameter);
			float num = spline.Length * (float)(bParam.Forward ? 1 : (-1));
			float num2 = distanceOffset / num;
			float num3 = bParam.parameter + num2;
			float num4 = splineSegment.ClampParameterToSegment(num3);
			float num5 = num3 - num4;
			bParam.parameter = num4;
			flag = !Mathf.Approximately(num5, 0f) && Advance(bParam, num5 * num, bController);
		}
		recoursionCounter = 0;
		return flag;
	}

	public Vector3 GetPosition(BranchingSplineParameter bParam)
	{
		if (!SplinesAvailable)
		{
			return Vector3.zero;
		}
		CheckParameter(bParam);
		return bParam.spline.GetPositionOnSpline(bParam.parameter);
	}

	public Quaternion GetOrientation(BranchingSplineParameter bParam)
	{
		if (!SplinesAvailable)
		{
			return Quaternion.identity;
		}
		CheckParameter(bParam);
		return bParam.spline.GetOrientationOnSpline(bParam.parameter);
	}

	public Vector3 GetTangent(BranchingSplineParameter bParam)
	{
		if (!SplinesAvailable)
		{
			return Vector3.zero;
		}
		CheckParameter(bParam);
		return bParam.spline.GetTangentToSpline(bParam.parameter);
	}

	public float GetCustomValue(BranchingSplineParameter bParam)
	{
		if (!SplinesAvailable)
		{
			return 0f;
		}
		CheckParameter(bParam);
		return bParam.spline.GetCustomValueOnSpline(bParam.parameter);
	}

	public Vector3 GetNormal(BranchingSplineParameter bParam)
	{
		if (!SplinesAvailable)
		{
			return Vector3.zero;
		}
		CheckParameter(bParam);
		return bParam.spline.GetNormalToSpline(bParam.parameter);
	}

	private BranchingSplinePath ChoseSpline(SplineNode switchNode, BranchingSplineParameter currentPath, BranchingController bController, bool positiveValue)
	{
		IList<Spline> splinesForNode = GetSplinesForNode(switchNode);
		List<BranchingSplinePath> list = new List<BranchingSplinePath>();
		if (splinesForNode.Count == 1 && splinesForNode[0] == currentPath.spline)
		{
			return new BranchingSplinePath(currentPath.spline, currentPath.direction);
		}
		if (IsMiddleNode(currentPath.spline, switchNode))
		{
			list.Add(new BranchingSplinePath(currentPath.spline, currentPath.direction));
		}
		foreach (Spline item in splinesForNode)
		{
			if (item == currentPath.spline)
			{
				continue;
			}
			if (IsMiddleNode(item, switchNode))
			{
				list.Add(new BranchingSplinePath(item, BranchingSplinePath.Direction.Forwards));
				list.Add(new BranchingSplinePath(item, BranchingSplinePath.Direction.Backwards));
				continue;
			}
			SplineNode[] splineNodes = item.SplineNodes;
			int num = Array.IndexOf(splineNodes, switchNode);
			if (num == 0)
			{
				list.Add(new BranchingSplinePath(item, (!positiveValue) ? BranchingSplinePath.Direction.Backwards : BranchingSplinePath.Direction.Forwards));
			}
			if (num == splineNodes.Length - 1)
			{
				list.Add(new BranchingSplinePath(item, positiveValue ? BranchingSplinePath.Direction.Backwards : BranchingSplinePath.Direction.Forwards));
			}
		}
		return bController(currentPath, list);
	}

	private SplineNode IsOnSplineNode(float param, Spline spline)
	{
		SplineNode[] segmentNodes = spline.SegmentNodes;
		foreach (SplineNode splineNode in segmentNodes)
		{
			if (Mathf.Approximately(splineNode.Parameters[spline].PosInSpline, param))
			{
				return splineNode;
			}
		}
		return null;
	}

	private SplineNode[] GetAdjacentSegmentNodes(Spline spline, SplineNode node)
	{
		SplineNode[] segmentNodes = spline.SegmentNodes;
		SplineNode[] array = new SplineNode[2];
		int num = Array.IndexOf(segmentNodes, node);
		array[0] = ((num > 0) ? segmentNodes[num - 1] : null);
		array[1] = ((num < segmentNodes.Length - 1) ? segmentNodes[num + 1] : null);
		return array;
	}

	private bool ForwardOnSpline(BranchingSplinePath.Direction direction, float v)
	{
		if (direction == BranchingSplinePath.Direction.Forwards)
		{
			return v > 0f;
		}
		return v < 0f;
	}

	private bool IsMiddleNode(Spline spline, SplineNode node)
	{
		SplineNode[] splineNodes = spline.SplineNodes;
		int num = Array.IndexOf(splineNodes, node);
		if (num == 0)
		{
			return false;
		}
		if (num == splineNodes.Length - 1)
		{
			return false;
		}
		return true;
	}

	private IList<Spline> GetSplinesForNode(SplineNode node)
	{
		List<Spline> list = new List<Spline>();
		foreach (Spline spline in splines)
		{
			SplineNode[] splineNodes = spline.SplineNodes;
			foreach (SplineNode splineNode in splineNodes)
			{
				if (node == splineNode)
				{
					list.Add(spline);
				}
			}
		}
		return list;
	}

	private void CheckParameter(BranchingSplineParameter bParam)
	{
		if (SplinesAvailable)
		{
			if (bParam.spline == null)
			{
				bParam.spline = splines[0];
			}
			else if (!splines.Contains(bParam.spline))
			{
				bParam.spline = splines[0];
			}
			bParam.parameter = Mathf.Clamp01(bParam.parameter);
		}
	}
}
