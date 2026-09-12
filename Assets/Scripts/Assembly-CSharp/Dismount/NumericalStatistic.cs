#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class NumericalStatistic
	{
		private bool displayPeaks = true;

		private float currValue;

		private float currDelta;

		private float localPeakValue;

		private float localPeakTime = -10f;

		private float shownValue;

		private int valueRiseCount;

		private float peakMinLimit = 10f;

		public float displayValue
		{
			get
			{
				return shownValue;
			}
		}

		public float peakBlink
		{
			get
			{
				float time = Time.time;
				if (displayPeaks && time < localPeakTime + 1f)
				{
					return localPeakTime + 1f - time;
				}
				return 0f;
			}
		}

		public NumericalStatistic(bool displayPeaks, float peakMinLimit)
		{
			this.displayPeaks = displayPeaks;
			this.peakMinLimit = peakMinLimit;
		}

		public void Reset(float value)
		{
			currValue = (currDelta = (localPeakValue = (localPeakTime = (shownValue = value))));
		}

		public void Update(float value, bool forward)
		{
			float time = Time.time;
			float num = currValue;
			currValue = value;
			float num2 = currValue - num;
			if (!displayPeaks || !forward)
			{
				shownValue = currValue;
				valueRiseCount = 0;
				localPeakTime = 0f;
			}
			if (num2 > 0.001f)
			{
				valueRiseCount++;
			}
			if (time < localPeakTime + 1f && currValue < localPeakValue)
			{
				valueRiseCount = 0;
				return;
			}
			if (num2 < 0f)
			{
				if (num > peakMinLimit && valueRiseCount >= 25)
				{
					shownValue = (localPeakValue = num);
					localPeakTime = time;
					valueRiseCount = 0;
					return;
				}
				valueRiseCount = 0;
			}
			shownValue = currValue;
		}
	}
}
