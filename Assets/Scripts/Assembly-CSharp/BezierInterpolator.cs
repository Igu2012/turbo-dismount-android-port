#pragma warning disable 0618,0619
public class BezierInterpolator : SplineInterpolator
{
	public BezierInterpolator()
	{
		base.CoefficientMatrix = new double[16]
		{
			-1.0, 3.0, -3.0, 1.0, 3.0, -6.0, 3.0, 0.0, -3.0, 3.0,
			0.0, 0.0, 1.0, 0.0, 0.0, 0.0
		};
		base.NodeIndices = new int[4] { 0, 1, 2, 3 };
	}
}
