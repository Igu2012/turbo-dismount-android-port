using System;
using System.Collections.Generic;
using UnityEngine;

public class Everyplay : MonoBehaviour
{
	public enum UserInterfaceIdiom
	{
		Phone = 0,
		Tablet = 1,
		TV = 2,
		iPhone = Phone,
		iPad = Tablet
	}

	public delegate void WasClosedDelegate();

	public delegate void ReadyForRecordingDelegate(bool enabled);

	public delegate void RecordingStartedDelegate();

	public delegate void RecordingStoppedDelegate();

	public delegate void FileReadyDelegate(string filepath);

	public delegate void ThumbnailTextureReadyDelegate(Texture2D texture, bool portrait);

	private const string nativeMethodSource = "__Internal";

	private static bool appIsClosing;

	private static bool hasMethods = true;

	private static bool seenInitialization;

	private static bool readyForRecording;

	private static Everyplay everyplayInstance;

	private static Texture2D currentThumbnailTargetTexture;

	private static Everyplay EveryplayInstance
	{
		get
		{
			if (everyplayInstance == null && !appIsClosing)
			{
				EveryplaySettings everyplaySettings = (EveryplaySettings)Resources.Load("EveryplaySettings");
				if (everyplaySettings != null && everyplaySettings.IsEnabled)
				{
					GameObject gameObject = new GameObject("Everyplay");
					if (gameObject != null)
					{
						gameObject.name += gameObject.GetInstanceID();
						everyplayInstance = gameObject.AddComponent<Everyplay>();
						if (everyplayInstance != null)
						{
							hasMethods = true;
							if (!seenInitialization)
							{
							}
							seenInitialization = true;
							if (everyplaySettings.testButtonsEnabled)
							{
								AddTestButtons(gameObject);
							}
							UnityEngine.Object.DontDestroyOnLoad(gameObject);
						}
					}
				}
			}
			return everyplayInstance;
		}
	}

	public static event WasClosedDelegate WasClosed;

	public static event ReadyForRecordingDelegate ReadyForRecording;

	public static event RecordingStartedDelegate RecordingStarted;

	public static event RecordingStoppedDelegate RecordingStopped;

	public static event FileReadyDelegate FileReady;

	public static event ThumbnailTextureReadyDelegate ThumbnailTextureReady;

	public static void Initialize()
	{
		if (EveryplayInstance == null)
		{
			Debug.Log("Unable to initialize Everyplay. Everyplay might be disabled for this platform or the app is closing.");
		}
	}

	public static void ShowSharingModal()
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void PlayLastRecording()
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void StartRecording()
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void GetFilepath()
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void StopRecording()
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void PauseRecording()
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void ResumeRecording()
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static bool IsRecording()
	{
		if (!(EveryplayInstance != null) || hasMethods)
		{
		}
		return false;
	}

	public static bool IsRecordingSupported()
	{
		if (!(EveryplayInstance != null) || hasMethods)
		{
		}
		return false;
	}

	public static bool IsPaused()
	{
		if (!(EveryplayInstance != null) || hasMethods)
		{
		}
		return false;
	}

	[Obsolete("Everyplay HUD-less functionality is no longer maintained and may not function properly.")]
	public static bool SnapshotRenderbuffer()
	{
		if (!(EveryplayInstance != null) || hasMethods)
		{
		}
		return false;
	}

	public static bool IsSupported()
	{
		if (!(EveryplayInstance != null) || hasMethods)
		{
		}
		return false;
	}

	public static bool IsSingleCoreDevice()
	{
		if (!(EveryplayInstance != null) || hasMethods)
		{
		}
		return false;
	}

	public static int GetUserInterfaceIdiom()
	{
		if (!(EveryplayInstance != null) || hasMethods)
		{
		}
		return 0;
	}

