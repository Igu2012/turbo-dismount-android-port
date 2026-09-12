#pragma warning disable 0618,0619
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using reLive;

namespace Dismount
{
	public class HUDManager : MonoBehaviour
	{
		public enum DamageIconType
		{
			BreakBone = 0,
			BreakRibs = 1,
			BreakSkull = 2,
			BreakSpine = 3,
			BreakHip = 4,
			DetachLimbJoint = 5,
			DetachNeck = 6,
			DetachSpine = 7,
			Flip = 8,
			Airtime = 9,
			Vehicle = 10,
			VehicleDetach = 11
		}

		public class DamageIcon
		{
			private int currentSlot;

			private float spawnTime;

			public bool isActive;

			public GameObject go;

			private Vector3 firstSlotPosition;

			private Vector3 firstSlotSize;

			private Vector3 currentSlotSize;

			private TweenScale scaleTweener;

			private TweenPosition positionTweener;

			private TweenAlpha alphaTweener;

			private UISprite sprite;

			public DamageIcon(GameObject cloneSource)
			{
				go = Object.Instantiate(cloneSource) as GameObject;
				go.transform.parent = cloneSource.transform.parent;
				firstSlotPosition = cloneSource.transform.localPosition;
				firstSlotSize = cloneSource.transform.localScale;
				currentSlotSize = firstSlotSize;
				go.transform.localPosition = firstSlotPosition;
				go.transform.localScale = firstSlotSize;
				scaleTweener = go.GetComponent<TweenScale>();
				positionTweener = go.GetComponent<TweenPosition>();
				alphaTweener = go.GetComponent<TweenAlpha>();
				scaleTweener.duration = 0.1f;
				positionTweener.duration = 0.1f;
				alphaTweener.duration = 0.1f;
				sprite = go.GetComponent<UISprite>();
				go.SetActive(false);
				go.AddComponent<RecordLossyScale>();
			}

			public void Spawn(DamageIconType type)
			{
				isActive = true;
				currentSlot = 0;
				spawnTime = Time.time;
				go.SetActive(true);
				currentSlotSize = firstSlotSize;
				scaleTweener.from.x = 0.1f;
				scaleTweener.from.y = 0.1f;
				scaleTweener.to.x = firstSlotSize.x;
				scaleTweener.to.y = firstSlotSize.y;
				scaleTweener.Reset();
				scaleTweener.Play(true);
				positionTweener.from = firstSlotPosition;
				positionTweener.to = firstSlotPosition;
				positionTweener.Reset();
				positionTweener.Play(true);
				alphaTweener.from = 0f;
				alphaTweener.to = 1f;
				alphaTweener.Reset();
				alphaTweener.Play(true);
				sprite.spriteName = "Damage_" + type;
			}

			public void Update()
			{
				if (Time.time - spawnTime > 2f)
				{
					Timeout();
				}
			}

			private void Timeout()
			{
				if (alphaTweener.to != 0f)
				{
					alphaTweener.from = 1f;
					alphaTweener.to = 0f;
					alphaTweener.Reset();
					alphaTweener.Play(true);
				}
			}

			public void Despawn()
			{
				isActive = false;
				go.SetActive(false);
			}

			public void NextSlot()
			{
				currentSlot++;
				if (alphaTweener.to == 0f && alphaTweener.alpha == 0f)
				{
					Despawn();
					return;
				}
				if (currentSlot == 7 && alphaTweener.to != 0f)
				{
					alphaTweener.from = 1f;
					alphaTweener.to = 0f;
					alphaTweener.Reset();
					alphaTweener.Play(true);
				}
				currentSlotSize *= 0.95f;
				scaleTweener.from.x = scaleTweener.to.x;
				scaleTweener.from.y = scaleTweener.to.y;
				scaleTweener.to.x = currentSlotSize.x;
				scaleTweener.to.y = currentSlotSize.y;
				scaleTweener.Reset();
				scaleTweener.Play(true);
				positionTweener.from = go.transform.localPosition;
				positionTweener.to = go.transform.localPosition - new Vector3(0f, currentSlotSize.y, 0f);
				positionTweener.Reset();
				positionTweener.Play(true);
			}
		}

