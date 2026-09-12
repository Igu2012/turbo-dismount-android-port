#pragma warning disable 0618,0619
using System.Globalization;
using Dismount;
using UnityEngine;
using reLive;

public class ReplayControls : MonoBehaviour
{
	private struct UISize
	{
		public float uiUnit;

		public float buttonWidth;

		public float buttonHeight;

		public float labelWidth;

		public float labelHeight;

		public float sliderWidth;

		public float sliderHeight;
	}

	public GameObject ReplayCamera;

	private Replay replay;

	private float targetPlaybackSpeed = 1f;

	private float playbackSpeed = 1f;

	private UISize uiSize;

	private bool showGUI;

	private float rewoundWait = -1f;

	private void Awake()
	{
		replay = Replay.Instance;
	}

	private void Start()
	{
		uiSize.uiUnit = Mathf.Round(5f / 256f * (float)Screen.width);
		if (uiSize.uiUnit < 20f)
		{
			uiSize.uiUnit = 20f;
		}
		uiSize.buttonWidth = uiSize.uiUnit * 6f;
		uiSize.buttonHeight = uiSize.uiUnit * 2f;
		uiSize.labelHeight = uiSize.uiUnit;
		uiSize.sliderHeight = uiSize.uiUnit;
	}

	private void FixedUpdate()
	{
		if (rewoundWait < 0f && !showGUI && replay.Activity == Replay.ReplayActivity.Playback && replay.PlaybackTime < 1.5f)
		{
			ReplayPlaybackReachedBeginning();
			rewoundWait = 1f;
		}
		if (rewoundWait > 0f && !showGUI && replay.Activity == Replay.ReplayActivity.Playback && replay.PlaybackSpeed < 0.1f)
		{
			rewoundWait -= Time.fixedDeltaTime;
			if (rewoundWait < 0f)
			{
				targetPlaybackSpeed = 1f;
				showGUI = true;
				GameObject gameObject = Camera.main.gameObject;
				GameObject sourceObjectFor = replay.GetSourceObjectFor(gameObject);
				Transform target = sourceObjectFor.GetComponent<OrbitCamera>().target;
				gameObject.SetActive(false);
				replay.SetCameraOnPlayback = false;
				ReplayCamera.GetComponent<ReplayCamera>().target = replay.GetReplayCloneFor(target.gameObject).transform;
				ReplayCamera.SetActive(true);
			}
		}
		if (!showGUI)
		{
			playbackSpeed = playbackSpeed * 0.95f + targetPlaybackSpeed * 0.05f;
		}
		else
		{
			playbackSpeed = playbackSpeed * 0.9f + targetPlaybackSpeed * 0.1f;
		}
		if (Mathf.Abs(targetPlaybackSpeed - playbackSpeed) < 0.01f)
		{
			playbackSpeed = targetPlaybackSpeed;
		}
		replay.PlaybackSpeed = playbackSpeed;
	}

	public void RewindAndGo()
	{
		replay.PlaybackSpeed = 0f;
		playbackSpeed = 0f;
		targetPlaybackSpeed = -6f;
		replay.SeekPlayback(replay.Duration - 3f, true);
	}

	private void ReplayPlaybackReachedBeginning()
	{
		targetPlaybackSpeed = 0f;
	}