	public static void SetTargetFPS(int fps)
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void SetMotionFactor(int factor)
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void SetAudioResamplerQuality(int quality)
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void SetMaxRecordingMinutesLength(int minutes)
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void SetMaxRecordingSecondsLength(int seconds)
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void SetLowMemoryDevice(bool state)
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void SetDisableSingleCoreDevices(bool state)
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void SetThumbnailTargetTexture(Texture2D texture)
	{
		if (EveryplayInstance != null && hasMethods)
		{
			currentThumbnailTargetTexture = texture;
		}
	}

	[Obsolete("Use SetThumbnailTargetTexture(Texture2D texture) instead.")]
	public static void SetThumbnailTargetTextureId(int textureId)
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	[Obsolete("Defining texture width is no longer required when SetThumbnailTargetTexture(Texture2D texture) is used.")]
	public static void SetThumbnailTargetTextureWidth(int textureWidth)
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	[Obsolete("Defining texture height is no longer required when SetThumbnailTargetTexture(Texture2D texture) is used.")]
	public static void SetThumbnailTargetTextureHeight(int textureHeight)
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static void TakeThumbnail()
	{
		if (EveryplayInstance != null && !hasMethods)
		{
		}
	}

	public static bool IsReadyForRecording()
	{
		if (EveryplayInstance != null && hasMethods)
		{
			return readyForRecording;
		}
		return false;
	}

	private static void RemoveAllEventHandlers()
	{
		WasClosed = null;
		ReadyForRecording = null;
		RecordingStarted = null;
		RecordingStopped = null;
		ThumbnailTextureReady = null;
		FileReady = null;
	}

	private static void Reset()
	{
	}

	private static void AddTestButtons(GameObject gameObject)
	{
		Texture2D texture2D = (Texture2D)Resources.Load("everyplay-test-buttons", typeof(Texture2D));
		if (texture2D != null)
		{
			EveryplayRecButtons everyplayRecButtons = gameObject.AddComponent<EveryplayRecButtons>();
			if (everyplayRecButtons != null)
			{
				everyplayRecButtons.atlasTexture = texture2D;
			}
		}
	}

	private void OnApplicationQuit()
	{
		Reset();
		if (currentThumbnailTargetTexture != null)
		{
			SetThumbnailTargetTexture(null);
			currentThumbnailTargetTexture = null;
		}
		RemoveAllEventHandlers();
		appIsClosing = true;
		everyplayInstance = null;
	}

	private void EveryplayHidden(string msg)
	{
		if (WasClosed != null)
		{
			WasClosed();
		}
	}

	private void EveryplayReadyForRecording(string jsonMsg)
	{
		Dictionary<string, object> dict = EveryplayDictionaryExtensions.JsonToDictionary(jsonMsg);
		bool value;
		if (dict.TryGetValue<bool>("enabled", out value))
		{
			readyForRecording = value;
			if (ReadyForRecording != null)
			{
				ReadyForRecording(value);
			}
		}
	}

	private void EveryplayFileReady(string jsonMsg)
	{
		Dictionary<string, object> dict = EveryplayDictionaryExtensions.JsonToDictionary(jsonMsg);
		string value;
		if (dict.TryGetValue<string>("videoURL", out value) && FileReady != null)
		{
			FileReady(value);
		}
	}

	private void EveryplayRecordingStarted(string msg)
	{
		if (RecordingStarted != null)
		{
			RecordingStarted();
		}
	}

	private void EveryplayRecordingStopped(string msg)
	{
		if (RecordingStopped != null)
		{
			RecordingStopped();
		}
	}

	private void EveryplayThumbnailTextureReady(string jsonMsg)
	{
		if (ThumbnailTextureReady == null)
		{
			return;
		}
		Dictionary<string, object> dict = EveryplayDictionaryExtensions.JsonToDictionary(jsonMsg);
		long value;
		bool value2;
		if (currentThumbnailTargetTexture != null && dict.TryGetValue<long>("texturePtr", out value) && dict.TryGetValue<bool>("portrait", out value2))
		{
			long num = (long)currentThumbnailTargetTexture.GetNativeTexturePtr();
			if (num == value)
			{
				ThumbnailTextureReady(currentThumbnailTargetTexture, value2);
			}
		}
	}
}
