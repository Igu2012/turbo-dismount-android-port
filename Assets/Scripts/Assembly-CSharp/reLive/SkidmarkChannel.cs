#pragma warning disable 0618,0619
using System;
using Dismount.Vehicular;
using UnityEngine;

namespace reLive
{
	public class SkidmarkChannel : RecorderChannel<SkidmarkChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public int slabCount;
		}

		private Skidmark sourceSkidmark;

		private Skidmark targetSkidmark;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			sourceSkidmark = sourceGameObject.GetComponent<Skidmark>();
			targetSkidmark = targetGameObject.GetComponent<Skidmark>();
		}

		protected override int GetFrameSize()
		{
			return 4;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.slabCount = sourceSkidmark.slabCount;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			targetSkidmark.RenderUpTo(frame.slabCount);
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			float slab = Mathf.Lerp(prevFrame.slabCount, nextFrame.slabCount, delta);
			targetSkidmark.RenderUpTo(slab);
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.slabCount == frame2.slabCount)
			{
				return true;
			}
			return false;
		}

		public override void PostProcessRecording()
		{
			base.PostProcessRecording();
			Array.Copy(sourceSkidmark.vertices, targetSkidmark.vertices, sourceSkidmark.vertices.Length);
			Array.Copy(sourceSkidmark.uvs, targetSkidmark.uvs, sourceSkidmark.uvs.Length);
			Array.Copy(sourceSkidmark.colors, targetSkidmark.colors, sourceSkidmark.colors.Length);
			Array.Copy(sourceSkidmark.triangles, targetSkidmark.triangles, sourceSkidmark.triangles.Length);
			targetSkidmark.slabCount = sourceSkidmark.slabCount;
			targetSkidmark.isReplayClone = true;
		}
	}
}
