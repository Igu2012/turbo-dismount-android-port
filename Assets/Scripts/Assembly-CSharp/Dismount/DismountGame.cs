#pragma warning disable 0618,0619
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Dismount.GameStates;
using Dismount.Vehicular;
using UnityEngine;
using reLive;

namespace Dismount
{
	public class DismountGame : MonoBehaviour
	{
		[Serializable]
		public struct CustomHead
		{
			public string resourceName;

			public int dateIndex;

			public bool enableSnow;
		}

		public enum AdPrizeType
		{
			Vehicle = 0,
			Level = 1,
			Character = 2
		}

		private enum ImageSelectType
		{
			Face = 0,
			VehicleLogo = 1
		}

		public List<GameObject> instantiatePrefabs = new List<GameObject>();

		public GameObject inventoryPrefab;

		public GameObject steerPathMeshPrefab;

		public Material faceMaterial;

		public Texture defaultFaceTexture;

		public Texture overriddenFaceTexture;

		public Texture vehicleLogoTexture;

		public Texture bubbleheadTexture;

		public CustomHead[] customHeadArray = new CustomHead[1];

		[HideInInspector]
		public GameObject customHead;

		public Material vehicleMaterial;

		public GameObject levelBorderPrefab;

		private static DismountGame mInstance;

		public GameObject[] boltPrefabs;

		[HideInInspector]
		public MultiObjectPool boltObjectPool;

		private Inventory mInventory;

		private StateManager mStateManager;

		private CameraManager mCameraManager;

		private AudioManager mAudioManager;

		private PlayerState mPlayerState;

		private Level mLevel;

		private UIManager mUIManager;

		private ParticleManager mParticleManager;

		private HUDManager mHUDManager;

		private EveryplayHelper mEveryplayHelper;

		private Challenges mChallenges;

		private TextureFetcher mTextureFetcher;

		private Steamworks mSteamworks;

		private OnlineConfig mOnlineConfig;

		[HideInInspector]
		public bool vehicleLogoOverridden;

		private static bool hasNotch;

		private static int performanceLevel;

		private static float maxShadowDistance = 40f;

		private static bool isTegra;

		public bool snowSeason;

		private bool reloadTextures;

		private bool adColonyInitialized;

		private bool vungleInitialized;

		private AdManager adManager;

		private bool GooglePlayClicked;

		private static bool firstUpdate = true;

		private float resetFillTime = -1f;

		private int controllerConnected = -1;

		public static bool touchscreenEnabled;

		private string publishFileName;

		private string previewImage;

		private UIManager.State statebeforeSubmit;

		private bool isPaused;

		private float previousTimeScale;

		private float previousLeaderboardRefreshTick = -1f;

		private readonly YieldInstruction waitForEndOfFrame = new WaitForEndOfFrame();

		private UIManager.State stateBeforeHandleSubscriptions;

		private GameItem vehicleAfterVideoAd;

		private GameItem levelAfterVideoAd;

		private GameItem characterAfterVideoAd;

		private bool switchVehicleOnNextUpdate;

		private bool switchLevelOnNextUpdate;

		private bool switchCharacterOnNextUpdate;

		private AdPrizeType adVideoPrizeType;

		private int mAdColonyVideosViewed;

		private int mVideoCount;

		private ImageSelectType imageSelectType;

		private readonly string[] _permissions = new string[1] { "android.permission.READ_EXTERNAL_STORAGE" };

		private int triggerCount;

		public GameObject showVechicleEffect;

		private static bool launchCountIncreased;

		public static DismountGame instance
		{
			get
			{
				return mInstance;
			}
		}

		public static Inventory inventory
		{
			get
			{
				return instance.mInventory;
			}
		}

		public static StateManager stateManager
		{
			get
			{
				return instance.mStateManager;
			}
		}

		public static CameraManager cameraManager
		{
			get
			{
				return instance.mCameraManager;
			}
		}

		public static AudioManager audioManager
		{
			get
			{
				return instance.mAudioManager;
			}
		}

		public static PlayerState playerState
		{
			get
			{
				return instance.mPlayerState;
			}
		}

		public static Level level
		{
			get
			{
				return instance.mLevel;
			}
		}

		public static TextureFetcher textureFetcher
		{
			get
			{
				return instance.mTextureFetcher;
			}
		}

		public static UIManager uiManager
		{
			get
			{
				return instance.mUIManager;
			}
		}

		public static HUDManager hudManager
		{
			get
			{
				return instance.mHUDManager;
			}
		}

		public static ParticleManager particleManager
		{
			get
			{
				return instance.mParticleManager;
			}
		}

		public static EveryplayHelper everyplayHelper
		{
			get
			{
				return instance.mEveryplayHelper;
			}
		}

		public static Challenges challenges
		{
			get
			{
				return instance.mChallenges;
			}
		}

		public static Steamworks steamworks
		{
			get
			{
				return instance.mSteamworks;
			}
		}

		public static OnlineConfig onlineConfig
		{
			get
			{
				return instance.mOnlineConfig;
			}
		}

		public bool paused
		{
			get
			{
				return isPaused;
			}
		}

		public static bool IsMetalDevice()
		{
			return false;
		}

		public static bool IsNotchDevice()
		{
			return hasNotch;
		}

		public static bool IsLowPerformanceDevice()
		{
			return performanceLevel == 0;
		}

		public static bool IsTVDevice()
		{
			return performanceLevel == 4;
		}

		public static bool IsLowMemoryDevice()
		{
			return SystemInfo.graphicsMemorySize + SystemInfo.systemMemorySize <= 512;
		}

		public static bool isEveryPlayCapable()
		{
			return !isTegra && performanceLevel > 0;
		}

		public static float GetMaxShadowDistance()
		{
			return maxShadowDistance;
		}

