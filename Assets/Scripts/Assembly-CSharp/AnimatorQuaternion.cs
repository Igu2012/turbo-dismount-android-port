#pragma warning disable 0618,0619
using UnityEngine;

public class AnimatorQuaternion : Animator<Quaternion>
{
	public Quaternion current
	{
		get
		{
			if (CheckDuration())
			{
				return mCurrentValue;
			}
			float t = InterpolatePhase(base.phase);
			mCurrentValue = Quaternion.Slerp(mStartValue, mTargetValue, t);
			return mCurrentValue;
		}
	}
}
