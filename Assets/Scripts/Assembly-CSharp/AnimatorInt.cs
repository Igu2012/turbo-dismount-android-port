#pragma warning disable 0618,0619
public class AnimatorInt : Animator<int>
{
	public int current
	{
		get
		{
			if (CheckDuration())
			{
				return mCurrentValue;
			}
			float num = InterpolatePhase(base.phase);
			mCurrentValue = mStartValue + (int)((float)(mTargetValue - mStartValue) * num);
			return mCurrentValue;
		}
	}
}
