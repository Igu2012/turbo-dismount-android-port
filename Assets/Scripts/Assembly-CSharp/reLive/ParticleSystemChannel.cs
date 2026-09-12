#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace reLive
{
	public class ParticleSystemChannel : RecorderChannel<ParticleSystemChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public bool isPlaying;

			public bool isStopped;

			public bool isPaused;

			public float playbackSpeed;

			public bool enableEmission;

			public float emissionRate;
		}

		private ParticleSystem sourceParticleSystem;

		private ParticleSystem targetParticleSystem;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			sourceParticleSystem = sourceGameObject.GetComponent<ParticleSystem>();
			targetParticleSystem = targetGameObject.GetComponent<ParticleSystem>();
		}

		protected override int GetFrameSize()
		{
			return 12;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.isPlaying = sourceParticleSystem.isPlaying;
			frame.isStopped = sourceParticleSystem.isStopped;
			frame.isPaused = sourceParticleSystem.isPaused;
			frame.playbackSpeed = sourceParticleSystem.playbackSpeed;
			frame.enableEmission = sourceParticleSystem.enableEmission;
			frame.emissionRate = sourceParticleSystem.emissionRate;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			if (frame.isPlaying && !targetParticleSystem.isPlaying)
			{
				targetParticleSystem.Play();
			}
			else if (frame.isStopped && !targetParticleSystem.isStopped)
			{
				targetParticleSystem.Stop();
			}
			else if (frame.isPaused && !targetParticleSystem.isPaused)
			{
				targetParticleSystem.Pause();
			}
			targetParticleSystem.enableEmission = frame.enableEmission;
			targetParticleSystem.playbackSpeed = frame.playbackSpeed * playbackSpeed;
			targetParticleSystem.emissionRate = frame.emissionRate;
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			Frame frame = ((!(playbackSpeed < 0f)) ? prevFrame : nextFrame);
			if (frame.isPlaying && !targetParticleSystem.isPlaying)
			{
				targetParticleSystem.Play();
			}
			else if (frame.isStopped && !targetParticleSystem.isStopped)
			{
				targetParticleSystem.Stop();
			}
			else if (frame.isPaused && !targetParticleSystem.isPaused)
			{
				targetParticleSystem.Pause();
			}
			bool flag = prevFrame.isPlaying && nextFrame.isPlaying;
			targetParticleSystem.enableEmission = frame.enableEmission;
			if (flag)
			{
				targetParticleSystem.playbackSpeed = Mathf.Lerp(prevFrame.playbackSpeed, nextFrame.playbackSpeed, delta) * playbackSpeed;
				targetParticleSystem.emissionRate = Mathf.Lerp(prevFrame.emissionRate, nextFrame.emissionRate, delta);
			}
			else
			{
				targetParticleSystem.playbackSpeed = frame.playbackSpeed * playbackSpeed;
				targetParticleSystem.emissionRate = frame.emissionRate;
			}
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.isPlaying == frame2.isPlaying && frame1.isStopped == frame2.isStopped && frame1.isPaused == frame2.isPaused && frame1.enableEmission == frame2.enableEmission && Mathf.Approximately(frame1.playbackSpeed, frame2.playbackSpeed) && Mathf.Approximately(frame1.emissionRate, frame2.emissionRate))
			{
				return true;
			}
			return false;
		}
	}
}
