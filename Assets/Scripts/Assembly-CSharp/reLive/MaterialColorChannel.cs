#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace reLive
{
	public class MaterialColorChannel : RecorderChannel<MaterialColorChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public Color color;
		}

		private Material sourceMaterial;

		private Material targetMaterial;

		public int MaterialIndex;

		public string PropertyName = "_Color";

		private int propertyId;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			sourceMaterial = sourceGameObject.GetComponent<Renderer>().materials[MaterialIndex];
			targetMaterial = targetGameObject.GetComponent<Renderer>().materials[MaterialIndex];
			propertyId = Shader.PropertyToID(PropertyName);
		}

		protected override int GetFrameSize()
		{
			return 16;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.color = sourceMaterial.GetColor(propertyId);
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			targetMaterial.SetColor(propertyId, frame.color);
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			targetMaterial.SetColor(propertyId, Color.Lerp(prevFrame.color, nextFrame.color, delta));
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.color == frame2.color)
			{
				return true;
			}
			return false;
		}
	}
}
