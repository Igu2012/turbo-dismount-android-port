#pragma warning disable 0618,0619
using System;
using UnityEngine;

public class Ploop : MonoBehaviour
{
	public AudioClip clip;

	private float[] clipData;

	private int clipSamples;

	private int count;

	private float sin(float t)
	{
		return Mathf.Sin(t);
	}

	private float saw(float t)
	{
		return -1f + 2f * (Mathf.Repeat(t, (float)Math.PI) / (float)Math.PI);
	}

	private float triangle(float t)
	{
		return -1f + 2f * Mathf.Abs(saw(t));
	}

	private float square(float t)
	{
		return Mathf.Sign(saw(t));
	}

	private void Awake()
	{
		clipSamples = clip.samples;
		clipData = new float[clip.channels * clipSamples];
		clip.GetData(clipData, 0);
	}

	private void FixedUpdate()
	{
		float fixedTime = Time.fixedTime;
		Vector3 position = new Vector3(sin(fixedTime), sin(fixedTime * 2f), 0f) * 5f;
		base.transform.position = position;
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		for (int i = 0; i < data.Length; i += 2)
		{
			count++;
			count %= clipSamples;
			data[i] *= clipData[count];
			data[i + 1] *= clipData[count];
		}
	}
}
