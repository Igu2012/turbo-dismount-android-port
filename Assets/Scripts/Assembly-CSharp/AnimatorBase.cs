#pragma warning disable 0618,0619
public abstract class AnimatorBase : IAnimator
{
	public enum Interpolator
	{
		Linear = 0,
		SmoothStep = 1,
		EaseIn = 2,
		EaseOut = 3,
		EaseInOut = 4
	}

	protected float mCurrentTime;

	protected float mStartTime;

	protected float mDuration;

	protected bool mGotTarget;

	protected Interpolator mInterpolator;

	public Interpolator interpolator
	{
		get
		{
			return mInterpolator;
		}
		set
		{
			mInterpolator = value;
		}
	}

	public float duration
	{
		get
		{
			return mDuration;
		}
		set
		{
			mDuration = value;
		}
	}

	public bool isAnimating
	{
		get
		{
			return mCurrentTime >= mStartTime && mCurrentTime < mStartTime + mDuration;
		}
	}

	public bool gotTarget
	{
		get
		{
			return mGotTarget;
		}
	}

	public float phase
	{
		get
		{
			if (mCurrentTime < mStartTime)
			{
				return 0f;
			}
			if (mCurrentTime >= mStartTime + mDuration)
			{
				return 1f;
			}
			return (mCurrentTime - mStartTime) / mDuration;
		}
	}

	public void Skip()
	{
		mStartTime = mCurrentTime - mDuration;
	}

	public void Update(float time)
	{
		mCurrentTime = time;
		if (!(mCurrentTime < mStartTime))
		{
		}
	}

	protected float InterpolatePhase(float p)
	{
		float num = p;
		switch (mInterpolator)
		{
		case Interpolator.SmoothStep:
			num = num * num * (3f - 2f * num);
			break;
		case Interpolator.EaseIn:
			num = num * num * num;
			break;
		case Interpolator.EaseOut:
		{
			float num3 = 1f - num;
			num = 1f - num3 * num3 * num3;
			break;
		}
		case Interpolator.EaseInOut:
		{
			float num2 = num * 2f;
			num = ((!(num2 < 1f)) ? (0.5f * (2f - (2f - num2) * (2f - num2) * (2f - num2))) : (0.5f * (num2 * num2 * num2)));
			break;
		}
		}
		return num;
	}
}