		public enum StatsMode
		{
			None = 0,
			Character = 1,
			Vehicle = 2
		}

		private const int numDamageIcons = 7;

		private UIAnchor[] uiAnchors;

		private List<DamageIconType> iconQueue = new List<DamageIconType>();

		private DamageIcon[] damageIcons = new DamageIcon[14];

		private GameObject damageIconPrefab;

		private Transform hudTextPanel;

		private GameObject hudTextElement;

		private UILabel scoreLabel;

		private UILabel highScoreLabel;

		private UILabel multiplierLabel;

		private MultiplierVisualTweak multiplierTweak;

		private GameObject dismountComplete;

		private GameObject dismountCompleteSign;

		private GameObject newHighScoreSign;

		private GameObject nailedItSign;

		private GameObject stalledSign;

		private GameObject dismountGo;

		private GameObject goSign;

		private GameObject fullSpeedSign;

		private ParticleSystem[] dismountCompleteParticleSystems;

		private GameObject instantReplay;

		private UITweener instantReplayTweener;

		private UILabel cameraName;

		private GameObject cameraNameObject;

		private UITweener cameraNameTweener;

		private List<KeyValuePair<GameObject, float>> notificationQueue = new List<KeyValuePair<GameObject, float>>();

		private GameObject notificationPanel;

		private GameObject notificationLabel;

		private Camera worldCamera;

		private Camera hudCamera;

		private GameObject ragdollStats;

		private GameObject vehicleStats;

		private Transform ragdollStatsFollowTarget;

		private Transform vehicleStatsFollowTarget;

		private UILabel vehicleSpeedLabel;

		private UILabel vehicleAltitudeLabel;

		private UILabel ragdollAirtimeLabel;

		private UILabel ragdollAltitudeLabel;

		private UILabel vehicleTrialsLeft;

		private UILabel levelTrialsLeft;

		private static Color textColor = new Color(1f, 1f, 1f);

		private static Color blinkColor = new Color(0.5f, 0.75f, 1f);

		private StatsMode statsMode;

		private bool statsEnabled;

		private bool _renderingEnabled = true;

		private int previousScoreUpdated = -1;

		private int previousHiscoreUpdated = -1;

		private int previousMultiplierUpdated = -1;

		private List<int> removeIndices = new List<int>();

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

		public DamageIcon[] GetDamageIcons()
		{
			return damageIcons;
		}

		private void EnableRendering()
		{
			hudCamera.gameObject.SetActive(true);
		}

		private void DisableRendering()
		{
			hudCamera.gameObject.SetActive(false);
		}

