#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using System.IO;
using Dismount.GameStates;
using UnityEngine;

namespace Dismount
{
	public class UIManager : MonoBehaviour
	{
		public enum State
		{
			Inactive = 0,
			MainMenu = 1,
			LevelSelect = 2,
			LevelIntro = 3,
			LevelIntroUnpublished = 4,
			AcceptNemesis = 5,
			SetupScene = 6,
			SetupObstacle = 7,
			SetupVehicle = 8,
			SetupCharacter = 9,
			SetupHead = 10,
			CustomizeCharacter = 11,
			SelectFacebookFriend = 12,
			DismountStarting = 13,
			DismountStartingUnpublished = 14,
			DismountRevingEngine = 15,
			DismountActive = 16,
			DismountEnded = 17,
			Replay = 18,
			ReplayWithLeaderboard = 19,
			Paused = 20,
			VideoOptions = 21,
			MainMenuOptions = 22,
			Dialog = 23,
			NameEntry = 24,
			Credits = 25,
			SubmitLevel = 26,
			Busy = 27,
			WebNag = 28,
			NewsPoster = 29,
			NewContentPoster = 30
		}

		private class StateWidgets
		{
			public string[] activeWidgets;

			public StateWidgets(params string[] activeWidgets)
			{
				this.activeWidgets = activeWidgets;
			}

