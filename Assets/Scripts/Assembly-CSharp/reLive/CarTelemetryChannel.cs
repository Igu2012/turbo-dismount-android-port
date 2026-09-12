#pragma warning disable 0618,0619
using System;
using Dismount.Vehicular;
using UnityEngine;

namespace reLive
{
	public class CarTelemetryChannel : RecorderChannel<CarTelemetryChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public float steer;

			public float throttle;

			public float brake;

			public float clutch;

			public int gear;

			public float rpm;

			public float outputTorque;

			public float frictionTorque;
		}

		private CarTelemetry source;

		private CarTelemetry target;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			source = sourceObject.GetComponent<CarTelemetry>();
			target = targetObject.GetComponent<CarTelemetry>();
		}

		protected override int GetFrameSize()
		{
			return 24;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.steer = source.steer;
			frame.throttle = source.throttle;
			frame.brake = source.brake;
			frame.clutch = source.clutch;
			frame.gear = source.gear;
			frame.rpm = source.rpm;
			frame.outputTorque = source.outputTorque;
			frame.frictionTorque = source.frictionTorque;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			target.steer = frame.steer;
			target.throttle = frame.throttle;
			target.brake = frame.brake;
			target.clutch = frame.clutch;
			target.gear = frame.gear;
			target.rpm = frame.rpm;
			target.outputTorque = frame.outputTorque;
			target.frictionTorque = frame.frictionTorque;
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			Frame frame = ((!(playbackSpeed < 0f)) ? prevFrame : nextFrame);
			target.steer = Mathf.Lerp(prevFrame.steer, nextFrame.steer, delta);
			target.throttle = Mathf.Lerp(prevFrame.throttle, nextFrame.throttle, delta);
			target.brake = Mathf.Lerp(prevFrame.brake, nextFrame.brake, delta);
			target.clutch = Mathf.Lerp(prevFrame.clutch, nextFrame.clutch, delta);
			target.gear = frame.gear;
			target.rpm = Mathf.Lerp(prevFrame.rpm, nextFrame.rpm, delta);
			target.outputTorque = Mathf.Lerp(prevFrame.outputTorque, nextFrame.outputTorque, delta);
			target.frictionTorque = Mathf.Lerp(prevFrame.frictionTorque, nextFrame.frictionTorque, delta);
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			return false;
		}
	}
}
