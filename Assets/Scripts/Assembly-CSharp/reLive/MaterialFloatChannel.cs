#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace reLive
{
	public class MaterialFloatChannel : RecorderChannel<MaterialFloatChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public float value;
		}

		private Material sourceMaterial;

		private Material targetMaterial;

		public int MaterialIndex;

		public string PropertyName = "_Intensity";

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
			return 4;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.value = sourceMaterial.GetFloat(propertyId);
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			targetMaterial.SetFloat(propertyId, frame.value);
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			targetMaterial.SetFloat(propertyId, Mathf.Lerp(prevFrame.value, nextFrame.value, delta));
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.value == frame2.value)
			{
				return true;
			}
			return false;
		}
	}
}