			public bool IsInList(string widget)
			{
				string[] array = activeWidgets;
				foreach (string text in array)
				{
					if (text.CompareTo(widget) == 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public enum ItemDialogType
		{
			Obstacles = 0,
			Vehicles = 1,
			Characters = 2,
			Levels = 3,
			Faces = 4,
			Heads = 5,
			NumDialogTypes = 6
		}

		private struct ButtonKey
		{
			public SXButtonKeys m_uibk;

			public int m_x;

			public ButtonKey(SXButtonKeys uibk, int x, bool buttonUp)
			{
				uibk.selectWithButtonUp = buttonUp;
				m_uibk = uibk;
				m_x = x;
			}
		}

		private struct Dialog
		{
			public bool applied;

			public Action<bool> dialogCompleteAction;

			public bool pausedInDialog;

			public string description;

			public string ok;

			public string cancel;

			public float timeOut;
		}

		private delegate int BoundedIndex(int num);

		public static string version = "1.43.0";

		private UIAnchor[] uiAnchors;

		private Dictionary<string, DismountUIWidget> widgetLookup = new Dictionary<string, DismountUIWidget>();

		private Dictionary<string, StateWidgets> states = new Dictionary<string, StateWidgets>();

		private Dictionary<string, List<object>> uiWidgetLookup = new Dictionary<string, List<object>>();

		private DismountUIWidget[] allWidgets;

		private GameObject[] dialogs = new GameObject[6];

		private GameObject[] dialogItems = new GameObject[6];

		private GameObject[] dialogBundleItems = new GameObject[6];

		private Camera uiCamera;

		private Transform replayControls;

		private UIScrollBar replayScrollBar;

		private UILabel replaySpeedLabel;

		private State currentState;

		private UILabel mainMenuChallenges;

		private UILabel ingameChallenges;

		private UILabel levelName;

		private UILabel highScore;

		private UILabel musicOnOffLabel;

		private UILabel sfxOnOffLabel;

		private HelpOverlayManager helpOverlayManager;

		private GameObject nag;

		private GameObject greenlight;

		private Resolution[] resolutions;

		private int currentResolutionIndex;

		private UICheckbox fullscreenCheckbox;

		private UICheckbox helpTextCheckbox;

		private UILabel unitsLabel;

		private UILabel dialogDescription;

		private UILabel dialogOkButtonText;

		private UILabel dialogCancelButtonText;

		private GameObject[] leaderboardRowTemplates;

		private Transform leaderboardContainer;

		private UILabel leaderboardLabel;

		private Vector3 leaderboardEntriesPanelTopPosition;

		private Vector3 leaderboardEntriesPanelJumpPosition;

		private UILabel leaderboardSwitchButtonLabel;

		private UILabel submitItemTitleLabel;

		private UILabel submitItemDescriptionLabel;

		private UILabel levelSelectDialogSwitchButtonLabel;

		private UILabel busyPanelLabel;

		private UILabel busyPanelLabel_controller;

		private UIImageButton steeringWheelButton;

		private UISprite steerPathSprite;

		private bool manualSteeringEnabled;

		public Texture2D defaultPreviewImageTexture;

		private bool videoAdAvailable;

		private bool premiumBundleEnabled;

		private GameObject inGameGroup;

		private GameObject mainMenuGroup;

		private bool _renderingEnabled = true;

		private readonly List<GameObject> _disableOnActivate = new List<GameObject>();

		private readonly List<GameObject> _touchscreenGameObjects = new List<GameObject>();

		private readonly List<DismountUIWidget> _touchscreenWidgets = new List<DismountUIWidget>();

		private string pendingPlatform = string.Empty;

		private string currentPlatform = "compleatbogus";

		private UIImageButton previousControllerButton;

		private bool friendsFetched;

		private State pendingState;

		private float itemScale = 1f;

		public int itemWidth = 170;

		public int itemHeight = 220;

		public int itemsPerLine = 4;

		public int bundleItemHeight = 220;

		public int bundleItemOffsetX = 50;

		public int bundleItemOffsetY = 50;

		private Action action;

		private string[] replaySpeedLabels = new string[15]
		{
			"-4x", "-2x", "-1x", "-1/2x", "-1/4x", "-1/8x", "-1/16x", "0x", "1/16x", "1/8x",
			"1/4x", "1/2x", "1x", "2x", "4x"
		};

		private float invokeCancelButtonClickedAt = -1f;

		private bool resetUICameraHighlightsOnApplyState;

		private bool _touchScreenEnabled;

		private List<Dialog> dialogList = new List<Dialog>();

		private int numberOfPreviousLockedBundles = -1;

		public UIAnchor[] UIAnchors
		{
			get
			{
				return uiAnchors;
			}
		}

		public bool renderingEnabled
		{
			get
			{
				return _renderingEnabled;
			}
			set
			{
				_renderingEnabled = value;
				if (_renderingEnabled)
				{
					EnableRendering();
				}
				else
				{
					DisableRendering();
				}
			}
		}

		public string CurrentPlatform
		{
			get
			{
				return currentPlatform;
			}
		}

		public State PendingState
		{
			get
			{
				return pendingState;
			}
		}

		public State CurrentState
		{
			get
			{
				return currentState;
			}
		}

		private void InitializeUIWidgetLookup()
		{
			Dictionary<string, List<object>> dictionary = new Dictionary<string, List<object>>();
			foreach (string key in uiWidgetLookup.Keys)
			{
				List<object> list = new List<object>();
				list.AddRange(uiWidgetLookup[key]);
				dictionary[key] = list;
			}
			uiWidgetLookup.Clear();
			UIWidget[] array = UnityEngine.Object.FindObjectsOfType<UIWidget>();
			UIWidget[] array2 = array;
			foreach (UIWidget uIWidget in array2)
			{
				List<object> value = null;
				if (!uiWidgetLookup.TryGetValue(uIWidget.name, out value))
				{
					value = new List<object>();
					uiWidgetLookup.Add(uIWidget.name, value);
				}
				value.Add(uIWidget);
			}
			UIImageButton[] array3 = UnityEngine.Object.FindObjectsOfType<UIImageButton>();
			UIImageButton[] array4 = array3;
			foreach (UIImageButton uIImageButton in array4)
			{
				List<object> value2 = null;
				if (!uiWidgetLookup.TryGetValue(uIImageButton.name, out value2))
				{
					value2 = new List<object>();
					uiWidgetLookup.Add(uIImageButton.name, value2);
				}
				value2.Add(uIImageButton);
			}
			UICheckbox[] array5 = UnityEngine.Object.FindObjectsOfType<UICheckbox>();
			UICheckbox[] array6 = array5;
			foreach (UICheckbox uICheckbox in array6)
			{
				List<object> value3 = null;
				if (!uiWidgetLookup.TryGetValue(uICheckbox.name, out value3))
				{
					value3 = new List<object>();
					uiWidgetLookup.Add(uICheckbox.name, value3);
				}
				value3.Add(uICheckbox);
			}
			UISlider[] array7 = UnityEngine.Object.FindObjectsOfType<UISlider>();
			UISlider[] array8 = array7;
			foreach (UISlider uISlider in array8)
			{
				List<object> value4 = null;
				if (!uiWidgetLookup.TryGetValue(uISlider.name, out value4))
				{
					value4 = new List<object>();
					uiWidgetLookup.Add(uISlider.name, value4);
				}
				value4.Add(uISlider);
			}
			ItemSelectDialogItem[] array9 = UnityEngine.Object.FindObjectsOfType<ItemSelectDialogItem>();
			ItemSelectDialogItem[] array10 = array9;
			foreach (ItemSelectDialogItem itemSelectDialogItem in array10)
			{
				List<object> value5 = null;
				if (!uiWidgetLookup.TryGetValue(itemSelectDialogItem.name, out value5))
				{
					value5 = new List<object>();
					uiWidgetLookup.Add(itemSelectDialogItem.name, value5);
				}
				value5.Add(itemSelectDialogItem);
			}
			foreach (string key2 in dictionary.Keys)
			{
				List<object> value6;
				List<object> value7;
				if (!dictionary.TryGetValue(key2, out value6) || !uiWidgetLookup.TryGetValue(key2, out value7) || value6.Count != 1 || value7.Count != 1)
				{
					continue;
				}
				object obj = value6[0];
				object obj2 = value7[0];
				if (obj != obj2 && obj.GetType() == obj2.GetType() && obj != null && !(obj as MonoBehaviour == null) && !((obj as MonoBehaviour).gameObject == null))
				{
					(obj2 as MonoBehaviour).gameObject.SetActive((obj as MonoBehaviour).gameObject.activeSelf);
					(obj as MonoBehaviour).gameObject.SetActive(true);
					if (obj is UILabel)
					{
						(obj2 as UILabel).text = (obj as UILabel).text;
					}
					else if (obj is UICheckbox)
					{
						(obj2 as UICheckbox).isChecked = (obj as UICheckbox).isChecked;
					}
				}
			}
		}

		private T FindUIWIdget<T>(string name)
		{
			List<object> value;
			if (uiWidgetLookup.TryGetValue(name, out value))
			{
				if (value.Count == 1)
				{
					return (T)Convert.ChangeType(value[0], typeof(T));
				}
				Debug.LogError("Request for a UIWidget with non unique name: " + name);
			}
			Debug.Log("Could not find UIWidget: " + name);
			return (T)Convert.ChangeType(null, typeof(T));
		}

		private void EnableRendering()
		{
			inGameGroup.SetActive(true);
			mainMenuGroup.SetActive(true);
		}

		private void DisableRendering()
		{
			inGameGroup.SetActive(false);
			mainMenuGroup.SetActive(false);
		}

		private void RemoveNagsFromState(string state)
		{
		}

		private void Awake()
		{
			uiAnchors = GetComponentsInChildren<UIAnchor>();
			allWidgets = GetComponentsInChildren<DismountUIWidget>();
			string id = Platform.GetId();
			ApplyPlatform(id);
		}

		public void SwitchPlatform(string platform)
		{
			pendingPlatform = platform;
		}

		private void ApplyPlatform(string platform)
		{
			DismountUIWidget[] array = allWidgets;
			foreach (DismountUIWidget dismountUIWidget in array)
			{
				dismountUIWidget.gameObject.SetActive(true);
			}
			if (previousControllerButton != null)
			{
				previousControllerButton.gameObject.SetActive(true);
			}
			mainMenuGroup = base.transform.Find("Camera/MainMenu").gameObject;
			inGameGroup = base.transform.Find("Camera/Ingame").gameObject;
			uiCamera = base.transform.Find("Camera").GetComponent<Camera>();
			InitializeDismountUIWidgets(platform);
			foreach (DismountUIWidget value in widgetLookup.Values)
			{
				value.gameObject.SetActive(true);
			}
			if ((bool)steeringWheelButton)
			{
				steeringWheelButton.gameObject.SetActive(true);
			}
			InitializeUIWidgetLookup();
			foreach (DismountUIWidget value2 in widgetLookup.Values)
			{
				value2.gameObject.SetActive(false);
			}
			replayControls = widgetLookup["ReplayControlsPanel"].transform;
			replayScrollBar = replayControls.Find("Control Bar/Scroll Bar").GetComponent<UIScrollBar>();
			replaySpeedLabel = replayScrollBar.transform.Find("SpeedLabel").GetComponent<UILabel>();
			states["Inactive"] = new StateWidgets();
			states["VideoOptions"] = new StateWidgets("VideoOptionsPanel");
			states["WebNag"] = new StateWidgets("WebNag");
			if (platform != "controller")
			{
				states["Paused"] = new StateWidgets("BackToMainManu Button", "Ingame Level Select Button", "OptionsPanel", "Continue Button", "Music Button", "SFX Button", "Music Volume", "SFX Volume", "DimmerPanel", "InGameHelp");
			}
			else
			{
				states["Paused"] = new StateWidgets("BackToMainManu Button", "Ingame Level Select Button", "OptionsPanel", "Continue Button", "Music Button", "SFX Button", "Music Volume", "SFX Volume", "DimmerPanel");
			}
			if (platform != "controller")
			{
				states["MainMenuOptions"] = new StateWidgets("OptionsPanel", "Credits Button", "QuitGame Button", "Music Button", "SFX Button", "Music Volume", "SFX Volume", "DimmerPanel", "InGameHelp");
			}
			else
			{
				states["MainMenuOptions"] = new StateWidgets("OptionsPanel", "Credits Button", "QuitGame Button", "Music Button", "SFX Button", "Music Volume", "SFX Volume", "DimmerPanel");
			}
			states["MainMenu"] = new StateWidgets("Menu Button", "Play Button", "Logo", "MainMenuChallenges", "Customize Button", "LoadCustomLevelButton", "Twitter Button", "Support Button", "Achievements Button", "Leaderboards Button", "Google Games Button", "LevelName");
			states["NewsPoster"] = new StateWidgets("NewsPoster");
			states["NewContentPoster"] = new StateWidgets("ContentUnlocked");
			states["SumbitLevel"] = new StateWidgets(string.Empty);
			states["Dialog"] = new StateWidgets("OkCancelDialog");
			states["LevelSelect"] = new StateWidgets("LevelSelectDialog");
			states["SelectFacebookFriend"] = new StateWidgets("FaceSelectDialog");
			states["SetupScene"] = new StateWidgets("Ready Button", "Clear Obstacles Button");
			states["SetupObstacle"] = new StateWidgets("ObstacleSelectDialog");
			states["SetupVehicle"] = new StateWidgets("VehicleSelectDialog");
			states["SetupCharacter"] = new StateWidgets("CharacterSelectDialog");
			states["SetupHead"] = new StateWidgets("HeadSelectDialog");
			states["CustomizeCharacter"] = new StateWidgets("CustomizeCharacterDialog");
			states["DismountStartingUnpublished"] = new StateWidgets("Menu Button", "Character Select Button", "Change Position Button", "Vehicle Select Button", "Dismount Button", "Setup Button", "Steer Path Button", "Power Slider", "Change Camera Button", "ReloadLevelButton", "SubmitLevelButton");
			states["DismountStarting"] = new StateWidgets("Menu Button", "Character Select Button", "Change Position Button", "Vehicle Select Button", "Dismount Button", "Setup Button", "Steer Path Button", "Power Slider", "Change Camera Button");
			states["DismountRevingEngine"] = new StateWidgets("Menu Button", "Dismount Button", "Power Slider", "Change Camera Button", "Steering Controls");
			states["DismountActive"] = new StateWidgets("Menu Button", "Dismount Button", "Power Slider", "Change Camera Button", "Steering Controls");
			states["DismountEnded"] = new StateWidgets("Menu Button", "Dismount Button", "Power Slider", "Change Camera Button", "Steering Controls");
			states["Replay"] = new StateWidgets("Menu Button", "Dismount Button", "Power Slider", "Change Camera Button", "ReplayControlsPanel", "ShowLeaderboardButton");
			states["ReplayWithLeaderboard"] = new StateWidgets("Menu Button", "Dismount Button", "Power Slider", "Change Camera Button", "ReplayControlsPanel", "Leaderboard");
			states["NameEntry"] = new StateWidgets("PlayerNameDialog");
			states["LevelIntro"] = new StateWidgets("Challenges", "LevelName");
			states["LevelIntroUnpublished"] = new StateWidgets("LevelName");
			states["Credits"] = new StateWidgets("Credits", "Credits2", "Credits3");
			states["SubmitLevel"] = new StateWidgets("SubmitItemDialog");
			states["Busy"] = new StateWidgets("BusyPanel");
			CreateDialogs();
			mainMenuChallenges = base.transform.Find("Camera/MainMenu/Left/MainMenuChallenges/Label").GetComponent<UILabel>();
			ingameChallenges = base.transform.Find("Camera/Ingame/Left/Challenges/Label").GetComponent<UILabel>();
			levelName = base.transform.Find("Camera/Ingame/TopRight/LevelName/Name").GetComponent<UILabel>();
			highScore = base.transform.Find("Camera/Ingame/TopRight/LevelName/Highscore").GetComponent<UILabel>();
			SetDismountPowerBar(0f);
			SetChallenges(string.Empty);
			helpOverlayManager = GetComponent<HelpOverlayManager>();
			Transform transform = base.transform.Find("Camera/MainMenu/Bottom/Panel/Version/Nag");
			if ((bool)transform)
			{
				nag = transform.gameObject;
				nag.SetActive(false);
			}
			Transform transform2 = base.transform.Find("Camera/MainMenu/Bottom/Panel/Version/Greenlight");
			if ((bool)transform2)
			{
				greenlight = transform2.gameObject;
				greenlight.SetActive(false);
			}
			if (platform == "controller")
			{
				UILabel uILabel = FindUIWIdget<UILabel>("ControlHelp");
				if ((bool)uILabel)
				{
					uILabel.gameObject.SetActive(false);
				}
			}
			musicOnOffLabel = FindUIWIdget<UILabel>("Music Button Label");
			sfxOnOffLabel = FindUIWIdget<UILabel>("SFX Button Label");
			resolutions = Screen.resolutions;
			fullscreenCheckbox = FindUIWIdget<UICheckbox>("FullscreenCheckbox");
			SyncVideoOptions();
			helpTextCheckbox = FindUIWIdget<UICheckbox>("HelpCheckbox");
			LoadHelpCheckboxState();
			SetVolumeSliders();
			unitsLabel = FindUIWIdget<UILabel>("CurrentUnitsLabel");
			unitsLabel.text = ((!DismountGame.playerState.metricUnits) ? "Imperial" : "Metric");
			_disableOnActivate.Clear();
			_touchscreenGameObjects.Clear();
			_touchscreenWidgets.Clear();
			Transform transform3 = base.transform.Find("Camera/MainMenu/Bottom/Panel/Twitter Button");
			AdjustPosition(transform3, 260f, false);
			_touchscreenWidgets.Add(transform3.GetComponent<DismountUIWidget>());
			Transform transform4 = base.transform.Find("Camera/MainMenu/Bottom/Panel/Support Button");
			AdjustPosition(transform4, -260f, false);
			_touchscreenWidgets.Add(transform4.GetComponent<DismountUIWidget>());
			if (platform == "controller")
			{
				Transform transform5 = base.transform.Find("Camera/Ingame/TopLeft_controller/Menu Button");
				_touchscreenWidgets.Add(transform5.GetComponent<DismountUIWidget>());
				Transform transform6 = base.transform.Find("Camera/Ingame/Center_controller/OptionsPanel/Controller Button");
				_touchscreenGameObjects.Add(transform6.gameObject);
				UIImageButton[] componentsInChildren = base.transform.GetComponentsInChildren<UIImageButton>(true);
				UIImageButton[] array2 = componentsInChildren;
				foreach (UIImageButton uIImageButton in array2)
				{
					if (uIImageButton.name.Equals("Close Button"))
					{
						_touchscreenGameObjects.Add(uIImageButton.gameObject);
					}
				}
			}
			if (DismountGame.IsLowMemoryDevice() || DismountGame.IsTVDevice())
			{
				_disableOnActivate.Add(base.transform.Find("Camera/MainMenu/Bottom/Panel/Customize Button").gameObject);
			}
			Transform transform7 = base.transform.Find("Camera/MainMenu/Bottom/Panel/Google Games Button/Image Button");
			GameObject gameObject = base.transform.Find("Camera/MainMenu/Bottom/Panel/Play Button/Image Button").gameObject;
			SXButtonKeys component = gameObject.GetComponent<SXButtonKeys>();
			component.targetDown = transform7.gameObject.GetComponent<SXButtonKeys>();
			transform7 = transform7.parent;
			AdjustPosition(transform7, 260f, true);
			if (platform != "controller")
			{
				dialogDescription = base.transform.Find("Camera/Ingame/Center/OkCancelDialog/Description").GetComponent<UILabel>();
				dialogOkButtonText = base.transform.Find("Camera/Ingame/Center/OkCancelDialog/Ok Button/Image Button/Label").GetComponent<UILabel>();
				dialogCancelButtonText = base.transform.Find("Camera/Ingame/Center/OkCancelDialog/Cancel Button/Image Button/Label").GetComponent<UILabel>();
			}
			else
			{
				dialogDescription = base.transform.Find("Camera/Ingame/Center_controller/OkCancelDialog/Description").GetComponent<UILabel>();
				dialogOkButtonText = base.transform.Find("Camera/Ingame/Center_controller/OkCancelDialog/Ok Button/Image Button/Label").GetComponent<UILabel>();
				dialogCancelButtonText = base.transform.Find("Camera/Ingame/Center_controller/OkCancelDialog/Cancel Button/Image Button/Label").GetComponent<UILabel>();
			}
			leaderboardLabel = FindUIWIdget<UILabel>("LeaderboardLabel");
			leaderboardSwitchButtonLabel = FindUIWIdget<UILabel>("LeaderboardSwitchButtonLabel");
			DismountUIWidget dismountUIWidget2 = widgetLookup["Leaderboard"];
			leaderboardContainer = dismountUIWidget2.transform.Find("EntriesPanel/Grid");
			leaderboardRowTemplates = new GameObject[3];
			leaderboardRowTemplates[0] = leaderboardContainer.Find("TemplateRows/LeaderboardEntryEven").gameObject;
			leaderboardRowTemplates[1] = leaderboardContainer.Find("TemplateRows/LeaderboardEntryOdd").gameObject;
			leaderboardRowTemplates[2] = leaderboardContainer.Find("TemplateRows/LeaderboardEntryPlayer").gameObject;
			leaderboardContainer.Find("TemplateRows").gameObject.SetActive(false);
			leaderboardEntriesPanelTopPosition = leaderboardContainer.parent.localPosition;
			leaderboardEntriesPanelJumpPosition = leaderboardEntriesPanelTopPosition;
			submitItemDescriptionLabel = base.transform.Find("Camera/Ingame/Center/SubmitItemDialog/Description Input/Label").GetComponent<UILabel>();
			submitItemTitleLabel = base.transform.Find("Camera/Ingame/Center/SubmitItemDialog/Title").GetComponent<UILabel>();
			levelSelectDialogSwitchButtonLabel = FindUIWIdget<UILabel>("LevelsetButtonLabel");
			UpdateLeaderboardSwitchButtonLabel();
			GameObject gameObject2 = levelSelectDialogSwitchButtonLabel.transform.parent.gameObject;
			gameObject2.SetActive(false);
			base.transform.Find("Camera/Ingame/Center/CustomizeCharacterDialog/ItemsPanel/Grid/3_ItemSelectDialogItem2").gameObject.SetActive(false);
			base.transform.Find("Camera/Ingame/Center/CustomizeCharacterDialog/ItemSelectPanel/Helplabel").gameObject.SetActive(false);
			base.transform.Find("Camera/Ingame/Center_controller/CustomizeCharacterDialog/ItemsPanel/Grid/3_ItemSelectDialogItem2").gameObject.SetActive(false);
			base.transform.Find("Camera/Ingame/Center_controller/CustomizeCharacterDialog/ItemSelectPanel/Helplabel").gameObject.SetActive(false);
			UIImageButton uIImageButton2 = FindUIWIdget<UIImageButton>("Controller Image Button");
			if ((bool)uIImageButton2)
			{
				uIImageButton2.gameObject.SetActive(SXInputManager.IsControllerConnected());
				previousControllerButton = uIImageButton2;
			}
			UpdateLevelSelectSwitchButtonLabel();
			busyPanelLabel = base.transform.Find("Camera/Ingame/Center/BusyPanel/Label").GetComponent<UILabel>();
			SetBusyLabel("Processing...");
			SetPreviewImageTexture(defaultPreviewImageTexture);
			UpdateControllerSprites(platform);
			pendingPlatform = (currentPlatform = platform);
			UILabel uILabel2 = FindUIWIdget<UILabel>("VersionLabel");
			uILabel2.text = "Version " + version;
			steeringWheelButton = FindUIWIdget<UIImageButton>("SteeringWheelButton");
			Transform transform8 = widgetLookup["Steer Path Button"].transform;
			steerPathSprite = transform8.Find("Image Button/Sprite").GetComponent<UISprite>();
			SetManualSteering(manualSteeringEnabled);
		}

		private void UpdateControllerSprites(string platform)
		{
		}

		private void InitializeDismountUIWidgets(string platform)
		{
			DismountUIWidget[] componentsInChildren = GetComponentsInChildren<DismountUIWidget>();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			widgetLookup.Clear();
			DismountUIWidget[] array = componentsInChildren;
			foreach (DismountUIWidget dismountUIWidget in array)
			{
				if (!widgetLookup.ContainsKey(dismountUIWidget.name))
				{
					string widgetPlatform = GetWidgetPlatform(dismountUIWidget.transform);
					if (string.IsNullOrEmpty(widgetPlatform) || widgetPlatform == platform)
					{
						widgetLookup[dismountUIWidget.name] = dismountUIWidget;
						dictionary[dismountUIWidget.name] = widgetPlatform;
					}
					dismountUIWidget.gameObject.SetActive(false);
					continue;
				}
				string text = dictionary[dismountUIWidget.name];
				if (text == platform)
				{
					dismountUIWidget.gameObject.SetActive(false);
					continue;
				}
				string widgetPlatform2 = GetWidgetPlatform(dismountUIWidget.transform);
				if (widgetPlatform2 == platform)
				{
					widgetLookup[dismountUIWidget.name] = dismountUIWidget;
					dictionary[dismountUIWidget.name] = widgetPlatform2;
					dismountUIWidget.gameObject.SetActive(false);
				}
				else
				{
					dismountUIWidget.gameObject.SetActive(false);
				}
			}
		}

		private string GetWidgetPlatform(Transform t)
		{
			string[] allIds = Platform.GetAllIds();
			while ((bool)t)
			{
				string[] array = allIds;
				foreach (string text in array)
				{
					if (t.name.EndsWith(text))
					{
						return text;
					}
				}
				t = t.parent;
			}
			return string.Empty;
		}

		public void SetBusyLabel(string text)
		{
			busyPanelLabel.text = text;
		}

		public void UpdateLevelSelectSwitchButtonLabel()
		{
			if (DismountGame.playerState.browsingCustomLevels)
			{
				levelSelectDialogSwitchButtonLabel.text = "Browse default levels";
			}
			else
			{
				levelSelectDialogSwitchButtonLabel.text = "Browse subscribed levels";
			}
		}

		public void SetSubmitItemTitle(string title)
		{
			submitItemTitleLabel.text = title;
		}

		private void SetPreviewImageTexture(Texture2D texture)
		{
			Transform transform = base.transform.Find("Camera/Ingame/Center/SubmitItemDialog/Quad");
			transform.GetComponent<Renderer>().material.mainTexture = texture;
			transform.gameObject.layer = 19;
		}

		public void SetSubmitPreviewImage(string filename)
		{
			if (File.Exists(filename))
			{
				Texture2D texture2D = new Texture2D(256, 256, TextureFormat.ARGB32, false, true);
				Utils.LoadImageIntoTexture(filename, texture2D);
				SetPreviewImageTexture(texture2D);
			}
		}

		public string GetSubmitItemTitle()
		{
			return submitItemTitleLabel.text;
		}

		public string GetSubmitItemDescription()
		{
			return submitItemDescriptionLabel.text;
		}

		public void UpdateLeaderboardSwitchButtonLabel()
		{
			if (leaderboardSwitchButtonLabel == null)
			{
				Debug.Log("leaderboardSwitchButtonLabel null");
			}
			else
			{
				Debug.Log("leaderboardSwitchButtonLabel " + leaderboardSwitchButtonLabel.text);
			}
			if (DismountGame.playerState.leaderboardTimeSpan == SXJNI.TimeSpan.DAILY)
			{
				leaderboardSwitchButtonLabel.text = "Daily";
			}
			else if (DismountGame.playerState.leaderboardTimeSpan == SXJNI.TimeSpan.WEEKLY)
			{
				leaderboardSwitchButtonLabel.text = "Weekly";
			}
			else
			{
				leaderboardSwitchButtonLabel.text = "All Time";
			}
		}

		private void SetVolumeSliders()
		{
			float sliderValue = 0.1f * (float)Prefs.GetInt("sfxVolume", 10);
			UISlider uISlider = FindUIWIdget<UISlider>("SFX Volume Slider");
			if ((bool)uISlider)
			{
				uISlider.sliderValue = sliderValue;
				uISlider.eventReceiver = DismountGame.instance.gameObject;
			}
			sliderValue = 0.1f * (float)Prefs.GetInt("musicVolume", 10);
			uISlider = FindUIWIdget<UISlider>("Music Volume Slider");
			if ((bool)uISlider)
			{
				uISlider.sliderValue = sliderValue;
				uISlider.eventReceiver = DismountGame.instance.gameObject;
			}
		}

		public void LoadHelpCheckboxState()
		{
			if (!(helpTextCheckbox == null))
			{
				helpTextCheckbox.isChecked = Prefs.GetInt("show_help", 1) == 1;
			}
		}

		public void SaveHelpCheckboxState()
		{
			if (!(helpTextCheckbox == null))
			{
				Prefs.SetInt("show_help", helpTextCheckbox.isChecked ? 1 : 0);
				Prefs.Save();
			}
		}

		public void SyncVideoOptions()
		{
		}

		private void updateCurrentResolutionLabel()
		{
		}

		public Transform GetReplayControls()
		{
			return replayControls;
		}

		public void StartHelpSequence(string sequenceName)
		{
			if (Platform.GetId().CompareTo("controller") != 0 && Prefs.GetInt("show_help", 1) == 1)
			{
				helpOverlayManager.ShowSequence(sequenceName);
			}
		}

		public void StopHelpSequence()
		{
			helpOverlayManager.StopSequence();
		}

		private bool StringArrayContains(string[] array, string value)
		{
			if (array == null || array.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (string.Compare(array[i], value) == 0)
				{
					return true;
				}
			}
			return false;
		}

		private void ActivateState(string state, params string[] forceDisabledWidgets)
		{
			StateWidgets stateWidgets = states[state];
			foreach (KeyValuePair<string, DismountUIWidget> item in widgetLookup)
			{
				DismountUIWidget value = item.Value;
				bool flag = StringArrayContains(forceDisabledWidgets, value.name);
				if ((!stateWidgets.IsInList(value.name) && value.Shown) || flag)
				{
					value.Hide();
				}
				if (!flag && StringArrayContains(stateWidgets.activeWidgets, value.name))
				{
					value.gameObject.SetActive(true);
					if (!value.Shown)
					{
						value.Show();
					}
				}
			}
			foreach (GameObject item2 in _disableOnActivate)
			{
				if (item2 != null)
				{
					item2.SetActive(false);
				}
			}
			if (_touchScreenEnabled)
			{
				return;
			}
			foreach (GameObject touchscreenGameObject in _touchscreenGameObjects)
			{
				touchscreenGameObject.SetActive(false);
			}
			foreach (DismountUIWidget touchscreenWidget in _touchscreenWidgets)
			{
				if (touchscreenWidget != null)
				{
					touchscreenWidget.Hide();
				}
			}
		}

		public void SetDefaultReplaySpeed()
		{
			replayScrollBar.scrollValue = 3510f / 4095f;
		}

		private void UpdateWokshopButtonState()
		{
			if (DismountGame.playerState.browsingCustomLevels)
			{
				FindUIWIdget<UIImageButton>("Open Workshop Button").gameObject.SetActive(true);
			}
			else
			{
				FindUIWIdget<UIImageButton>("Open Workshop Button").gameObject.SetActive(false);
			}
		}

		private void EnterDialog(Dialog dialog)
		{
			dialogList.Add(dialog);
		}

		private Dialog ExitDialog()
		{
			Dialog result = dialogList[0];
			dialogList.RemoveAt(0);
			invokeCancelButtonClickedAt = -1f;
			return result;
		}

		public void ChangeState(State state)
		{
			pendingState = state;
		}

		private void AdjustPosition(Transform go, float adjust, bool enable)
		{
			if (go != null)
			{
				DismountUIWidget component = go.gameObject.GetComponent<DismountUIWidget>();
				TweenPosition tweenPosition = component.showTweener as TweenPosition;
				TweenPosition tweenPosition2 = component.hideTweener as TweenPosition;
				tweenPosition.from.x = adjust;
				tweenPosition.to.x = adjust;
				tweenPosition2.from.x = adjust;
				tweenPosition2.to.x = adjust;
				if (enable)
				{
					tweenPosition.enabled = true;
				}
			}
		}

		private void ApplyState(State state)
		{
			if (DismountGame.playerState.isUnpublishedLevel)
			{
				switch (state)
				{
				case State.LevelIntro:
					state = State.LevelIntroUnpublished;
					break;
				case State.DismountStarting:
					state = State.DismountStartingUnpublished;
					break;
				}
			}
			string text = state.ToString();
			if (state == State.Inactive)
			{
				SetDismountPowerBar(0f);
				foreach (KeyValuePair<string, DismountUIWidget> item in widgetLookup)
				{
					if (item.Value.name.CompareTo("GarageDoor") != 0)
					{
						item.Value.Reset();
						item.Value.gameObject.SetActive(false);
					}
				}
			}
			else if (DismountGame.playerState.isCustomLevel)
			{
				ActivateState(text, "ShowLeaderboardButton", "Leaderboard");
			}
			else if (SXJNI.Instance.IsSignedIn())
			{
				ActivateState(text);
			}
			else
			{
				ActivateState(text, "ShowLeaderboardButton", "Leaderboard");
			}
			switch (state)
			{
			case State.DismountStarting:
			case State.DismountStartingUnpublished:
				FindUIWIdget<UILabel>("DismountButtonLabel").text = "DISMOUNT!";
				break;
			case State.DismountActive:
				FindUIWIdget<UILabel>("DismountButtonLabel").text = "RESET";
				break;
			case State.LevelSelect:
				FreeNewsPoster();
				break;
			}
			if (state == State.ReplayWithLeaderboard)
			{
				if (!leaderboardContainer)
				{
					Debug.Log("leaderboardContainer null");
				}
				SpringPanel.Begin(leaderboardContainer.parent.gameObject, leaderboardEntriesPanelJumpPosition, 13f);
			}
			if (state == State.Replay && DismountGame.stateManager.currentState == StateManager.State.ReplayMode)
			{
				DismountGame.stateManager.currentStateObject.SendMessage("UpdateRecordingControls");
			}
			if (state == State.VideoOptions)
			{
				SyncVideoOptions();
			}
			if (state == State.LevelSelect)
			{
				UpdateWokshopButtonState();
			}
			if (state == State.MainMenuOptions || state == State.Paused)
			{
				LoadHelpCheckboxState();
			}
			if (CurrentState == State.MainMenuOptions || CurrentState == State.Paused)
			{
				SaveHelpCheckboxState();
			}
			if (state != State.Replay && state != State.ReplayWithLeaderboard)
			{
				renderingEnabled = true;
				DismountGame.hudManager.renderingEnabled = true;
			}
			Screen.sleepTimeout = -1;
			if (state != State.Dialog && state != State.Credits && state != State.DismountActive && state != State.DismountEnded && state != State.DismountRevingEngine && state != State.DismountStarting && state != State.DismountStartingUnpublished && state != State.Replay && state != State.ReplayWithLeaderboard)
			{
				Screen.sleepTimeout = -2;
			}
			StopHelpSequence();
			StartHelpSequence(text);
			if (state == State.SelectFacebookFriend && !friendsFetched)
			{
				FacebookWrapper.Login(FacebookLoginComplete);
			}
			if (resetUICameraHighlightsOnApplyState)
			{
				resetUICameraHighlightsOnApplyState = false;
				UICamera.ResetHighlight();
			}
			UpdateDialogs();
			currentState = state;
		}

		private void GetFriendsComplete(List<FacebookWrapper.FacebookUser> friends)
		{
			if (friends != null)
			{
				UpdateFriendsDialog(friends);
			}
			friendsFetched = true;
		}

		private void UpdateFriendsDialog(List<FacebookWrapper.FacebookUser> friends)
		{
			List<GameItem> list = new List<GameItem>();
			foreach (FacebookWrapper.FacebookUser friend in friends)
			{
				GameItem gameItem = ScriptableObject.CreateInstance<GameItem>();
				gameItem.itemName = friend.name;
				gameItem.itemId = friend.id;
				gameItem.isLocked = false;
				gameItem.IAPProductId = "1";
				list.Add(gameItem);
			}
			list[0].IAPProductId = "0";
			SetItems(ItemDialogType.Faces, list);
		}

		private void FacebookLoginComplete(bool success)
		{
			if (success)
			{
				FacebookWrapper.GetFriends(GetFriendsComplete);
			}
		}

		private void UpdateDialogs()
		{
			for (int i = 0; i < 6; i++)
			{
				ItemSelectDialogItem[] componentsInChildren = dialogs[i].gameObject.GetComponentsInChildren<ItemSelectDialogItem>();
				bool flag = false;
				if (i == 3)
				{
					flag = true;
				}
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					string text = string.Empty;
					string strB = string.Empty;
					switch ((ItemDialogType)i)
					{
					case ItemDialogType.Obstacles:
						strB = ((!(DismountGame.stateManager.currentStateObject.GetComponent<SetupScene>().CurrentHotSpot.item != null)) ? "Empty Square" : DismountGame.stateManager.currentStateObject.GetComponent<SetupScene>().CurrentHotSpot.item.itemName);
						text = componentsInChildren[j].Item.itemName;
						break;
					case ItemDialogType.Vehicles:
						strB = DismountGame.playerState.currentVehicleName;
						if ((bool)componentsInChildren[j].Item)
						{
							text = componentsInChildren[j].Item.referenceName;
						}
						break;
					case ItemDialogType.Characters:
						strB = DismountGame.playerState.currentCharacterName;
						if ((bool)componentsInChildren[j].Item)
						{
							text = componentsInChildren[j].Item.referenceName;
						}
						break;
					case ItemDialogType.Heads:
						strB = DismountGame.playerState.currentHeadName;
						if ((bool)componentsInChildren[j].Item)
						{
							text = componentsInChildren[j].Item.referenceName;
						}
						break;
					case ItemDialogType.Levels:
						strB = ((!DismountGame.playerState.isCustomLevel) ? DismountGame.playerState.currentLevel : Utils.GetItemIdFromFilename(DismountGame.playerState.customLevelFilename));
						if (componentsInChildren[j].Item != null)
						{
							text = (DismountGame.playerState.browsingCustomLevels ? componentsInChildren[j].Item.itemId : componentsInChildren[j].Item.referenceName);
						}
						break;
					case ItemDialogType.Faces:
						text = componentsInChildren[j].Item.itemName;
						break;
					default:
						Debug.LogError("Unknown item type in dialog!");
						text = "1";
						strB = "2";
						break;
					}
					Transform transform = componentsInChildren[j].gameObject.transform.Find("Image Button/Selected");
					if ((bool)transform)
					{
						GameObject gameObject = transform.gameObject;
						if ((bool)gameObject)
						{
							if (text.CompareTo(strB) == 0 && !componentsInChildren[j].Item.isLocked)
							{
								if (i == 3)
								{
									flag = false;
								}
								gameObject.gameObject.SetActive(true);
								SXButtonKeys component = transform.parent.GetComponent<SXButtonKeys>();
								if (component != null)
								{
									component.panel.Reset();
									component.panel.selectionList = new SXButtonKeys[1];
									component.panel.selectionList[0] = component;
									component.panel.Activate(component, true);
								}
							}
							else
							{
								gameObject.gameObject.SetActive(false);
							}
						}
					}
					Transform transform2 = componentsInChildren[j].gameObject.transform.Find("Image Button/New");
					if (!transform2)
					{
						continue;
					}
					GameObject gameObject2 = transform2.gameObject;
					if ((bool)gameObject2)
					{
						string itemName = text;
						if (i == 3 && DismountGame.playerState.browsingCustomLevels)
						{
							itemName = text + "/level.lvl";
						}
						if (DismountGame.playerState.hasItemBeenUsedBefore(itemName) && !componentsInChildren[j].Item.isLocked)
						{
							gameObject2.SetActive(false);
						}
						else
						{
							gameObject2.SetActive(true);
						}
					}
				}
				if (!flag || componentsInChildren.Length <= 0 || componentsInChildren[0].Item.isLocked)
				{
					continue;
				}
				Transform transform3 = componentsInChildren[0].gameObject.transform.Find("Image Button");
				if ((bool)transform3)
				{
					SXButtonKeys component2 = transform3.GetComponent<SXButtonKeys>();
					if (component2 != null)
					{
						component2.panel.Reset();
						component2.panel.selectionList = new SXButtonKeys[1];
						component2.panel.selectionList[0] = component2;
						component2.panel.Activate(component2, true);
					}
				}
			}
		}

		public void SetControllerButtonState(bool enabled)
		{
			UILabel uILabel = FindUIWIdget<UILabel>("Controller Button Label");
			if (uILabel != null)
			{
				uILabel.text = ((!enabled) ? "Controller Off" : "Controller On");
			}
		}

		public void SetGooglePlayButtonState(string text)
		{
			UILabel uILabel = FindUIWIdget<UILabel>("GPGS Label");
			if (uILabel != null)
			{
				uILabel.text = text;
			}
		}

		public void SetMusicButtonState(bool enabled)
		{
			musicOnOffLabel.text = ((!enabled) ? "Music Off" : "Music On");
		}

		public void SetSFXButtonState(bool enabled)
		{
			sfxOnOffLabel.text = ((!enabled) ? "SFX Off" : "SFX On");
		}

		public void SetDismountPowerBar(float value)
		{
			DismountUIWidget dismountUIWidget = widgetLookup["Power Slider"];
			if (!dismountUIWidget)
			{
				return;
			}
			GameObject gameObject = dismountUIWidget.transform.Find("Dismount Power Slider").gameObject;
			GameObject gameObject2 = dismountUIWidget.transform.Find("Dismount Power Slider Part 2").gameObject;
			if (!gameObject)
			{
				return;
			}
			UISlider component = gameObject.GetComponent<UISlider>();
			UISlider component2 = gameObject2.GetComponent<UISlider>();
			if ((bool)component)
			{
				if (value < 0.75f)
				{
					component.sliderValue = 0.25f + value;
					component2.sliderValue = 0f;
				}
				else
				{
					component.sliderValue = 1f;
					component2.sliderValue = 4f * (value - 0.75f) * 0.9f;
				}
			}
		}

		private GameObject CreateDialogItem(Transform parent, ItemDialogType DialogType, GameItem gameItem, bool facebookFaceItem)
		{
			GameObject original = dialogItems[(int)DialogType];
			GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
			Vector3 localScale = gameObject.transform.localScale;
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = new Vector3(0f, 0f, -1f);
			gameObject.transform.localScale = localScale;
			ItemSelectDialogItem component = gameObject.GetComponent<ItemSelectDialogItem>();
			component.Item = gameItem;
			gameObject.SetActive(true);
			if (facebookFaceItem)
			{
				FacebookPicContainer component2 = Utils.FindChildRecursive("Picture", gameObject.transform).gameObject.GetComponent<FacebookPicContainer>();
				component2.id = gameItem.itemId;
			}
			Transform transform = Utils.FindChildRecursive("Unavailable", gameObject.transform);
			if ((bool)transform && !gameItem.isLocked)
			{
				transform.gameObject.SetActive(false);
			}
			return gameObject;
		}

		private GameObject CreateDialogBundleItem(Transform parent, GameObject prefab, IAPProduct product)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab) as GameObject;
			Vector3 localScale = gameObject.transform.localScale;
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = new Vector3(0f, 0f, -1f);
			gameObject.transform.localScale = localScale;
			ItemSelectDialogItem component = gameObject.GetComponent<ItemSelectDialogItem>();
			GameItem gameItem = ScriptableObject.CreateInstance<GameItem>();
			if (product != null)
			{
				gameItem.IAPProductId = product.productId;
				component.Item = gameItem;
				if (product.storeInfo != null)
				{
					Utils.FindChildRecursive("PriceLabel", gameObject.transform).gameObject.GetComponent<UILabel>().text = product.storeInfo.price + " " + product.storeInfo.currency;
				}
			}
			gameObject.SetActive(true);
			return gameObject;
		}

