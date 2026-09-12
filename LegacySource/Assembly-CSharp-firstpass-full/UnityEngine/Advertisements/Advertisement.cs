using System;

namespace UnityEngine.Advertisements
{
	public static class Advertisement
	{
		[Flags]
		private enum DebugLevelInternal
		{
			None = 0,
			Error = 1,
			Warning = 2,
			Info = 4,
			Debug = 8
		}

		[Flags]
		[Obsolete("Use Advertisement.debugMode instead.")]
		public enum DebugLevel
		{
			None = 0,
			Error = 1,
			Warning = 2,
			Info = 4,
			Debug = 8
		}

		private static bool s_Initialized;

		private static readonly IPlatform s_Platform = GetPlatform();

		private static bool s_Showing;

		private static DebugLevelInternal s_DebugLevel = ((!Debug.isDebugBuild) ? (DebugLevelInternal.Error | DebugLevelInternal.Warning | DebugLevelInternal.Info) : (DebugLevelInternal.Error | DebugLevelInternal.Warning | DebugLevelInternal.Info | DebugLevelInternal.Debug));

		[Obsolete("Use Advertisement.debugMode instead.")]
		public static DebugLevel debugLevel
		{
			get
			{
				return (DebugLevel)s_DebugLevel;
			}
			set
			{
				s_DebugLevel = (DebugLevelInternal)value;
			}
		}

		public static bool isInitialized
		{
			get
			{
				return s_Initialized;
			}
			private set
			{
				s_Initialized = value;
			}
		}

		public static bool isSupported
		{
			get
			{
				return Application.isEditor || (Application.platform == RuntimePlatform.Android && s_Platform.isSupported) || (Application.platform == RuntimePlatform.IPhonePlayer && s_Platform.isSupported);
			}
		}

		public static bool debugMode
		{
			get
			{
				return s_Platform.debugMode;
			}
			set
			{
				s_Platform.debugMode = value;
			}
		}

		public static string version
		{
			get
			{
				return s_Platform.version;
			}
		}

		public static bool isShowing
		{
			get
			{
				return false;
			}
		}

		private static IPlatform GetPlatform()
		{
			try
			{
				return new AndroidPlatform();
			}
			catch (Exception exception)
			{
				Debug.LogError("Initializing Unity Ads.");
				Debug.LogException(exception);
				return new UnsupportedPlatform();
			}
		}

		public static void Initialize(string gameId)
		{
			Initialize(gameId, false);
		}

		public static void Initialize(string gameId, bool testMode)
		{
			if (!isInitialized)
			{
				isInitialized = true;
				MetaData metaData = new MetaData("framework");
				metaData.Set("name", "Unity");
				metaData.Set("version", Application.unityVersion);
				SetMetaData(metaData);
				MetaData metaData2 = new MetaData("adapter");
				metaData2.Set("name", "AssetStore");
				metaData2.Set("version", version);
				SetMetaData(metaData2);
				s_Platform.Initialize(gameId, testMode);
			}
		}

		public static bool IsReady()
		{
			return IsReady(null);
		}

		public static bool IsReady(string placementId)
		{
			return s_Platform.IsReady((!string.IsNullOrEmpty(placementId)) ? placementId : null);
		}

		public static PlacementState GetPlacementState()
		{
			return GetPlacementState(null);
		}

		public static PlacementState GetPlacementState(string placementId)
		{
			return s_Platform.GetPlacementState((!string.IsNullOrEmpty(placementId)) ? placementId : null);
		}

		public static void Show()
		{
			Show(null, null);
		}

		public static void Show(ShowOptions showOptions)
		{
			Show(null, showOptions);
		}

		public static void Show(string placementId)
		{
			Show(placementId, null);
		}

		public static void Show(string placementId, ShowOptions showOptions)
		{
			Action<ShowResult> callback = null;
			if (showOptions != null)
			{
				if (showOptions.resultCallback != null)
				{
					callback = showOptions.resultCallback;
				}
				if (!string.IsNullOrEmpty(showOptions.gamerSid))
				{
					MetaData metaData = new MetaData("player");
					metaData.Set("server_id", showOptions.gamerSid);
					SetMetaData(metaData);
				}
			}
			s_Platform.Show((!string.IsNullOrEmpty(placementId)) ? placementId : null, callback);
		}

		public static void SetMetaData(MetaData metaData)
		{
			s_Platform.SetMetaData(metaData);
		}
	}
}
