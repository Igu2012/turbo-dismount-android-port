#pragma warning disable 0618,0619
using System;
using Dismount.Vehicular;
using UnityEngine;

namespace reLive
{
	public class WheelTelemetryChannel : RecorderChannel<WheelTelemetryChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public bool hit;

			public float force;

			public Vector3 forwardDir = Vector3.forward;

			public float forwardSlip;

			public Vector3 normal = Vector3.up;

			public Vector3 point = Vector3.zero;

			public Vector3 sidewaysDir = Vector3.right;

			public float sidewaysSlip;
		}

		private WheelTelemetry source;

		private WheelTelemetry target;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			source = sourceObject.GetComponent<WheelTelemetry>();
			target = targetObject.GetComponent<WheelTelemetry>();
		}

		protected override int GetFrameSize()
		{
			return 64;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.hit = source.hit;
			frame.force = source.force;
			frame.forwardDir = source.forwardDir;
			frame.forwardSlip = source.forwardSlip;
			frame.normal = source.normal;
			frame.point = source.point;
			frame.sidewaysDir = source.sidewaysDir;
			frame.sidewaysSlip = source.sidewaysSlip;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			target.hit = frame.hit;
			target.force = frame.force;
			target.forwardDir = frame.forwardDir;
			target.forwardSlip = frame.forwardSlip;
			target.normal = frame.normal;
			target.point = frame.point;
			target.sidewaysDir = frame.sidewaysDir;
			target.sidewaysSlip = frame.sidewaysSlip;
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			target.hit = prevFrame.hit & nextFrame.hit;
			target.force = Mathf.Lerp(prevFrame.force, nextFrame.force, delta);
			target.forwardDir = Vector3.Slerp(prevFrame.forwardDir, nextFrame.forwardDir, delta);
			target.forwardSlip = Mathf.Lerp(prevFrame.forwardSlip, nextFrame.forwardSlip, delta);
			target.normal = Vector3.Slerp(prevFrame.normal, nextFrame.normal, delta);
			target.point = Vector3.Lerp(prevFrame.point, nextFrame.point, delta);
			target.sidewaysDir = Vector3.Slerp(prevFrame.sidewaysDir, nextFrame.sidewaysDir, delta);
			target.sidewaysSlip = Mathf.Lerp(prevFrame.sidewaysSlip, nextFrame.sidewaysSlip, delta);
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			return false;
		}
	}
}