		private List<GameItem> CreateSortedItems(ItemDialogType dialogType, List<GameItem> items)
		{
			List<GameItem> list = new List<GameItem>(items);
			list.Sort((GameItem p1, GameItem p2) =>
			{
				int num = p1.IAPProductId.CompareTo(p2.IAPProductId);
				if (num == 0)
				{
					num = p1.itemName.CompareTo(p2.itemName);
				}
				return num;
			});
			return list;
		}

		private static string CreatePriceString(IAPProduct product)
		{
			if (product.storeInfo != null)
			{
				return product.storeInfo.price + " " + product.storeInfo.currency;
			}
			return "Offline";
		}

		public void SetItems(ItemDialogType dialogType, List<GameItem> gameItems)
		{
			int num = itemWidth;
			int num2 = itemHeight;
			int num3 = itemsPerLine;
			int num4 = bundleItemHeight;
			int num5 = bundleItemOffsetX;
			int num6 = bundleItemOffsetY;
			float num7 = itemScale;
			num3 = 4;
			num7 *= 1.1f;
			num = (int)((float)num * 1.2f);
			num2 = (int)((float)num2 * 1.15f);
			Transform transform = dialogs[(int)dialogType].transform.Find("ItemsPanel/Grid").transform;
			if (dialogType == ItemDialogType.Levels)
			{
				UpdateWokshopButtonState();
				if (gameItems.Count == 0)
				{
					FindUIWIdget<UILabel>("NoSubscriptionsMessage").gameObject.SetActive(true);
				}
				else
				{
					FindUIWIdget<UILabel>("NoSubscriptionsMessage").gameObject.SetActive(false);
				}
			}
			SXButtonPanel sXButtonPanel = transform.gameObject.GetComponent<SXButtonPanel>();
			if (sXButtonPanel != null)
			{
				sXButtonPanel.Reset();
			}
			else
			{
				sXButtonPanel = transform.gameObject.AddComponent<SXButtonPanel>();
			}
			List<Transform> list = new List<Transform>();
			foreach (Transform item6 in transform)
			{
				list.Add(item6);
			}
			foreach (Transform item7 in list)
			{
				if (item7.name.Contains("(Clone)"))
				{
					item7.gameObject.SetActive(false);
					UnityEngine.Object.Destroy(item7.gameObject);
				}
			}
			float num8 = 0f;
			float num9 = 0f;
			int num10 = 0;
			List<ButtonKey> list2 = new List<ButtonKey>();
			num8 = num5;
			Transform transform2 = Utils.FindChildRecursive("com.secretexit.turbodismount.premium", dialogs[(int)dialogType].transform);
			if (dialogType == ItemDialogType.Characters)
			{
				transform2 = null;
			}
			if ((bool)transform2)
			{
				if (premiumBundleEnabled)
				{
					transform2.gameObject.SetActive(true);
					GameObject gameObject = transform2.gameObject;
					gameObject.transform.localPosition = new Vector3(num8, num9, gameObject.transform.position.z);
					bool flag = DismountGame.onlineConfig.showSales;
					int numberOfLockedIAPs = DismountGame.inventory.GetNumberOfLockedIAPs();
					int num11 = numberOfLockedIAPs - 4;
					if (num11 > 6)
					{
						num11 = 6;
					}
					int num12 = Mathf.CeilToInt((float)num11 / 2f);
					if (num12 <= 0 || num11 < 2)
					{
						flag = false;
					}
					GameItem gameItem = ScriptableObject.CreateInstance<GameItem>();
					string text = "com.secretexit.turbodismount.premium" + num11;
					if (num11 < 2)
					{
						text = "com.secretexit.turbodismount.premium1";
					}
					if (num11 > 6)
					{
						text = "com.secretexit.turbodismount.premium6";
					}
					IAPProduct iAPProduct = DismountGame.inventory.GetIAPProduct(text);
					gameItem.IAPProductId = text;
					ItemSelectDialogItem component = transform2.gameObject.GetComponent<ItemSelectDialogItem>();
					component.Item = gameItem;
					string text2 = CreatePriceString(iAPProduct);
					Utils.FindChildRecursive("PriceLabel", transform2.transform).gameObject.GetComponent<UILabel>().text = text2;
					if (flag)
					{
						text = "com.secretexit.turbodismount.premium" + num12;
						iAPProduct = DismountGame.inventory.GetIAPProduct(text);
						gameItem.IAPProductId = text;
						Utils.FindChildRecursive("OriginalPriceLabel", transform2.transform).gameObject.GetComponent<UILabel>().text = text2;
						string text3 = string.Empty;
						for (int i = 0; i < text2.Length; i++)
						{
							text3 += "_";
						}
						Utils.FindChildRecursive("StrikeThrough", transform2.transform).gameObject.GetComponent<UILabel>().text = text3;
						Utils.FindChildRecursive("SalesPriceLabel", transform2.transform).gameObject.GetComponent<UILabel>().text = CreatePriceString(iAPProduct);
						Utils.FindChildRecursive("PriceLabel", transform2.transform).gameObject.SetActive(false);
						Utils.FindChildRecursive("PriceBG", transform2.transform).gameObject.SetActive(false);
						Utils.FindChildRecursive("StrikeThrough", transform2.transform).gameObject.SetActive(true);
						Utils.FindChildRecursive("OriginalPriceLabel", transform2.transform).gameObject.SetActive(true);
						Utils.FindChildRecursive("SalesPriceLabel", transform2.transform).gameObject.SetActive(true);
						Utils.FindChildRecursive("SalesPriceBG", transform2.transform).gameObject.SetActive(true);
						Utils.FindChildRecursive("LimitedOfferLabel", transform2.transform).gameObject.SetActive(true);
					}
					else
					{
						Utils.FindChildRecursive("PriceLabel", transform2.transform).gameObject.SetActive(true);
						Utils.FindChildRecursive("PriceBG", transform2.transform).gameObject.SetActive(true);
						Utils.FindChildRecursive("StrikeThrough", transform2.transform).gameObject.SetActive(false);
						Utils.FindChildRecursive("OriginalPriceLabel", transform2.transform).gameObject.SetActive(false);
						Utils.FindChildRecursive("SalesPriceLabel", transform2.transform).gameObject.SetActive(false);
						Utils.FindChildRecursive("SalesPriceBG", transform2.transform).gameObject.SetActive(false);
						Utils.FindChildRecursive("LimitedOfferLabel", transform2.transform).gameObject.SetActive(false);
					}
					Transform transform3 = gameObject.transform.Find("Image Button");
					if ((bool)transform3)
					{
						ButtonKey item2 = new ButtonKey(transform3.gameObject.AddComponent<SXButtonKeys>(), 0, true);
						list2.Add(item2);
						item2.m_uibk.panel = sXButtonPanel;
					}
					num9 -= (float)num4;
				}
				else
				{
					transform2.gameObject.SetActive(false);
				}
			}
			num8 = 0f;
			foreach (GameItem gameItem2 in gameItems)
			{
				if (!gameItem2.isLocked)
				{
					bool facebookFaceItem = false;
					if (dialogType == ItemDialogType.Faces)
					{
						facebookFaceItem = true;
					}
					GameObject gameObject2 = CreateDialogItem(transform, dialogType, gameItem2, facebookFaceItem);
					gameObject2.transform.localPosition = new Vector3(num8, num9, gameObject2.transform.position.z);
					gameObject2.transform.localScale *= num7;
					Transform transform4 = gameObject2.transform.Find("Image Button");
					if ((bool)transform4)
					{
						ButtonKey item3 = new ButtonKey(transform4.gameObject.AddComponent<SXButtonKeys>(), num10, false);
						list2.Add(item3);
						item3.m_uibk.panel = sXButtonPanel;
					}
					num8 += (float)num;
					num10++;
					if (num10 >= num3)
					{
						num8 = 0f;
						num9 -= (float)num2;
						num10 = 0;
					}
				}
			}
			if (num8 != 0f)
			{
				num9 -= (float)num2;
			}
			num8 = 0f;
			GameItem.ItemCategory category = GameItem.ItemCategory.Bundle;
			bool flag2 = false;
			switch (dialogType)
			{
			case ItemDialogType.Characters:
				flag2 = true;
				category = GameItem.ItemCategory.Character;
				break;
			case ItemDialogType.Levels:
				flag2 = true;
				category = GameItem.ItemCategory.Level;
				break;
			case ItemDialogType.Vehicles:
				flag2 = true;
				category = GameItem.ItemCategory.Vehicle;
				break;
			case ItemDialogType.Heads:
				flag2 = false;
				category = GameItem.ItemCategory.Head;
				break;
			}
			if (flag2)
			{
				num8 = num5;
				num9 -= (float)num6;
				bool flag3 = true;
				string text4 = string.Empty;
				if (dialogType == ItemDialogType.Vehicles)
				{
					if (DismountGame.inventory.GetLockedItemsOfCategory(GameItem.ItemCategory.Vehicle).Count == 0)
					{
						flag3 = false;
					}
					text4 = "_vehicle";
				}
				if (dialogType == ItemDialogType.Levels)
				{
					if (DismountGame.inventory.GetLockedItemsOfCategory(GameItem.ItemCategory.Level).Count == 0)
					{
						flag3 = false;
					}
					text4 = "_level";
				}
				if (dialogType == ItemDialogType.Characters)
				{
					if (DismountGame.inventory.GetLockedItemsOfCategory(GameItem.ItemCategory.Character).Count == 0)
					{
						flag3 = false;
					}
					text4 = "_character";
				}
				if (!videoAdAvailable)
				{
					flag3 = false;
				}
				Transform transform5 = Utils.FindChildRecursive("videoadbanner" + text4, dialogs[(int)dialogType].transform);
				if ((bool)transform5)
				{
					if (flag3)
					{
						transform5.gameObject.SetActive(true);
						GameObject gameObject3 = transform5.gameObject;
						gameObject3.transform.localPosition = new Vector3(num8, num9, gameObject3.transform.position.z);
						Transform transform6 = gameObject3.transform.Find("Image Button");
						if ((bool)transform6)
						{
							ButtonKey item4 = new ButtonKey(transform6.gameObject.AddComponent<SXButtonKeys>(), 0, true);
							list2.Add(item4);
							item4.m_uibk.panel = sXButtonPanel;
						}
						num9 -= (float)num4;
					}
					else
					{
						transform5.gameObject.SetActive(false);
					}
				}
				List<IAPProduct> iAPItemsOfCategory = DismountGame.inventory.GetIAPItemsOfCategory(category);
				foreach (IAPProduct item8 in iAPItemsOfCategory)
				{
					Transform transform7 = Utils.FindChildRecursive(item8.productId, dialogs[(int)dialogType].transform);
					if ((bool)transform7)
					{
						GameObject gameObject4 = transform7.gameObject;
						GameObject gameObject5 = CreateDialogBundleItem(transform, gameObject4, item8);
						gameObject5.transform.localPosition = new Vector3(num8, num9, gameObject5.transform.position.z);
						Transform transform8 = gameObject5.transform.Find("Image Button");
						if ((bool)transform8)
						{
							ButtonKey item5 = new ButtonKey(transform8.gameObject.AddComponent<SXButtonKeys>(), 0, true);
							list2.Add(item5);
							item5.m_uibk.panel = sXButtonPanel;
						}
						gameObject4.SetActive(false);
						num9 -= (float)num4;
					}
					else
					{
						Debug.LogError("No bundle item for product: " + item8.productId + " found!");
					}
				}
			}
			foreach (Transform item9 in dialogs[(int)dialogType].transform.Find("ItemsPanel/Grid"))
			{
				if (!item9.name.EndsWith("(Clone)") && !item9.name.StartsWith("videoadbanner") && item9.name.CompareTo("com.secretexit.turbodismount.premium") != 0)
				{
					item9.gameObject.SetActive(false);
				}
			}
			UpdateDialogs();
			bool activeInHierarchy = dialogs[(int)dialogType].activeInHierarchy;
			if (!activeInHierarchy)
			{
				dialogs[(int)dialogType].SetActive(true);
				GameObject gameObject6 = Utils.FindChildRecursive("ItemsPanel", dialogs[(int)dialogType].transform).gameObject;
				if ((bool)gameObject6)
				{
					gameObject6.GetComponent<UIDraggablePanel>().ResetPosition();
					UIDraggablePanelSelectedObjectFollower component2 = gameObject6.GetComponent<UIDraggablePanelSelectedObjectFollower>();
					if (component2 == null && SXInputManager.IsControllerConnected())
					{
						gameObject6.AddComponent<UIDraggablePanelSelectedObjectFollower>();
					}
				}
				dialogs[(int)dialogType].SetActive(activeInHierarchy);
			}
			ButtonKey[] array = list2.ToArray();
			int n = array.Length;
			BoundedIndex boundedIndex = (int index) => Mathf.Max(index - 1, 0);
			BoundedIndex boundedIndex2 = (int index) => Mathf.Min(index + 1, n - 1);
			for (int num13 = 0; num13 < array.Length; num13++)
			{
				int num14 = boundedIndex(num13);
				int num15 = boundedIndex2(num13);
				array[num13].m_uibk.targetLeft = ((num14 == num13) ? null : array[num14].m_uibk);
				array[num13].m_uibk.targetRight = ((num15 == num13) ? null : array[num15].m_uibk);
				int num16 = num13;
				while (array[boundedIndex(num16)].m_x < array[num16].m_x)
				{
					num16 = boundedIndex(num16);
				}
				if (num16 != boundedIndex(num16))
				{
					num16 = boundedIndex(num16);
					while (array[num16].m_x > array[num13].m_x)
					{
						num16 = boundedIndex(num16);
					}
					array[num13].m_uibk.targetUp = array[num16].m_uibk;
				}
				else
				{
					array[num13].m_uibk.targetUp = null;
				}
				num16 = num13;
				while (array[boundedIndex2(num16)].m_x > array[num16].m_x)
				{
					num16 = boundedIndex2(num16);
				}
				if (num16 != boundedIndex2(num16))
				{
					num16 = boundedIndex2(num16);
					while (array[num16].m_x < array[num13].m_x && array[num16].m_x < array[boundedIndex2(num16)].m_x)
					{
						num16 = boundedIndex2(num16);
					}
					array[num13].m_uibk.targetDown = array[num16].m_uibk;
				}
				else
				{
					array[num13].m_uibk.targetDown = null;
				}
			}
		}