		private void Start()
		{
			uiAnchors = GetComponentsInChildren<UIAnchor>();
			damageIconPrefab = base.transform.Find("Camera/TopRight/Panel/DamageIcon").gameObject;
			for (int i = 0; i < damageIcons.Length; i++)
			{
				damageIcons[i] = new DamageIcon(damageIconPrefab);
			}
			damageIconPrefab.SetActive(false);
			InvokeRepeating("ProcessDamageIconQueue", 0f, 0.1f);
			scoreLabel = base.transform.Find("Camera/TopRight/Panel/Score").gameObject.GetComponent<UILabel>();
			highScoreLabel = base.transform.Find("Camera/TopRight/Panel/HighScore").gameObject.GetComponent<UILabel>();
			multiplierLabel = base.transform.Find("Camera/TopRight/Panel/Multiplier").gameObject.GetComponent<UILabel>();
			multiplierTweak = multiplierLabel.GetComponent<MultiplierVisualTweak>();
			dismountComplete = base.transform.Find("Camera/Center/DismountComplete").gameObject;
			dismountCompleteSign = base.transform.Find("Camera/Center/DismountComplete/DismountCompleteSign").gameObject;
			newHighScoreSign = base.transform.Find("Camera/Center/DismountComplete/NewHighScoreSign").gameObject;
			nailedItSign = base.transform.Find("Camera/Center/DismountComplete/NailedItSign").gameObject;
			stalledSign = base.transform.Find("Camera/Center/DismountComplete/StalledSign").gameObject;
			dismountCompleteParticleSystems = dismountComplete.GetComponentsInChildren<ParticleSystem>();
			dismountComplete.SetActive(false);
			dismountCompleteSign.SetActive(false);
			newHighScoreSign.SetActive(false);
			nailedItSign.SetActive(false);
			stalledSign.SetActive(false);
			DisableDismountCompleteParticles();
			dismountGo = base.transform.Find("Camera/Center/DismountGo").gameObject;
			goSign = base.transform.Find("Camera/Center/DismountGo/GoSign").gameObject;
			fullSpeedSign = base.transform.Find("Camera/Center/DismountGo/FullspeedSign").gameObject;
			dismountGo.SetActive(false);
			goSign.SetActive(false);
			fullSpeedSign.SetActive(false);
			instantReplay = base.transform.Find("Camera/Top/Instant Replay").gameObject;
			instantReplayTweener = instantReplay.GetComponent<UITweener>();
			instantReplay.SetActive(false);
			hudTextPanel = base.transform.Find("Camera/Center/HUDTextPanel");
			hudTextElement = base.transform.Find("Camera/Center/HUDTextPanel/HUDTextElement").gameObject;
			hudTextElement.SetActive(false);
			cameraNameObject = base.transform.Find("Camera/Top/Panel/CameraName").gameObject;
			cameraName = cameraNameObject.GetComponent<UILabel>();
			cameraNameTweener = cameraNameObject.GetComponent<UITweener>();
			notificationPanel = base.transform.Find("Camera/Top/Notifications").gameObject;
			notificationLabel = base.transform.Find("Camera/Top/Notifications/Label").gameObject;
			notificationLabel.SetActive(false);
			worldCamera = DismountGame.cameraManager.globalCamera.GetComponent<Camera>();
			hudCamera = base.transform.Find("Camera").GetComponent<Camera>();
			vehicleStats = base.transform.Find("Camera/TopLeft/Panel/VehicleStats").gameObject;
			ragdollStats = base.transform.Find("Camera/TopLeft/Panel/RagdollStats").gameObject;
			vehicleSpeedLabel = vehicleStats.transform.Find("Offset/SpeedLabel").GetComponent<UILabel>();
			vehicleAltitudeLabel = vehicleStats.transform.Find("Offset/AltitudeLabel").GetComponent<UILabel>();
			ragdollAirtimeLabel = ragdollStats.transform.Find("Offset/AirtimeLabel").GetComponent<UILabel>();
			ragdollAltitudeLabel = ragdollStats.transform.Find("Offset/AltitudeLabel").GetComponent<UILabel>();
			vehicleStats.SetActive(false);
			ragdollStats.SetActive(false);
			vehicleTrialsLeft = base.transform.Find("Camera/Top/Panel/VehicleTryLabel").gameObject.GetComponent<UILabel>();
			levelTrialsLeft = base.transform.Find("Camera/Top/Panel/LevelTryLabel").gameObject.GetComponent<UILabel>();
		}

