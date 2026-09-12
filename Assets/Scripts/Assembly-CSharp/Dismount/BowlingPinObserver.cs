#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class BowlingPinObserver : MonoBehaviour
	{
		private int pinCount;

		private int pinsToppled;

		private void Awake()
		{
			BowlingPin[] componentsInChildren = base.transform.GetComponentsInChildren<BowlingPin>();
			BowlingPin[] array = componentsInChildren;
			foreach (BowlingPin bowlingPin in array)
			{
				bowlingPin.SetObserver(this);
				pinCount++;
			}
		}

		public void PinToppled()
		{
			if (++pinsToppled == pinCount)
			{
				DismountGame.playerState.statistics.BowlingStrike();
			}
		}
	}
}
