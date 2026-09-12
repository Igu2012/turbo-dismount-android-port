#pragma warning disable 0618,0619
using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SplinePathCamera : MonoBehaviour
{
	public enum LookAt
	{
		CameraTransformation = 0,
		LookAtTarget = 1,
		LookAtPath = 2,
		LookAtPathKeepUpVector = 3
	}

	public enum FOV
	{
		CameraOriginal = 0,
		CameraPathValue = 1,
		TargetSize = 2
	}

	public Spline CameraPath;

	public bool UseControlTarget;

	public Transform ControlTarget;

	public Spline ControlTargetTrack;

	[Range(0.1f, 60f)]
	public float AnimationDuration = 10f;

	public bool LoopAnimation;

	public LookAt LookAtMode;

	public Transform LookAtTarget;

	public Spline LookAtPath;

	public FOV FOVMode;

	public float TargetSize = 10f;

	private float mStartTime;

	private void Start()
	{
		if (!CameraPath)
		{
			Debug.LogError("SplinePathCamera: CameraPath missing");
		}
		else
		{
			CameraPath.updateMode = Spline.UpdateMode.DontUpdate;
		}
		if (UseControlTarget)
		{
			if (!ControlTargetTrack)
			{
				Debug.LogError("SplinePathCamera: ControlTargetTrack missing");
			}
			else
			{
				ControlTargetTrack.updateMode = Spline.UpdateMode.DontUpdate;
			}
		}
		if (LookAtMode == LookAt.LookAtPath && !LookAtPath)
		{
			Debug.LogError("SplinePathCamera: LookAtPath missing");
		}
		else if ((bool)LookAtPath)
		{
			LookAtPath.updateMode = Spline.UpdateMode.DontUpdate;
		}
	}

	private void OnEnable()
	{
		if (!UseControlTarget)
		{
			mStartTime = Time.fixedTime;
		}
	}

	private void FixedUpdate()
	{
		if ((!UseControlTarget || (bool)ControlTarget) && (LookAtMode != LookAt.LookAtTarget || (bool)LookAtTarget))
		{
			float num = 0f;
			if (!UseControlTarget)
			{
				num = (Time.fixedTime - mStartTime) / AnimationDuration;
				num = ((!LoopAnimation) ? Mathf.Clamp01(num) : (num - Mathf.Floor(num)));
			}
			else
			{
				num = Mathf.Clamp(ControlTargetTrack.GetClosestPointParam(ControlTarget.position, 5, 0f, 1f), 0f, 1f);
			}
			base.transform.position = CameraPath.GetPositionOnSplineFast(num);
			if (LookAtMode == LookAt.CameraTransformation)
			{
				base.transform.rotation = CameraPath.GetOrientationOnSplineFast(num);
			}
			else if (LookAtMode == LookAt.LookAtPath)
			{
				base.transform.LookAt(LookAtPath.GetPositionOnSplineFast(num));
			}
			else if (LookAtMode == LookAt.LookAtPathKeepUpVector)
			{
				base.transform.rotation = CameraPath.GetOrientationOnSplineFast(num);
				base.transform.LookAt(LookAtPath.GetPositionOnSplineFast(num), base.transform.up);
			}
			else
			{
				base.transform.LookAt(LookAtTarget.position);
			}
			if (FOVMode == FOV.CameraPathValue)
			{
				base.GetComponent<Camera>().fieldOfView = Mathf.Clamp(CameraPath.GetCustomValueOnSplineFast(num), 2f, 178f);
			}
			else if (FOVMode == FOV.TargetSize)
			{
				float magnitude = (base.transform.position - LookAtTarget.position).magnitude;
				float num2 = Mathf.Atan(TargetSize * 0.5f / magnitude);
				base.GetComponent<Camera>().fieldOfView = Mathf.Clamp(num2 * 2f * 180f / (float)Math.PI, 2f, 178f);
			}
		}
	}
}
