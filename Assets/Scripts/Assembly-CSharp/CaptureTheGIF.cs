#pragma warning disable 0618,0619
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Dismount;
using Gif.Components;
using UnityEngine;

public class CaptureTheGIF : MonoBehaviour
{
	private static CaptureTheGIF instance;

	public static bool running;

	private static Camera[] gifCameras;

	public static CaptureTheGIF Instance
	{
		get
		{
			if (!instance)
			{
				GameObject gameObject = new GameObject("CaptureTheGIF");
				gameObject.AddComponent<CaptureTheGIF>();
				Object.DontDestroyOnLoad(gameObject);
			}
			return instance;
		}
	}

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("there is already an instance of CaptureTheGIF");
			Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	public static FileStream MakeFile(string dirName, string name)
	{
		if (!Directory.Exists(dirName))
		{
			Directory.CreateDirectory(dirName);
		}
		string text = Path.Combine(dirName, name + ".gif");
		int num = 0;
		while (File.Exists(text))
		{
			text = Path.Combine(dirName, name + num + ".gif");
			num++;
		}
		Debug.Log("Creating file: " + text);
		return File.Create(text);
	}

	public static RenderTexture Snapshot(int width, int height)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 24);
		Camera[] array = gifCameras;
		foreach (Camera camera in array)
		{
			RenderTexture targetTexture = camera.targetTexture;
			camera.targetTexture = temporary;
			camera.Render();
			camera.targetTexture = targetTexture;
			camera.ResetAspect();
		}
		return temporary;
	}

	public Coroutine Capture(int frames, int width, int height, float frameRate, string dir, string name, bool lockFramerate = false)
	{
		return StartCoroutine(CaptureRoutine(frames, width, height, frameRate, dir, name, lockFramerate));
	}

	private IEnumerator CaptureRoutine(int frames, int width, int height, float frameRate, string dir, string name, bool lockFramerate)
	{
		running = true;
		gifCameras = DismountGame.instance.GetGifCameras();
		Debug.Log("Capturing frames");
		MemoryStream memStream = new MemoryStream();
		IEnumerator capRoutine = CaptureRoutine(frames, width, height, frameRate, memStream, false, lockFramerate);
		while (capRoutine.MoveNext())
		{
			yield return capRoutine.Current;
		}
		using (FileStream file = MakeFile(dir, name))
		{
			Thread thread = new Thread(() =>
			{
				memStream.WriteTo(file);
				file.Flush();
				file.Close();
			});
			thread.Start();
			while (thread.ThreadState == ThreadState.Running)
			{
				yield return null;
			}
			running = false;
			Debug.Log("Done");
			DismountGame.instance.OnGIFRecordDone();
		}
	}

	public Coroutine Capture(int frames, int width, int height, float frameRate, Stream stream, bool closeWhenDone, bool lockFramerate = false)
	{
		return StartCoroutine(CaptureRoutine(frames, width, height, frameRate, stream, closeWhenDone, lockFramerate));
	}

	private IEnumerator CaptureRoutine(int frames, int width, int height, float frameRate, Stream stream, bool closeWhenDone, bool lockFramerate)
	{
		List<RenderTexture> textures = new List<RenderTexture>();
		float t = Time.time;
		float waitUntil = t;
		if (lockFramerate)
		{
			Time.captureFramerate = (int)frameRate;
		}
		for (int f = 0; f < frames; f++)
		{
			if (!lockFramerate)
			{
				while (waitUntil > Time.time)
				{
					yield return null;
				}
			}
			yield return new WaitForEndOfFrame();
			waitUntil += 1f / frameRate;
			textures.Add(Snapshot(width, height));
		}
		Time.captureFramerate = 0;
		float period = 1f / frameRate;
		yield return MakeGif(textures, period, stream, closeWhenDone);
	}

	public Coroutine MakeGif(List<RenderTexture> textures, float frameLength, Stream output, bool closeWhenDone)
	{
		return StartCoroutine(MakeGifRoutine(textures, frameLength, output, closeWhenDone));
	}

	private static IEnumerator MakeGifRoutine(List<RenderTexture> textures, float frameLength, Stream output, bool closeWhenDone)
	{
		Debug.Log("Encoding gif");
		DismountGame.instance.OnGIFRecordEncoding();
		AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();
		gifEncoder.SetQuality(10);
		gifEncoder.SetRepeat(0);
		gifEncoder.SetDelay((int)(frameLength * 1000f));
		gifEncoder.Start(output);
		int w = textures[0].width;
		int h = textures[0].height;
		Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
		ManualResetEvent imageStart = new ManualResetEvent(false);
		Image image = null;
		bool done = false;
		bool processed = false;
		Thread worker = new Thread(() =>
		{
			while (!done)
			{
				imageStart.WaitOne();
				imageStart.Reset();
				gifEncoder.AddFrame(image);
				processed = true;
			}
		});
		worker.Priority = System.Threading.ThreadPriority.Highest;
		worker.Start();
		for (int picCount = 0; picCount < textures.Count; picCount++)
		{
			RenderTexture tempTex = (RenderTexture.active = textures[picCount]);
			tex.ReadPixels(new Rect(0f, 0f, w, h), 0, 0);
			RenderTexture.ReleaseTemporary(tempTex);
			image = new Image(tex);
			processed = false;
			imageStart.Set();
			while (!processed)
			{
				yield return null;
			}
		}
		done = true;
		textures.Clear();
		gifEncoder.Finish();
		Object.DestroyImmediate(tex);
		output.Flush();
		if (closeWhenDone)
		{
			output.Close();
		}
	}
}