		public void UpdateTrialsLeftLabels()
		{
			float num = -10f;
			if (DismountGame.stateManager.currentState == StateManager.State.ReplayMode)
			{
				num -= 50f;
			}
			if (DismountGame.playerState.IsUsingTrialVehicle() && DismountGame.playerState.videoPrizeVehicleUseCount != 0)
			{
				vehicleTrialsLeft.gameObject.SetActive(true);
				Vector3 localPosition = vehicleTrialsLeft.transform.localPosition;
				vehicleTrialsLeft.transform.localPosition = new Vector3(localPosition.x, num, localPosition.z);
				vehicleTrialsLeft.text = "Test drives remaining: " + DismountGame.playerState.videoPrizeVehicleUseCount;
				num -= 20f;
			}
			else
			{
				vehicleTrialsLeft.gameObject.SetActive(false);
			}
			if (DismountGame.playerState.IsUsingTrialLevel() && DismountGame.playerState.videoPrizeLevelUseCount != 0)
			{
				levelTrialsLeft.gameObject.SetActive(true);
				Vector3 localPosition2 = levelTrialsLeft.transform.localPosition;
				levelTrialsLeft.transform.localPosition = new Vector3(localPosition2.x, num, localPosition2.z);
				levelTrialsLeft.text = "Level uses remaining: " + DismountGame.playerState.videoPrizeLevelUseCount;
			}
			else
			{
				levelTrialsLeft.gameObject.SetActive(false);
			}
		}

		private void UpdateScoreLabel(int score)
		{
			if (score != previousScoreUpdated)
			{
				previousScoreUpdated = score;
				scoreLabel.text = "Score: " + score;
			}
		}

		private void UpdateHiscoreLabel(int score)
		{
			if (score != previousHiscoreUpdated)
			{
				previousHiscoreUpdated = score;
				highScoreLabel.text = "High Score: " + score;
			}
		}

		private void UpdateMultiplierLabel(int multiplier)
		{
			if (multiplier != previousMultiplierUpdated)
			{
				previousMultiplierUpdated = multiplier;
				if (multiplier == 0)
				{
					multiplierLabel.text = string.Empty;
				}
				else
				{
					multiplierLabel.text = multiplier + "x";
				}
			}
		}

		private void Update()
		{
			for (int i = 0; i < damageIcons.Length; i++)
			{
				if (damageIcons[i].isActive)
				{
					damageIcons[i].Update();
				}
			}
			if ((bool)DismountGame.playerState)
			{
				UpdateScoreLabel(DismountGame.playerState.statistics.realtime.displayScore);
				int score = DismountGame.playerState.currentHighScore;
				int displayScore = DismountGame.playerState.statistics.realtime.displayScore;
				if (displayScore > DismountGame.playerState.currentHighScore)
				{
					score = displayScore;
				}
				UpdateHiscoreLabel(score);
				int displayMultiplier = DismountGame.playerState.statistics.realtime.displayMultiplier;
				UpdateMultiplierLabel(displayMultiplier);
				if (displayMultiplier != 0)
				{
					MultiplierVisualTweak.MultiplierParams multiplierParams = multiplierTweak.paramsPerMultiplier[displayMultiplier];
					multiplierLabel.color = multiplierParams.fillColor;
					multiplierLabel.effectColor = multiplierParams.outlineColor;
					Vector3 localScale = multiplierLabel.transform.localScale;
					localScale.x = (localScale.y = multiplierParams.size);
					multiplierLabel.transform.localScale = localScale;
				}
			}
			removeIndices.Clear();
			float time = Time.time;
			for (int j = 0; j < notificationQueue.Count; j++)
			{
				if (time > notificationQueue[j].Value)
				{
					removeIndices.Add(j);
				}
			}
			for (int num = removeIndices.Count - 1; num >= 0; num--)
			{
				Object.Destroy(notificationQueue[num].Key);
				notificationQueue.RemoveAt(num);
			}
			Vector3 localPosition = notificationLabel.transform.localPosition;
			for (int k = 0; k < notificationQueue.Count; k++)
			{
				GameObject key = notificationQueue[k].Key;
				Vector3 localPosition2 = key.transform.localPosition;
				localPosition2.y = localPosition.y + (float)(k * -30);
				TweenPosition tweenPosition = key.GetComponent<TweenPosition>();
				if (!tweenPosition)
				{
					tweenPosition = key.AddComponent<TweenPosition>();
				}
				tweenPosition.from = key.transform.localPosition;
				tweenPosition.to = localPosition2;
				tweenPosition.ignoreTimeScale = true;
				tweenPosition.method = UITweener.Method.EaseOut;
				tweenPosition.duration = 0.25f;
				tweenPosition.Reset();
			}
		}