		private void CreateDialogs()
		{
			for (int i = 0; i < 6; i++)
			{
				switch ((ItemDialogType)i)
				{
				case ItemDialogType.Obstacles:
					dialogs[i] = widgetLookup["ObstacleSelectDialog"].gameObject;
					break;
				case ItemDialogType.Vehicles:
					dialogs[i] = widgetLookup["VehicleSelectDialog"].gameObject;
					break;
				case ItemDialogType.Characters:
					dialogs[i] = widgetLookup["CharacterSelectDialog"].gameObject;
					break;
				case ItemDialogType.Levels:
					dialogs[i] = widgetLookup["LevelSelectDialog"].gameObject;
					break;
				case ItemDialogType.Heads:
					dialogs[i] = widgetLookup["HeadSelectDialog"].gameObject;
					break;
				case ItemDialogType.Faces:
					dialogs[i] = widgetLookup["FaceSelectDialog"].gameObject;
					break;
				}
				dialogItems[i] = Utils.FindChildRecursive("ItemSelectDialogItem", dialogs[i].transform).gameObject;
				dialogItems[i].SetActive(false);
				Transform transform = Utils.FindChildRecursive("ItemSelectDialogBundleItem", dialogs[i].transform);
				if ((bool)transform)
				{
					dialogBundleItems[i] = transform.gameObject;
					dialogBundleItems[i].SetActive(false);
				}
			}
		}

