#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace reLive
{
	public class ExtendedLocalEulersChannel : RecorderChannel<ExtendedLocalEulersChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public Vector3 localEulers;
		}

		private ExtendedLocalEulers sourceEulers;

		private Transform targetTransform;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			sourceEulers = sourceGameObject.GetComponent<ExtendedLocalEulers>();
			targetTransform = targetGameObject.transform;
		}

		protected override int GetFrameSize()
		{
			return 12;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.localEulers = sourceEulers.localEulers;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			targetTransform.localEulerAngles = frame.localEulers;
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			targetTransform.localEulerAngles = Vector3.Lerp(prevFrame.localEulers, nextFrame.localEulers, delta);
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			return frame1.localEulers == frame2.localEulers;
		}
	}
}
