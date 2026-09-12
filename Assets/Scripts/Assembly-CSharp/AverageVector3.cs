#pragma warning disable 0618,0619
using UnityEngine;

public struct AverageVector3
{
	private Vector3[] frames;

	private int writeHead;

	private Vector3 sum;

	public AverageVector3(int frameCount = 10)
	{
		writeHead = 0;
		sum = Vector3.zero;
		frames = new Vector3[frameCount];
		for (int i = 0; i < frameCount; i++)
		{
			frames[i] = Vector3.zero;
		}
	}

	public Vector3 Add(Vector3 value)
	{
		sum -= frames[writeHead];
		frames[writeHead] = value;
		sum += value;
		writeHead = (writeHead + 1) % frames.Length;
		return sum / frames.Length;
	}

	public Vector3 Get()
	{
		return sum / frames.Length;
	}
}
