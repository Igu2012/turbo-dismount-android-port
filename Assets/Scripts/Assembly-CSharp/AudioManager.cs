#pragma warning disable 0618,0619
using System.Collections.Generic;
using Dismount;
using UnityEngine;
using reLive;

public class AudioManager : MonoBehaviour
{
	private float musicVolumeLevel = 1f;

	private float sfxVolumeLevel = 1f;

	private bool isMusicEnabled = true;

	private bool isSfxEnabled = true;

	private float sfxFadeStartTime = -1f;

	private float sfxFadeDuration = 1f;

	private float sfxFadeLevel = 1f;

	private float sfxRewindLevel = 1f;

	private float musicSourceLevel = 1f;

	private float musicDuckStartTime = -1f;

	private float musicDuckDuration = 1f;

	private float musicDuckLevel = 1f;

	public int poolSize = 16;

	private List<GameObject> sources = new List<GameObject>();

	public AudioItem[] effects;

	private Dictionary<string, AudioItem> effectsLookup = new Dictionary<string, AudioItem>();

	private AudioSource music;

	private AudioItem musicList;

	private AudioSource[] otherSources;

	private GameObject parentSource;

	private List<KeyValuePair<AudioSource, bool>> audioSourcePlayingStateWhenPaused = new List<KeyValuePair<AudioSource, bool>>();

	public bool musicEnabled
	{
		get
		{
			return isMusicEnabled;
		}
		set
		{
			isMusicEnabled = value;
			ApplyMusicVolume();
		}
	}

	public float musicVolume
	{
		get
		{
			return musicVolumeLevel;
		}
		set
		{
			musicVolumeLevel = Mathf.Clamp01(value);
			ApplyMusicVolume();
		}
	}

	public bool sfxEnabled
	{
		get
		{
			return isSfxEnabled;
		}
		set
		{
			isSfxEnabled = value;
			ApplySfxVolume();
		}
	}

	public float sfxVolume
	{
		get
		{
			return sfxVolumeLevel;
		}
		set
		{
			sfxVolumeLevel = Mathf.Clamp01(value);
			ApplySfxVolume();
		}
	}

	private float mixedMusicVolume
	{
		get
		{
			return musicVolumeLevel * musicSourceLevel * musicDuckLevel;
		}
	}

	private float mixedSfxVolume
	{
		get
		{
			return sfxVolumeLevel * sfxRewindLevel * sfxFadeLevel;
		}
	}

	private void Start()
	{
		AudioItem[] array = effects;
		foreach (AudioItem audioItem in array)
		{
			effectsLookup.Add(audioItem.name, audioItem);
		}
		parentSource = new GameObject();
		parentSource.name = "AudioPool";
		parentSource.transform.parent = DismountGame.instance.transform;
		otherSources = Object.FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		for (int j = 0; j < poolSize; j++)
		{
			sources.Add(createNewAudioSource());
		}
		music = base.transform.Find("Music").GetComponent<AudioSource>();
		musicList = base.transform.Find("Music").GetComponent<AudioItem>();
		musicSourceLevel = music.volume;
		isMusicEnabled = Prefs.GetInt("musicEnabled", 1) != 0;
		musicVolume = 0.1f * (float)Prefs.GetInt("musicVolume", 10);
		isSfxEnabled = Prefs.GetInt("sfxEnabled", 1) != 0;
		sfxVolume = 0.1f * (float)Prefs.GetInt("sfxVolume", 10);
		ChangeMusic();
		PlayMusic();
	}

	private void ApplyMusicVolume()
	{
		music.volume = ((!isMusicEnabled) ? 0f : mixedMusicVolume);
	}

	private void ApplySfxVolume()
	{
		AudioListener.volume = ((!isSfxEnabled) ? 0f : mixedSfxVolume);
	}

	public void SFXFadeIn(float duration = 0.25f)
	{
		sfxFadeStartTime = Time.fixedTime;
		sfxFadeLevel = 0f;
		sfxFadeDuration = duration;
		ApplySfxVolume();
	}

	public void PlayMusic()
	{
		if (!music.isPlaying)
		{
			music.Play();
		}
	}