		private void Awake()
		{
			if ((bool)mInstance)
			{
				UnityEngine.Object.DestroyImmediate(base.gameObject);
				return;
			}
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			mInstance = this;
			boltObjectPool = new MultiObjectPool(boltPrefabs);
			boltObjectPool.Prealloc(50);
			if ((float)Screen.width / (float)Screen.height >= 2f)
			{
				hasNotch = true;
			}
			string input = SystemInfo.graphicsDeviceName.ToLower() + " ";
			Debug.Log("Screen: " + Screen.width + "x" + Screen.height);
			Debug.Log("DeviceName:      " + SystemInfo.graphicsDeviceName);
			Debug.Log("System Memory:   " + SystemInfo.systemMemorySize);
			Debug.Log("Graphics Memory: " + SystemInfo.graphicsMemorySize);
			Debug.Log("DeviceModel: " + SystemInfo.deviceModel);
			bool flag = Regex.IsMatch(SystemInfo.graphicsDeviceVersion.ToLower(), "opengl[\\s\\|]*es\\s*2");
			isTegra = Regex.IsMatch(input, "\\s*tegra");
			bool flag2 = Regex.IsMatch(input, "\\s*mali-");
			bool flag3 = flag2 && Regex.IsMatch(input, "\\s*mali-4\\d\\d");
			bool flag4 = (isTegra && SystemInfo.supportsShadows) || (flag2 && !flag3);
			bool flag5 = Regex.IsMatch(input, "adreno");
			bool flag6 = !flag && flag5 && Regex.IsMatch(input, "\\D30\\d\\D");
			bool flag7 = !flag && flag5 && Regex.IsMatch(input, "\\D320\\D");
			bool flag8 = !flag && flag5 && Regex.IsMatch(input, "\\D330\\D");
			bool flag9 = flag5 && Regex.IsMatch(input, "\\D4\\d\\d\\D");
			int qualityLevel = QualitySettings.GetQualityLevel();
			Debug.Log("Preset PerformanceLevel: " + qualityLevel);
			if (qualityLevel == 4)
			{
				performanceLevel = 4;
				Application.targetFrameRate = 60;
			}
			else if (qualityLevel == 3)
			{
				performanceLevel = 3;
				PlayerPrefs.SetInt("UnityGraphicsQuality", 3);
				PlayerPrefs.Save();
				Application.targetFrameRate = 60;
			}
			else if (qualityLevel == 2 || flag4)
			{
				if (qualityLevel == 0)
				{
					QualitySettings.SetQualityLevel(1, false);
					PlayerPrefs.Save();
				}
				performanceLevel = 2;
				PlayerPrefs.SetInt("UnityGraphicsQuality", 2);
				PlayerPrefs.Save();
				Application.targetFrameRate = 60;
			}
			else if (flag || flag8 || flag7 || flag6)
			{
				performanceLevel = 0;
				QualitySettings.SetQualityLevel(0, false);
				PlayerPrefs.Save();
				Application.targetFrameRate = ((!flag8) ? 30 : 60);
			}
			else
			{
				performanceLevel = 1;
				QualitySettings.SetQualityLevel(1, false);
				PlayerPrefs.Save();
				Application.targetFrameRate = 60;
			}
			Debug.Log("Selected PerformanceLevel: " + performanceLevel);
			GameObject gameObject = GameObject.Find("Inventory");
			if ((bool)gameObject)
			{
				gameObject.transform.parent = base.transform;
				mInventory = gameObject.GetComponent<Inventory>();
			}
			else
			{
				gameObject = UnityEngine.Object.Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity) as GameObject;
				gameObject.transform.parent = base.transform;
				gameObject.name = inventoryPrefab.name;
				mInventory = gameObject.GetComponent<Inventory>();
			}
			mStateManager = GetComponent<StateManager>();
			mCameraManager = GetComponent<CameraManager>();
			mAudioManager = GetComponent<AudioManager>();
			mPlayerState = GetComponent<PlayerState>();
			mLevel = base.gameObject.AddComponent<Level>();
			mParticleManager = GetComponent<ParticleManager>();
			mEveryplayHelper = new EveryplayHelper();
			mEveryplayHelper.Init();
			mChallenges = GetComponent<Challenges>();
			mOnlineConfig = base.gameObject.AddComponent<OnlineConfig>();
			mOnlineConfig.TryReadConfig();
			mTextureFetcher = base.gameObject.AddComponent<TextureFetcher>();
			mSteamworks = base.transform.Find("Steamworks").gameObject.GetComponent<Steamworks>();
			foreach (GameObject instantiatePrefab in instantiatePrefabs)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(instantiatePrefab, Vector3.zero, Quaternion.identity) as GameObject;
				gameObject2.transform.parent = base.transform;
				gameObject2.name = instantiatePrefab.name;
			}
			FacebookWrapper.Init();
			FacebookInitComplete(FacebookWrapper.IsLoggedIn());
			LoadSelectFace();
			LoadVehicleLogo();
			setupSeasonalEffects(DateTime.UtcNow);
			Prefs.SetString("versionCode", UIManager.version);
		}

		public void setupSeasonalEffects(DateTime now)
		{
		}

		private void OnApplicationFocus(bool focus)
		{
			Debug.Log("DismountGame.OnApplicationFocus(" + focus + ")");
			if (focus)
			{
				reloadTextures = true;
			}
		}

		private IEnumerator OnApplicationPause(bool pause)
		{
			yield return null;
			Debug.Log("DismountGame.OnApplicationPause(" + pause + ")");
			if (pause)
			{
				Pause();
			}
			if (!pause && (uiManager == null || uiManager.CurrentState != UIManager.State.Paused))
			{
				Unpause();
			}
			if (mOnlineConfig != null && !pause)
			{
				mOnlineConfig.TryReadConfig();
			}
		}

		private void SetGPGSButtonState(bool s, bool t)
		{
			uiManager.SetGooglePlayButtonState(s ? "Enabled" : ((!t) ? "Disabled" : "Signing in"));
		}

		private void InitializeAdColony()
		{
		}

		private void InitializeVungle()
		{
		}

		public void OnUILoaded()
		{
			mUIManager = GameObject.Find("/DismountGame/GameUI").GetComponent<UIManager>();
			mHUDManager = GameObject.Find("/DismountGame/HUD").GetComponent<HUDManager>();
			mUIManager.SetPlayerName(mPlayerState.playerName);
			adManager = new AdManager(mOnlineConfig.maxVideoAdsPerSession);
			AdManager obj = adManager;
			obj.OnVideoAvailabilityChanged = (AdPlayer.VideoAvailabilityChangedDelegate)Delegate.Combine(obj.OnVideoAvailabilityChanged, new AdPlayer.VideoAvailabilityChangedDelegate(OnVideoAvailabilityChanged));
			AdManager obj2 = adManager;
			obj2.OnVideoFinished = (AdManager.VideoFinishedDelegate)Delegate.Combine(obj2.OnVideoFinished, new AdManager.VideoFinishedDelegate(OnVideoAdFinished));
			int num = 3;
			if (IsLowMemoryDevice())
			{
				num = 1;
			}
			int num2 = 0;
			if (mOnlineConfig.useUnityAd && num2 < num)
			{
				adManager.InitializeUnityAds(mOnlineConfig.unityAdPriority, mOnlineConfig.unityAdPrioritizedVideoCount, "1017936", "1017937");
				num2++;
			}
			inventory.Initialize();
			SXJNI.Instance.SetPersistentSignInCallback((bool status, string id, string name) =>
			{
				SetGPGSButtonState(status, false);
			});
			if (SXJNI.Instance.IsSignedIn())
			{
				SetGPGSButtonState(true, false);
			}
			else if (SXJNI.Instance.IsAutoLoginEnabled())
			{
				GooglePlayClicked = true;
				mUIManager.ShowDialog("Log in to Google Play Game Services?", "OK", "Cancel", (bool x) =>
				{
					if (x)
					{
						SetGPGSButtonState(false, true);
						SXJNI.Instance.SignIn(false, false, (bool status, string id, string name) =>
						{
							SetGPGSButtonState(status, false);
							GooglePlayClicked = false;
						});
					}
					else
					{
						SXJNI.Instance.SignOut(true, false);
						SetGPGSButtonState(false, false);
						GooglePlayClicked = false;
					}
				}, 0f, false);
			}
			else
			{
				SetGPGSButtonState(false, false);
			}
		}

		private void OnApplicationQuit()
		{
		}

		private void OnAppStoreClicked()
		{
			if (Analytics.gua != null)
			{
				Analytics.gua.sendEventHit("Outgoing", "Click", "Main Menu Stair Dismount App Store");
			}
			Application.OpenURL("https://itunes.apple.com/app/stair-dismount-universal/id326469137?mt=8");
		}

		private void OnGooglePlayClicked()
		{
			if (!GooglePlayClicked)
			{
				GooglePlayClicked = true;
				if (SXJNI.Instance.IsSignedIn())
				{
					SXJNI.Instance.SignOut(true, false);
					GooglePlayClicked = false;
				}
				else
				{
					SetGPGSButtonState(false, true);
					SXJNI.Instance.SignIn(false, false, (bool s, string i, string n) =>
					{
						SetGPGSButtonState(s, false);
						GooglePlayClicked = false;
					});
				}
			}
			Input.ResetInputAxes();
		}

		private void OnSteamClicked()
		{
			if (Analytics.gua != null)
			{
				Analytics.gua.sendEventHit("Outgoing", "Click", "Main Menu Stair Dismount Steam");
			}
			Application.OpenURL("http://store.steampowered.com/app/263760/");
		}

		private void OnGreenlightClicked()
		{
			Application.OpenURL("http://store.steampowered.com/app/263760/");
		}

		private void OnNagClicked()
		{
			OnSteamClicked();
		}

		private void GameCenterAuthenticateComplete(bool authenticated)
		{
			if (authenticated && playerState != null && playerState.levelItem != null)
			{
				GameCenter.GetPlayerScore(playerState.levelItem.itemId, PlayerScoreFetchComplete);
			}
		}

		private void OnOpenLeaderboardsClicked()
		{
			if (playerState.levelItem != null && !GooglePlayClicked)
			{
				GooglePlayClicked = true;
				if (SXJNI.Instance.IsSignedIn())
				{
					SXJNI.Instance.ShowLeaderboard(playerState.levelItem.GPGSId, SXJNI.TimeSpan.DAILY, (bool success) =>
					{
						GooglePlayClicked = false;
					});
				}
				else
				{
					SetGPGSButtonState(false, true);
					SXJNI.Instance.SignIn(false, true, (bool signedIn, string i, string n) =>
					{
						SetGPGSButtonState(signedIn, false);
						if (signedIn)
						{
							SXJNI.Instance.ShowLeaderboard(playerState.levelItem.GPGSId, SXJNI.TimeSpan.DAILY, (bool success) =>
							{
								GooglePlayClicked = false;
							});
						}
						else
						{
							GooglePlayClicked = false;
						}
					});
				}
			}
			Input.ResetInputAxes();
		}

		private void OnOpenAchievementsClicked()
		{
			if (!GooglePlayClicked)
			{
				GooglePlayClicked = true;
				if (SXJNI.Instance.IsSignedIn())
				{
					SXJNI.Instance.ShowAchievements((bool success) =>
					{
						GooglePlayClicked = false;
					});
				}
				else
				{
					SetGPGSButtonState(false, true);
					SXJNI.Instance.SignIn(false, true, (bool signedIn, string i, string n) =>
					{
						SetGPGSButtonState(signedIn, false);
						if (signedIn)
						{
							SXJNI.Instance.ShowAchievements((bool success) =>
							{
								GooglePlayClicked = false;
							});
						}
						else
						{
							GooglePlayClicked = false;
						}
					});
				}
			}
			Input.ResetInputAxes();
		}

		private void FacebookInitComplete(bool isLoggedIn)
		{
			string friendFaceId = playerState.friendFaceId;
			if (friendFaceId != string.Empty)
			{
				FacebookWrapper.GetUserPicture(friendFaceId, FaceReady);
			}
		}

		private void Start()
		{
			playerState.currentVehicleItemId = "vehicle.splitvan";
			playerState.currentVehicleName = "MilkVan_split";
			playerState.currentCharacterItemId = "character.mrdismount";
			playerState.currentCharacterName = "MrDismount";
			playerState.currentCharacterStartPosition = 0;
			stateManager.ChangeState(StateManager.State.Splash);
		}

		private void UpdateUIPlatform(bool forceUpdate = false)
		{
			int num = (SXInputManager.IsControllerConnected() ? 1 : 0);
			if (num != controllerConnected || forceUpdate)
			{
				controllerConnected = num;
				if (controllerConnected == 1 && uiManager.CurrentPlatform != "controller")
				{
					uiManager.SwitchPlatform("controller");
				}
				else if (controllerConnected == 0 && uiManager.CurrentPlatform != "tablet")
				{
					uiManager.SwitchPlatform("tablet");
				}
				uiManager.UpdateControllerButtonVisibility(controllerConnected == 1);
			}
		}

		private void Update()
		{
			if (uiManager == null)
			{
				return;
			}
			if (!touchscreenEnabled)
			{
				touchscreenEnabled = Input.touchCount > 0;
			}
			UIManager.State currentState = uiManager.CurrentState;
			if (((uiManager.CurrentState == UIManager.State.Replay || uiManager.CurrentState == UIManager.State.ReplayWithLeaderboard) && SXInputManager.GetButtonDown(SXInputManager.Button.DPADD)) || (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.H)))
			{
				uiManager.renderingEnabled = !uiManager.renderingEnabled;
				hudManager.renderingEnabled = !hudManager.renderingEnabled;
			}
			if (cameraManager.GetActiveCameraId() != "FreeFly" && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) && (currentState == UIManager.State.DismountActive || currentState == UIManager.State.DismountStarting || currentState == UIManager.State.DismountStartingUnpublished || currentState == UIManager.State.DismountRevingEngine || currentState == UIManager.State.Replay || currentState == UIManager.State.ReplayWithLeaderboard))
			{
				cameraManager.ActivateFreeFlyCamera();
			}
			bool flag = SXInputManager.GetButtonDown(SXInputManager.Button.BACK) || Input.GetKeyDown(KeyCode.Escape);
			switch (currentState)
			{
			case UIManager.State.Dialog:
				if (SXInputManager.GetButtonDown(SXInputManager.Button.A))
				{
					uiManager.OKButtonClicked();
				}
				else if (SXInputManager.GetButtonDown(SXInputManager.Button.B))
				{
					uiManager.CancelButtonClicked();
				}
				break;
			case UIManager.State.Credits:
				if (flag || SXInputManager.GetButtonDown(SXInputManager.Button.B) || SXInputManager.GetMouseButtonDown(0))
				{
					uiManager.ChangeState(UIManager.State.MainMenu);
				}
				flag = false;
				break;
			case UIManager.State.SetupScene:
				if (SXInputManager.GetButtonDown(SXInputManager.Button.B) || SXInputManager.GetButtonDown(SXInputManager.Button.START))
				{
					flag = true;
				}
				if (SXInputManager.GetButtonDown(SXInputManager.Button.DPADL) || SXInputManager.GetButtonDown(SXInputManager.Button.DPADU))
				{
					level.SelectNextObstacleHotspot(true);
				}
				if (SXInputManager.GetButtonDown(SXInputManager.Button.DPADR) || SXInputManager.GetButtonDown(SXInputManager.Button.DPADD))
				{
					level.SelectNextObstacleHotspot(false);
				}
				if (SXInputManager.GetButtonDown(SXInputManager.Button.A))
				{
					level.OpenSelectedObstacleHotspot();
				}
				if (SXInputManager.GetButtonDown(SXInputManager.Button.X))
				{
					OnClearObstacles();
				}
				if (SXInputManager.IsControllerConnected() && SXInputManager.controllerEnabled)
				{
					level.HighlightSelectedObstacleHotspot();
				}
				break;
			case UIManager.State.LevelIntro:
				if (SXInputManager.GetButtonDown(SXInputManager.Button.A) || SXInputManager.GetButtonDown(SXInputManager.Button.B) || SXInputManager.GetButtonDown(SXInputManager.Button.START) || Input.GetKeyDown(KeyCode.Space))
				{
					flag = true;
				}
				break;
			case UIManager.State.LevelSelect:
				if (SXInputManager.GetButtonDown(SXInputManager.Button.Y))
				{
					OnRestorePurchases();
				}
				else if (SXInputManager.GetButtonDown(SXInputManager.Button.B) || SXInputManager.GetButtonDown(SXInputManager.Button.START))
				{
					flag = true;
				}
				break;
			case UIManager.State.DismountStarting:
			case UIManager.State.DismountStartingUnpublished:
			case UIManager.State.DismountRevingEngine:
			case UIManager.State.DismountActive:
			case UIManager.State.DismountEnded:
			{
				if (currentState == UIManager.State.DismountStarting || currentState == UIManager.State.DismountStartingUnpublished)
				{
					if (SXInputManager.GetButtonDown(SXInputManager.Button.B))
					{
						level.CycleSteerPath();
					}
					if (SXInputManager.GetButtonDown(SXInputManager.Button.X))
					{
						BroadcastMessage("OnSwitchCharacterStartPosition");
					}
					if (SXInputManager.GetButtonDown(SXInputManager.Button.START))
					{
						BroadcastMessage("OnLevelSelect");
					}
					else if (SXInputManager.GetButtonDown(SXInputManager.Button.DPADU))
					{
						BroadcastMessage("OnVehicleSelect");
					}
					else if (SXInputManager.GetButtonDown(SXInputManager.Button.DPADL))
					{
						BroadcastMessage("OnCharacterSelect");
					}
					else if (SXInputManager.GetButtonDown(SXInputManager.Button.DPADR))
					{
						OnSetupScene();
					}
				}
				bool flag2 = SXInputManager.GetButtonDown(SXInputManager.Button.A) || Input.GetKeyDown(KeyCode.Space);
				bool flag3 = SXInputManager.GetButtonUp(SXInputManager.Button.A) || Input.GetKeyUp(KeyCode.Space);
				if (flag2 || SXInputManager.GetButtonDown(SXInputManager.Button.R2))
				{
					if (currentState == UIManager.State.DismountStarting || currentState == UIManager.State.DismountStartingUnpublished || currentState == UIManager.State.DismountRevingEngine)
					{
						BroadcastMessage("OnDismountPressed");
					}
					else if (flag2)
					{
						instance.OnResetPressed();
					}
				}
				if (flag3 || (!playerState.manualControls && SXInputManager.GetButtonUp(SXInputManager.Button.R2)))
				{
					if (currentState == UIManager.State.DismountStarting || currentState == UIManager.State.DismountStartingUnpublished || currentState == UIManager.State.DismountRevingEngine)
					{
						BroadcastMessage("OnDismountReleased");
					}
					else if (flag3)
					{
						instance.OnResetReleased();
					}
				}
				break;
			}
			case UIManager.State.Replay:
			case UIManager.State.ReplayWithLeaderboard:
				if (SXInputManager.GetButtonDown(SXInputManager.Button.A) || Input.GetKeyDown(KeyCode.Space))
				{
					instance.OnResetPressed();
				}
				if (SXInputManager.GetButtonUp(SXInputManager.Button.A) || Input.GetKeyUp(KeyCode.Space))
				{
					instance.OnResetReleased();
				}
				if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus) || SXInputManager.GetButtonDown(SXInputManager.Button.R1))
				{
					BroadcastMessage("SpeedUpReplay");
				}
				if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) || SXInputManager.GetButtonDown(SXInputManager.Button.L1))
				{
					BroadcastMessage("SlowDownReplay");
				}
				if (currentState == UIManager.State.ReplayWithLeaderboard)
				{
					if (SXInputManager.GetButtonDown(SXInputManager.Button.B))
					{
						OnCloseLeaderboard();
					}
					else if (SXInputManager.GetButtonDown(SXInputManager.Button.X))
					{
						BroadcastMessage("OnSwitchLeaderboard");
					}
				}
				else if (SXInputManager.GetButtonDown(SXInputManager.Button.X))
				{
					OnShowLeaderboard();
				}
				break;
			case UIManager.State.SetupObstacle:
			case UIManager.State.SetupVehicle:
			case UIManager.State.SetupCharacter:
			case UIManager.State.CustomizeCharacter:
			case UIManager.State.SelectFacebookFriend:
			case UIManager.State.Paused:
			case UIManager.State.VideoOptions:
			case UIManager.State.MainMenuOptions:
				if (SXInputManager.GetButtonDown(SXInputManager.Button.B))
				{
					flag = true;
				}
				if (currentState == UIManager.State.SetupCharacter && SXInputManager.GetButtonDown(SXInputManager.Button.X))
				{
					uiManager.ChangeState(UIManager.State.SetupHead);
				}
				break;
			case UIManager.State.NewsPoster:
			case UIManager.State.NewContentPoster:
				if (SXInputManager.GetButtonDown(SXInputManager.Button.A) || SXInputManager.GetButtonDown(SXInputManager.Button.B))
				{
					stateManager.currentStateObject.SendMessage("OnGotoPreviousUIState");
				}
				break;
			}
			if (flag)
			{
				stateManager.currentStateObject.SendMessage("OnGotoPreviousUIState", SendMessageOptions.DontRequireReceiver);
			}
			if (onlineConfig.showSalesStateChanged)
			{
				PopulateItemDialogs();
				onlineConfig.showSalesStateChanged = false;
			}
			UpdateUIPlatform();
			if (Input.GetKeyDown(KeyCode.P))
			{
				ScreenCapture.CaptureScreenshot("TDM_ScreenShot-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
			}
			if (Input.GetKeyDown(KeyCode.G))
			{
				OnGIFRecordClicked();
			}
			if ((SXInputManager.GetButtonDown(SXInputManager.Button.Y) || SXInputManager.GetMouseButtonDown(2)) && (currentState == UIManager.State.DismountActive || currentState == UIManager.State.DismountStarting || currentState == UIManager.State.DismountStartingUnpublished || currentState == UIManager.State.DismountRevingEngine || currentState == UIManager.State.Replay || currentState == UIManager.State.ReplayWithLeaderboard))
			{
				OnSwitchCamera();
			}
			if (resetFillTime >= 0f)
			{
				float time = Time.time - resetFillTime;
				float num = Utils.ResetBarFillFunction(time, 1f);
				uiManager.SetDismountPowerBar(num);
				if (num > 0.995f)
				{
					resetFillTime = -1f;
					OnReset();
				}
			}
			if (Steamworks.getSubscribedContentInProgress)
			{
				uiManager.SetBusyLabel("Updating subscriptions..." + Steamworks.GetSubscribedContentProgress() + "%");
			}
			if (firstUpdate)
			{
				RandomizeVideoAdVehiclePrize();
				RandomizeVideoAdLevelPrize();
				RandomizeVideoAdCharacterPrize();
			}
			if (switchVehicleOnNextUpdate)
			{
				stateManager.currentStateObject.SendMessage("SelectVehicleOverride", vehicleAfterVideoAd);
				playerState.videoPrizeVehicleUseCount = 3;
				playerState.videoPrizeVehicleItem = vehicleAfterVideoAd;
				switchVehicleOnNextUpdate = false;
				hudManager.UpdateTrialsLeftLabels();
			}
			if (switchLevelOnNextUpdate)
			{
				stateManager.currentStateObject.SendMessage("SelectLevelOverride", levelAfterVideoAd);
				playerState.videoPrizeLevelUseCount = 3;
				playerState.videoPrizeLevelItem = levelAfterVideoAd;
				switchLevelOnNextUpdate = false;
				hudManager.UpdateTrialsLeftLabels();
			}
			if (switchCharacterOnNextUpdate)
			{
				stateManager.currentStateObject.SendMessage("OnSelectCharacter", characterAfterVideoAd);
				switchCharacterOnNextUpdate = false;
				hudManager.UpdateTrialsLeftLabels();
			}
			uiManager.updatePremiumBanner();
			if (reloadTextures)
			{
				LoadSelectFace();
				LoadVehicleLogo();
				reloadTextures = false;
			}
			if (adManager != null)
			{
				adManager.Update();
			}
			firstUpdate = false;
		}

		private string GetGifPath()
		{
			return Application.persistentDataPath;
		}

		public bool CaptureGif()
		{
			if (!CaptureTheGIF.running)
			{
				int num = 640;
				int num2 = (int)((float)Screen.height / (float)Screen.width * (float)num);
				if (num2 > 320)
				{
					num2 = 320;
					num = (int)((float)Screen.width / (float)Screen.height * (float)num2);
				}
				string text = "TDM_Anim-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
				CaptureTheGIF.Instance.Capture(50, num, num2, 10f, GetGifPath(), text);
				return true;
			}
			return false;
		}

		public Camera[] GetGifCameras()
		{
			List<Camera> list = new List<Camera>();
			list.Add(Camera.main);
			Transform transform = base.transform.Find("GameUI/VideoDecorationCamera");
			if ((bool)transform)
			{
				list.Add(transform.GetComponent<Camera>());
			}
			return list.ToArray();
		}

		private void FilePublishedDialogComplete(bool ok)
		{
			if (ok)
			{
				Steamworks.OpenPublishedItemWorkshopPage();
			}
			uiManager.ChangeState(statebeforeSubmit);
		}

		private void FilePublishedDialogFailedComplete(bool ok)
		{
			if (ok)
			{
				OnSubmit();
			}
		}

		private void FilePublished(bool success)
		{
			if (success)
			{
				uiManager.ChangeState(statebeforeSubmit);
				uiManager.ShowDialog("Level published successfully!", "Workshop", "OK", FilePublishedDialogComplete, -1f);
			}
			else
			{
				uiManager.ChangeState(statebeforeSubmit);
				uiManager.ShowDialog("Publishing failed!", "OK", "Try Again", FilePublishedDialogComplete, -1f);
			}
		}

		private void UpdateUserPublishedItemsComplete(bool success)
		{
			if (success)
			{
				uiManager.SetBusyLabel("Publishing...");
				Steamworks.PublishFile(publishFileName, previewImage, 263760, uiManager.GetSubmitItemTitle(), uiManager.GetSubmitItemDescription(), FilePublished);
			}
			else
			{
				uiManager.ChangeState(statebeforeSubmit);
				uiManager.ShowDialog("Publishing failed!", "OK", "Try Again", FilePublishedDialogComplete, -1f);
			}
		}

		private void SelectPreviewLevelComplete(string fileName)
		{
		}

		private void SelectLocalCustomLevelComplete(string filename)
		{
			if (!string.IsNullOrEmpty(filename))
			{
				instance.LoadCustomLevel(filename, true);
			}
		}

		private void OnLoadCustomLevel()
		{
		}

		private void OnShowSubmitDialog()
		{
			statebeforeSubmit = uiManager.CurrentState;
			uiManager.SetSubmitItemTitle(level.customLevelName);
			uiManager.ChangeState(UIManager.State.SubmitLevel);
		}

		private void OnPreviewImageSelected(string filename)
		{
			previewImage = filename;
			uiManager.SetSubmitPreviewImage(filename);
		}

		private void OnSelectLevelImage()
		{
		}

		private void OnSubmit()
		{
			SelectPreviewLevelComplete(playerState.customLevelFilename);
		}

		private void OnCancelSubmit()
		{
			uiManager.ChangeState(statebeforeSubmit);
		}

		private void OnReloadLevelClicked()
		{
			OnReset();
		}

		public void OnResetPressed()
		{
			resetFillTime = Time.time;
		}

		public void OnResetReleased()
		{
			uiManager.SetDismountPowerBar(-1f);
			resetFillTime = -1f;
		}

		private void ControllerButtonPressed()
		{
			bool flag = Prefs.GetInt("controllerEnabled", 1) != 0;
			flag = !flag;
			uiManager.SetControllerButtonState(flag);
			SXInputManager.controllerEnabled = flag;
			Prefs.SetInt("controllerEnabled", flag ? 1 : 0);
			Prefs.Save();
			UpdateUIPlatform(true);
		}

		private void MusicButtonPressed()
		{
			bool flag = Prefs.GetInt("musicEnabled", 1) != 0;
			flag = !flag;
			audioManager.musicEnabled = flag;
			uiManager.SetMusicButtonState(flag);
			Prefs.SetInt("musicEnabled", flag ? 1 : 0);
			Prefs.Save();
		}

		private void SFXButtonPressed()
		{
			bool flag = Prefs.GetInt("sfxEnabled", 1) != 0;
			flag = !flag;
			audioManager.sfxEnabled = flag;
			uiManager.SetSFXButtonState(flag);
			Prefs.SetInt("sfxEnabled", flag ? 1 : 0);
			Prefs.Save();
		}

		private void OnSfxVolumeChange(float volume)
		{
			audioManager.sfxVolume = volume;
			Prefs.SetInt("sfxVolume", Mathf.RoundToInt(10f * volume));
			Prefs.Save();
		}

		private void OnMusicVolumeChange(float volume)
		{
			audioManager.musicVolume = volume;
			Prefs.SetInt("musicVolume", Mathf.RoundToInt(10f * volume));
			Prefs.Save();
		}

		private void OnReset()
		{
			if (stateManager.currentState == StateManager.State.Dismount)
			{
				stateManager.currentStateObject.GetComponent<Dismount.GameStates.Dismount>().CancelCountDown();
			}
			playerState.SaveCamera();
			uiManager.CloseDoor(() =>
			{
				playerState.currentVehicleInstance.GetComponent<Vehicle>().RecycleSkidmarks();
				particleManager.Reset();
				hudManager.Reset();
				uiManager.ChangeState(UIManager.State.Inactive);
				Replay.Instance.StopRecordingDontPostprocess();
				level.ReActivateRecordedObjects();
				level.OnDismountReset();
				playerState.currentVehicleInstance.SetActive(false);
				playerState.currentCharacterInstance.SetActive(false);
				playerState.stateAfterLevelLoad = StateManager.State.Dismount;
				playerState.setSavedCameraOrientation = true;
				playerState.statistics.Save();
				stateManager.ChangeState(StateManager.State.LevelLoader);
			});
			instance.ShowReminder(0);
		}

		public void OnReady()
		{
			playerState.setSavedCameraOrientation = true;
			stateManager.ChangeState(StateManager.State.Dismount);
		}

		public void OnClearObstacles()
		{
			level.ClearObstacles();
		}

		private void OnSelectSteerPath()
		{
			level.CycleSteerPath();
		}

		private void OnSetupScene()
		{
			playerState.SaveCamera();
			playerState.currentVehicleInstance.GetComponent<Vehicle>().RecycleSkidmarks();
			particleManager.Reset();
			hudManager.Reset();
			uiManager.ChangeState(UIManager.State.Inactive);
			Replay.Instance.StopRecordingDontPostprocess();
			stateManager.ChangeState(StateManager.State.SetupScene);
		}

		private void OnOpenVideoOptions()
		{
			uiManager.ChangeState(UIManager.State.VideoOptions);
		}

		private void OnSwitchCamera()
		{
			cameraManager.CycleActiveCamera(0.25f);
		}

		private void OnSwitchStats()
		{
			playerState.showStats = !playerState.showStats;
			hudManager.EnableStats(playerState.showStats);
		}

		private void OnPlayGame()
		{
			stateManager.ChangeState(StateManager.State.Dismount);
		}

		private void OnSelectLevel(GameItem item)
		{
			BroadcastMessage("DoSelectLevel", item);
		}

		private void OnCredits()
		{
			uiManager.ChangeState(UIManager.State.Credits);
		}

		private void OnShowLeaderboard()
		{
			if (uiManager.CurrentState == UIManager.State.Replay)
			{
				uiManager.ChangeState(UIManager.State.ReplayWithLeaderboard);
			}
			Prefs.SetInt("showLeaderboard", 1);
			Prefs.Save();
		}

		private void OnCloseLeaderboard()
		{
			if (uiManager.CurrentState == UIManager.State.ReplayWithLeaderboard)
			{
				uiManager.ChangeState(UIManager.State.Replay);
			}
			Prefs.SetInt("showLeaderboard", 0);
			Prefs.Save();
		}

		public void Pause()
		{
			if (!isPaused)
			{
				isPaused = true;
				previousTimeScale = Time.timeScale;
				Time.timeScale = 0f;
				if ((bool)stateManager && stateManager.currentState == StateManager.State.ReplayMode && (bool)stateManager.currentStateObject)
				{
					stateManager.currentStateObject.SendMessage("OnPause", SendMessageOptions.DontRequireReceiver);
				}
				if ((bool)audioManager)
				{
					audioManager.PauseEffects();
				}
				if ((bool)hudManager)
				{
					hudManager.Disable();
				}
				if ((bool)cameraManager)
				{
					cameraManager.EnableBlur(true);
				}
			}
		}

		public void Unpause()
		{
			if (isPaused)
			{
				isPaused = false;
				if ((bool)audioManager)
				{
					audioManager.ResumeEffects();
				}
				if ((bool)stateManager && (bool)hudManager && stateManager.currentState != StateManager.State.MainMenu && stateManager.currentState != StateManager.State.LevelIntro && stateManager.currentState != StateManager.State.SetupScene)
				{
					hudManager.Enable();
				}
				if ((bool)cameraManager)
				{
					cameraManager.EnableBlur(false);
				}
				Time.timeScale = previousTimeScale;
				if ((bool)uiManager && uiManager.CurrentState == UIManager.State.DismountActive)
				{
					stateManager.currentStateObject.SendMessage("OnGotoPreviousUIState");
				}
			}
		}

		public void OnPause()
		{
			stateManager.currentStateObject.SendMessage("OnGotoPreviousUIState");
		}

		private void OnBackToMainMenu()
		{
			Time.timeScale = previousTimeScale;
			uiManager.CloseDoor(() =>
			{
				playerState.currentVehicleInstance.GetComponent<Vehicle>().RecycleSkidmarks();
				isPaused = false;
				particleManager.Reset();
				hudManager.Reset();
				uiManager.ChangeState(UIManager.State.Inactive);
				Replay.Instance.StopRecordingDontPostprocess();
				cameraManager.EnableBlur(false);
				level.ReActivateRecordedObjects();
				playerState.currentVehicleInstance.SetActive(false);
				playerState.currentCharacterInstance.SetActive(false);
				playerState.stateAfterLevelLoad = StateManager.State.MainMenu;
				stateManager.ChangeState(StateManager.State.LevelLoader);
				playerState.statistics.Save();
			});
		}

		public bool OnLevelSelected(GameItem levelItem, bool alwaysActivate)
		{
			playerState.setItemHasBeenUsed(levelItem.referenceName);
			if (levelItem == playerState.levelItem)
			{
				if (alwaysActivate)
				{
					OnPlayGame();
					return true;
				}
				return false;
			}
			Unpause();
			uiManager.CloseDoor(() =>
			{
				playerState.currentVehicleInstance.GetComponent<Vehicle>().RecycleSkidmarks();
				audioManager.ChangeMusic();
				level.NextLevel(levelItem.referenceName, false, false);
				playerState.stateAfterLevelLoad = StateManager.State.LevelIntro;
				hudManager.Reset();
				stateManager.ChangeState(StateManager.State.LevelLoader);
			});
			return true;
		}

		public bool OnLevelSelected(GameItem levelItem)
		{
			return OnLevelSelected(levelItem, false);
		}

		public bool LoadCustomLevel(string filename, bool unpublished)
		{
			Unpause();
			uiManager.CloseDoor(() =>
			{
				playerState.currentVehicleInstance.GetComponent<Vehicle>().RecycleSkidmarks();
				audioManager.ChangeMusic();
				level.NextLevel(filename, true, unpublished);
				playerState.stateAfterLevelLoad = StateManager.State.LevelIntro;
				hudManager.Reset();
				stateManager.ChangeState(StateManager.State.LevelLoader);
			});
			return true;
		}

		private void GetTopscoresComplete(List<Leaderboards.LeaderboardEntry> entries, int playerRank)
		{
			uiManager.PopulateLeaderboard(entries, playerRank - 1);
			UpdateWebPlayerLeaderboard(entries, playerRank);
		}

		public void RefreshLeaderboard(float delay)
		{
			CancelInvoke("DoRefreshLeaderboard");
			Invoke("DoRefreshLeaderboard", delay);
		}

		public void RefreshLeaderboard()
		{
			DoRefreshLeaderboard();
		}

		private void GetHiscoreComplete(int playerScore, int topScore)
		{
			playerState.currentHighScore = playerScore;
			uiManager.SetLevelNameAndHighscore(playerState.levelItem.itemName, playerScore);
		}

		private void DoRefreshLeaderboard()
		{
			previousLeaderboardRefreshTick = Time.time;
		}

		private void PlayerScoreFetchComplete(string leaderboard, long score)
		{
			if ((leaderboard.CompareTo(playerState.levelItem.itemId) == 0 || leaderboard.CompareTo(playerState.levelItem.itemName) == 0 || leaderboard.CompareTo(playerState.levelItem.GPGSId) == 0) && score != -1)
			{
				playerState.currentHighScore = (int)score;
				playerState.SaveHighscore();
				if (playerState.levelItem != null)
				{
					uiManager.SetLevelNameAndHighscore(playerState.levelItem.itemName, playerState.currentHighScore);
				}
			}
		}

		private IEnumerator Quitter()
		{
			yield return waitForEndOfFrame;
			Application.Quit();
		}

		private void OnQuitGame()
		{
			StartCoroutine(Quitter());
		}

		private void OnNextResolution()
		{
			uiManager.NextResolution();
		}

		private void OnPreviousResolution()
		{
			uiManager.PreviousResolution();
		}

		private void OnNextUnitMode()
		{
			uiManager.ToggleUnitMode();
		}

		private void OnPreviousUnitMode()
		{
			uiManager.ToggleUnitMode();
		}

		private void OnShowWorkshopLicense()
		{
			Steamworks.OpenURL("http://steamcommunity.com/sharedfiles/workshoplegalagreement");
		}

		private void ResolutionConfirmed(bool answer)
		{
			if (answer)
			{
				Resolution currentResolution = uiManager.GetCurrentResolution();
				bool flag = uiManager.IsFullScreenChecked();
				Prefs.SetInt("ScreenWidth", currentResolution.width);
				Prefs.SetInt("ScreenHeight", currentResolution.height);
				Prefs.SetString("FullScreen", (!flag) ? "false" : "true");
			}
			else
			{
				Screen.SetResolution(Prefs.GetInt("ScreenWidth"), Prefs.GetInt("ScreenHeight"), Prefs.GetString("FullScreen") == "true");
				uiManager.SyncVideoOptions();
			}
		}

		private void OnApplyResolution()
		{
			Resolution currentResolution = uiManager.GetCurrentResolution();
			bool flag = uiManager.IsFullScreenChecked();
			if (Screen.width != currentResolution.width || Screen.height != currentResolution.height || Screen.fullScreen != flag)
			{
				Screen.SetResolution(currentResolution.width, currentResolution.height, flag);
				uiManager.ShowDialog("Keep the current resolution?\n(The settings will revert in 10 seconds)", "Keep", "Cancel", ResolutionConfirmed, 10f);
			}
		}

		private IEnumerator hackAudioListenerToWork()
		{
			Transform listenerTransform = base.transform.Find("MainListener");
			if ((bool)listenerTransform)
			{
				AudioListener listener = listenerTransform.GetComponent<AudioListener>();
				listener.enabled = false;
				yield return 0;
				listener.enabled = true;
			}
		}

		private void OpenWorkshopLevelBrowser()
		{
			Steamworks.OpenURL("http://steamcommunity.com/workshop/browse/?appid=263760");
		}

		public void SteamOverlayActivated(bool active)
		{
			if (!active && uiManager.CurrentState == UIManager.State.LevelSelect)
			{
				HandleSubscriptions();
			}
		}

		public void OnLevelLoaded()
		{
			StartCoroutine("hackAudioListenerToWork");
			boltObjectPool.Prealloc(50);
			GameItem levelItem = playerState.levelItem;
			playerState.levelItem = inventory.GetItemByReferenceName(playerState.currentLevel);
			particleManager.Reset();
			audioManager.Reset();
			audioManager.SFXFadeIn();
			RenderSettingsOverride renderSettingsOverride = UnityEngine.Object.FindObjectOfType<RenderSettingsOverride>();
			if ((bool)renderSettingsOverride)
			{
				cameraManager.OverrideSkyboxMaterial = renderSettingsOverride.customSkybox;
			}
			bool flag = false;
			if (stateManager.currentState == StateManager.State.LevelLoader || stateManager.currentState == StateManager.State.Splash)
			{
				cameraManager.FindSceneCameras();
			}
			if (stateManager.currentState == StateManager.State.Dismount)
			{
			}
			if (Time.time - previousLeaderboardRefreshTick > 300f)
			{
				flag = true;
			}
			if ((playerState.levelItem != null && levelItem != playerState.levelItem) ? true : false)
			{
				SXJNI.Instance.GetScore(playerState.levelItem.GPGSId, SXJNI.TimeSpan.ALL, (long score) =>
				{
					PlayerScoreFetchComplete(playerState.levelItem.GPGSId, score);
				});
			}
			if (playerState.levelItem != null)
			{
				playerState.currentLevelName = playerState.levelItem.itemName;
			}
			else
			{
				playerState.currentLevelName = level.customLevelName;
			}
			uiManager.SetLevelNameAndHighscore(playerState.currentLevelName, playerState.currentHighScore);
			stateManager.ChangeState(playerState.stateAfterLevelLoad);
			Transform transform = base.transform.Find("/Loader");
			if ((bool)transform)
			{
				transform.SendMessage("LoadingComplete");
			}
			playerState.ResetDismount();
		}

		public void HandleSubscriptions()
		{
		}

		private List<GameItem> CreateSortedItems(List<GameItem> items)
		{
			List<GameItem> list = new List<GameItem>(items);
			list.Sort((GameItem p1, GameItem p2) => p1.itemName.CompareTo(p2.itemName));
			return list;
		}

		private void getSubscribedContentComplete(Dictionary<string, object> contentInfo)
		{
		}

		public void OnSwitchLevelSet()
		{
			playerState.browsingCustomLevels = !playerState.browsingCustomLevels;
			if (playerState.browsingCustomLevels)
			{
				instance.HandleSubscriptions();
				playerState.customLevelsRefreshed = true;
			}
			else
			{
				UpdateLevelSelection();
			}
			uiManager.UpdateLevelSelectSwitchButtonLabel();
		}

		public void UpdateLevelSelection()
		{
			uiManager.SetItems(UIManager.ItemDialogType.Levels, inventory.GetItemsOfCategory(GameItem.ItemCategory.Level));
		}

		public void PopulateItemDialogs()
		{
			uiManager.SetItems(UIManager.ItemDialogType.Obstacles, inventory.GetItemsOfCategory(GameItem.ItemCategory.Obstacle));
			uiManager.SetItems(UIManager.ItemDialogType.Vehicles, inventory.GetItemsOfCategory(GameItem.ItemCategory.Vehicle));
			uiManager.SetItems(UIManager.ItemDialogType.Characters, inventory.GetItemsOfCategory(GameItem.ItemCategory.Character));
			uiManager.SetItems(UIManager.ItemDialogType.Heads, inventory.GetItemsOfCategory(GameItem.ItemCategory.Head));
			uiManager.SetItems(UIManager.ItemDialogType.Levels, inventory.GetItemsOfCategory(GameItem.ItemCategory.Level));
		}

		private void InitiatePurchase(GameItem item)
		{
			inventory.PurchaseProduct(item.IAPProductId);
		}

		public void ShowFreeWithAnyPurchasesUnlocked()
		{
			if (Prefs.GetInt("CharacterBundleUnlockedShown", 0) == 0 && (bool)uiManager)
			{
				uiManager.ShowDialog("Thank You!\nThe Dismount Friends character\nbundle and the complimentary\nvehicles have been unlocked", "OK", null, (bool s) =>
				{
				}, 0f, false);
				Prefs.SetInt("CharacterBundleUnlockedShown", 1);
			}
		}

		public void RandomizeVideoAdVehiclePrize()
		{
			List<GameItem> lockedItemsOfCategory = inventory.GetLockedItemsOfCategory(GameItem.ItemCategory.Vehicle);
			if (lockedItemsOfCategory.Count != 0)
			{
				UnityEngine.Random.seed = (int)(Time.realtimeSinceStartup * 1000f * (float)lockedItemsOfCategory.Count);
				GameItem gameItem = vehicleAfterVideoAd;
				GameItem gameItem2 = null;
				int num = UnityEngine.Random.Range(0, lockedItemsOfCategory.Count);
				gameItem2 = ((gameItem != lockedItemsOfCategory[num]) ? lockedItemsOfCategory[num] : ((num > lockedItemsOfCategory.Count - 2) ? lockedItemsOfCategory[num - 1] : lockedItemsOfCategory[num + 1]));
				vehicleAfterVideoAd = gameItem2;
				uiManager.UpdateVideoAdAvailability(IsVideoAdAvailable(), vehicleAfterVideoAd, AdPrizeType.Vehicle);
			}
		}

		public void RandomizeVideoAdLevelPrize()
		{
			List<GameItem> lockedItemsOfCategory = inventory.GetLockedItemsOfCategory(GameItem.ItemCategory.Level);
			if (lockedItemsOfCategory.Count != 0)
			{
				UnityEngine.Random.seed = (int)(Time.realtimeSinceStartup * 1000f * (float)lockedItemsOfCategory.Count);
				GameItem gameItem = levelAfterVideoAd;
				GameItem gameItem2 = null;
				int num = UnityEngine.Random.Range(0, lockedItemsOfCategory.Count);
				gameItem2 = ((gameItem != lockedItemsOfCategory[num]) ? lockedItemsOfCategory[num] : ((num > lockedItemsOfCategory.Count - 2) ? lockedItemsOfCategory[num - 1] : lockedItemsOfCategory[num + 1]));
				levelAfterVideoAd = gameItem2;
				uiManager.UpdateVideoAdAvailability(IsVideoAdAvailable(), levelAfterVideoAd, AdPrizeType.Level);
			}
		}

		public void RandomizeVideoAdCharacterPrize()
		{
			List<GameItem> lockedItemsOfCategory = inventory.GetLockedItemsOfCategory(GameItem.ItemCategory.Character);
			if (lockedItemsOfCategory.Count != 0)
			{
				UnityEngine.Random.seed = (int)(Time.realtimeSinceStartup * 1000f * (float)lockedItemsOfCategory.Count);
				GameItem gameItem = characterAfterVideoAd;
				GameItem gameItem2 = null;
				int num = UnityEngine.Random.Range(0, lockedItemsOfCategory.Count);
				gameItem2 = ((gameItem != lockedItemsOfCategory[num]) ? lockedItemsOfCategory[num] : ((num <= lockedItemsOfCategory.Count - 2) ? lockedItemsOfCategory[num + 1] : ((num == 0) ? lockedItemsOfCategory[lockedItemsOfCategory.Count - 1] : lockedItemsOfCategory[num - 1])));
				characterAfterVideoAd = gameItem2;
				uiManager.UpdateVideoAdAvailability(IsVideoAdAvailable(), characterAfterVideoAd, AdPrizeType.Character);
			}
		}

		private void UpdateAdAvailability()
		{
			bool available = IsVideoAdAvailable();
			uiManager.UpdateVideoAdAvailability(available, vehicleAfterVideoAd, AdPrizeType.Vehicle);
			uiManager.UpdateVideoAdAvailability(available, levelAfterVideoAd, AdPrizeType.Level);
			uiManager.UpdateVideoAdAvailability(available, characterAfterVideoAd, AdPrizeType.Character);
		}

		private void OnVideoAvailabilityChanged(bool available, string zone)
		{
			UpdateAdAvailability();
		}

		public void OnAdColonyVideoAvailabilityChange(bool available, string zone_id)
		{
			Debug.Log("AdColony video availability at zone " + zone_id + ": " + available);
			UpdateAdAvailability();
		}

		private void OnAdColonyVideAdFinished(bool ad_shown)
		{
			mAdColonyVideosViewed++;
			OnVideoAdFinished(ad_shown);
		}

		private void OnVideoAdFinished(bool ad_shown)
		{
			audioManager.PlayMusic();
			audioManager.ResumeEffects();
			if (ad_shown)
			{
				mVideoCount++;
				if (adVideoPrizeType == AdPrizeType.Vehicle)
				{
					switchVehicleOnNextUpdate = true;
				}
				else if (adVideoPrizeType == AdPrizeType.Level)
				{
					switchLevelOnNextUpdate = true;
				}
				else
				{
					switchCharacterOnNextUpdate = true;
				}
				Invoke("RandomizeAdPrizes", 0.5f);
			}
		}

		private void videoComplete()
		{
		}

		private void RandomizeAdPrizes()
		{
			RandomizeVideoAdVehiclePrize();
			RandomizeVideoAdLevelPrize();
			RandomizeVideoAdCharacterPrize();
		}

		private bool IsVideoAdAvailable()
		{
			if (IsTVDevice())
			{
				return false;
			}
			return adManager.IsAdAvailable();
		}

		private void PlayAdColonyVideoAd()
		{
		}

		private void PlayVungleVideoAd()
		{
		}

		private void OnShowVideoAd(AdPrizeType type)
		{
			adVideoPrizeType = type;
			audioManager.PauseMusic();
			audioManager.PauseEffects();
			adManager.ShowAd();
		}

		private void OnShowVideoAdForLevel()
		{
			OnShowVideoAd(AdPrizeType.Level);
		}

		private void OnShowVideoAdForVehicle()
		{
			OnShowVideoAd(AdPrizeType.Vehicle);
		}

		private void OnShowVideoAdForCharacter()
		{
			OnShowVideoAd(AdPrizeType.Character);
		}

		private void OnRestorePurchases()
		{
			inventory.RestorePurchases();
		}

		private void SaveTextureToFile(Texture2D texture, string filename)
		{
		}

		private string GetCustomFacePath()
		{
			return Application.persistentDataPath + "/selected_face.png";
		}

		private string GetCustomLogoPath()
		{
			return Application.persistentDataPath + "/selected_logo.png";
		}

		private void LoadSelectFace()
		{
			string customFacePath = GetCustomFacePath();
			if (!playerState.IsBubbleHeadMode)
			{
				if (File.Exists(customFacePath))
				{
					Utils.LoadImageIntoTexture(customFacePath, overriddenFaceTexture as Texture2D);
					faceMaterial.SetTexture("_MainTex", overriddenFaceTexture);
					if (playerState.currentCharacterInstance != null)
					{
						MrDismount component = playerState.currentCharacterInstance.GetComponent<MrDismount>();
						component.SetNormalHeadMode();
					}
					playerState.achievements.ReportComplete("com.secretexit.turbodismount.InsultToInjury");
				}
				else
				{
					Debug.Log("File not found: " + customFacePath);
				}
			}
			else if (File.Exists(customFacePath))
			{
				Utils.LoadImageIntoTexture(customFacePath, bubbleheadTexture as Texture2D);
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.InsultToInjury");
			}
			else
			{
				Debug.Log("File not found: " + customFacePath);
				playerState.IsBubbleHeadMode = false;
			}
		}

		private void LoadVehicleLogo()
		{
			string customLogoPath = GetCustomLogoPath();
			if (File.Exists(customLogoPath))
			{
				Utils.LoadImageIntoTexture(customLogoPath, vehicleLogoTexture as Texture2D);
				vehicleMaterial.SetTexture("_MainTex", vehicleLogoTexture);
				if (playerState != null && playerState.currentVehicleInstance != null)
				{
					playerState.currentVehicleInstance.GetComponent<Vehicle>().ShowBillboards(true);
				}
				vehicleLogoOverridden = true;
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.InsultToInjury");
			}
			else
			{
				Debug.Log("File not found: " + customLogoPath);
			}
		}

		private Vector2 RestrictSize(Vector2 size, float maxDimension)
		{
			Vector2 result = size;
			if (size.x > 0f && size.y > 0f && (size.x > maxDimension || size.y > maxDimension))
			{
				if (size.x > size.y)
				{
					result.x = maxDimension;
					result.y = size.y / size.x * maxDimension;
				}
				else if (size.y > size.x)
				{
					result.x = size.x / size.y * maxDimension;
					result.y = maxDimension;
				}
				else
				{
					result.x = maxDimension;
					result.y = maxDimension;
				}
			}
			return result;
		}

		private void SelectBubbleheadDialogComplete(string fileName)
		{
		}

		private void OnResetHead()
		{
			faceMaterial.SetTexture("_MainTex", defaultFaceTexture);
			string customFacePath = GetCustomFacePath();
			if (File.Exists(customFacePath))
			{
				try
				{
					File.Delete(customFacePath);
				}
				catch (Exception)
				{
					Debug.Log("Deleting face texture failed");
				}
			}
			playerState.currentCharacterInstance.GetComponent<MrDismount>().SetNormalHeadMode();
			playerState.friendFaceId = string.Empty;
		}

		private void OnResetVehicleLogo()
		{
			vehicleMaterial.SetTexture("_MainTex", null);
			string customLogoPath = GetCustomLogoPath();
			if (File.Exists(customLogoPath))
			{
				try
				{
					File.Delete(customLogoPath);
				}
				catch (Exception)
				{
					Debug.Log("Deleting logo texture failed");
				}
			}
			vehicleLogoOverridden = false;
			playerState.currentVehicleInstance.GetComponent<Vehicle>().ShowBillboards(false);
		}

		private void OnSelectBubblehead()
		{
		}

		private void imageSetterFace(Texture2D texture)
		{
			if (texture != null)
			{
				faceMaterial.SetTexture("_MainTex", texture);
				GC.Collect();
				playerState.currentCharacterInstance.GetComponent<MrDismount>().SetNormalHeadMode();
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.InsultToInjury");
			}
		}

		private void imageSetterLogo(Texture2D texture)
		{
			if (texture != null)
			{
				vehicleLogoOverridden = true;
				vehicleMaterial.SetTexture("_MainTex", texture);
				playerState.currentVehicleInstance.GetComponent<Vehicle>().ShowBillboards(true);
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.InsultToInjury");
			}
		}

		private void PickImage(string path, int w, int h)
		{
			GC.Collect();
			if (!SXJNI.Instance.CheckPermission("android.permission.READ_EXTERNAL_STORAGE"))
			{
				SXJNI.Instance.RequestPermissions(_permissions, (string[] permissions, bool[] grants) =>
				{
					for (int i = 0; i < permissions.Length; i++)
					{
						if (permissions[i].Equals("android.permission.READ_EXTERNAL_STORAGE"))
						{
							if (grants[i])
							{
								SXJNI.Instance.LaunchPicker(path, w, h);
							}
							break;
						}
					}
				});
			}
			else
			{
				SXJNI.Instance.LaunchPicker(path, w, h);
			}
		}

		private void OnSelectLogo()
		{
			imageSelectType = ImageSelectType.VehicleLogo;
			PickImage(GetCustomLogoPath(), 512, 512);
		}

		private void OnSelectFace()
		{
			imageSelectType = ImageSelectType.Face;
			PickImage(GetCustomFacePath(), 512, 512);
		}

		public void OnAchievementComplete(string id)
		{
			Debug.Log("Achievement completed: " + id);
		}

		public void OnChallengeComplete(Challenge challenge)
		{
		}

		public void UpdateWebPlayerLeaderboard(List<Leaderboards.LeaderboardEntry> entries, int playerRank)
		{
			List<string> list = new List<string>();
			int num = -1;
			for (int i = 0; i < entries.Count; i++)
			{
				Leaderboards.LeaderboardEntry leaderboardEntry = entries[i];
				if (leaderboardEntry.score == num)
				{
					list.Add("=");
				}
				else
				{
					list.Add((i + 1).ToString());
				}
				list.Add(leaderboardEntry.name);
				list.Add(Utils.ScoreString(leaderboardEntry.score));
				num = leaderboardEntry.score;
			}
			ExternalUpdateLeaderboard(list.ToArray(), playerRank - 1);
		}

		public void ExternalUpdateLeaderboard(string[] splitLeaderboardEntries, int playerEntryIndex)
		{
			Application.ExternalCall("SetLevelName", playerState.levelItem.itemName);
			Application.ExternalCall("SetLeaderboardEntries", splitLeaderboardEntries, playerEntryIndex);
		}

		public void ShowReminder(int type)
		{
			triggerCount++;
			if (triggerCount == 1 || triggerCount % 3 == 0)
			{
				Application.ExternalCall("ShowReminder", type);
			}
		}

		private void FaceReady(Texture2D texture)
		{
			if ((bool)texture)
			{
				faceMaterial.SetTexture("_MainTex", texture);
			}
		}

		private void OnFaceSelected(GameItem item)
		{
			playerState.friendFaceId = item.itemId;
			FacebookWrapper.GetUserPicture(item.itemId, FaceReady);
			uiManager.ChangeState(UIManager.State.MainMenu);
		}

		public void displayShowVehicleEffect()
		{
			if ((bool)showVechicleEffect)
			{
				UnityEngine.Object.Instantiate(showVechicleEffect, playerState.currentVehicleInstance.transform.position, Quaternion.identity);
			}
		}

		private void OnWatchEveryplayClicked()
		{
			if (Everyplay.IsRecordingSupported())
			{
				everyplayHelper.Show();
			}
			Input.ResetInputAxes();
		}

		private void OnOpenTwitterClicked()
		{
			Application.OpenURL("http://twitter.com/secretexit/");
			Input.ResetInputAxes();
		}

		private void OnSupportClicked()
		{
			Application.OpenURL("http://support.turbodismount.com/android");
			Input.ResetInputAxes();
		}

		public void OnGIFRecordClicked()
		{
			if (CaptureGif() && stateManager.currentState == StateManager.State.ReplayMode)
			{
				stateManager.currentStateObject.SendMessage("GIFRecordCapturing");
			}
		}

		public void OnGIFRecordEncoding()
		{
			if (stateManager.currentState == StateManager.State.ReplayMode)
			{
				stateManager.currentStateObject.SendMessage("GIFRecordEncoding");
			}
		}

		public void OnGIFRecordDone()
		{
			if (stateManager.currentState == StateManager.State.ReplayMode)
			{
				stateManager.currentStateObject.SendMessage("GIFRecordDone");
			}
		}

		public void reviewDialogComplete(bool ok)
		{
			if (ok)
			{
				EtceteraAndroid.openReviewPageInPlayStore();
				Prefs.SetInt("noReviews", 1);
				return;
			}
			int num = Prefs.GetInt("reviewCancelCount", 0);
			Prefs.SetInt("reviewCancelCount", num + 1);
			if (num >= 3)
			{
				Prefs.SetInt("noReviews", 1);
			}
		}

		public bool RequestReview()
		{
			bool result = false;
			if (!launchCountIncreased && !IsLowPerformanceDevice() && Prefs.GetInt("noReviews", 0) != 1)
			{
				int num = Prefs.GetInt("launchCount", 0);
				Prefs.SetInt("launchCount", num + 1);
				DateTime utcNow = DateTime.UtcNow;
				int num2 = utcNow.DayOfYear - Prefs.GetInt("reviewAskedTime", utcNow.DayOfYear);
				if (num2 < 0)
				{
					num2 += 366;
				}
				if (num2 > 2 && num > 3)
				{
					uiManager.ShowDialog("Thanks for playing Turbo Dismount!\nIf you've enjoyed the game, please rate it!", "Sure!", "Later", reviewDialogComplete, 0f, false);
					result = true;
					Prefs.SetInt("launchCount", 0);
					Prefs.SetInt("reviewAskedTime", utcNow.DayOfYear);
				}
				launchCountIncreased = true;
			}
			return result;
		}

		public void EnableNotchUI(bool enable)
		{
			OffsetAnchors(uiManager.UIAnchors, LayerMask.NameToLayer("TDUI"), new Vector2(0.02f, 0f), enable);
			OffsetAnchors(hudManager.UIAnchors, LayerMask.NameToLayer("HUD"), new Vector2(0.025f, 0f), enable);
		}

		private void OffsetAnchors(UIAnchor[] anchors, int layer, Vector2 offset, bool notchMode)
		{
			if (anchors == null)
			{
				return;
			}
			if (!notchMode)
			{
				offset *= -1f;
			}
			foreach (UIAnchor uIAnchor in anchors)
			{
				if (uIAnchor.gameObject.layer == layer)
				{
					if (uIAnchor.side == UIAnchor.Side.BottomLeft || uIAnchor.side == UIAnchor.Side.Left || uIAnchor.side == UIAnchor.Side.TopLeft)
					{
						uIAnchor.relativeOffset += offset;
					}
					else if (uIAnchor.side == UIAnchor.Side.BottomRight || uIAnchor.side == UIAnchor.Side.Right || uIAnchor.side == UIAnchor.Side.TopRight)
					{
						uIAnchor.relativeOffset -= offset;
					}
				}
			}
		}
	}
}
