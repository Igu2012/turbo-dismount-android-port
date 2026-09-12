#pragma warning disable 0618,0619
using UnityEngine;
using reLive;

namespace Dismount
{
	public class AmbientSound : MonoBehaviour
	{
		public float groundAmbienceLevel = 20f;

		public float skyAmbienceLevel = 100f;

		private AudioSource groundAmbience;

		private AudioSource skyAmbience;

		private float groundAmbienceOriginalVolume = 1f;

		private float skyAmbienceOriginalVolume = 1f;

		private bool updateAmbience;

		private void Awake()
		{
			Transform transform = base.transform.Find("Ground");
			if ((bool)transform && (bool)transform.GetComponent<AudioSource>())
			{
				groundAmbience = transform.GetComponent<AudioSource>();
				groundAmbienceOriginalVolume = groundAmbience.volume;
			}
			transform = base.transform.Find("Sky");
			if ((bool)transform && (bool)transform.GetComponent<AudioSource>())
			{
				skyAmbience = transform.GetComponent<AudioSource>();
				skyAmbienceOriginalVolume = skyAmbience.volume;
			}
		}

		private void Start()
		{
			updateAmbience = true;
			if ((bool)groundAmbience)
			{
				groundAmbience.volume = 0f;
				groundAmbience.Play();
			}
			if ((bool)skyAmbience)
			{
				skyAmbience.volume = 0f;
				skyAmbience.Play();
			}
		}

		private void FixedUpdate()
		{
			if (updateAmbience)
			{
				float y = DismountGame.cameraManager.globalCamera.transform.position.y;
				float num = 1f;
				float num2 = 0f;
				if (y > skyAmbienceLevel)
				{
					num = 0f;
					num2 = 1f;
				}
				else if (y > groundAmbienceLevel)
				{
					num2 = (y - groundAmbienceLevel) / (skyAmbienceLevel - groundAmbienceLevel);
					num = 1f - num2;
				}
				float pitch = 1f;
				Replay instance = Replay.Instance;
				if (instance.IsRunning && instance.Activity == Replay.ReplayActivity.Playback)
				{
					pitch = instance.PlaybackSpeed;
				}
				if ((bool)groundAmbience)
				{
					groundAmbience.volume = num * groundAmbienceOriginalVolume;
					groundAmbience.pitch = pitch;
				}
				if ((bool)skyAmbience)
				{
					skyAmbience.volume = num2 * skyAmbienceOriginalVolume;
					skyAmbience.pitch = pitch;
				}
			}
		}
	}
}
