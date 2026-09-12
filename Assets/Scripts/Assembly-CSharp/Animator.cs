#pragma warning disable 0618,0619
public class Animator<T> : AnimatorBase
{
	protected T mStartValue;

	protected T mTargetValue;

	protected T mCurrentValue;

	public T start
	{
		get
		{
			return mStartValue;
		}
	}

	public T target
	{
		get
		{
			return mTargetValue;
		}
		set
		{
			mStartValue = mCurrentValue;
			mTargetValue = value;
			mStartTime = mCurrentTime;
			mGotTarget = false;
		}
	}

	public void Reset(T newValue)
	{
		mStartValue = (mCurrentValue = (mTargetValue = newValue));
		mGotTarget = false;
	}

	protected bool CheckDuration()
	{
		if (mCurrentTime < mStartTime)
		{
			mCurrentValue = mStartValue;
			return true;
		}
		if (mCurrentTime >= mStartTime + mDuration)
		{
			mCurrentValue = mTargetValue;
			mGotTarget = true;
			return true;
		}
		return false;
	}
}
