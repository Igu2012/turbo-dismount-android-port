#pragma warning disable 0618,0619
using System;
using UnityEngine;

public class MathUtils
{
	private struct IncrementalTrig
	{
		public float cos;

		public float sin;

		private float cosd;

		private float sind;

		private float cosn;

		private float sinn;

		public void Init(int divs)
		{
			float f = (float)Math.PI * 2f / (float)divs;
			sin = 0f;
			cos = 1f;
			sind = Mathf.Sin(f);
			cosd = Mathf.Cos(f);
		}

		public void Init(float startAngle, int divs)
		{
			float delta = (float)Math.PI * 2f / (float)divs;
			Init(startAngle, delta);
		}

		public void Init(float startAngle, float delta)
		{
			sin = Mathf.Sin(startAngle);
			cos = Mathf.Cos(startAngle);
			sind = Mathf.Sin(delta);
			cosd = Mathf.Cos(delta);
		}

		public void Inc()
		{
			sinn = sin * cosd + cos * sind;
			cosn = cos * cosd - sin * sind;
			sin = sinn;
			cos = cosn;
		}
	}

	private static float Sin(float t)
	{
		return Mathf.Sin(t);
	}

	private static float Saw(float t)
	{
		return -1f + 2f * (Mathf.Repeat(t, (float)Math.PI) / (float)Math.PI);
	}

	private static float Triangle(float t)
	{
		return -1f + 2f * Mathf.Abs(Saw(t));
	}

	private static float Square(float t)
	{
		return Mathf.Sign(Saw(t));
	}

	private static Vector3 CubicBezierInterpolate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		Vector3 vector = Vector3.Lerp(p0, p1, t);
		Vector3 vector2 = Vector3.Lerp(p1, p2, t);
		Vector3 to = Vector3.Lerp(p2, p3, t);
		Vector3 vector3 = Vector3.Lerp(vector, vector2, t);
		Vector3 to2 = Vector3.Lerp(vector2, to, t);
		return Vector3.Lerp(vector3, to2, t);
	}

	private static Vector3 CatmullRomInterpolate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t);
	}

	private static float SmoothMin(float a, float b, float k)
	{
		float num = Mathf.Clamp01(0.5f + 0.5f * (b - a) / k);
		return Mathf.Lerp(b, a, num) - k * num * (1f - num);
	}

	private static float AvoidZero(float x, float m, float n)
	{
		if (x > m)
		{
			return x;
		}
		float num = 2f * n - m;
		float num2 = 2f * m - 3f * n;
		float num3 = x / m;
		return (num * num3 + num2) * num3 * num3 + n;
	}

	private static float Impulse(float k, float x)
	{
		float num = k * x;
		return num * Mathf.Exp(1f - num);
	}

	private static float CubicPulse(float c, float w, float x)
	{
		x = Mathf.Abs(x - c);
		if (x > w)
		{
			return 0f;
		}
		x /= w;
		return 1f - x * x * (3f - 2f * x);
	}

	private static float ExpStep(float x, float k, float n)
	{
		return Mathf.Exp((0f - k) * Mathf.Pow(x, n));
	}

	private static float Parabola(float x, float k)
	{
		return Mathf.Pow(4f * x * (1f - x), k);
	}

	private static float PCurve(float x, float a, float b)
	{
		float num = Mathf.Pow(a + b, a + b) / (Mathf.Pow(a, a) * Mathf.Pow(b, b));
		return num * Mathf.Pow(x, a) * Mathf.Pow(1f - x, b);
	}
}
