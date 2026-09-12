#pragma warning disable 0618,0619
using System;
using Dismount;
using UnityEngine;

namespace reLive
{
	public class NPCWheelRotationChannel : RecorderChannel<NPCWheelRotationChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public float angle;
		}

		private NPCWheelRotation sourceRotation;

		private NPCWheelRotation targetRotation;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			sourceRotation = sourceGameObject.GetComponent<NPCWheelRotation>();
			targetRotation = targetGameObject.GetComponent<NPCWheelRotation>();
		}

		protected override int GetFrameSize()
		{
			return 4;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.angle = sourceRotation.angle;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			targetRotation.angle = frame.angle;
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			targetRotation.angle = Mathf.Lerp(prevFrame.angle, nextFrame.angle, delta);
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			return frame1.angle == frame2.angle;
		}
	}
}
