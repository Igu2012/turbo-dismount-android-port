#pragma warning disable 0618,0619
using System;

[Serializable]
public class BranchingSplineParameter : BranchingSplinePath
{
	public float parameter;

	public bool Forward
	{
		get
		{
			return direction == Direction.Forwards;
		}
	}

	public BranchingSplineParameter()
	{
		spline = null;
		parameter = 0f;
		direction = Direction.Forwards;
	}

	public BranchingSplineParameter(Spline spline, float parameter)
	{
		base.spline = spline;
		this.parameter = parameter;
		direction = Direction.Forwards;
	}

	public BranchingSplineParameter(Spline spline, float parameter, bool forward)
	{
		base.spline = spline;
		this.parameter = parameter;
		direction = ((!forward) ? Direction.Backwards : Direction.Forwards);
	}
}
