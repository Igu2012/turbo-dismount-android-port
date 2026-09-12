#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	public class EngineSoundCrossfade : MonoBehaviour
	{
		public enum ClampMode
		{
			None = 0,
			SampleRect = 1,
			SampleEdges = 2
		}

		[HideInInspector]
		public EngineSample[] samples;

		public float minRpm = 1000f;

		public float maxRpm = 8000f;

		public float muteStartRpm = 800f;

		public float muteEndRpm = 1000f;

		[Range(0f, 1f)]
		public float minThrottleVolume = 0.8f;

		[Range(0f, 1f)]
		public float minRpmVolume = 1f;

		public float globalMaxDistance = -1f;

		public ClampMode clamping = ClampMode.SampleRect;

		public bool exaggerateLoad;

		[HideInInspector]
		public Vector2 sampleRectMin = new Vector2(0f, 0f);

		[HideInInspector]
		public Vector2 sampleRectMax = new Vector2(1f, 1f);

		public EngineSample[] edgeSamples;

		[HideInInspector]
		public Vector2 rpmLoad = Vector2.zero;

		[HideInInspector]
		public float volumeMul = 1f;

		private int numMixedSamples;

		private void Awake()
		{
			samples = base.transform.GetComponentsInChildren<EngineSample>(true);
		}

		private void Start()
		{
			CalculateSampleRect();
			CalculateSampleEdges();
		}

		public void Play()
		{
			if (samples != null && samples.Length >= 1)
			{
				for (int i = 0; i < samples.Length; i++)
				{
					EngineSample engineSample = samples[i];
					engineSample.audioSource.Play();
				}
			}
		}

		public void Stop()
		{
			if (samples != null && samples.Length >= 1)
			{
				for (int i = 0; i < samples.Length; i++)
				{
					EngineSample engineSample = samples[i];
					engineSample.audioSource.Stop();
				}
			}
		}

		public void UpdateSound(float throttle, float load, float rpm)
		{
			if (samples == null || samples.Length < 1)
			{
				return;
			}
			if (exaggerateLoad)
			{
				load = load * load * (3f - 2f * load);
			}
			float num = Mathf.Lerp(minThrottleVolume, 1f, throttle);
			float num2 = Mathf.InverseLerp(minRpm, maxRpm, rpm);
			float num3 = Mathf.Lerp(minRpmVolume, 1f, num2);
			float num4 = 1f;
			if (rpm < muteEndRpm)
			{
				num4 = Mathf.InverseLerp(muteStartRpm, muteEndRpm, rpm);
			}
			volumeMul = num * num3 * num4;
			rpmLoad = new Vector2(num2, load);
			if (clamping == ClampMode.SampleRect)
			{
				ClampToSampleRect();
			}
			else if (clamping == ClampMode.SampleEdges)
			{
				ClampToSampleEdges();
			}
			float num5 = 0f;
			numMixedSamples = 0;
			for (int i = 0; i < samples.Length; i++)
			{
				EngineSample engineSample = samples[i];
				if (engineSample.isActiveAndEnabled)
				{
					float sqrMagnitude = (rpmLoad - engineSample.rpmLoad).sqrMagnitude;
					float num6 = Mathf.Clamp(engineSample.distanceBias + engineSample.attenuation * sqrMagnitude, 0.001f, 10f);
					engineSample.mix = 1f / num6;
					if (globalMaxDistance > 0f)
					{
						float num7 = globalMaxDistance * globalMaxDistance;
						float num8 = Mathf.InverseLerp(num7, 0f, Mathf.Clamp(sqrMagnitude, 0f, num7));
						float num9 = 1f - num8;
						num8 = 1f - num9 * num9;
						engineSample.mix *= num8;
					}
					if (engineSample.mix > 0.1f)
					{
						num5 += engineSample.mix;
						numMixedSamples++;
					}
					else
					{
						engineSample.mix = 0f;
					}
				}
			}
			if (numMixedSamples == 0)
			{
				return;
			}
			float num10 = 1f / num5;
			for (int j = 0; j < samples.Length; j++)
			{
				EngineSample engineSample2 = samples[j];
				if (engineSample2.isActiveAndEnabled)
				{
					float num11 = engineSample2.mix * num10;
					engineSample2.mix = 1f - (1f - num11) * (1f - num11);
					engineSample2.audioSource.volume = engineSample2.mix * volumeMul * engineSample2.originalVolume;
					engineSample2.audioSource.pitch = rpm / engineSample2.referenceRpm * engineSample2.originalPitch;
				}
			}
		}

		public void CalculateSampleRect()
		{
			sampleRectMin = Vector2.one;
			sampleRectMax = Vector2.zero;
			for (int i = 0; i < samples.Length; i++)
			{
				EngineSample engineSample = samples[i];
				if (engineSample.isActiveAndEnabled)
				{
					sampleRectMin = Vector2.Min(engineSample.rpmLoad, sampleRectMin);
					sampleRectMax = Vector2.Max(engineSample.rpmLoad, sampleRectMax);
				}
			}
		}

		private void ClampToSampleRect()
		{
			rpmLoad.x = Mathf.Clamp(rpmLoad.x, sampleRectMin.x, sampleRectMax.x);
			rpmLoad.y = Mathf.Clamp(rpmLoad.y, sampleRectMin.y, sampleRectMax.y);
		}

		public void CalculateSampleEdges()
		{
			if (edgeSamples != null && edgeSamples.Length >= 2)
			{
				for (int i = 0; i < edgeSamples.Length; i++)
				{
					EngineSample engineSample = edgeSamples[i];
					EngineSample engineSample2 = edgeSamples[(i + 1) % edgeSamples.Length];
					engineSample.edgeTangent = engineSample2.rpmLoad - engineSample.rpmLoad;
					engineSample.edgeLength = engineSample.edgeTangent.magnitude;
					engineSample.edgeTangent /= engineSample.edgeLength;
					engineSample.edgeNormal = new Vector2(0f - engineSample.edgeTangent.y, engineSample.edgeTangent.x);
				}
			}
		}

		private void ClampToSampleEdges()
		{
			if (edgeSamples == null || edgeSamples.Length < 2)
			{
				return;
			}
			float num = float.MaxValue;
			int num2 = -1;
			for (int i = 0; i < edgeSamples.Length; i++)
			{
				float sqrMagnitude = (edgeSamples[i].rpmLoad - rpmLoad).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					num2 = i;
				}
			}
			EngineSample engineSample = edgeSamples[num2];
			EngineSample engineSample2 = edgeSamples[(num2 + edgeSamples.Length - 1) % edgeSamples.Length];
			Vector2 rhs = rpmLoad - engineSample2.rpmLoad;
			Vector2 rhs2 = rpmLoad - engineSample.rpmLoad;
			float num3 = Vector2.Dot(engineSample2.edgeNormal, rhs);
			float num4 = Vector2.Dot(engineSample.edgeNormal, rhs2);
			if (num3 < 0f && num4 < 0f)
			{
				return;
			}
			if (num3 < 0f || num4 < 0f)
			{
				if (num3 > 0f)
				{
					float num5 = Mathf.Clamp(Vector2.Dot(engineSample2.edgeTangent, rhs), 0f, engineSample2.edgeLength);
					rpmLoad = engineSample2.rpmLoad + num5 * engineSample2.edgeTangent;
				}
				else
				{
					float num6 = Mathf.Clamp(Vector2.Dot(engineSample.edgeTangent, rhs2), 0f, engineSample.edgeLength);
					rpmLoad = engineSample.rpmLoad + num6 * engineSample.edgeTangent;
				}
				return;
			}
			float num7 = Vector2.Dot(engineSample2.edgeTangent, rhs);
			float num8 = Vector2.Dot(engineSample.edgeTangent, rhs2);
			if (num7 > 0f && num7 < engineSample2.edgeLength)
			{
				num7 = Mathf.Clamp(num7, 0f, engineSample2.edgeLength);
				rpmLoad = engineSample2.rpmLoad + num7 * engineSample2.edgeTangent;
			}
			else if (num8 > 0f && num8 < engineSample.edgeLength)
			{
				num8 = Mathf.Clamp(num8, 0f, engineSample.edgeLength);
				rpmLoad = engineSample.rpmLoad + num8 * engineSample.edgeTangent;
			}
			else
			{
				rpmLoad = engineSample.rpmLoad;
			}
		}
	}
}