		public Vector3 ProjectToHUDPosition(Vector3 worldPos)
		{
			return hudCamera.ViewportToWorldPoint(worldCamera.WorldToViewportPoint(worldPos));
		}

		public void ProjectHUDElement(GameObject hudElement, Vector3 worldPos)
		{
			Transform transform = hudElement.transform;
			transform.position = ProjectToHUDPosition(worldPos);
			Vector3 localPosition = transform.localPosition;
			localPosition.x = Mathf.FloorToInt(localPosition.x);
			localPosition.y = Mathf.FloorToInt(localPosition.y);
			localPosition.z = 0f;
			transform.localPosition = localPosition;
		}

		public void Reset()
		{
			iconQueue.Clear();
			for (int i = 0; i < damageIcons.Length; i++)
			{
				damageIcons[i].Despawn();
			}
			dismountComplete.SetActive(false);
			dismountGo.SetActive(false);
			DisableDismountCompleteParticles();
			ClearHUDTexts();
			cameraNameObject.SetActive(false);
		}

		public HUDText SpawnHUDText(object obj, Color color, float stayDuration, HUDText hudTextInstance, Transform followTransform)
		{
			HUDText hUDText = hudTextInstance;
			if (hUDText == null)
			{
				GameObject gameObject = Object.Instantiate(hudTextElement) as GameObject;
				gameObject.SetActive(true);
				gameObject.transform.parent = hudTextElement.transform.parent;
				gameObject.transform.localScale = Vector3.one;
				hUDText = gameObject.GetComponent<HUDText>();
			}
			hUDText.Add(obj, color, stayDuration);
			UIFollowTarget component = hUDText.GetComponent<UIFollowTarget>();
			if (followTransform != null)
			{
				component.enabled = true;
				component.gameCamera = DismountGame.cameraManager.globalCamera.GetComponent<Camera>();
				component.target = followTransform;
			}
			else
			{
				component.enabled = false;
			}
			return hUDText;
		}

		private void ClearHUDTexts()
		{
			HUDText[] componentsInChildren = hudTextPanel.GetComponentsInChildren<HUDText>();
			HUDText[] array = componentsInChildren;
			foreach (HUDText hUDText in array)
			{
				if (hUDText.gameObject != hudTextElement)
				{
					hUDText.gameObject.SetActive(false);
					Object.Destroy(hUDText.gameObject);
				}
			}
		}

		private DamageIcon FindFreeDamageIcon()
		{
			for (int i = 0; i < damageIcons.Length; i++)
			{
				if (!damageIcons[i].isActive)
				{
					return damageIcons[i];
				}
			}
			return null;
		}

		private void SpawnNewDamageIcon(DamageIconType type)
		{
			for (int i = 0; i < damageIcons.Length; i++)
			{
				if (damageIcons[i].isActive)
				{
					damageIcons[i].NextSlot();
				}
			}
			DamageIcon damageIcon = FindFreeDamageIcon();
			damageIcon.Spawn(type);
		}

		private void ProcessDamageIconQueue()
		{
			if (iconQueue.Count > 0)
			{
				DamageIconType type = iconQueue[0];
				iconQueue.RemoveRange(0, 1);
				SpawnNewDamageIcon(type);
			}
		}

		public void AddDamageIcon(DamageIconType type)
		{
			iconQueue.Add(type);
		}