	private void OnGUI()
	{
		if (!showGUI)
		{
			return;
		}
		float uiUnit = uiSize.uiUnit;
		float num = uiSize.buttonWidth + uiUnit;
		float num2 = uiSize.buttonHeight + uiUnit;
		float num3 = (float)Screen.height - num2;
		if (replay.Activity == Replay.ReplayActivity.Idle)
		{
			if (!replay.HasRecording)
			{
				if (Button(uiUnit, num3, "Start Recording"))
				{
					replay.StartRecording();
				}
				return;
			}
			if (Button(uiUnit, num3, "Start Playback"))
			{
				replay.StartPlayback();
			}
			if (replay.HasRecording && Button(uiUnit, num3 - num2, "Restart Recording"))
			{
				replay.StartRecording();
			}
			Label(uiUnit + num, (float)Screen.height - uiSize.labelHeight - uiUnit, "Replay complete, " + replay.AllocatedMemory / 1024 + "kB");
		}
		else if (replay.Activity == Replay.ReplayActivity.Record)
		{
			if (!replay.IsRunning)
			{
				if (Button(uiUnit, num3, "Start Playback"))
				{
					replay.StartPlayback();
				}
				return;
			}
			if (Button(uiUnit, num3, "Stop Recording"))
			{
				replay.StopRecording();
			}
			float x = uiUnit + num;
			float num4 = (float)Screen.height - 3f * uiSize.labelHeight - uiUnit;
			Label(x, num4, "Time: " + NumberString(replay.Duration) + "s");
			num4 += uiUnit;
			Label(x, num4, "Frames: " + replay.FrameCount);
			num4 += uiUnit;
			Label(x, num4, "Memory: " + replay.AllocatedMemory / 1024 + "kB");
		}
		else
		{
			if (replay.Activity != Replay.ReplayActivity.Playback)
			{
				return;
			}
			if (!replay.IsRunning)
			{
				if (Button(uiUnit, num3, "Resume Playback"))
				{
					replay.ResumePlayback();
				}
			}
			else if (Button(uiUnit, num3, "Stop Playback"))
			{
				replay.StopPlayback();
			}
			if (Button(uiUnit, num3 - num2, "Restart Playback"))
			{
				replay.StartPlayback();
			}
			float x2 = uiUnit + num;
			float num5 = (float)Screen.height - 2f * uiSize.labelHeight - 2f * uiSize.sliderHeight - 2f * uiUnit;
			Label(x2, num5, "Time: " + NumberString(replay.PlaybackTime) + " / " + NumberString(replay.Duration) + "s, frame: " + replay.PlaybackFrame + " / " + (replay.FrameCount - 1));
			num5 += uiSize.labelHeight;
			float num6 = Slider(x2, num5, replay.PlaybackTime, 0f, replay.Duration);
			if (num6 != replay.PlaybackTime)
			{
				replay.SeekPlayback(num6, replay.IsRunning);
			}
			num5 += uiSize.sliderHeight + uiUnit;
			Label(x2, num5, "Playback speed: " + NumberString(replay.PlaybackSpeed) + "x");
			num5 += uiSize.labelHeight;
			float currValue = targetPlaybackSpeed;
			currValue = Slider(x2, num5, currValue, -2f, 2f);
			currValue *= 20f;
			currValue = Mathf.Round(currValue);
			currValue *= 0.05f;
			if (Mathf.Abs(currValue - targetPlaybackSpeed) >= 0.05f)
			{
				targetPlaybackSpeed = currValue;
			}
		}
	}

	private bool Button(float x, float y, string text)
	{
		return GUI.Button(new Rect(x, y, uiSize.buttonWidth, uiSize.buttonHeight), text);
	}

	private void Label(float x, float y, string text)
	{
		float width = (float)Screen.width - uiSize.uiUnit - x;
		GUI.Label(new Rect(x, y, width, uiSize.labelHeight), text);
	}

	private float Slider(float x, float y, float currValue, float minValue, float maxValue)
	{
		float width = (float)Screen.width - uiSize.buttonWidth - uiSize.uiUnit - x;
		return GUI.HorizontalSlider(new Rect(x, y, width, uiSize.sliderHeight), currValue, minValue, maxValue);
	}

	private string NumberString(float number, int decimals = 2)
	{
		NumberFormatInfo numberFormat = new CultureInfo("en-US", false).NumberFormat;
		numberFormat.NumberDecimalDigits = decimals;
		return number.ToString("N", numberFormat);
	}
}
