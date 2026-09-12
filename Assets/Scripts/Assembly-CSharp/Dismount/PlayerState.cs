#pragma warning disable 0618,0619
using System.Collections.Generic;
using System.Text;
using Dismount.Vehicular;
using UnityEngine;
using reLive;

namespace Dismount
{
	public class PlayerState : MonoBehaviour
	{
		public string currentVehicleItemId;

		public string currentCharacterItemId;

		public string currentHeadItemId;

		public int currentCharacterStartPosition;

		public int currentLevelSteerPath = -1;

		public GameItem levelItem;

		public GameItem currentVehicleItem;

		public GameItem currentCharacterItem;

		public GameItem currentHeadItem;

		public string currentVehicleName;

		public string currentCharacterName;

		public string currentLevelName;

		public string currentHeadName;

		private NumericalStatistic vehicleSpeed = new NumericalStatistic(false, 3f);

		private NumericalStatistic vehicleAltitude = new NumericalStatistic(true, 3f);

		private NumericalStatistic characterAirTime = new NumericalStatistic(true, 2f);

		private NumericalStatistic characterAltitude = new NumericalStatistic(true, 3f);

		private StringBuilder vehicleSpeedText = new StringBuilder(10);

		private StringBuilder vehicleAltitudeText = new StringBuilder(10);

		private StringBuilder characterAirTimeText = new StringBuilder(10);

		private StringBuilder characterAltitudeText = new StringBuilder(10);

		private int vehicleSpeedValue;

		private int vehicleAltitudeValue;

		private int characterAirTimeValue;

		private int characterAltitudeValue;

		private bool metricText;

		public float revStartTime;

		private bool hudAvailable;

		public int videoPrizeVehicleUseCount;

		public GameItem videoPrizeVehicleItem;

		public int videoPrizeLevelUseCount;

		public GameItem videoPrizeLevelItem;

		public bool browsingCustomLevels;

		public bool customLevelsRefreshed;

		private int _currentHighScore;

		private int _metricUnits = -1;

		private int _manualControls = -1;

		public bool manualSteering;

		public GameObject currentVehicleInstance;

		public GameObject currentCharacterInstance;

		public StateManager.State stateAfterLevelLoad = StateManager.State.MainMenu;

		public bool setSavedCameraOrientation;

		public Statistics statistics = new Statistics();

		public Achievements achievements = new Achievements();

		private MrDismount character;

		private Vehicle vehicle;

		private Level level;

		private bool updateData;

		public bool IsBubbleHeadMode
		{
			get
			{
				return Prefs.GetInt("bubbleHeadMode", 0) == 1;
			}
			set
			{
				Prefs.SetInt("bubbleHeadMode", value ? 1 : 0);
				Prefs.Save();
			}
		}

		public bool multipleRagdollsActive
		{
			get
			{
				return Prefs.GetInt("multipleRagdollsActive", 0) == 1;
			}
			set
			{
				Prefs.SetInt("multipleRagdollsActive", value ? 1 : 0);
			}
		}

		public string currentLevel
		{
			get
			{
				return Prefs.GetString("currentLevel", "LevelDevelopment");
			}
			set
			{
				Prefs.SetString("currentLevel", value);
				Prefs.Save();
			}
		}

		public bool isUnpublishedLevel
		{
			get
			{
				return Prefs.GetInt("unpublishedLevel", 0) == 1;
			}
			set
			{
				Prefs.SetInt("unpublishedLevel", value ? 1 : 0);
				Prefs.Save();
			}
		}

		public bool isCustomLevel
		{
			get
			{
				return Utils.IsCustomLevelKey(currentLevel);
			}
		}

		public string customLevelFilename
		{
			get
			{
				return Prefs.GetString("customLevelFilename", string.Empty);
			}
			set
			{
				Prefs.SetString("customLevelFilename", value);
				Prefs.Save();
			}
		}

		public string playerName
		{
			get
			{
				return Prefs.GetString("playerName", "Guest");
			}
			set
			{
				Prefs.SetString("playerName", value);
				Prefs.Save();
			}
		}

		public SXJNI.TimeSpan leaderboardTimeSpan
		{
			get
			{
				return (SXJNI.TimeSpan)Prefs.GetInt("leaderboardTimeSpan", 0);
			}
			set
			{
				Prefs.SetInt("leaderboardTimeSpan", (int)value);
				Prefs.Save();
			}
		}

		public string friendFaceId
		{
			get
			{
				return Prefs.GetString("friendFaceId", string.Empty);
			}
			set
			{
				Prefs.SetString("friendFaceId", value);
				Prefs.Save();
			}
		}