		public Vector3 GetScreenPosition(Vector3 worldPosition)
		{
			return uiCamera.WorldToScreenPoint(worldPosition);
		}

		public bool IsMouseObstructedByUI()
		{
			Ray ray = uiCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, (1 << base.gameObject.layer) | 0x10000))
			{
				return true;
			}
			return false;
		}

		public void OpenDoor()
		{
			widgetLookup["GarageDoor"].Hide();
		}

		private void DoCloseDoorAction()
		{
			DismountGame.hudManager.RemoveParticles();
			action();
		}

		public void CloseDoor()
		{
			widgetLookup["GarageDoor"].gameObject.SetActive(true);
			widgetLookup["GarageDoor"].Show();
		}

		public void CloseDoor(Action action)
		{
			CloseDoor();
			this.action = action;
			Invoke("DoCloseDoorAction", 0.4f);
		}

		public void SetReplayScrollbarPosition(float position)
		{
			replayScrollBar.scrollValue = position;
		}

		public float GetReplayScrollbarPosition()
		{
			replayScrollBar.scrollValue = (float)Mathf.RoundToInt(14f * replayScrollBar.scrollValue) / 14f;
			return replayScrollBar.scrollValue;
		}

		public void UpdateControllerButtonVisibility(bool visible)
		{
			if (previousControllerButton != null)
			{
				previousControllerButton.gameObject.SetActive(visible);
			}
		}

