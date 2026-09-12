#pragma warning disable 0618,0619
using System;
using System.Linq;
using UnityEngine;

public class CaptureGifOnKey : MonoBehaviour
{
	public enum ButtonType
	{
		KeyCode = 0,
		InputManager = 1,
		Mobile = 2
	}

	public string buttonName = "p";

	public ButtonType buttonType;

	public string dir = "gifs";

	public new string name = "gif";

	public int frames = 30;

	public float frameRate = 10f;

	public int height = 480;

	public float aspect = 1.3333334f;

	public bool lockFramerate;

	public bool restrictDuplicateCaptures = true;

	private void Update()
	{
		bool flag = false;
		switch (buttonType)
		{
		case ButtonType.Mobile:
			flag = Input.touches.Count() > 0;
			break;
		case ButtonType.KeyCode:
		{
			KeyCode key = (KeyCode)(int)Enum.Parse(typeof(KeyCode), buttonName, true);
			flag = Input.GetKeyDown(key);
			break;
		}
		case ButtonType.InputManager:
			flag = Input.GetButtonDown(buttonName);
			break;
		default:
			throw new NotImplementedException("unknown button type: " + buttonType);
		}
		if (flag && (!restrictDuplicateCaptures || !CaptureTheGIF.running))
		{
			CaptureTheGIF.Instance.Capture(frames, (int)(aspect * (float)height), height, frameRate, dir, name, lockFramerate);
		}
	}
}
