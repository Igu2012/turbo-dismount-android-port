#pragma warning disable 0618,0619
public class BSplineInterpolator : SplineInterpolator
{
	public BSplineInterpolator()
	{
		base.CoefficientMatrix = new double[16]
		{
			-1.0 / 6.0,
			0.5,
			-0.5,
			1.0 / 6.0,
			0.5,
			-1.0,
			0.0,
			2.0 / 3.0,
			-0.5,
			0.5,
			0.5,
			1.0 / 6.0,
			1.0 / 6.0,
			0.0,
			0.0,
			0.0
		};
		base.NodeIndices = new int[4] { -1, 0, 1, 2 };
	}
}
