#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(UILabel))]
	public class HelpOverlayItem : MonoBehaviour
	{
		private const float completeFadeDuration = 0.5f;

		private UILabel label;

		private float sourceAlpha;

		private float targetAlpha;

		private float fadeEndTime = -1f;

		private float fadeDuration = 0.5f;

		private void Awake()
		{
			label = GetComponent<UILabel>();
			label.alpha = 0f;
			label.enabled = false;
		}

		private void Update()
		{
			float time = Time.time;
			if (time < fadeEndTime)
			{
				float t = 1f - (fadeEndTime - time) / fadeDuration;
				label.alpha = Mathf.Lerp(sourceAlpha, targetAlpha, t);
			}
			else if (fadeEndTime > 0f)
			{
				label.alpha = targetAlpha;
				if (targetAlpha == 0f)
				{
					label.enabled = false;
				}
				fadeEndTime = -1f;
			}
		}

		public void Show()
		{
			label.enabled = true;
			sourceAlpha = label.alpha;
			targetAlpha = 1f;
			fadeDuration = Mathf.Abs(targetAlpha - sourceAlpha) * 0.5f;
			fadeEndTime = Time.time + fadeDuration;
		}

		public void Hide()
		{
			sourceAlpha = label.alpha;
			targetAlpha = 0f;
			fadeDuration = Mathf.Abs(targetAlpha - sourceAlpha) * 0.5f;
			fadeEndTime = Time.time + fadeDuration;
		}

		public void HideNow()
		{
			label.alpha = 0f;
			label.enabled = false;
			fadeEndTime = -1f;
		}
	}
}