		public bool showStats
		{
			get
			{
				return Prefs.GetInt("showStats", 0) != 0;
			}
			set
			{
				Prefs.SetInt("showStats", value ? 1 : 0);
				Prefs.Save();
			}
		}

		public int currentHighScore
		{
			get
			{
				return _currentHighScore ^ 0x34F432A;
			}
			set
			{
				_currentHighScore = value ^ 0x34F432A;
			}
		}

		public bool metricUnits
		{
			get
			{
				if (_metricUnits >= 0)
				{
					return _metricUnits == 1;
				}
				_metricUnits = (Utils.IsSystemMetric() ? 1 : 0);
				_metricUnits = Prefs.GetInt("metricUnits", _metricUnits);
				return _metricUnits == 1;
			}
			set
			{
				_metricUnits = (value ? 1 : 0);
				Prefs.SetInt("metricUnits", _metricUnits);
				Prefs.Save();
			}
		}

		public bool manualControls
		{
			get
			{
				if (_manualControls < 0)
				{
					_manualControls = Prefs.GetInt("manualControls", 0);
				}
				return _manualControls == 1;
			}
			set
			{
				_manualControls = (value ? 1 : 0);
			}
		}

		public void DecreaseVideoAdVehicleUseCount()
		{
			videoPrizeVehicleUseCount--;
			if (videoPrizeVehicleUseCount < 0)
			{
				videoPrizeVehicleUseCount = 0;
			}
		}

		public bool IsUsingTrialVehicle()
		{
			if (videoPrizeVehicleItem == null)
			{
				return false;
			}
			return currentVehicleItemId.CompareTo(videoPrizeVehicleItem.itemId) == 0;
		}

		public void DecreaseVideoAdLevelUseCount()
		{
			videoPrizeLevelUseCount--;
			if (videoPrizeLevelUseCount < 0)
			{
				videoPrizeLevelUseCount = 0;
			}
		}

		public bool IsUsingTrialLevel()
		{
			if (videoPrizeLevelItem == null)
			{
				return false;
			}
			return currentLevel.CompareTo(videoPrizeLevelItem.referenceName) == 0;
		}

		private void Start()
		{
			currentHighScore = Prefs.GetInt(currentLevel + ".highscore", 0);
			achievements.Init();
			statistics.Init();
		}

		public void ResetDismount()
		{
			currentHighScore = Prefs.GetInt(currentLevel + ".highscore", 0);
			statistics.ResetDismount();
			hudAvailable = true;
			ResetHUDData(ref statistics.realtime);
		}

		public void SaveHighscore()
		{
			Prefs.SetInt(currentLevel + ".highscore", currentHighScore);
			Prefs.Save();
		}

		public void SaveCamera()
		{
			if (!(DismountGame.cameraManager.currentCamera == null))
			{
				Prefs.SetString("cameraId", DismountGame.cameraManager.GetActiveCameraId());
				OrbitCamera component = DismountGame.cameraManager.currentCamera.GetComponent<OrbitCamera>();
				if ((bool)component)
				{
					Prefs.SetInt("cameraIsOrbit", 1);
					Prefs.SetFloat("cameraOrbitDistance", component.distance);
					Prefs.SetFloat("cameraOrbitEuler.x", component.euler.x);
					Prefs.SetFloat("cameraOrbitEuler.y", component.euler.y);
					Prefs.SetFloat("cameraOrbitEuler.z", component.euler.z);
				}
				else
				{
					Prefs.SetInt("cameraIsOrbit", 0);
				}
				FreeFlyCamera component2 = DismountGame.cameraManager.currentCamera.GetComponent<FreeFlyCamera>();
				if ((bool)component2)
				{
					Prefs.SetFloat("freeFlyEuler.x", component2.euler.x);
					Prefs.SetFloat("freeFlyEuler.y", component2.euler.y);
					Prefs.SetFloat("freeFlyEuler.z", component2.euler.z);
					Prefs.SetFloat("freeFlyPos.x", component2.position.x);
					Prefs.SetFloat("freeFlyPos.y", component2.position.y);
					Prefs.SetFloat("freeFlyPos.z", component2.position.z);
					Prefs.SetFloat("freeFlyFov", component2.fov);
				}
				Prefs.Save();
			}
		}

