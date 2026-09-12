#pragma warning disable 0618,0619
using UnityEngine;

public class ApproximatedCurve
{
	public Vector2[] keys;

	private float startX;

	private float endX;

	private float range;

	private float invRange;

	public ApproximatedCurve(Vector2[] newKeys)
	{
		keys = newKeys;
		startX = keys[0].x;
		endX = keys[keys.Length - 1].x;
		range = endX - startX;
		invRange = 1f / range;
	}

	public ApproximatedCurve(AnimationCurve curve, float xScale, float yScale, int samples = 256)
	{
		samples = Mathf.Clamp(samples, 1, 4096);
		keys = new Vector2[samples];
		startX = curve.keys[0].time * xScale;
		endX = curve.keys[curve.keys.Length - 1].time * xScale;
		range = endX - startX;
		float num = range / (float)samples;
		invRange = 1f / range;
		float num2 = startX;
		for (int i = 0; i < samples; i++)
		{
			keys[i].x = num2;
			keys[i].y = curve.Evaluate(num2 / range) * yScale;
			num2 += num;
		}
	}

	public float Evaluate(float x)
	{
		if (x <= startX)
		{
			return keys[0].y;
		}
		if (x >= endX)
		{
			return keys[keys.Length - 1].y;
		}
		float num = (x - startX) * invRange * (float)keys.Length;
		int num2 = Mathf.FloorToInt(num);
		int num3 = num2 + 1;
		if (num3 >= keys.Length)
		{
			return keys[num2].y;
		}
		return Mathf.Lerp(keys[num2].y, keys[num3].y, num - (float)num2);
	}
}
