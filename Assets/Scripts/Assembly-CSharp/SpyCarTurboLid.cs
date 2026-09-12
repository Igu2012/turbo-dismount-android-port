#pragma warning disable 0618,0619
using UnityEngine;

public class SpyCarTurboLid : MonoBehaviour
{
	public float closedAngle;

	public float openAngle = 105f;

	public float duration = 0.25f;

	private Transform xf;

	private float currAngle;

	private float sourceAngle;

	private float targetAngle;

	private float sourceTime = -1f;

	private float targetTime = -1f;

	private void Awake()
	{
		xf = base.transform;
	}

	private void TurboBoostStarted()
	{
		sourceAngle = currAngle;
		targetAngle = openAngle;
		sourceTime = Time.fixedTime;
		targetTime = sourceTime + (openAngle - sourceAngle) / (openAngle - closedAngle) * duration;
	}

	private void TurboBoostEnded()
	{
		sourceAngle = currAngle;
		targetAngle = closedAngle;
		sourceTime = Time.fixedTime;
		targetTime = sourceTime + (sourceAngle - closedAngle) / (openAngle - closedAngle) * duration;
	}

	private void TurboLidBroken()
	{
		sourceTime = -1f;
		targetTime = -1f;
	}

	private void UpdateAngle()
	{
		Vector3 localEulerAngles = xf.localEulerAngles;
		localEulerAngles.x = currAngle;
		xf.localEulerAngles = localEulerAngles;
	}

	private void FixedUpdate()
	{
		if (!(targetTime < 0f))
		{
			float fixedTime = Time.fixedTime;
			if (fixedTime > targetTime)
			{
				currAngle = targetAngle;
				UpdateAngle();
				sourceTime = -1f;
				targetTime = -1f;
			}
			else
			{
				currAngle = Mathf.Lerp(sourceAngle, targetAngle, Mathf.InverseLerp(sourceTime, targetTime, fixedTime));
				UpdateAngle();
			}
		}
	}
}