		public void ShowNag()
		{
			if ((bool)nag)
			{
				nag.SetActive(true);
			}
		}

		private void Update()
		{
			if (!_touchScreenEnabled && DismountGame.touchscreenEnabled && !DismountGame.IsTVDevice())
			{
				foreach (GameObject touchscreenGameObject in _touchscreenGameObjects)
				{
					touchscreenGameObject.SetActive(true);
				}
				string key = currentState.ToString();
				StateWidgets stateWidgets = ((!states.ContainsKey(key)) ? new StateWidgets() : states[key]);
				foreach (DismountUIWidget touchscreenWidget in _touchscreenWidgets)
				{
					if (touchscreenWidget != null)
					{
						touchscreenWidget.gameObject.SetActive(true);
						if (!touchscreenWidget.Shown && stateWidgets.IsInList(touchscreenWidget.name))
						{
							touchscreenWidget.Show();
						}
					}
				}
				_touchScreenEnabled = true;
			}
			if (SXButtonPanel.currentlyActivated != null && (currentState == State.Dialog || currentState == State.VideoOptions || currentState == State.MainMenu || currentState == State.LevelSelect || currentState == State.CustomizeCharacter || currentState == State.SetupCharacter || currentState == State.SetupVehicle || currentState == State.SetupObstacle || currentState == State.SetupHead || currentState == State.Paused || currentState == State.MainMenuOptions))
			{
				SXButtonPanel.currentlyActivated.HandleButtons();
			}
			if (replaySpeedLabel.gameObject.activeSelf)
			{
				float replayScrollbarPosition = GetReplayScrollbarPosition();
				replaySpeedLabel.text = replaySpeedLabels[Mathf.RoundToInt(replayScrollbarPosition * 14f)];
			}
			if (invokeCancelButtonClickedAt > 0f && Time.realtimeSinceStartup >= invokeCancelButtonClickedAt)
			{
				CancelButtonClicked();
			}
			if (SXInputManager.GetMouseButtonDown(0) || SXInputManager.GetMouseButtonDown(1))
			{
				resetUICameraHighlightsOnApplyState = true;
			}
		}

