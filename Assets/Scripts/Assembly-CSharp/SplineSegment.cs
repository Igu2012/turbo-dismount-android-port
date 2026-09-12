#pragma warning disable 0618,0619
using System;
using UnityEngine;

public class SplineSegment
{
	private readonly Spline parentSpline;

	private readonly SplineNode startNode;

	private readonly SplineNode endNode;

	public Spline ParentSpline
	{
		get
		{
			return parentSpline;
		}
	}

	public SplineNode StartNode
	{
		get
		{
			return startNode;
		}
	}

	public SplineNode EndNode
	{
		get
		{
			return endNode;
		}
	}

	public float Length
	{
		get
		{
			return startNode.Parameters[parentSpline].Length * parentSpline.Length;
		}
	}

	public float NormalizedLength
	{
		get
		{
			return startNode.Parameters[parentSpline].Length;
		}
	}

	public SplineSegment(Spline pSpline, SplineNode sNode, SplineNode eNode)
	{
		if (pSpline != null)
		{
			parentSpline = pSpline;
			startNode = sNode;
			endNode = eNode;
			return;
		}
		throw new ArgumentNullException("Parent Spline must not be null");
	}

	public float ConvertSegmentToSplineParamter(float param)
	{
		return startNode.Parameters[parentSpline].PosInSpline + param * startNode.Parameters[parentSpline].Length;
	}

	public float ConvertSplineToSegmentParamter(float param)
	{
		if (param < startNode.Parameters[parentSpline].PosInSpline)
		{
			return 0f;
		}
		if (param > startNode.Parameters[parentSpline].PosInSpline + startNode.Parameters[parentSpline].Length)
		{
			return 1f;
		}
		return (param - startNode.Parameters[parentSpline].PosInSpline) / startNode.Parameters[parentSpline].Length;
	}

	public float ClampParameterToSegment(float param)
	{
		if (param < startNode.Parameters[parentSpline].PosInSpline)
		{
			return startNode.Parameters[parentSpline].PosInSpline;
		}
		if (param > startNode.Parameters[parentSpline].PosInSpline + startNode.Parameters[parentSpline].Length)
		{
			return startNode.Parameters[parentSpline].PosInSpline + startNode.Parameters[parentSpline].Length;
		}
		return param;
	}

	public bool IsParameterInRange(float param)
	{
		if (Mathf.Approximately(param, startNode.Parameters[parentSpline].PosInSpline))
		{
			return true;
		}
		if (Mathf.Approximately(param, startNode.Parameters[parentSpline].PosInSpline + startNode.Parameters[parentSpline].Length))
		{
			return true;
		}
		if (param < startNode.Parameters[parentSpline].PosInSpline)
		{
			return false;
		}
		if (param > startNode.Parameters[parentSpline].PosInSpline + startNode.Parameters[parentSpline].Length)
		{
			return false;
		}
		return true;
	}
}
