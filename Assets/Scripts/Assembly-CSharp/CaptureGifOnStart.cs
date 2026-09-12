#pragma warning disable 0618,0619
using UnityEngine;

public class CaptureGifOnStart : MonoBehaviour
{
	public string dir = "gifs";

	public new string name = "gif";

	public int frames = 30;

	public float frameRate = 10f;

	public int height = 640;

	public float aspect = 1.3333334f;

	private void Start()
	{
		CaptureTheGIF.Instance.Capture(frames, (int)(aspect * (float)height), height, frameRate, dir, name);
	}
}