		public void ShowDismountComplete(string result)
		{
			newHighScoreSign.SetActive(false);
			dismountCompleteSign.SetActive(false);
			nailedItSign.SetActive(false);
			stalledSign.SetActive(false);
			switch (result)
			{
			case "complete":
				DismountGame.audioManager.PlaySoundEffect("Result", null, 1f, true);
				dismountCompleteSign.SetActive(true);
				break;
			case "highscore":
				DismountGame.audioManager.PlaySoundEffect("Congrats", null, 1f, true);
				newHighScoreSign.SetActive(true);
				Invoke("EnableDismountCompleteParticles", 0.4f);
				break;
			case "nailed":
				DismountGame.audioManager.PlaySoundEffect("NailedIt", null, 1f, true);
				nailedItSign.SetActive(true);
				break;
			case "stalled":
				DismountGame.audioManager.PlaySoundEffect("NailedIt", null, 1f, true);
				stalledSign.SetActive(true);
				break;
			}
			dismountComplete.SetActive(true);
			UITweener[] componentsInChildren = dismountComplete.GetComponentsInChildren<UITweener>();
			UITweener[] array = componentsInChildren;
			foreach (UITweener uITweener in array)
			{
				uITweener.Play(true);
			}
		}

		public void ShowDismountGo(bool Go, bool Fullspeed)
		{
			SetDismountGoAlpha(1f);
			dismountGo.SetActive(true);
			goSign.SetActive(Go && !Fullspeed);
			fullSpeedSign.SetActive(Go && Fullspeed);
			UITweener[] componentsInChildren = dismountGo.GetComponentsInChildren<UITweener>();
			UITweener[] array = componentsInChildren;
			foreach (UITweener uITweener in array)
			{
				uITweener.Reset();
				uITweener.Play(true);
			}
		}

		public void HideDismountGo()
		{
			dismountGo.SetActive(false);
		}

		public void SetDismountGoAlpha(float alpha)
		{
			goSign.GetComponent<UISprite>().alpha = alpha;
			fullSpeedSign.GetComponent<UISprite>().alpha = alpha;
		}

		private void DisableDismountCompleteParticles()
		{
			CancelInvoke("EnableDismountCompleteParticles");
			ParticleSystem[] array = dismountCompleteParticleSystems;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.Stop();
			}
		}

