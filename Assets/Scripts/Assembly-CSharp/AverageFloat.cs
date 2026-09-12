#pragma warning disable 0618,0619
public struct AverageFloat
{
	private float[] frames;

	private int writeHead;

	private float sum;

	public AverageFloat(int frameCount = 10)
	{
		writeHead = 0;
		sum = 0f;
		frames = new float[frameCount];
		for (int i = 0; i < frameCount; i++)
		{
			frames[i] = 0f;
		}
	}

	public float Add(float value)
	{
		sum -= frames[writeHead];
		frames[writeHead] = value;
		sum += value;
		writeHead = (writeHead + 1) % frames.Length;
		return sum / (float)frames.Length;
	}

	public float Get()
	{
		return sum / (float)frames.Length;
	}
}
