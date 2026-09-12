#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class ScoreMultiplier
	{
		private const float ignoreWindowDuration = 0.25f;

		private const float increaseWindowDuration = 1.5f;

		private int multiplier;

		private float multiplierIgnoreWindow;

		private float multiplierIncreaseWindow;

		public void Reset()
		{
			multiplier = 0;
			multiplierIgnoreWindow = 0f;
			multiplierIncreaseWindow = 0f;
		}

		public int GetMultiplier()
		{
			return multiplier;
		}

		public int Maintain()
		{
			float fixedTime = Time.fixedTime;
			multiplierIncreaseWindow = fixedTime + 1.5f;
			return multiplier;
		}

		public int Increase()
		{
			float fixedTime = Time.fixedTime;
			if (multiplier == 0)
			{
				multiplier = 1;
				multiplierIgnoreWindow = fixedTime + 0.25f;
				multiplierIncreaseWindow = fixedTime + 1.5f;
				return multiplier;
			}
			if (fixedTime < multiplierIgnoreWindow)
			{
				multiplierIncreaseWindow = fixedTime + 1.5f;
				return multiplier;
			}
			if (fixedTime < multiplierIncreaseWindow)
			{
				multiplier++;
				if (multiplier > 10)
				{
					multiplier = 10;
				}
				multiplierIgnoreWindow = fixedTime + 0.25f;
				multiplierIncreaseWindow = fixedTime + 1.5f;
			}
			return multiplier;
		}

		public void FixedUpdate()
		{
			if (multiplier > 0 && Time.fixedTime > multiplierIncreaseWindow)
			{
				multiplier = 0;
				multiplierIgnoreWindow = 0f;
				multiplierIncreaseWindow = 0f;
			}
		}
	}
}
