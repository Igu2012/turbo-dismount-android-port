#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	public class Zither : MonoBehaviour
	{
		private const float collisionInterval = 1f;

		private const float breakInterval = 0.6f;

		public AudioSource boostAudio;

		public AudioSource collisionAudio;

		public AudioSource breakAudio;

		public AudioClip[] boostSounds;

		public AudioClip[] collisionSounds;

		public AudioClip[] breakSounds;

		private float collisionOriginalVolume = 1f;

		private float lastCollisionTime = -1f;

		private float lastBreakTime = -1f;

		private void Awake()
		{
			if (collisionAudio != null)
			{
				collisionOriginalVolume = collisionAudio.volume;
			}
		}

		public void PlayBoostSound()
		{
			if (!(boostAudio == null) && boostSounds != null && !boostAudio.isPlaying)
			{
				AudioClip clip = boostSounds[Random.Range(0, boostSounds.Length)];
				boostAudio.clip = clip;
				boostAudio.Play();
			}
		}

		public void PlayCollisionSound(float volumeMultiplier)
		{
			if (!(collisionAudio == null) && collisionSounds != null && !(Time.fixedTime < lastCollisionTime + 1f))
			{
				volumeMultiplier = Mathf.Clamp01(volumeMultiplier * 2.5f);
				if (!(volumeMultiplier < 0.1f))
				{
					AudioClip clip = collisionSounds[Random.Range(0, collisionSounds.Length)];
					collisionAudio.clip = clip;
					collisionAudio.volume = collisionOriginalVolume * volumeMultiplier;
					collisionAudio.Play();
					lastCollisionTime = Time.fixedTime;
				}
			}
		}

		public void PlayBreakSound()
		{
			if (!(breakAudio == null) && breakSounds != null && !(Time.fixedTime < lastBreakTime + 0.6f))
			{
				AudioClip clip = breakSounds[Random.Range(0, breakSounds.Length)];
				breakAudio.clip = clip;
				breakAudio.Play();
				lastBreakTime = Time.fixedTime;
			}
		}
	}
}
