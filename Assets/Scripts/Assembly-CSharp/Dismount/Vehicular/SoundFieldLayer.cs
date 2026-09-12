#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	public class SoundFieldLayer : MonoBehaviour
	{
		public SoundField field;

		public SoundFieldSample[] samples;

		[Range(0f, 2f)]
		public float maxDistance = -1f;

		public bool normalizeMix = true;

		public bool allowVolumeMultiplier = true;

		private float volumeMul = 1f;

		[Range(0f, 1f)]
		public float additionalPitchInertia;

		private int numMixedSamples;

		private float prevXScale = -1f;

		private float prevYScale = -1f;

		private float inputNodeX;

		public float volumeMultiplier
		{
			get
			{
				return volumeMul;
			}
			set
			{
				if (allowVolumeMultiplier)
				{
					volumeMul = value;
				}
			}
		}

		public void Play()
		{
			if (samples == null || samples.Length < 1)
			{
				return;
			}
			for (int i = 0; i < samples.Length; i++)
			{
				SoundFieldSample soundFieldSample = samples[i];
				if ((bool)soundFieldSample.audioSource)
				{
					soundFieldSample.audioSource.Play();
				}
			}
		}

		public void Stop()
		{
			if (samples == null || samples.Length < 1)
			{
				return;
			}
			for (int i = 0; i < samples.Length; i++)
			{
				SoundFieldSample soundFieldSample = samples[i];
				if ((bool)soundFieldSample.audioSource)
				{
					soundFieldSample.audioSource.Stop();
				}
			}
		}

		public void UpdateSampleScale()
		{
			if (samples != null && samples.Length >= 1)
			{
				for (int i = 0; i < samples.Length; i++)
				{
					SoundFieldSample soundFieldSample = samples[i];
					soundFieldSample.scaledX = soundFieldSample.x / field.xScale;
					soundFieldSample.scaledY = soundFieldSample.y / field.yScale;
					soundFieldSample.scaledWidth = soundFieldSample.width / field.xScale;
					soundFieldSample.scaledHeight = soundFieldSample.height / field.yScale;
				}
			}
		}

		public void UpdateSound(Vector2 inputNode, Vector2 scaledNode)
		{
			if (samples == null || samples.Length < 1)
			{
				return;
			}
			if (additionalPitchInertia > 0f)
			{
				inputNodeX = Mathf.Lerp(inputNode.x, inputNodeX, additionalPitchInertia);
			}
			else
			{
				inputNodeX = inputNode.x;
			}
			if (prevXScale != field.xScale || prevYScale != field.yScale)
			{
				UpdateSampleScale();
				prevXScale = field.xScale;
				prevYScale = field.yScale;
			}
			float num = 0f;
			numMixedSamples = 0;
			for (int i = 0; i < samples.Length; i++)
			{
				SoundFieldSample soundFieldSample = samples[i];
				if (!soundFieldSample.isActiveAndEnabled)
				{
					continue;
				}
				float num2 = 0f;
				float num3 = 0f;
				if (soundFieldSample.width > 0f)
				{
					float num4 = soundFieldSample.scaledX - soundFieldSample.scaledWidth * 0.5f;
					float num5 = soundFieldSample.scaledX + soundFieldSample.scaledWidth * 0.5f;
					if (scaledNode.x < num4)
					{
						num2 = num4 - scaledNode.x;
					}
					else if (scaledNode.x > num5)
					{
						num2 = scaledNode.x - num5;
					}
				}
				else
				{
					num2 = scaledNode.x - soundFieldSample.scaledX;
				}
				if (soundFieldSample.height > 0f)
				{
					float num6 = soundFieldSample.scaledY - soundFieldSample.scaledHeight * 0.5f;
					float num7 = soundFieldSample.scaledY + soundFieldSample.scaledHeight * 0.5f;
					if (scaledNode.y < num6)
					{
						num3 = num6 - scaledNode.y;
					}
					else if (scaledNode.y > num7)
					{
						num3 = scaledNode.y - num7;
					}
				}
				else
				{
					num3 = scaledNode.y - soundFieldSample.scaledY;
				}
				float num8 = num2 * num2 + num3 * num3;
				float num9 = Mathf.Clamp(soundFieldSample.distanceBias + soundFieldSample.attenuation * num8, 0.001f, 10f);
				if (normalizeMix)
				{
					soundFieldSample.mix = 1f / num9;
				}
				else
				{
					soundFieldSample.mix = Mathf.Clamp01(0.01f / num9);
				}
				if (maxDistance > 0f)
				{
					float num10 = maxDistance * maxDistance;
					float num11 = Mathf.InverseLerp(num10, 0f, Mathf.Clamp(num8, 0f, num10));
					float num12 = 1f - num11;
					num11 = 1f - num12 * num12;
					soundFieldSample.mix *= num11;
				}
				if (soundFieldSample.mix > 0.05f)
				{
					num += soundFieldSample.mix;
					numMixedSamples++;
				}
				else
				{
					soundFieldSample.mix = 0f;
					soundFieldSample.audioSource.volume = 0f;
				}
			}
			if (normalizeMix && numMixedSamples == 0)
			{
				return;
			}
			if (normalizeMix)
			{
				float num13 = 1f / num;
				for (int j = 0; j < samples.Length; j++)
				{
					SoundFieldSample soundFieldSample2 = samples[j];
					if (soundFieldSample2.isActiveAndEnabled)
					{
						float num14 = soundFieldSample2.mix * num13;
						soundFieldSample2.mix = 1f - (1f - num14) * (1f - num14);
						soundFieldSample2.audioSource.volume = soundFieldSample2.mix * volumeMul * soundFieldSample2.originalVolume;
						if (soundFieldSample2.pitchReferenceX > 0f)
						{
							soundFieldSample2.audioSource.pitch = inputNodeX / soundFieldSample2.pitchReferenceX * soundFieldSample2.originalPitch;
						}
					}
				}
				return;
			}
			for (int k = 0; k < samples.Length; k++)
			{
				SoundFieldSample soundFieldSample3 = samples[k];
				if (soundFieldSample3.isActiveAndEnabled)
				{
					soundFieldSample3.audioSource.volume = soundFieldSample3.mix * volumeMul * soundFieldSample3.originalVolume;
					if (soundFieldSample3.pitchReferenceX > 0f)
					{
						soundFieldSample3.audioSource.pitch = inputNodeX / soundFieldSample3.pitchReferenceX * soundFieldSample3.originalPitch;
					}
				}
			}
		}
	}
}
