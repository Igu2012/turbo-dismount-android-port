#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace reLive
{
	public class LocalTransfomChannel : RecorderChannel<LocalTransfomChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public bool active;

			public Vector3 localPosition;

			public Quaternion localRotation;

			public Vector3 localScale;
		}

		private const float SkipInterpolationThresholdPerSecond = 1000f;

		private Transform sourceTransform;

		private Transform targetTransform;

		private Camera camera;

		private AudioListener audioListener;

		private float skipInterpolationThreshold;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			sourceTransform = sourceObject.transform;
			targetTransform = targetObject.transform;
			camera = targetGameObject.GetComponent<Camera>();
			audioListener = targetGameObject.GetComponent<AudioListener>();
			skipInterpolationThreshold = 1000f * replay.RecordInterval;
			skipInterpolationThreshold *= skipInterpolationThreshold;
		}

		protected override int GetFrameSize()
		{
			return 41;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.active = sourceGameObject.activeInHierarchy;
			frame.localPosition = sourceTransform.localPosition;
			frame.localRotation = sourceTransform.localRotation;
			frame.localScale = sourceTransform.localScale;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			if (targetGameObject.activeInHierarchy != frame.active)
			{
				if ((bool)camera && replay.SetCameraOnPlayback)
				{
					targetGameObject.SetActive(frame.active);
					camera.enabled = false;
					if ((bool)audioListener)
					{
						audioListener.enabled = false;
					}
				}
				else if (!camera)
				{
					targetGameObject.SetActive(frame.active);
				}
			}
			targetTransform.localPosition = frame.localPosition;
			targetTransform.localRotation = frame.localRotation;
			targetTransform.localScale = frame.localScale;
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			Frame frame = ((!(playbackSpeed < 0f)) ? prevFrame : nextFrame);
			bool active = frame.active;
			bool flag = ((!(playbackSpeed < 0f)) ? nextFrame.active : prevFrame.active);
			bool flag2 = prevFrame.active && nextFrame.active;
			if (targetGameObject.activeInHierarchy != active)
			{
				if ((bool)camera && replay.SetCameraOnPlayback)
				{
					targetGameObject.SetActive(active);
					camera.enabled = false;
					if ((bool)audioListener)
					{
						audioListener.enabled = false;
					}
				}
				else if (!camera)
				{
					targetGameObject.SetActive(active);
				}
			}
			if ((prevFrame.localPosition - nextFrame.localPosition).sqrMagnitude > skipInterpolationThreshold)
			{
				flag2 = false;
			}
			if (flag2)
			{
				targetTransform.localPosition = Vector3.Lerp(prevFrame.localPosition, nextFrame.localPosition, delta);
				targetTransform.localRotation = Quaternion.Slerp(prevFrame.localRotation, nextFrame.localRotation, delta);
				targetTransform.localScale = Vector3.Lerp(prevFrame.localScale, nextFrame.localScale, delta);
			}
			else if (!flag)
			{
				targetGameObject.SetActive(false);
			}
			else
			{
				targetTransform.localPosition = frame.localPosition;
				targetTransform.localRotation = frame.localRotation;
				targetTransform.localScale = frame.localScale;
			}
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.active == frame2.active && frame1.localPosition == frame2.localPosition && frame1.localRotation == frame2.localRotation && frame1.localScale == frame2.localScale)
			{
				return true;
			}
			return false;
		}
	}
}
