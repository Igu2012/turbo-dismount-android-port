#pragma warning disable 0618,0619
public class LinearInterpolator : SplineInterpolator
{
	public LinearInterpolator()
	{
		base.CoefficientMatrix = new double[16]
		{
			0.0, 0.0, -1.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0,
			0.0, 0.0, 0.0, 0.0, 0.0, 0.0
		};
		base.NodeIndices = new int[4] { 0, 1, 2, 3 };
	}
}