	public void PauseMusic()
	{
		music.Pause();
	}

	public void StopMusic()
	{
		music.Stop();
	}

	public void ChangeMusic()
	{
		AudioClip clip = music.clip;
		if (musicList.clips.Length > 0)
		{
			music.Stop();
			int num = 0;
			while (music.clip == clip && num < 5)
			{
				music.clip = musicList.clips[Random.Range(0, musicList.clips.Length)];
				num++;
			}
			music.Play();
		}
	}

	public void Reset()
	{
		foreach (GameObject source in sources)
		{
			Object.Destroy(source);
		}
		sources.Clear();
		for (int i = 0; i < poolSize; i++)
		{
			sources.Add(createNewAudioSource());
		}
		audioSourcePlayingStateWhenPaused.Clear();
		sfxFadeStartTime = -1f;
		sfxFadeDuration = 1f;
		sfxFadeLevel = 1f;
		sfxRewindLevel = 1f;
		musicDuckStartTime = -1f;
		musicDuckDuration = 1f;
		musicDuckLevel = 1f;
		ApplyMusicVolume();
		ApplySfxVolume();
	}

	public void EnableRewindEffects(bool rewind, float rewindSpeed = 1f)
	{
		sfxRewindLevel = 1f;
		if (rewind && Mathf.Abs(rewindSpeed) > 1f)
		{
			sfxRewindLevel = 1f / Mathf.Abs(rewindSpeed);
		}
		ApplySfxVolume();
	}

	public void PauseEffects()
	{
		AudioSource[] array = Object.FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		AudioSource[] array2 = array;
		foreach (AudioSource audioSource in array2)
		{
			if (!audioSource.ignoreListenerPause)
			{
				audioSourcePlayingStateWhenPaused.Add(new KeyValuePair<AudioSource, bool>(audioSource, audioSource.isPlaying));
				audioSource.Pause();
			}
		}
	}

	public void ResumeEffects()
	{
		foreach (KeyValuePair<AudioSource, bool> item in audioSourcePlayingStateWhenPaused)
		{
			if (item.Value && item.Key != null)
			{
				item.Key.Play();
			}
		}
		audioSourcePlayingStateWhenPaused.Clear();
	}

	private void DuckMusic(float duration)
	{
		if (isSfxEnabled && !(sfxVolumeLevel < 0.05f))
		{
			musicDuckDuration = duration;
			musicDuckStartTime = Time.fixedTime;
		}
	}

	private void FixedUpdate()
	{
		float fixedTime = Time.fixedTime;
		if (sfxFadeStartTime >= 0f)
		{
			if (fixedTime > sfxFadeStartTime + sfxFadeDuration)
			{
				sfxFadeStartTime = -1f;
				sfxFadeLevel = 1f;
				ApplySfxVolume();
			}
			else if (fixedTime > sfxFadeStartTime)
			{
				sfxFadeLevel = (fixedTime - sfxFadeStartTime) / sfxFadeDuration;
				ApplySfxVolume();
			}
		}
		if (musicDuckStartTime < 0f)
		{
			return;
		}
		if (fixedTime > musicDuckStartTime + musicDuckDuration)
		{
			musicDuckStartTime = -1f;
			musicDuckDuration = 0f;
			musicDuckLevel = 1f;
			ApplyMusicVolume();
		}
		else if (fixedTime > musicDuckStartTime)
		{
			float num = 0.25f;
			if (fixedTime > musicDuckStartTime + musicDuckDuration - 0.2f)
			{
				float num2 = (fixedTime - (musicDuckStartTime + musicDuckDuration - 0.2f)) / 0.2f;
				num = 0.25f + num2 * 0.75f;
			}
			else if (fixedTime < musicDuckStartTime + 0.2f)
			{
				float num3 = 1f - (fixedTime - musicDuckStartTime) / 0.2f;
				num = 0.25f + num3 * 0.75f;
			}
			musicDuckLevel = num;
			ApplyMusicVolume();
		}
	}

