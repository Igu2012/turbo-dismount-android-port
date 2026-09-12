#pragma warning disable 0618,0619
using System;
using Dismount;
using UnityEngine;

namespace reLive
{
	public class DismountDataChannel : RecorderChannel<DismountDataChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public int displayScore;

			public int displayMultiplier;

			public float characterAltitude;

			public float characterAirTime;

			public float vehicleSpeed;

			public float vehicleAltitude;

			public float vehicleAirTime;

			public float headPain;
		}

		private Statistics.RealtimeStatistics realtimeData;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			realtimeData = DismountGame.playerState.statistics.realtime;
		}

		protected override int GetFrameSize()
		{
			return 32;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.displayScore = realtimeData.displayScore;
			frame.displayMultiplier = realtimeData.displayMultiplier;
			frame.characterAltitude = realtimeData.characterAltitude;
			frame.characterAirTime = realtimeData.characterAirTime;
			frame.vehicleSpeed = realtimeData.vehicleSpeed;
			frame.vehicleAltitude = realtimeData.vehicleAltitude;
			frame.vehicleAirTime = realtimeData.vehicleAirTime;
			frame.headPain = realtimeData.headPain;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			realtimeData.displayScore = frame.displayScore;
			realtimeData.displayMultiplier = frame.displayMultiplier;
			realtimeData.characterAltitude = frame.characterAltitude;
			realtimeData.characterAirTime = frame.characterAirTime;
			realtimeData.vehicleSpeed = frame.vehicleSpeed;
			realtimeData.vehicleAltitude = frame.vehicleAltitude;
			realtimeData.vehicleAirTime = frame.vehicleAirTime;
			realtimeData.headPain = frame.headPain;
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			realtimeData.displayScore = (int)Mathf.Lerp(prevFrame.displayScore, nextFrame.displayScore, delta);
			realtimeData.displayMultiplier = (int)Mathf.Lerp(prevFrame.displayMultiplier, nextFrame.displayMultiplier, delta);
			realtimeData.characterAltitude = Mathf.Lerp(prevFrame.characterAltitude, nextFrame.characterAltitude, delta);
			realtimeData.characterAirTime = Mathf.Lerp(prevFrame.characterAirTime, nextFrame.characterAirTime, delta);
			realtimeData.vehicleSpeed = Mathf.Lerp(prevFrame.vehicleSpeed, nextFrame.vehicleSpeed, delta);
			realtimeData.vehicleAltitude = Mathf.Lerp(prevFrame.vehicleAltitude, nextFrame.vehicleAltitude, delta);
			realtimeData.vehicleAirTime = Mathf.Lerp(prevFrame.vehicleAirTime, nextFrame.vehicleAirTime, delta);
			realtimeData.headPain = Mathf.Lerp(prevFrame.headPain, nextFrame.headPain, delta);
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.displayScore == frame2.displayScore && frame1.displayMultiplier == frame2.displayMultiplier && Mathf.Approximately(frame1.characterAltitude, frame2.characterAltitude) && Mathf.Approximately(frame1.characterAirTime, frame2.characterAirTime) && Mathf.Approximately(frame1.vehicleSpeed, frame2.vehicleSpeed) && Mathf.Approximately(frame1.vehicleAltitude, frame2.vehicleAltitude) && Mathf.Approximately(frame1.vehicleAirTime, frame2.vehicleAirTime) && Mathf.Approximately(frame1.headPain, frame2.headPain))
			{
				return true;
			}
			return false;
		}
	}
}