		private void EnableDismountCompleteParticles()
		{
			ParticleSystem[] array = dismountCompleteParticleSystems;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.startDelay = Random.Range(0f, 0.25f);
				particleSystem.Play();
			}
		}

		public void HideDismountComplete()
		{
			dismountComplete.SetActive(true);
			UITweener[] componentsInChildren = dismountComplete.GetComponentsInChildren<UITweener>();
			UITweener[] array = componentsInChildren;
			foreach (UITweener uITweener in array)
			{
				uITweener.Play(false);
			}
			DisableDismountCompleteParticles();
			CancelInvoke("EnableDismountCompleteParticles");
		}

		public void RemoveParticles()
		{
			ParticleSystem[] array = dismountCompleteParticleSystems;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.Clear();
			}
		}

		public void Enable()
		{
			base.gameObject.SetActive(true);
		}

		public void Disable()
		{
			base.gameObject.SetActive(false);
		}

		public void EnableInstantReplay()
		{
			instantReplay.SetActive(true);
			instantReplayTweener.Reset();
			instantReplayTweener.Play(true);
		}

		public void DisableInstantReplay()
		{
			instantReplay.SetActive(false);
		}

		public void ShowCameraName(string name)
		{
			cameraNameObject.SetActive(true);
			cameraName.text = name;
			cameraNameTweener.Reset();
			cameraNameTweener.Play(true);
		}

		public void ShowNotifications()
		{
			notificationPanel.SetActive(true);
		}

		public void HideNotifications()
		{
			notificationPanel.SetActive(true);
		}

		public void AddNotification(string text)
		{
			GameObject gameObject = Object.Instantiate(notificationLabel) as GameObject;
			gameObject.transform.parent = notificationLabel.transform.parent;
			gameObject.transform.localPosition = notificationLabel.transform.localPosition;
			gameObject.transform.localRotation = notificationLabel.transform.localRotation;
			gameObject.transform.localScale = notificationLabel.transform.localScale;
			Vector3 localPosition = gameObject.transform.localPosition;
			localPosition.y += (float)notificationQueue.Count * -30f;
			gameObject.transform.localPosition = localPosition;
			UILabel component = gameObject.GetComponent<UILabel>();
			component.text = text;
			gameObject.SetActive(true);
			notificationQueue.Add(new KeyValuePair<GameObject, float>(gameObject, Time.time + 3f));
		}

		public void SetStatsMode(StatsMode mode)
		{
			statsMode = mode;
			if (statsEnabled)
			{
				ragdollStats.SetActive(true);
				vehicleStats.SetActive(true);
			}
		}

		public void SetStatsTargets(Transform vehicleFollowTarget, Transform ragdollFollowTarget)
		{
			ragdollStatsFollowTarget = ragdollFollowTarget;
			vehicleStatsFollowTarget = vehicleFollowTarget;
		}

		public void EnableStats(bool enabled)
		{
			statsEnabled = enabled;
			if (enabled)
			{
				ragdollStats.SetActive(true);
				vehicleStats.SetActive(true);
			}
			else
			{
				vehicleStats.SetActive(false);
				ragdollStats.SetActive(false);
			}
		}

		private static int DisplayTripleDigits(UILabel label, StringBuilder text, int currentValue, bool isTime, float displayValue, float peakBlink, float toMetric)
		{
			int num = Mathf.FloorToInt(displayValue * toMetric);
			if (num < 0)
			{
				num = 0;
			}
			else if (num > 999)
			{
				num = 999;
			}
			if (currentValue != num)
			{
				if (isTime)
				{
					if (num == 0)
					{
						text[0] = '-';
						text[1] = '-';
						text[3] = '-';
					}
					else
					{
						text[0] = (char)(48 + num / 100 % 10);
						text[1] = (char)(48 + num / 10 % 10);
						text[3] = (char)(48 + num % 10);
					}
				}
				else
				{
					text[0] = (char)(48 + num / 100 % 10);
					text[1] = (char)(48 + num / 10 % 10);
					text[2] = (char)(48 + num % 10);
				}
				label.text = text.ToString();
			}
			if (peakBlink > 0f)
			{
				label.color = Color.Lerp(textColor, blinkColor, peakBlink);
			}
			else
			{
				label.color = textColor;
			}
			return num;
		}

		public int DisplayVehicleSpeed(StringBuilder text, int currentValue, NumericalStatistic speed)
		{
			if (!vehicleStats.activeSelf)
			{
				return currentValue;
			}
			return DisplayTripleDigits(vehicleSpeedLabel, text, currentValue, false, speed.displayValue, speed.peakBlink, (!DismountGame.playerState.metricUnits) ? 2.23694f : 3.6f);
		}

		public int DisplayVehicleAltitude(StringBuilder text, int currentValue, NumericalStatistic altitude)
		{
			if (!vehicleStats.activeSelf)
			{
				return currentValue;
			}
			return DisplayTripleDigits(vehicleAltitudeLabel, text, currentValue, false, altitude.displayValue, altitude.peakBlink, (!DismountGame.playerState.metricUnits) ? 3.28084f : 1f);
		}

		public int DisplayCharacterAltitude(StringBuilder text, int currentValue, NumericalStatistic altitude)
		{
			if (!ragdollStats.activeSelf)
			{
				return currentValue;
			}
			return DisplayTripleDigits(ragdollAltitudeLabel, text, currentValue, false, altitude.displayValue, altitude.peakBlink, (!DismountGame.playerState.metricUnits) ? 3.28084f : 1f);
		}

		public int DisplayCharacterAirtime(StringBuilder text, int currentValue, NumericalStatistic airtime)
		{
			if (!ragdollStats.activeSelf)
			{
				return currentValue;
			}
			float num = airtime.displayValue;
			if (num <= 1f)
			{
				num = 0f;
			}
			return DisplayTripleDigits(ragdollAirtimeLabel, text, currentValue, true, num, airtime.peakBlink, 10f);
		}
	}
}