	private GameObject createNewAudioSource()
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "AudioClip";
		gameObject.SetActive(false);
		gameObject.AddComponent<AudioSource>();
		gameObject.AddComponent<AudioInfo>();
		gameObject.AddComponent<RecordMeInReplay>();
		gameObject.transform.parent = parentSource.transform;
		return gameObject;
	}

	private GameObject allocAudioSource()
	{
		if (string.IsNullOrEmpty(DismountGame.level.loadedLevel))
		{
			return null;
		}
		foreach (GameObject source in sources)
		{
			if (source == null)
			{
				Debug.LogError("Missing reference in allocAudioSource()");
				return null;
			}
			if (!source.GetComponent<AudioSource>().isPlaying)
			{
				source.GetComponent<AudioInfo>().StartTime = Time.fixedTime;
				return source;
			}
		}
		GameObject gameObject = sources[0];
		foreach (GameObject source2 in sources)
		{
			if (source2.GetComponent<AudioInfo>().StartTime < gameObject.GetComponent<AudioInfo>().StartTime)
			{
				gameObject = source2;
			}
		}
		gameObject.GetComponent<AudioInfo>().StartTime = Time.fixedTime;
		return gameObject;
	}

	public void PlaySoundEffect(string name, Transform parent, float volumeMultiplier = 1f, bool duckMusic = false)
	{
		AudioItem value;
		if (effectsLookup.TryGetValue(name, out value))
		{
			PlaySoundEffect(value, parent, volumeMultiplier, duckMusic);
		}
		else
		{
			Debug.LogWarning("Sound effect " + name + " not found!");
		}
	}

	public void PlaySoundEffect(AudioItem item, Transform parent, float volumeMultiplier = 1f, bool duckMusic = false)
	{
		GameObject gameObject = allocAudioSource();
		if (!(gameObject == null))
		{
			if ((bool)parent)
			{
				gameObject.transform.parent = parent;
				gameObject.transform.position = parent.position;
			}
			else
			{
				gameObject.transform.parent = parentSource.transform;
				gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			}
			AudioSource audioSource = gameObject.GetComponent<AudioSource>();
			AudioSource audioSource2 = item.GetComponent<AudioSource>();
			audioSource.clip = item.clips[Random.Range(0, item.clips.Length)];
			audioSource.name = "AudioClip (" + item.name + ")";
			audioSource.loop = false;
			audioSource.playOnAwake = false;
			if (duckMusic)
			{
				float length = audioSource.clip.length;
				DuckMusic(length);
			}
			if ((bool)audioSource2)
			{
				audioSource.volume = audioSource2.volume * volumeMultiplier;
				audioSource.pitch = Random.Range(item.minPitchMultiplier, item.maxPitchMultiplier) * audioSource2.pitch;
				audioSource.bypassEffects = audioSource2.bypassEffects;
				audioSource.dopplerLevel = audioSource2.dopplerLevel;
				audioSource.ignoreListenerVolume = audioSource2.ignoreListenerVolume;
				audioSource.maxDistance = audioSource2.maxDistance;
				audioSource.minDistance = audioSource2.minDistance;
				audioSource.panStereo = audioSource2.panStereo;
				audioSource.spatialBlend = audioSource2.spatialBlend;
				audioSource.priority = audioSource2.priority;
				audioSource.rolloffMode = audioSource2.rolloffMode;
				audioSource.spread = audioSource2.spread;
				audioSource.velocityUpdateMode = audioSource2.velocityUpdateMode;
				audioSource.rolloffMode = audioSource2.rolloffMode;
			}
			else
			{
				audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
				audioSource.minDistance = 10f;
				audioSource.maxDistance = 500f;
				audioSource.dopplerLevel = 1f;
				audioSource.spread = 0f;
				audioSource.volume = volumeMultiplier;
				audioSource.pitch = Random.Range(item.minPitchMultiplier, item.maxPitchMultiplier);
			}
			gameObject.SetActive(true);
			audioSource.Play();
		}
	}

	public void alterPitch(float value)
	{
		foreach (GameObject source in sources)
		{
			source.GetComponent<AudioSource>().pitch = value;
		}
		AudioSource[] array = otherSources;
		foreach (AudioSource audioSource in array)
		{
			audioSource.pitch = value;
		}
	}
}
