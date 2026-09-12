#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class ScoreDisplay : MonoBehaviour
	{
		private float dismountStartTime = -1f;

		public DigitalDisplay digitalDisplay;

		private char[] emptyChars = new char[10] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };

		private char[] gogoChars = new char[10] { 'G', '0', ' ', ' ', 'G', '0', ' ', ' ', 'G', '0' };

		private char[] scrollChars = new char[10];

		public void OnDismountStarted()
		{
			dismountStartTime = Time.fixedTime;
		}

		public void OnDismountReset()
		{
		}

		private void FixedUpdate()
		{
			if (dismountStartTime < 0f)
			{
				return;
			}
			float num = Time.fixedTime - dismountStartTime;
			char[] chars;
			if (num < 1f)
			{
				chars = ((!(Mathf.Repeat(num, 0.5f) < 0.25f)) ? emptyChars : gogoChars);
			}
			else if (num < 2f)
			{
				chars = gogoChars;
			}
			else if (!(num < 2.5f))
			{
				chars = ((!(num < 3f)) ? DismountGame.playerState.statistics.realtime.displayScore.ToString("0").ToCharArray() : emptyChars);
			}
			else
			{
				int num2 = (int)((num - 2f) / 0.5f * 10f);
				for (int i = 0; i < 10; i++)
				{
					int num3 = i + num2;
					if (num3 < 0 || num3 >= 10)
					{
						scrollChars[i] = ' ';
					}
					else
					{
						scrollChars[i] = gogoChars[num3];
					}
				}
				chars = scrollChars;
			}
			digitalDisplay.Print(chars);
		}
	}
}
