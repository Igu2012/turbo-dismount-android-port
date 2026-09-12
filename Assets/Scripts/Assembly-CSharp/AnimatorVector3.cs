#pragma warning disable 0618,0619
using UnityEngine;

public class AnimatorVector3 : Animator<Vector3>
{
	public Vector3 current
	{
		get
		{
			if (CheckDuration())
			{
				return mCurrentValue;
			}
			float t = InterpolatePhase(base.phase);
			mCurrentValue = Vector3.Lerp(mStartValue, mTargetValue, t);
			return mCurrentValue;
		}
	}
}