		private void LateUpdate()
		{
			if (dialogList.Count > 0)
			{
				Dialog value = dialogList[0];
				if (!value.applied)
				{
					dialogDescription.text = value.description;
					dialogOkButtonText.text = value.ok;
					if (value.cancel == null)
					{
						dialogCancelButtonText.transform.parent.gameObject.SetActive(false);
					}
					else
					{
						dialogCancelButtonText.transform.parent.gameObject.SetActive(true);
						dialogCancelButtonText.text = value.cancel;
					}
					if (value.timeOut > 0f)
					{
						invokeCancelButtonClickedAt = Time.realtimeSinceStartup + value.timeOut;
					}
					ApplyState(State.Dialog);
					value.applied = true;
					dialogList[0] = value;
				}
				return;
			}
			if (pendingPlatform != currentPlatform)
			{
				DismountUIWidget dismountUIWidget = widgetLookup["GarageDoor"];
				if (!dismountUIWidget.gameObject.activeSelf || !dismountUIWidget.Shown)
				{
					DismountUIWidget[] array = allWidgets;
					foreach (DismountUIWidget dismountUIWidget2 in array)
					{
						dismountUIWidget2.Hide();
					}
					ApplyPlatform(pendingPlatform);
					DismountGame.instance.PopulateItemDialogs();
					ApplyState(pendingState);
				}
			}
			if (pendingState != CurrentState)
			{
				ApplyState(pendingState);
			}
		}

		private void OnDismountButtonPressed()
		{
			if (CurrentState == State.DismountStarting || CurrentState == State.DismountStartingUnpublished)
			{
				DismountGame.instance.BroadcastMessage("OnDismountPressed");
			}
			else if (CurrentState == State.DismountActive || CurrentState == State.DismountEnded || CurrentState == State.Replay || CurrentState == State.ReplayWithLeaderboard)
			{
				DismountGame.instance.BroadcastMessage("OnResetPressed");
			}
		}

