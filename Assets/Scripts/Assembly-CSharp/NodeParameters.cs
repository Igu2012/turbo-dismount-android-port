#pragma warning disable 0618,0619
public class NodeParameters
{
	public double position;

	public double length;

	public Spline spline;

	public float PosInSpline
	{
		get
		{
			return (float)position;
		}
	}

	public float Length
	{
		get
		{
			return (float)length;
		}
	}

	public NodeParameters(Spline spline, float position, float length)
	{
		this.position = position;
		this.length = length;
		this.spline = spline;
	}

	public void Reset()
	{
		position = 0.0;
		length = 0.0;
	}
}
