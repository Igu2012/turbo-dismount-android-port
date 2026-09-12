#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace reLive
{
	public class CameraChannel : RecorderChannel<CameraChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public bool enabled;

			public float fov;
		}

		private Camera sourceCamera;

		private Camera targetCamera;

		private AudioListener targetAudioListener;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			sourceCamera = sourceGameObject.GetComponent<Camera>();
			targetCamera = targetGameObject.GetComponent<Camera>();
			targetAudioListener = targetGameObject.GetComponent<AudioListener>();
		}

		protected override int GetFrameSize()
		{
			return 5;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.enabled = sourceCamera.enabled;
			frame.fov = sourceCamera.fieldOfView;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			if (replay.SetCameraOnPlayback && targetCamera.enabled != frame.enabled)
			{
				targetCamera.enabled = frame.enabled;
				if ((bool)targetAudioListener)
				{
					targetAudioListener.enabled = frame.enabled;
				}
			}
			if (frame.enabled)
			{
				targetCamera.fieldOfView = frame.fov;
			}
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			Frame frame = ((!(playbackSpeed < 0f)) ? prevFrame : nextFrame);
			bool enabled = frame.enabled;
			if (replay.SetCameraOnPlayback && targetCamera.enabled != enabled)
			{
				targetCamera.enabled = enabled;
				if ((bool)targetAudioListener)
				{
					targetAudioListener.enabled = enabled;
				}
			}
			if (enabled)
			{
				targetCamera.fieldOfView = Mathf.Lerp(prevFrame.fov, nextFrame.fov, delta);
			}
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.enabled == frame2.enabled && Mathf.Approximately(frame1.fov, frame2.fov))
			{
				return true;
			}
			return false;
		}
	}
}
