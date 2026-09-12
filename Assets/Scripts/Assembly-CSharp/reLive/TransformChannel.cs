#pragma warning disable 0618,0619
using System;
using Dismount;
using UnityEngine;

namespace reLive
{
	public class TransformChannel : RecorderChannel<TransformChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public bool active;

			public Vector3 position;

			public Quaternion rotation;

			public Vector3 lossyScale;
		}

		private const float SkipInterpolationThresholdPerSecond = 1000f;

		private Transform sourceTransform;

		private Transform targetTransform;

		private Camera camera;

		private AudioListener audioListener;

		private bool recordLossyScale;

		private bool doUpdateTransfrom = true;

		private float skipInterpolationThreshold;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			sourceTransform = sourceObject.transform;
			targetTransform = targetObject.transform;
			camera = targetGameObject.GetComponent<Camera>();
			audioListener = targetGameObject.GetComponent<AudioListener>();
			if ((bool)targetGameObject.GetComponent<RecordLossyScale>())
			{
				recordLossyScale = true;
			}
			if ((bool)targetGameObject.GetComponent<KeepInMainCamera>())
			{
				doUpdateTransfrom = false;
			}
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
			frame.position = sourceTransform.position;
			frame.rotation = sourceTransform.rotation;
			if (recordLossyScale)
			{
				frame.lossyScale = sourceTransform.lossyScale;
			}
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
			if (doUpdateTransfrom)
			{
				targetTransform.position = frame.position;
				targetTransform.rotation = frame.rotation;
				if (recordLossyScale)
				{
					targetTransform.localScale = frame.lossyScale;
				}
			}
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
			if (!doUpdateTransfrom)
			{
				return;
			}
			if ((prevFrame.position - nextFrame.position).sqrMagnitude > skipInterpolationThreshold)
			{
				flag2 = false;
			}
			if (flag2)
			{
				targetTransform.position = Vector3.Lerp(prevFrame.position, nextFrame.position, delta);
				targetTransform.rotation = Quaternion.Slerp(prevFrame.rotation, nextFrame.rotation, delta);
				if (recordLossyScale)
				{
					targetTransform.localScale = Vector3.Lerp(prevFrame.lossyScale, nextFrame.lossyScale, delta);
				}
			}
			else if (!flag)
			{
				targetGameObject.SetActive(false);
			}
			else
			{
				targetTransform.position = frame.position;
				targetTransform.rotation = frame.rotation;
				if (recordLossyScale)
				{
					targetTransform.localScale = frame.lossyScale;
				}
			}
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.active == frame2.active && frame1.position == frame2.position && frame1.rotation == frame2.rotation && frame1.lossyScale == frame2.lossyScale)
			{
				return true;
			}
			return false;
		}
	}
}