		private void OnDismountButtonReleased()
		{
			if (CurrentState == State.DismountStarting || CurrentState == State.DismountStartingUnpublished || CurrentState == State.DismountRevingEngine)
			{
				DismountGame.instance.BroadcastMessage("OnDismountReleased");
			}
			else if (CurrentState == State.DismountActive || CurrentState == State.DismountEnded || CurrentState == State.Replay || CurrentState == State.ReplayWithLeaderboard)
			{
				DismountGame.instance.BroadcastMessage("OnResetReleased");
			}
		}

		public void SetChallenges(string text)
		{
			ingameChallenges.text = text;
			mainMenuChallenges.text = text;
		}

		public void SetLevelNameAndHighscore(string name, int highscore)
		{
			levelName.text = name;
			highScore.text = "High Score\n" + highscore;
		}

		public string GetPlayerName()
		{
			string caratChar = base.transform.Find("Camera/Ingame/Center/PlayerNameDialog/Panel/Input").gameObject.GetComponent<UIInput>().caratChar;
			string text = base.transform.Find("Camera/Ingame/Center/PlayerNameDialog/Panel/Input/Label").gameObject.GetComponent<UILabel>().text;
			int num = text.LastIndexOf(caratChar);
			if (num != -1)
			{
				text = text.Remove(num);
			}
			return text;
		}

		public void SetPlayerName(string name)
		{
			string caratChar = base.transform.Find("Camera/Ingame/Center/PlayerNameDialog/Panel/Input").gameObject.GetComponent<UIInput>().caratChar;
			UILabel component = base.transform.Find("Camera/Ingame/Center/PlayerNameDialog/Panel/Input/Label").gameObject.GetComponent<UILabel>();
			component.text = name + caratChar;
		}

		public void NextResolution()
		{
			currentResolutionIndex++;
			if (currentResolutionIndex >= resolutions.Length)
			{
				currentResolutionIndex = 0;
			}
			updateCurrentResolutionLabel();
		}

		public void PreviousResolution()
		{
			currentResolutionIndex--;
			if (currentResolutionIndex < 0)
			{
				currentResolutionIndex = resolutions.Length - 1;
			}
			updateCurrentResolutionLabel();
		}

		public void ToggleUnitMode()
		{
			DismountGame.playerState.metricUnits = !DismountGame.playerState.metricUnits;
			unitsLabel.text = ((!DismountGame.playerState.metricUnits) ? "Imperial" : "Metric");
		}

		public bool IsFullScreenChecked()
		{
			return fullscreenCheckbox.isChecked;
		}

		public Resolution GetCurrentResolution()
		{
			return resolutions[currentResolutionIndex];
		}

		public void ShowDialog(string description, string ok, string cancel, Action<bool> completeAction, float timeOut, bool pause = true)
		{
			if (!DismountGame.instance.paused && pause)
			{
				DismountGame.instance.Pause();
			}
			else
			{
				pause = false;
			}
			EnterDialog(new Dialog
			{
				applied = false,
				dialogCompleteAction = completeAction,
				pausedInDialog = pause,
				description = description,
				ok = ok,
				cancel = cancel,
				timeOut = timeOut
			});
		}

		public void OKButtonClicked()
		{
			Dialog dialog = ExitDialog();
			if (dialog.pausedInDialog)
			{
				DismountGame.instance.Unpause();
			}
			dialog.dialogCompleteAction(true);
		}

		public void CancelButtonClicked()
		{
			Dialog dialog = ExitDialog();
			if (dialog.pausedInDialog)
			{
				DismountGame.instance.Unpause();
			}
			dialog.dialogCompleteAction(false);
		}

		public void PopulateLeaderboard(List<Leaderboards.LeaderboardEntry> entries, int playerIndex)
		{
			leaderboardContainer.parent.GetComponent<UIDraggablePanel>().ResetPosition();
			for (int i = 0; i < leaderboardContainer.childCount; i++)
			{
				Transform child = leaderboardContainer.GetChild(i);
				if (child.name != "TemplateRows")
				{
					UnityEngine.Object.Destroy(child.gameObject);
				}
			}
			leaderboardEntriesPanelJumpPosition = leaderboardEntriesPanelTopPosition;
			int num = -1;
			float cellHeight = leaderboardContainer.GetComponent<UIGrid>().cellHeight;
			for (int j = 0; j < entries.Count; j++)
			{
				Leaderboards.LeaderboardEntry leaderboardEntry = entries[j];
				GameObject gameObject = ((j == playerIndex) ? (UnityEngine.Object.Instantiate(leaderboardRowTemplates[2]) as GameObject) : (((j & 1) != 1) ? (UnityEngine.Object.Instantiate(leaderboardRowTemplates[0]) as GameObject) : (UnityEngine.Object.Instantiate(leaderboardRowTemplates[1]) as GameObject)));
				gameObject.SetActive(true);
				gameObject.transform.parent = leaderboardContainer;
				Vector3 localPosition = leaderboardRowTemplates[0].transform.localPosition;
				localPosition.y -= (float)j * cellHeight;
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.localPosition = localPosition;
				if (j == playerIndex && playerIndex > 5)
				{
					leaderboardEntriesPanelJumpPosition = leaderboardEntriesPanelTopPosition + new Vector3(0f, 0f - localPosition.y, 0f) - 5f * new Vector3(0f, cellHeight, 0f);
				}
				UILabel component = gameObject.transform.Find("Rank").GetComponent<UILabel>();
				component.text = ((leaderboardEntry.rank != num) ? (string.Empty + leaderboardEntry.rank) : "=");
				component = gameObject.transform.Find("Name").GetComponent<UILabel>();
				component.text = leaderboardEntry.name;
				component = gameObject.transform.Find("Score").GetComponent<UILabel>();
				component.text = Utils.ScoreString(leaderboardEntry.score);
				num = leaderboardEntry.rank;
			}
			if (DismountGame.playerState.levelItem != null)
			{
				leaderboardLabel.text = DismountGame.playerState.levelItem.itemName;
			}
			else
			{
				leaderboardLabel.text = "Leaderboard";
			}
			leaderboardContainer.parent.GetComponent<UIDraggablePanel>().ResetPosition();
			SpringPanel.Begin(leaderboardContainer.parent.gameObject, leaderboardEntriesPanelJumpPosition, 13f);
		}

		public void UpdateVideoAdAvailability(bool available, GameItem item, DismountGame.AdPrizeType type)
		{
			bool flag = videoAdAvailable;
			videoAdAvailable = available;
			if (available)
			{
				FindUIWIdget<UILabel>("VideoAdDescription_vehicle").text = "Watch a video and try a new vehicle!";
				FindUIWIdget<UILabel>("VideoAdDescription_level").text = "Watch a video and play on this awesome level!";
			}
			else
			{
				FindUIWIdget<UILabel>("VideoAdDescription_vehicle").text = "No test drives available, try again later!";
				FindUIWIdget<UILabel>("VideoAdDescription_level").text = "No level trials available, try again later!";
				FindUIWIdget<ItemSelectDialogItem>("videoadbanner_vehicle").Item = DismountGame.inventory.GetItem("obstacle.emptysquare");
				FindUIWIdget<ItemSelectDialogItem>("videoadbanner_level").Item = DismountGame.inventory.GetItem("obstacle.emptysquare");
			}
			if ((bool)item && available)
			{
				switch (type)
				{
				case DismountGame.AdPrizeType.Vehicle:
					FindUIWIdget<ItemSelectDialogItem>("videoadbanner_vehicle").Item = item;
					break;
				case DismountGame.AdPrizeType.Level:
					FindUIWIdget<ItemSelectDialogItem>("videoadbanner_level").Item = item;
					break;
				}
			}
			if (flag != videoAdAvailable)
			{
				DismountGame.instance.PopulateItemDialogs();
			}
		}

		public void RemoveRecordingHelpItems()
		{
			helpOverlayManager.RemoveRecordingHelpItems();
		}

		public void updatePremiumBanner()
		{
			int numberOfLockedIAPs = DismountGame.inventory.GetNumberOfLockedIAPs();
			if (numberOfLockedIAPs != numberOfPreviousLockedBundles)
			{
				premiumBundleEnabled = true;
				if (numberOfLockedIAPs < 1)
				{
					premiumBundleEnabled = false;
				}
				DismountGame.instance.PopulateItemDialogs();
				numberOfPreviousLockedBundles = numberOfLockedIAPs;
			}
		}

		private void FreeNewsPoster()
		{
			Transform transform = widgetLookup["NewsPoster"].transform.Find("Texture");
			if ((bool)transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}

		public void LoadNewsPoster(string path)
		{
			Transform transform = widgetLookup["NewsPoster"].transform.Find("Texture");
			if ((bool)transform && File.Exists(path))
			{
				try
				{
					byte[] data = File.ReadAllBytes(path);
					Texture2D texture2D = transform.GetComponent<Renderer>().material.mainTexture as Texture2D;
					texture2D.LoadImage(data);
				}
				catch (Exception)
				{
					File.Delete(path);
				}
			}
		}

		public UIImageButton GetSteeringWheelButton()
		{
			return steeringWheelButton;
		}

		public void SetManualSteering(bool enabled)
		{
			if (enabled)
			{
				steeringWheelButton.gameObject.SetActive(true);
				steerPathSprite.spriteName = "icon_bt_manualsteering";
			}
			else
			{
				steeringWheelButton.gameObject.SetActive(false);
				steerPathSprite.spriteName = "icon_bt_steering";
			}
			manualSteeringEnabled = enabled;
		}
	}
}
