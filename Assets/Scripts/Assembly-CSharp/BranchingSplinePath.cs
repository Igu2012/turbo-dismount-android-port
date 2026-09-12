#pragma warning disable 0618,0619
using System;

[Serializable]
public class BranchingSplinePath
{
	public enum Direction
	{
		Forwards = 0,
		Backwards = 1
	}

	public Spline spline;

	public Direction direction;

	protected BranchingSplinePath()
	{
		spline = null;
		direction = Direction.Forwards;
	}

	public BranchingSplinePath(Spline spline)
	{
		this.spline = spline;
		direction = Direction.Forwards;
	}

	public BranchingSplinePath(Spline spline, Direction direction)
	{
		this.spline = spline;
		this.direction = direction;
	}
}
