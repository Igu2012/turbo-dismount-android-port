#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace reLive
{
	public class OneShotAudioHelper : MonoBehaviour
	{
		public int MaxSounds = 64;

		private static OneShotAudioHelper instance;

		private List<AudioSource> sounds = new List<AudioSource>();

		private static OneShotAudioHelper Instance
		{
			get
			{
				if (!instance)
				{
					GameObject gameObject = new GameObject("OneShotAudioHelper");
					gameObject.transform.position = Vector3.zero;
					gameObject.AddComponent<OneShotAudioHelper>();
					instance = gameObject.GetComponent<OneShotAudioHelper>();
				}
				return instance;
			}
		}

		private bool PriorityKillSound(int priority)
		{
			AudioSource audioSource = null;
			foreach (AudioSource sound in sounds)
			{
				if (sound.priority < priority)
				{
					audioSource = sound;
					break;
				}
			}
			if ((bool)audioSource)
			{
				audioSource.Stop();
				sounds.Remove(audioSource);
				if ((bool)Replay.Instance)
				{
					Replay.Instance.RemoveRecordedObject(audioSource.gameObject);
				}
				Object.Destroy(audioSource.gameObject);
				return true;
			}
			return false;
		}

		private AudioSource PlayClipAtPointInternal(AudioClip clip, Vector3 position, float volume, AudioSource copySettingsFrom)
		{
			if (sounds.Count > MaxSounds)
			{
				int priority = 128;
				if (copySettingsFrom != null)
				{
					priority = copySettingsFrom.priority;
				}
				if (!PriorityKillSound(priority))
				{
					return null;
				}
			}
			GameObject gameObject = new GameObject("(Temp Audio)");
			gameObject.transform.position = position;
			gameObject.transform.parent = base.transform;
			gameObject.AddComponent<AudioSource>();
			AudioSource audioSource = gameObject.GetComponent<AudioSource>();
			audioSource.clip = clip;
			audioSource.loop = false;
			audioSource.playOnAwake = false;
			audioSource.volume = volume;
			if ((bool)copySettingsFrom)
			{
				gameObject.name = copySettingsFrom.name + " " + gameObject.name;
				audioSource.bypassEffects = copySettingsFrom.bypassEffects;
				audioSource.dopplerLevel = copySettingsFrom.dopplerLevel;
				audioSource.ignoreListenerVolume = copySettingsFrom.ignoreListenerVolume;
				audioSource.maxDistance = copySettingsFrom.maxDistance;
				audioSource.minDistance = copySettingsFrom.minDistance;
				audioSource.panStereo = copySettingsFrom.panStereo;
				audioSource.spatialBlend = copySettingsFrom.spatialBlend;
				audioSource.pitch = copySettingsFrom.pitch;
				audioSource.priority = copySettingsFrom.priority;
				audioSource.rolloffMode = copySettingsFrom.rolloffMode;
				audioSource.spread = copySettingsFrom.spread;
				audioSource.velocityUpdateMode = copySettingsFrom.velocityUpdateMode;
				audioSource.rolloffMode = copySettingsFrom.rolloffMode;
			}
			sounds.Add(audioSource);
			if ((bool)Replay.Instance)
			{
				Replay.Instance.AddObjectForRecording(gameObject);
			}
			audioSource.Play();
			return audioSource;
		}

		public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume, AudioSource copySettingsFrom = null)
		{
			OneShotAudioHelper oneShotAudioHelper = Instance;
			return oneShotAudioHelper.PlayClipAtPointInternal(clip, position, volume, copySettingsFrom);
		}

		private void Update()
		{
			List<AudioSource> list = new List<AudioSource>();
			foreach (AudioSource sound in sounds)
			{
				if (!sound.isPlaying)
				{
					list.Add(sound);
				}
			}
			foreach (AudioSource item in list)
			{
				sounds.Remove(item);
				if ((bool)Replay.Instance)
				{
					Replay.Instance.RemoveRecordedObject(item.gameObject);
				}
				Object.Destroy(item.gameObject);
			}
		}
	}
}
