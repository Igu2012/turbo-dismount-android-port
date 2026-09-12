#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class FPSCounter : MonoBehaviour
	{
		public int averagedFrames = 256;

		public bool showGUI;

		private int numAveragedFrames = 10;

		private float[] frameTimes;

		private int frameCount;

		private float cumulatedFrameTime;

		private float currFrameTime;

		public float currentFrameTime
		{
			get
			{
				return currFrameTime;
			}
		}

		public float averageFrameTime
		{
			get
			{
				if (frameCount < 1)
				{
					return -1f;
				}
				int num = numAveragedFrames;
				if (frameCount < numAveragedFrames)
				{
					num = frameCount;
				}
				return cumulatedFrameTime / (float)num;
			}
		}

		public float currentFPS
		{
			get
			{
				return 1f / currFrameTime;
			}
		}

		public float averageFPS
		{
			get
			{
				return 1f / averageFrameTime;
			}
		}

		private void Start()
		{
			numAveragedFrames = averagedFrames;
			frameTimes = new float[numAveragedFrames];
			for (int i = 0; i < numAveragedFrames; i++)
			{
				frameTimes[i] = 0f;
			}
		}
	}
}
