#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace reLive
{
	public class UISpriteChannel : RecorderChannel<UISpriteChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public float alpha;

			public string spriteName;
		}

		private UISprite sourceUISprite;

		private UISprite targetUISprite;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			sourceUISprite = sourceGameObject.GetComponent<UISprite>();
			targetUISprite = targetGameObject.GetComponent<UISprite>();
		}

		protected override int GetFrameSize()
		{
			return 20;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.alpha = sourceUISprite.alpha;
			frame.spriteName = sourceUISprite.spriteName;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			targetUISprite.alpha = frame.alpha;
			targetUISprite.spriteName = frame.spriteName;
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			Frame frame = ((!(playbackSpeed < 0f)) ? prevFrame : nextFrame);
			targetUISprite.alpha = Mathf.Lerp(prevFrame.alpha, nextFrame.alpha, delta);
			targetUISprite.spriteName = frame.spriteName;
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			return Mathf.Approximately(frame1.alpha, frame2.alpha);
		}
	}
}
