#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace reLive
{
	public class MaterialTexCoordsChannel : RecorderChannel<MaterialTexCoordsChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public Vector2 scale;

			public Vector2 offset;
		}

		private const float SkipInterpolationThresholdPerSecond = 50f;

		private Material sourceMaterial;

		private Material targetMaterial;

		private bool interpolate;

		private float skipInterpolationThreshold;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			RecordMaterialTexCoords component = targetObject.GetComponent<RecordMaterialTexCoords>();
			int materialIndex = component.MaterialIndex;
			sourceMaterial = sourceGameObject.GetComponent<Renderer>().materials[materialIndex];
			targetMaterial = targetGameObject.GetComponent<Renderer>().materials[materialIndex];
			interpolate = component.Interpolate;
			skipInterpolationThreshold = 50f * replay.RecordInterval;
			skipInterpolationThreshold *= skipInterpolationThreshold;
		}

		protected override int GetFrameSize()
		{
			return 16;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.scale = sourceMaterial.mainTextureScale;
			frame.offset = sourceMaterial.mainTextureOffset;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			targetMaterial.mainTextureScale = frame.scale;
			targetMaterial.mainTextureOffset = frame.offset;
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			bool flag = interpolate;
			if ((flag && (prevFrame.offset - nextFrame.offset).sqrMagnitude > skipInterpolationThreshold) || (prevFrame.scale - nextFrame.scale).sqrMagnitude > skipInterpolationThreshold)
			{
				flag = false;
			}
			if (flag)
			{
				targetMaterial.mainTextureScale = Vector2.Lerp(prevFrame.scale, nextFrame.scale, delta);
				targetMaterial.mainTextureOffset = Vector2.Lerp(prevFrame.offset, nextFrame.offset, delta);
			}
			else if (playbackSpeed < 0f)
			{
				targetMaterial.mainTextureScale = nextFrame.scale;
				targetMaterial.mainTextureOffset = nextFrame.offset;
			}
			else
			{
				targetMaterial.mainTextureScale = prevFrame.scale;
				targetMaterial.mainTextureOffset = prevFrame.offset;
			}
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.scale == frame2.scale && frame1.offset == frame2.offset)
			{
				return true;
			}
			return false;
		}
	}
}
