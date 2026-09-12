#pragma warning disable 0618,0619
using UnityEngine;

public class AnimatorFloat : Animator<float>
{
	public float current
	{
		get
		{
			if (CheckDuration())
			{
				return mCurrentValue;
			}
			float t = InterpolatePhase(base.phase);
			mCurrentValue = Mathf.Lerp(mStartValue, mTargetValue, t);
			return mCurrentValue;
		}
	}
}
