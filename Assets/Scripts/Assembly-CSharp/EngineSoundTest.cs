#pragma warning disable 0618,0619
using UnityEngine;

public class EngineSoundTest : MonoBehaviour
{
	private const float minRpm = 1000f;

	public AudioSource engine;

	public float originalRpm = 3000f;

	public float maxRpm = 8000f;

	public FilterParams volumeParams;

	public FilterParams distortionParams;

	public FilterParams lowPassParams;

	public FilterParams highPassParams;

	[Range(0f, 1f)]
	public float throttle;

	public float load;

	public float rpm = 1000f;

	private AudioDistortionFilter distortion;

	private AudioLowPassFilter lowPass;

	private AudioHighPassFilter highPass;

	private void Awake()
	{
		distortion = engine.GetComponent<AudioDistortionFilter>();
		lowPass = engine.GetComponent<AudioLowPassFilter>();
		highPass = engine.GetComponent<AudioHighPassFilter>();
	}

	private void FixedUpdate()
	{
		float num = 0.25f + throttle * 0.5f;
		float num2 = Mathf.Lerp(500f, maxRpm, throttle);
		float num3 = (maxRpm - 1000f) * 0.25f;
		num += (num2 - rpm) / num3;
		num = Mathf.Clamp01(num);
		load = Mathf.Lerp(load, num, 0.1f);
		float num4 = Mathf.Clamp01(Mathf.Abs(num2 - rpm) * 1E-05f);
		if (num2 < rpm)
		{
			num4 *= 0.33f;
		}
		rpm = Mathf.Lerp(rpm, num2, num4);
		rpm = Mathf.Clamp(rpm, 1000f, maxRpm);
		engine.volume = volumeParams.constant + load * volumeParams.load + rpm / maxRpm * volumeParams.rpm;
		engine.pitch = rpm / originalRpm;
		distortion.distortionLevel = Mathf.Clamp(distortionParams.constant + load * distortionParams.load + rpm / maxRpm * distortionParams.rpm, 0f, 0.7f);
		lowPass.cutoffFrequency = lowPassParams.constant + load * lowPassParams.load + rpm / maxRpm * lowPassParams.rpm;
		highPass.cutoffFrequency = Mathf.Clamp(highPassParams.constant + load * highPassParams.load + rpm / maxRpm * highPassParams.rpm, 10f, 1000f);
		if (!engine.isPlaying)
		{
			engine.Play();
		}
	}
}
