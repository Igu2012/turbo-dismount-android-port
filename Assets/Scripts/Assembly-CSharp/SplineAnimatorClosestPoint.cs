#pragma warning disable 0618,0619
using UnityEngine;

[AddComponentMenu("SuperSplines/Animation/Closest Point Animator")]
public class SplineAnimatorClosestPoint : MonoBehaviour
{
	public Spline spline;

	public WrapMode wMode = WrapMode.Once;

	public Transform target;

	public int iterations = 5;

	public float diff = 0.5f;

	public float offset;

	private void Update()
	{
		if (!(target == null) && !(spline == null))
		{
			float param = WrapValue(spline.GetClosestPointParam(target.position, iterations, 0f, 1f) + offset, 0f, 1f, wMode);
			base.transform.position = spline.GetPositionOnSpline(param);
			base.transform.rotation = spline.GetOrientationOnSpline(param);
		}
	}

	private float WrapValue(float v, float start, float end, WrapMode wMode)
	{
		switch (wMode)
		{
		case WrapMode.Once:
		case WrapMode.ClampForever:
			return Mathf.Clamp(v, start, end);
		case WrapMode.Default:
		case WrapMode.Loop:
			return Mathf.Repeat(v, end - start) + start;
		case WrapMode.PingPong:
			return Mathf.PingPong(v, end - start) + start;
		default:
			return v;
		}
	}
}
