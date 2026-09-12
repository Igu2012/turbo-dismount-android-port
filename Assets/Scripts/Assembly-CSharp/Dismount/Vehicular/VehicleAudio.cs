#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount.Vehicular
{
	public class VehicleAudio : MonoBehaviour
	{
		public EngineSoundField engineSound;

		public AudioSource skidSound;

		public AudioSource rollSound;

		public AudioSource boostSound;

		public float minSkidSlip = 0.5f;

		public float maxSkidSlip = 10f;

		public float minRollSpeed;

		public float maxRollSpeed = 20f;

		public float rollPitchRange = 0.25f;

		private float skidSoundOriginalVolume = 1f;

		private float skidSoundOriginalPitch = 1f;

		private float rollSoundOriginalVolume = 1f;

		private float rollSoundOriginalPitch = 1f;

		private bool engineRunning;

		private float prevRoll;

		private void Awake()
		{
			if ((bool)skidSound)
			{
				skidSoundOriginalVolume = skidSound.volume;
				skidSoundOriginalPitch = skidSound.pitch;
				skidSound.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
				skidSound.volume = 0f;
				skidSound.Play();
			}
			if ((bool)rollSound)
			{
				rollSoundOriginalVolume = rollSound.volume;
				rollSoundOriginalPitch = rollSound.pitch;
				rollSound.volume = 0f;
				rollSound.Play();
			}
			if ((bool)boostSound)
			{
				boostSound.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
			}
		}

		public void UpdateRoll(float rollSpeed)
		{
			if ((bool)rollSound)
			{
				float num = maxRollSpeed - minRollSpeed;
				float num2 = Mathf.Clamp01((rollSpeed - minRollSpeed) / num);
				num2 = (prevRoll = prevRoll * 0.95f + num2 * 0.05f);
				rollSound.pitch = (1f + rollPitchRange * num2) * rollSoundOriginalPitch;
				rollSound.volume = num2 * num2 * rollSoundOriginalVolume;
			}
		}

		public void UpdateSkid(float slipAmount)
		{
			if ((bool)skidSound)
			{
				float num = Mathf.InverseLerp(minSkidSlip, maxSkidSlip, slipAmount);
				float num2 = 0.5f * num + 0.5f * (num * num * (3f - 2f * num));
				skidSound.volume = num2 * skidSoundOriginalVolume;
				float num3 = 0.5f * num2;
				if (num2 > 0.6f)
				{
					float num4 = (num2 - 0.6f) * 2.5f;
					num4 *= num4;
					num3 -= num4 * 0.3f;
				}
				skidSound.pitch = (1f + num3) * skidSoundOriginalPitch;
			}
		}

		public void UpdateEngine(float throttle, float rpm, float load)
		{
			if ((bool)engineSound)
			{
				engineSound.UpdateSound(throttle, rpm, load);
			}
		}

		public void StartEngine()
		{
			engineRunning = true;
			if ((bool)engineSound)
			{
				engineSound.Play();
			}
		}

		public void KillEngine()
		{
			engineRunning = false;
			if ((bool)engineSound)
			{
				engineSound.Stop();
			}
		}

		public void StartBoost()
		{
			if (engineRunning && (bool)boostSound)
			{
				boostSound.Play();
			}
		}

		public void KillBoost()
		{
			if ((bool)boostSound)
			{
				boostSound.Stop();
			}
		}

		private void SetClipData(AudioSource source, float hz, int wave)
		{
			AudioClip clip = source.clip;
			float[] array = new float[clip.samples];
			float num = (float)clip.frequency / hz;
			int num2 = (int)((float)clip.samples / num);
			num = clip.samples / num2;
			switch (wave)
			{
			case 0:
			{
				float num4 = (float)Math.PI * 2f / num;
				for (int j = 0; j < clip.samples; j++)
				{
					array[j] = Mathf.Sin((float)j * num4) * 0.5f + 0.5f;
				}
				break;
			}
			case 1:
			{
				float num5 = (float)Math.PI * 2f / num;
				for (int k = 0; k < clip.samples; k++)
				{
					array[k] = Mathf.Sign(Mathf.Sin((float)k * num5 + 1.7f)) * 0.5f + 0.5f;
				}
				break;
			}
			default:
			{
				float num3 = 1f / num;
				for (int i = 0; i < clip.samples; i++)
				{
					array[i] = Mathf.Repeat((float)i * num3, 1f);
				}
				break;
			}
			}
			clip.SetData(array, 0);
		}
	}
}