		public bool LoadCamera(bool setCameraOrientation)
		{
			string text = Prefs.GetString("cameraId", "None");
			if (text == "None")
			{
				return false;
			}
			DismountGame.cameraManager.SetActiveCamera(text);
			bool flag = Prefs.GetInt("cameraIsOrbit", 0) != 0;
			OrbitCamera component = DismountGame.cameraManager.currentCamera.GetComponent<OrbitCamera>();
			if (flag && component != null)
			{
				if (setCameraOrientation)
				{
					float num = 10f;
					Vector3 zero = Vector3.zero;
					num = Prefs.GetFloat("cameraOrbitDistance", 5f);
					zero.x = Prefs.GetFloat("cameraOrbitEuler.x", 0f);
					zero.y = Prefs.GetFloat("cameraOrbitEuler.y", 0f);
					zero.z = Prefs.GetFloat("cameraOrbitEuler.z", 0f);
					component.distance = num;
					component.euler = zero;
				}
				return true;
			}
			FreeFlyCamera component2 = DismountGame.cameraManager.currentCamera.GetComponent<FreeFlyCamera>();
			if ((bool)component2)
			{
				if (setCameraOrientation)
				{
					Vector3 zero2 = Vector3.zero;
					Vector3 zero3 = Vector3.zero;
					float num2 = 0f;
					zero2.x = Prefs.GetFloat("freeFlyEuler.x", 0f);
					zero2.y = Prefs.GetFloat("freeFlyEuler.y", 0f);
					zero2.z = Prefs.GetFloat("freeFlyEuler.z", 0f);
					zero3.x = Prefs.GetFloat("freeFlyPos.x", 0f);
					zero3.y = Prefs.GetFloat("freeFlyPos.y", 0f);
					zero3.z = Prefs.GetFloat("freeFlyPos.z", 0f);
					num2 = Prefs.GetFloat("freeFlyFov", 50f);
					component2.euler = zero2;
					component2.position = zero3;
					component2.fov = num2;
					DismountGame.cameraManager.hasValidFreeFlyPosition = true;
				}
				else
				{
					Transform transform = currentVehicleInstance.transform;
					Transform transform2 = component2.transform;
					transform2.position = transform.position + transform.forward * 30f + transform.right * 10f + Vector3.up * 10f;
					transform2.LookAt(transform.position, Vector3.up);
					component2.euler = transform2.eulerAngles;
					component2.position = transform2.position;
					component2.fov = 50f;
					DismountGame.cameraManager.hasValidFreeFlyPosition = true;
				}
				return true;
			}
			return true;
		}

		public void SaveVehicleAndCharacter()
		{
			Prefs.SetString("vehicle", currentVehicleItemId);
			Prefs.SetString("character", currentCharacterItemId);
			Prefs.SetString("head", currentHeadItemId);
			Prefs.SetInt("startPosition", currentCharacterStartPosition);
			Prefs.Save();
		}

		public void LoadVehicleAndCharacter()
		{
			currentVehicleItemId = Prefs.GetString("vehicle");
			currentCharacterItemId = Prefs.GetString("character");
			currentHeadItemId = Prefs.GetString("head");
			currentCharacterStartPosition = Prefs.GetInt("startPosition", 0);
			GameItem item = DismountGame.inventory.GetItem(currentVehicleItemId);
			if (item == null)
			{
				item = DismountGame.inventory.GetItem("vehicle.splitvan");
			}
			GameItem item2 = DismountGame.inventory.GetItem(currentCharacterItemId);
			if (item2 == null || item2.isLocked)
			{
				item2 = DismountGame.inventory.GetItem("character.mrdismount");
			}
			GameItem item3 = DismountGame.inventory.GetItem(currentHeadItemId);
			if (item3 == null)
			{
				item3 = DismountGame.inventory.GetItem("head.emptysquare");
			}
			currentVehicleItem = item;
			currentVehicleName = item.referenceName;
			currentCharacterItem = item2;
			currentCharacterName = item2.referenceName;
			currentHeadItem = item3;
			currentHeadName = item3.referenceName;
		}

		public bool CheckVehicleSave()
		{
			return Prefs.HasKey("vehicle");
		}

		public void LoadLevelSteerPath()
		{
			string key = currentLevel + ".steerPath";
			currentLevelSteerPath = Prefs.GetInt(key, 0);
		}

		public void SaveLevelSteerPath()
		{
			string key = currentLevel + ".steerPath";
			Prefs.SetInt(key, currentLevelSteerPath);
			Prefs.Save();
		}

		private void UpdateCurrentData()
		{
			statistics.realtime.characterAirTime = character.airTime;
			statistics.realtime.characterAltitude = character.cameraTarget.position.y;
			statistics.realtime.vehicleSpeed = vehicle.GetComponent<Rigidbody>().velocity.magnitude;
			statistics.realtime.vehicleAltitude = vehicle.transform.position.y - level.groundLevel;
			statistics.realtime.vehicleAirTime = vehicle.airTime;
		}

