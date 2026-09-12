#pragma warning disable 0618,0619
using System;
using Dismount;
using UnityEngine;

namespace reLive
{
	public class AudioSourceChannel : RecorderChannel<AudioSourceChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public bool enabled;

			public AudioClip clip;

			public bool isPlaying;

			public float volume;

			public float pitch;

			public float time;
		}

		private AudioSource sourceAudioSource;

		private AudioSource targetAudioSource;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			sourceAudioSource = sourceGameObject.GetComponent<AudioSource>();
			targetAudioSource = targetGameObject.GetComponent<AudioSource>();
			targetAudioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
		}

		protected override int GetFrameSize()
		{
			return 22;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.clip = sourceAudioSource.clip;
			frame.enabled = sourceAudioSource.enabled;
			frame.isPlaying = sourceAudioSource.isPlaying;
			frame.volume = sourceAudioSource.volume;
			frame.pitch = sourceAudioSource.pitch;
			frame.time = sourceAudioSource.time;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			if (targetAudioSource.enabled != frame.enabled)
			{
				targetAudioSource.enabled = frame.enabled;
			}
			if (!frame.enabled)
			{
				return;
			}
			bool flag = false;
			if (recorder.DidJustActivate && targetAudioSource.isPlaying)
			{
				flag = true;
			}
			if (targetAudioSource.clip != frame.clip)
			{
				targetAudioSource.clip = frame.clip;
				if (targetAudioSource.clip == null)
				{
					targetAudioSource.Stop();
					return;
				}
				flag = true;
			}
			bool flag2 = flag || (frame.isPlaying && !targetAudioSource.isPlaying);
			bool flag3 = !frame.isPlaying && targetAudioSource.isPlaying;
			if (flag2)
			{
				targetAudioSource.time = frame.time;
				if (!DismountGame.instance.paused)
				{
					targetAudioSource.Play();
				}
			}
			else if (flag3)
			{
				targetAudioSource.Stop();
			}
			targetAudioSource.volume = frame.volume;
			targetAudioSource.pitch = frame.pitch * playbackSpeed;
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			Frame frame = ((!(playbackSpeed < 0f)) ? prevFrame : nextFrame);
			bool enabled = frame.enabled;
			if (targetAudioSource.enabled != enabled)
			{
				targetAudioSource.enabled = enabled;
			}
			if (!enabled)
			{
				return;
			}
			bool flag = false;
			if (recorder.DidJustActivate && targetAudioSource.isPlaying)
			{
				flag = true;
			}
			if (targetAudioSource.clip != frame.clip)
			{
				targetAudioSource.clip = frame.clip;
				if (targetAudioSource.clip == null)
				{
					targetAudioSource.Stop();
					return;
				}
				flag = true;
			}
			bool isPlaying = frame.isPlaying;
			bool flag2 = flag || (prevFrame.isPlaying && nextFrame.isPlaying && !targetAudioSource.isPlaying);
			bool flag3 = !isPlaying && targetAudioSource.isPlaying;
			bool flag4 = prevFrame.isPlaying && nextFrame.isPlaying;
			if (frame.clip != targetAudioSource.clip)
			{
				flag4 = false;
			}
			if (flag2)
			{
				float recordInterval = replay.RecordInterval;
				float time = 0f;
				bool flag5 = false;
				if (playbackSpeed < 0f)
				{
					if (nextFrame.time > targetAudioSource.clip.length - recordInterval)
					{
						time = targetAudioSource.clip.length - recordInterval;
					}
					else if (nextFrame.time < recordInterval && !targetAudioSource.loop)
					{
						flag5 = true;
					}
					else
					{
						time = Mathf.Lerp(prevFrame.time, nextFrame.time, delta);
					}
				}
				else if (prevFrame.time < recordInterval)
				{
					time = 0f;
				}
				else if (prevFrame.time > targetAudioSource.clip.length - recordInterval && !targetAudioSource.loop)
				{
					flag5 = true;
				}
				else
				{
					time = Mathf.Lerp(prevFrame.time, nextFrame.time, delta);
				}
				targetAudioSource.time = time;
				if (!targetAudioSource.isPlaying && !flag5 && !DismountGame.instance.paused)
				{
					targetAudioSource.Play();
				}
			}
			else if (flag3)
			{
				targetAudioSource.Stop();
			}
			if (flag4)
			{
				targetAudioSource.volume = Mathf.Lerp(prevFrame.volume, nextFrame.volume, delta);
				targetAudioSource.pitch = Mathf.Lerp(prevFrame.pitch, nextFrame.pitch, delta) * playbackSpeed;
			}
			else
			{
				targetAudioSource.volume = frame.volume;
				targetAudioSource.pitch = frame.pitch * playbackSpeed;
			}
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.enabled == frame2.enabled && frame1.clip == frame2.clip && frame1.isPlaying == frame2.isPlaying && Mathf.Approximately(frame1.time, frame2.time) && Mathf.Approximately(frame1.pitch, frame2.pitch) && Mathf.Approximately(frame1.volume, frame2.volume))
			{
				return true;
			}
			return false;
		}
	}
}