		private void ResetHUDStrings()
		{
			vehicleSpeedText = new StringBuilder();
			vehicleAltitudeText = new StringBuilder();
			characterAirTimeText = new StringBuilder();
			characterAltitudeText = new StringBuilder();
			metricText = metricUnits;
			if (metricUnits)
			{
				vehicleSpeedText.Append("000 km/h");
				vehicleAltitudeText.Append("000 m");
				characterAltitudeText.Append("000 m");
			}
			else
			{
				vehicleSpeedText.Append("000 mph");
				vehicleAltitudeText.Append("000 ft");
				characterAltitudeText.Append("000 ft");
			}
			characterAirTimeText.Append("--.- s");
			vehicleSpeedValue = 0;
			vehicleAltitudeValue = 0;
			characterAirTimeValue = 0;
			characterAltitudeValue = 0;
		}

		private void ResetHUDData(ref Statistics.RealtimeStatistics data)
		{
			vehicleSpeed.Reset(data.vehicleSpeed);
			vehicleAltitude.Reset(data.vehicleAltitude);
			characterAirTime.Reset(data.characterAirTime);
			characterAltitude.Reset(data.characterAltitude);
			ResetHUDStrings();
			Update();
		}

		private void DisplayHUDData(ref Statistics.RealtimeStatistics data)
		{
			bool forward = true;
			Replay instance = Replay.Instance;
			if (instance.Activity == Replay.ReplayActivity.Playback && instance.PlaybackSpeed < 0f)
			{
				forward = false;
			}
			vehicleSpeed.Update(data.vehicleSpeed, forward);
			vehicleAltitude.Update(data.vehicleAltitude, forward);
			characterAirTime.Update(data.characterAirTime, forward);
			characterAltitude.Update(data.characterAltitude, forward);
		}

		public void OnDismountStarted(float throttle)
		{
			character = currentCharacterInstance.GetComponent<MrDismount>();
			vehicle = currentVehicleInstance.GetComponent<Vehicle>();
			level = DismountGame.level;
			statistics.DismountStarted(throttle);
			updateData = true;
		}

		public void OnDismountAborted()
		{
			updateData = false;
		}

		public void OnDismountComplete(bool nailedIt)
		{
			UpdateCurrentData();
			updateData = false;
			SXJNI.Instance.SubmitScore(DismountGame.playerState.levelItem.GPGSId, statistics.realtime.score, (bool success) =>
			{
			});
			if (statistics.realtime.score > currentHighScore)
			{
				currentHighScore = statistics.realtime.score;
				SaveHighscore();
				string text = DismountGame.playerState.playerName;
				if (text == string.Empty)
				{
					text = "Guest";
				}
				if (!(DismountGame.playerState.levelItem != null))
				{
				}
			}
			statistics.DismountComplete(nailedIt);
		}

		private void ReportScoreSuccessfull(string leaderboardId)
		{
			if (leaderboardId == DismountGame.playerState.levelItem.itemId)
			{
				Steamworks.GetGlobalLeaderboardEntries(leaderboardId, GetGlobalLeaderboardEntriesComplete);
			}
		}

		private void GetGlobalLeaderboardEntriesComplete(List<Leaderboards.LeaderboardEntry> entries, int playerIndex)
		{
			DismountGame.uiManager.PopulateLeaderboard(entries, playerIndex);
		}

		private void FixedUpdate()
		{
			if (updateData)
			{
				UpdateCurrentData();
				statistics.EvaluateRealtime();
			}
			DisplayHUDData(ref statistics.realtime);
		}

		private void Update()
		{
			if (hudAvailable)
			{
				if (metricText != metricUnits)
				{
					ResetHUDStrings();
				}
				HUDManager hudManager = DismountGame.hudManager;
				vehicleSpeedValue = hudManager.DisplayVehicleSpeed(vehicleSpeedText, vehicleSpeedValue, vehicleSpeed);
				vehicleAltitudeValue = hudManager.DisplayVehicleAltitude(vehicleAltitudeText, vehicleAltitudeValue, vehicleAltitude);
				characterAirTimeValue = hudManager.DisplayCharacterAirtime(characterAirTimeText, characterAirTimeValue, characterAirTime);
				characterAltitudeValue = hudManager.DisplayCharacterAltitude(characterAltitudeText, characterAltitudeValue, characterAltitude);
			}
		}

		public bool hasItemBeenUsedBefore(string itemName)
		{
			return Prefs.GetInt(itemName + "_used", 0) == 1;
		}

		public void setItemHasBeenUsed(string itemName)
		{
			Prefs.SetInt(itemName + "_used", 1);
		}
	}
}
