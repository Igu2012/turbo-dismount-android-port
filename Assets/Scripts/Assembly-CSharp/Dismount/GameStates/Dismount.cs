#pragma warning disable 0618,0619
using System;
using System.Collections;
using System.Collections.Generic;
using Dismount.Vehicular;
using UnityEngine;
using reLive;

namespace Dismount.GameStates
{
	public class Dismount : MonoBehaviour
	{
		private enum DismountState
		{
			Init = 0,
			DismountSetup = 1,
			RevingEngine = 2,
			DismountActive = 3,
			DismountComplete = 4
		}

		private struct InvokeParams
		{
			public string methodName;

			public float time;
		}

		public enum ScoreCategory
		{
			Ragdoll = 0,
			Vehicle = 1
		}

		private DismountState currentState;

		private UILabel startCounter;

		private UISprite startCounterSprite;

		private Collider setupScene;

		private ScoreMultiplier scoreMultiplier = new ScoreMultiplier();

		private Vehicle vehicle;

		private MrDismount character;

		private float hudTextTime = -2f;

		private HUDText hudTextInstance;

		private Transform hudTextFollowTransform;

		private int dismountEndedCount;

		private float dismountStartTime = -1f;

		private float dismountEndTime = -1f;

		private bool dismountEnding;

		private bool countDownCanceled;

		private bool stalled;

		private AnimatorFloat scoreAnimator = new AnimatorFloat();

		private AnimatorInt multiplierAnimator = new AnimatorInt();

		private List<InvokeParams> invokes = new List<InvokeParams>();

		private EndZoneChecker[] endZones;

		private bool initialized;

		private UIManager.State previousUIState;

		private float steeringStartX = -1f;

		private float steerAngle;

		private bool steeringWheelDown;

		private Transform steeringWheelButton;

		private bool forceLevelSelect;

		private bool forceVehicleSelect;

		private static int dismountsBeforeNag = 5;

		private float targetThrottle;

		private float powerBarValue;

		private int ragdollScore;

		private int vehicleScore;

		private UIManager.State stateBeforeNag;

		public bool isDismountActive
		{
			get
			{
				return currentState == DismountState.DismountActive;
			}
		}

		private void EnterState()
		{
			DismountGame.hudManager.Enable();
			DismountGame.level.ShowSteerPath();
			invokes.Clear();
			if (!initialized)
			{
				string text = "/DismountGame";
				startCounter = base.transform.Find(text + "/HUD/Camera/Center/StartPanel/StartCounter").GetComponent<UILabel>();
				startCounterSprite = base.transform.Find(text + "/HUD/Camera/Center/StartPanel/Sprite").GetComponent<UISprite>();
				startCounterSprite.gameObject.SetActive(false);
				startCounter.gameObject.SetActive(false);
				steeringWheelButton = DismountGame.uiManager.GetSteeringWheelButton().transform;
				initialized = true;
			}
			if (!DismountGame.playerState.LoadCamera(DismountGame.playerState.setSavedCameraOrientation))
			{
				Debug.LogWarning("Wasn't able to restore camera");
				DismountGame.cameraManager.SetActiveCamera("Character");
			}
			vehicle = DismountGame.playerState.currentVehicleInstance.GetComponent<Vehicle>();
			vehicle.inputBrake = 1f;
			character = DismountGame.playerState.currentCharacterInstance.GetComponent<MrDismount>();
			DismountGame.cameraManager.SetCameraTarget("Character", character.cameraTarget);
			DismountGame.cameraManager.SetCameraTarget("Vehicle", vehicle.cameraTarget);
			DismountGame.cameraManager.SetCameraTarget("1stPerson", character.firstPersonCameraHead);
			DismountGame.cameraManager.SetCameraTarget("Scene", character.cameraTarget, vehicle.cameraTarget);
			vehicle.SetGameLogic(this);
			character.SetGameLogic(this);
			steeringStartX = -1f;
			steerAngle = 0f;
			steeringWheelDown = false;
			vehicle.inputSteering = 0f;
			scoreAnimator.Reset(0f);
			scoreAnimator.duration = 0.2f;
			scoreAnimator.interpolator = AnimatorBase.Interpolator.Linear;
			scoreMultiplier.Reset();
			multiplierAnimator.Reset(0);
			multiplierAnimator.duration = 0.2f;
			multiplierAnimator.interpolator = AnimatorBase.Interpolator.Linear;
			ResetLogic();
			endZones = UnityEngine.Object.FindObjectsOfType<EndZoneChecker>();
			DismountGame.uiManager.ChangeState(UIManager.State.DismountStarting);
			Replay.ReclaimChannelDataItems();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Resources.UnloadUnusedAssets();
			DismountGame.uiManager.OpenDoor();
			DismountGame.hudManager.ShowNotifications();
			forceLevelSelect = false;
			if (DismountGame.playerState.videoPrizeLevelUseCount <= 0 && (bool)DismountGame.playerState.videoPrizeLevelItem && DismountGame.playerState.videoPrizeLevelItem.referenceName == DismountGame.playerState.currentLevel)
			{
				forceLevelSelect = true;
				DismountGame.uiManager.ChangeState(UIManager.State.LevelSelect);
			}
			forceVehicleSelect = false;
			if (!forceLevelSelect && DismountGame.playerState.videoPrizeVehicleUseCount <= 0 && (bool)DismountGame.playerState.videoPrizeVehicleItem && DismountGame.playerState.videoPrizeVehicleItem.referenceName == DismountGame.playerState.currentVehicleName)
			{
				forceVehicleSelect = true;
				DismountGame.uiManager.ChangeState(UIManager.State.SetupVehicle);
			}
			DismountGame.hudManager.UpdateTrialsLeftLabels();
		}

		private void ExitState()
		{
			DismountGame.level.OnDismountExit();
			CancelMyInvokes();
			Replay.Instance.StopRecordingDontPostprocess();
			DismountGame.playerState.OnDismountAborted();
			DismountGame.uiManager.StopHelpSequence();
			HideAllControls();
			DismountGame.hudManager.EnableStats(false);
			DismountGame.hudManager.HideNotifications();
			base.gameObject.SetActive(false);
		}

		private void HideAllControls()
		{
			DismountGame.hudManager.HideDismountComplete();
		}

		private void MyInvoke(string methodName, float delay)
		{
			InvokeParams item = new InvokeParams
			{
				methodName = methodName,
				time = Time.fixedTime + delay
			};
			invokes.Add(item);
		}

		private void UpdateMyInvokes()
		{
			for (int i = 0; i < invokes.Count; i++)
			{
				InvokeParams value = invokes[i];
				if (value.time > 0f && Time.fixedTime > value.time)
				{
					value.time = -1f;
					base.gameObject.SendMessage(value.methodName);
					invokes[i] = value;
				}
			}
		}

		private void CancelMyInvokes()
		{
			for (int i = 0; i < invokes.Count; i++)
			{
				InvokeParams value = invokes[i];
				value.time = -1f;
				invokes[i] = value;
			}
		}

		public void CancelCountDown()
		{
			countDownCanceled = true;
		}

		private void FixedUpdate()
		{
			scoreMultiplier.FixedUpdate();
		}

		private void Update()
		{
			UpdateMyInvokes();
			scoreAnimator.Update(Time.time);
			if (!scoreAnimator.gotTarget)
			{
				DismountGame.playerState.statistics.realtime.displayScore = Mathf.RoundToInt(scoreAnimator.current);
			}
			if (multiplierAnimator.target != scoreMultiplier.GetMultiplier())
			{
				multiplierAnimator.target = scoreMultiplier.GetMultiplier();
			}
			multiplierAnimator.Update(Time.time);
			if (!multiplierAnimator.gotTarget)
			{
				DismountGame.playerState.statistics.realtime.displayMultiplier = multiplierAnimator.current;
			}
			if (DismountGame.playerState.manualControls)
			{
				vehicle.inputThrottle = Mathf.Clamp01(SXInputManager.GetAxisValue(SXInputManager.Axis.R2, 0.1f));
				vehicle.inputBrake = Mathf.Clamp01(SXInputManager.GetAxisValue(SXInputManager.Axis.L2, 0.1f));
				if (vehicle.steerSpline == null)
				{
					float axisValue = SXInputManager.GetAxisValue(SXInputManager.Axis.LSTICKH, 0.2f);
					float to = axisValue * axisValue * axisValue;
					vehicle.inputSteering = Mathf.Lerp(axisValue, to, 0.5f);
				}
				if (SXInputManager.GetButtonDown(SXInputManager.Button.R1))
				{
					if (vehicle.overrideGear < 0)
					{
						vehicle.overrideGear = -2;
					}
					else
					{
						vehicle.overrideGear++;
					}
				}
				if (SXInputManager.GetButtonDown(SXInputManager.Button.L1))
				{
					if (vehicle.overrideGear < 0)
					{
						vehicle.overrideGear = -2;
					}
					else
					{
						vehicle.overrideGear--;
					}
				}
				Vector3 localEulerAngles = steeringWheelButton.localEulerAngles;
				localEulerAngles.z = vehicle.inputSteering * -135f;
				steeringWheelButton.localEulerAngles = localEulerAngles;
				powerBarValue = vehicle.inputThrottle;
				DismountGame.uiManager.SetDismountPowerBar(powerBarValue);
			}
			else if (DismountGame.playerState.manualSteering)
			{
				if (steeringWheelDown)
				{
					float num = DismountGame.uiManager.GetScreenPosition(steeringWheelButton.position).x * 0.9f;
					steerAngle = Input.mousePosition.x - steeringStartX;
					steerAngle /= num;
					steerAngle = Mathf.Clamp(steerAngle, -1f, 1f);
					float to2 = steerAngle * steerAngle * steerAngle;
					vehicle.inputSteering = Mathf.Lerp(steerAngle, to2, 0.5f);
				}
				else if (SXInputManager.IsControllerConnected())
				{
					steerAngle = SXInputManager.GetAxisValue(SXInputManager.Axis.LSTICKH, 0.2f);
					vehicle.inputSteering = Mathf.Sign(steerAngle) * (steerAngle * steerAngle);
				}
				else if (Input.GetKey(KeyCode.LeftArrow))
				{
					if (steerAngle >= 0f)
					{
						steerAngle -= Time.deltaTime * 2.5f;
					}
					else
					{
						steerAngle -= Time.deltaTime * 1.5f;
					}
					steerAngle = Mathf.Clamp(steerAngle, -1f, 1f);
					float to3 = Mathf.Sign(steerAngle) * (steerAngle * steerAngle);
					vehicle.inputSteering = Mathf.Lerp(steerAngle, to3, 0.5f);
				}
				else if (Input.GetKey(KeyCode.RightArrow))
				{
					if (steerAngle <= 0f)
					{
						steerAngle += Time.deltaTime * 2.5f;
					}
					else
					{
						steerAngle += Time.deltaTime * 1.5f;
					}
					steerAngle = Mathf.Clamp(steerAngle, -1f, 1f);
					float to4 = Mathf.Sign(steerAngle) * (steerAngle * steerAngle);
					vehicle.inputSteering = Mathf.Lerp(steerAngle, to4, 0.5f);
				}
				else
				{
					if (steerAngle > 0f)
					{
						steerAngle -= Time.deltaTime * 1.5f;
						if (steerAngle < 0f)
						{
							steerAngle = 0f;
						}
					}
					else if (steerAngle < 0f)
					{
						steerAngle += Time.deltaTime * 1.5f;
						if (steerAngle > 0f)
						{
							steerAngle = 0f;
						}
					}
					float to5 = Mathf.Sign(steerAngle) * (steerAngle * steerAngle);
					vehicle.inputSteering = Mathf.Lerp(steerAngle, to5, 0.5f);
				}
				Vector3 localEulerAngles2 = steeringWheelButton.localEulerAngles;
				localEulerAngles2.z = vehicle.inputSteering * -135f;
				steeringWheelButton.localEulerAngles = localEulerAngles2;
			}
			if (currentState == DismountState.DismountSetup || currentState == DismountState.RevingEngine)
			{
				PumpGas();
			}
			if (currentState == DismountState.DismountActive)
			{
				CheckDismountGo();
				CheckDismountComplete();
			}
		}

		public void OnSteeringWheelPressed()
		{
			steeringWheelDown = true;
			steeringStartX = Input.mousePosition.x;
			steerAngle = 0f;
		}

		public void OnSteeringWheelReleased()
		{
			steeringWheelDown = false;
			steeringStartX = -1f;
		}

		public void ResetLogic()
		{
			scoreAnimator.Reset(0f);
			DismountGame.playerState.statistics.realtime.displayScore = 0;
			SetScore(0);
			scoreMultiplier.Reset();
			multiplierAnimator.Reset(0);
			DismountGame.playerState.statistics.realtime.displayMultiplier = 0;
			currentState = DismountState.Init;
			StartEngine();
			stalled = false;
		}

		private void StartEngine()
		{
			currentState = DismountState.DismountSetup;
			if (DismountGame.stateManager.previousState != StateManager.State.SetupScene)
			{
				vehicle.gameObject.SendMessage("StartEngine");
				DismountGame.playerState.revStartTime = Time.time;
			}
			targetThrottle = 0.525f;
		}

		private void StartRevingEngine()
		{
			targetThrottle = 1f;
			DismountGame.playerState.revStartTime = Time.fixedTime - Utils.DismountBarFillInverseFunction(powerBarValue, targetThrottle);
			currentState = DismountState.RevingEngine;
		}

		private void PumpGas()
		{
			if (DismountGame.playerState.manualControls)
			{
				powerBarValue = vehicle.inputThrottle;
				DismountGame.uiManager.SetDismountPowerBar(powerBarValue);
				return;
			}
			if (currentState == DismountState.DismountSetup)
			{
				powerBarValue = Utils.DismountBarFillFunctionSetup(Time.time - DismountGame.playerState.revStartTime, targetThrottle);
			}
			else
			{
				powerBarValue = Utils.DismountBarFillFunction(Time.time - DismountGame.playerState.revStartTime, targetThrottle);
			}
			DismountGame.uiManager.SetDismountPowerBar(powerBarValue);
			vehicle.inputThrottle = powerBarValue;
		}

		private void DoSelectLevel(GameItem LevelItem)
		{
			if (!LevelItem.isLocked)
			{
				if (!DismountGame.instance.OnLevelSelected(LevelItem))
				{
					CloseDialog();
				}
			}
			else
			{
				stateBeforeNag = DismountGame.uiManager.CurrentState;
				DismountGame.uiManager.ChangeState(UIManager.State.WebNag);
			}
		}

		private void OnDismountPressed()
		{
			if (currentState != DismountState.RevingEngine)
			{
				countDownCanceled = false;
				DismountGame.uiManager.StopHelpSequence();
				DismountGame.playerState.SaveCamera();
				DismountGame.uiManager.ChangeState(UIManager.State.DismountRevingEngine);
				StartRevingEngine();
				ragdollScore = 0;
				vehicleScore = 0;
			}
		}

		private void OnDismountReleased()
		{
			if (currentState == DismountState.RevingEngine)
			{
				DismountGame.level.AddObjectsForReplay();
				Replay.Instance.StartRecording();
				DismountGame.uiManager.ChangeState(UIManager.State.DismountActive);
				DismountGame.level.HideSteerPath();
				PopClutch();
				DismountGame.playerState.DecreaseVideoAdVehicleUseCount();
				DismountGame.playerState.DecreaseVideoAdLevelUseCount();
				DismountGame.hudManager.UpdateTrialsLeftLabels();
				dismountsBeforeNag--;
			}
		}

		private IEnumerator EndCountdown(float waitTime)
		{
			if (!countDownCanceled)
			{
				SetCounter("3", waitTime);
				yield return new WaitForSeconds(waitTime);
			}
			if (!countDownCanceled)
			{
				SetCounter("2", waitTime);
				yield return new WaitForSeconds(waitTime);
			}
			if (!countDownCanceled)
			{
				SetCounter("1", waitTime);
				yield return new WaitForSeconds(waitTime);
			}
			if (!countDownCanceled)
			{
				SetCounter(string.Empty, waitTime);
				DismountComplete();
			}
		}

		private void SetCounter(string text, float waitTime)
		{
			if (text.CompareTo("3") > 0 || text == string.Empty)
			{
				startCounterSprite.gameObject.SetActive(false);
				startCounter.gameObject.SetActive(true);
				startCounter.text = text;
				startCounter.alpha = 1f;
				if (waitTime > 0f)
				{
					TweenAlpha.Begin(startCounter.gameObject, waitTime, 0f);
				}
				return;
			}
			startCounterSprite.spriteName = "counter_" + text;
			startCounterSprite.gameObject.SetActive(true);
			startCounterSprite.MakePixelPerfect();
			startCounterSprite.transform.localScale = startCounterSprite.transform.localScale * 0.5f;
			startCounter.gameObject.SetActive(false);
			startCounterSprite.alpha = 1f;
			if (waitTime > 0f)
			{
				TweenAlpha tweenAlpha = TweenAlpha.Begin(startCounterSprite.gameObject, waitTime, 0f);
				tweenAlpha.method = UITweener.Method.EaseIn;
				tweenAlpha.steeperCurves = true;
			}
		}

		public void VehicleStalled()
		{
			stalled = true;
		}

		private void PopClutch()
		{
			currentState = DismountState.DismountActive;
			DismountGame.level.OnDismountStarted();
			DismountGame.playerState.OnDismountStarted(vehicle.inputThrottle);
			if (vehicle.inputThrottle > 0.98f)
			{
				DismountGame.hudManager.ShowDismountGo(true, true);
			}
			else
			{
				DismountGame.hudManager.ShowDismountGo(true, false);
			}
			DismountGame.hudManager.EnableStats(true);
			dismountStartTime = Time.fixedTime;
			dismountEndTime = dismountStartTime + DismountGame.level.dismountTimeLimit;
			dismountEnding = false;
			vehicle.fuel = DismountGame.level.vehicleFuelAmount;
			vehicle.inputBrake = 0f;
			vehicle.gameObject.BroadcastMessage("OnDismountStarted");
			if (character.kinematicDismountSetup)
			{
				character.transform.parent = null;
			}
			character.SendMessage("OnDismountStarted");
		}

		private void CheckDismountGo()
		{
			float fixedTime = Time.fixedTime;
			if (fixedTime > dismountStartTime + 0.85f)
			{
				float num = 1f - Mathf.Lerp(1f, 0f, (1f - (fixedTime - dismountStartTime)) / 0.15f);
				if (num < 0f)
				{
					num = 0f;
				}
				DismountGame.hudManager.SetDismountGoAlpha(num);
				if (fixedTime > dismountStartTime + 1f)
				{
					DismountGame.hudManager.HideDismountGo();
				}
			}
		}

		private void CheckDismountComplete()
		{
			float fixedTime = Time.fixedTime;
			if (dismountEnding || dismountEndTime < 0f)
			{
				return;
			}
			float num = 5f;
			if (stalled)
			{
				num = 1f;
			}
			if (fixedTime < dismountStartTime + num)
			{
				return;
			}
			if (stalled || (fixedTime > dismountEndTime && !dismountEnding))
			{
				dismountEnding = true;
				StartCoroutine("EndCountdown", 1f);
				return;
			}
			if (character.IsMoving() || vehicle.IsMoving())
			{
				dismountEndedCount = 0;
				return;
			}
			dismountEndedCount++;
			if (dismountEndedCount == 10)
			{
				dismountEnding = true;
				StartCoroutine("EndCountdown", 1f);
			}
		}

		private void DismountComplete()
		{
			PlayerState playerState = DismountGame.playerState;
			int currentHighScore = playerState.currentHighScore;
			currentState = DismountState.DismountComplete;
			bool flag = false;
			EndZoneChecker[] array = endZones;
			foreach (EndZoneChecker endZoneChecker in array)
			{
				if (endZoneChecker.HasCharacter())
				{
					flag = true;
					break;
				}
			}
			bool flag2 = false;
			if (flag && Time.fixedTime - character.lastVehicleContactTime < 0.5f && DismountGame.playerState.statistics.Get("dismount.bodyPartDetaches") == 0)
			{
				flag2 = true;
			}
			DismountGame.level.OnDismountComplete();
			playerState.OnDismountComplete(flag2);
			Replay.Instance.StopRecording();
			string result = "complete";
			if (playerState.currentHighScore > currentHighScore)
			{
				result = "highscore";
			}
			else if (flag2)
			{
				result = "nailed";
			}
			else if (stalled)
			{
				result = "stalled";
			}
			DismountGame.hudManager.ShowDismountComplete(result);
			DismountGame.uiManager.ChangeState(UIManager.State.DismountEnded);
			MyInvoke("StartReplayMode", 3f);
		}

		private void StartReplayMode()
		{
			DismountGame.stateManager.ChangeState(StateManager.State.ReplayMode);
		}

		public void AddImpactScore(int score, int bonus, ScoreCategory category)
		{
			if (isDismountActive)
			{
				int num = scoreMultiplier.GetMultiplier();
				if (score > 0)
				{
					num = scoreMultiplier.Increase();
				}
				else if (bonus > 0)
				{
					num = scoreMultiplier.Maintain();
				}
				if (num < 1)
				{
					num = 1;
				}
				int num2 = num * (score + bonus);
				if (num2 >= 10000 && category == ScoreCategory.Ragdoll)
				{
					AddScoreFloater(num2);
				}
				AddScore(num2, category);
			}
		}

		public void AddSlideScore(int score, int bonus, ScoreCategory category)
		{
			if (isDismountActive)
			{
				scoreMultiplier.Maintain();
				int num = scoreMultiplier.GetMultiplier();
				if (num < 1)
				{
					num = 1;
				}
				AddScore(num * (score + bonus), category);
			}
		}

		private void AddScore(int score, ScoreCategory category)
		{
			if (score < 0)
			{
				Debug.LogWarning("Ignoring negative score, this shouldn't happen");
				return;
			}
			switch (category)
			{
			case ScoreCategory.Ragdoll:
				ragdollScore += score;
				break;
			case ScoreCategory.Vehicle:
				vehicleScore += score;
				break;
			}
			int score2 = DismountGame.playerState.statistics.realtime.score;
			SetScore(score2 + score);
		}

		private void SetScore(int score)
		{
			scoreAnimator.target = score;
			DismountGame.playerState.statistics.realtime.score = score;
		}

		public void AddScoreFloater(int score)
		{
			if (hudTextInstance == null || Time.fixedTime > hudTextTime + 0.5f)
			{
				GameObject gameObject = new GameObject("HUD Text target");
				gameObject.transform.position = character.cameraTarget.position + Vector3.up * 0.5f;
				hudTextFollowTransform = gameObject.transform;
				hudTextInstance = DismountGame.hudManager.SpawnHUDText(score, Color.white, 0.5f, null, hudTextFollowTransform);
				hudTextTime = Time.fixedTime;
			}
			else
			{
				DismountGame.hudManager.SpawnHUDText(score, Color.white, 0.5f, hudTextInstance, hudTextFollowTransform);
			}
		}

		private void OnCharacterSelect()
		{
			previousUIState = DismountGame.uiManager.CurrentState;
			DismountGame.uiManager.ChangeState(UIManager.State.SetupCharacter);
			DismountGame.uiManager.StopHelpSequence();
		}

		private void OnHeadSelect()
		{
			previousUIState = DismountGame.uiManager.CurrentState;
			DismountGame.uiManager.ChangeState(UIManager.State.SetupHead);
			DismountGame.uiManager.StopHelpSequence();
		}

		private void OnSelectCharacter(GameItem CharacterItem)
		{
			OnSelectCharacter(CharacterItem, 0);
		}

		private void OnSelectCharacter(GameItem CharacterItem, int characterPosition)
		{
			if (CharacterItem == null)
			{
				CharacterItem = DismountGame.inventory.GetItem("character.mrdismount");
			}
			DismountGame.playerState.SaveCamera();
			DismountGame.playerState.currentCharacterItem = CharacterItem;
			DismountGame.playerState.currentCharacterItemId = CharacterItem.itemId;
			DismountGame.playerState.currentCharacterName = CharacterItem.referenceName;
			DismountGame.playerState.currentCharacterStartPosition = characterPosition;
			DismountGame.playerState.stateAfterLevelLoad = StateManager.State.Dismount;
			DismountGame.playerState.setSavedCameraOrientation = true;
			DismountGame.level.ChangeVehicle(DismountGame.level.VehicleHotSpot);
			DismountGame.uiManager.ChangeState(UIManager.State.DismountStarting);
		}

		private void OnVehicleSelect()
		{
			previousUIState = DismountGame.uiManager.CurrentState;
			DismountGame.uiManager.ChangeState(UIManager.State.SetupVehicle);
			DismountGame.uiManager.StopHelpSequence();
		}

		private void SelectVehicleOverride(GameItem VehicleItem)
		{
			DismountGame.playerState.SaveCamera();
			DismountGame.playerState.setItemHasBeenUsed(VehicleItem.referenceName);
			if (DismountGame.playerState.currentVehicleName.CompareTo(VehicleItem.referenceName) != 0)
			{
				DismountGame.playerState.currentCharacterStartPosition = 0;
				DismountGame.playerState.currentVehicleItem = VehicleItem;
				DismountGame.playerState.currentVehicleItemId = VehicleItem.itemId;
				DismountGame.playerState.currentVehicleName = VehicleItem.referenceName;
				DismountGame.playerState.stateAfterLevelLoad = StateManager.State.Dismount;
				DismountGame.playerState.setSavedCameraOrientation = true;
				DismountGame.level.ChangeVehicle(DismountGame.level.VehicleHotSpot);
			}
			DismountGame.uiManager.ChangeState(UIManager.State.DismountStarting);
		}

		private void OnSelectVehicle(GameItem VehicleItem)
		{
			if (!VehicleItem.isLocked)
			{
				SelectVehicleOverride(VehicleItem);
				return;
			}
			stateBeforeNag = DismountGame.uiManager.CurrentState;
			DismountGame.uiManager.ChangeState(UIManager.State.WebNag);
		}

		private void ChangeCharacterPosition(int position)
		{
			DismountGame.playerState.SaveCamera();
			if (DismountGame.playerState.currentCharacterStartPosition != position)
			{
				DismountGame.playerState.currentCharacterStartPosition = position;
				DismountGame.playerState.stateAfterLevelLoad = StateManager.State.Dismount;
				DismountGame.playerState.setSavedCameraOrientation = true;
				DismountGame.level.ChangeVehicle(DismountGame.level.VehicleHotSpot);
			}
			DismountGame.uiManager.ChangeState(UIManager.State.DismountStarting);
		}

		private void OnSelectHead(GameItem headItem)
		{
			DismountGame.playerState.IsBubbleHeadMode = false;
			DismountGame.playerState.SaveCamera();
			DismountGame.playerState.currentHeadItem = headItem;
			DismountGame.playerState.currentHeadItemId = headItem.itemId;
			DismountGame.playerState.currentHeadName = headItem.referenceName;
			DismountGame.playerState.setItemHasBeenUsed(headItem.referenceName);
			DismountGame.playerState.stateAfterLevelLoad = StateManager.State.Dismount;
			DismountGame.level.ChangeVehicle(DismountGame.level.VehicleHotSpot);
			DismountGame.uiManager.ChangeState(UIManager.State.DismountStarting);
		}

		private void OnSelectCharacterPosition1()
		{
			ChangeCharacterPosition(0);
		}

		private void OnSelectCharacterPosition2()
		{
			ChangeCharacterPosition(1);
		}

		private void OnSelectCharacterPosition3()
		{
			ChangeCharacterPosition(2);
		}

		private void OnSwitchCharacterStartPosition()
		{
			DismountGame.playerState.multipleRagdollsActive = false;
			DismountGame.playerState.currentCharacterStartPosition++;
			if (DismountGame.playerState.currentCharacterStartPosition >= DismountGame.level.GetCurrentStartingPositionCount())
			{
				DismountGame.playerState.currentCharacterStartPosition = 0;
			}
			DismountGame.playerState.SaveCamera();
			DismountGame.playerState.stateAfterLevelLoad = StateManager.State.Dismount;
			DismountGame.playerState.setSavedCameraOrientation = true;
			DismountGame.level.ChangeVehicle(DismountGame.level.VehicleHotSpot);
		}

		private void SelectLevelOverride(GameItem LevelItem)
		{
			if (!DismountGame.instance.OnLevelSelected(LevelItem, true))
			{
				CloseDialog();
			}
		}

		private void OnLevelSelect()
		{
			DismountGame.playerState.SaveCamera();
			DismountGame.playerState.setSavedCameraOrientation = false;
			DismountGame.cameraManager.hasValidFreeFlyPosition = false;
			previousUIState = DismountGame.uiManager.CurrentState;
			DismountGame.uiManager.ChangeState(UIManager.State.LevelSelect);
		}

		private void CloseDialog()
		{
			if (DismountGame.uiManager.CurrentState == UIManager.State.VideoOptions)
			{
				DismountGame.uiManager.SyncVideoOptions();
			}
			OnGotoPreviousUIState();
		}

		private void OnGotoPreviousUIState()
		{
			if (forceLevelSelect || forceVehicleSelect)
			{
				return;
			}
			switch (DismountGame.uiManager.CurrentState)
			{
			case UIManager.State.DismountStarting:
			case UIManager.State.DismountStartingUnpublished:
			case UIManager.State.DismountRevingEngine:
			case UIManager.State.DismountActive:
			case UIManager.State.DismountEnded:
				DismountGame.uiManager.ChangeState(UIManager.State.Paused);
				DismountGame.instance.Pause();
				break;
			case UIManager.State.Paused:
				if (currentState == DismountState.DismountActive)
				{
					DismountGame.uiManager.ChangeState(UIManager.State.DismountActive);
				}
				else if (currentState == DismountState.DismountComplete)
				{
					DismountGame.uiManager.ChangeState(UIManager.State.DismountEnded);
				}
				else if (currentState == DismountState.DismountSetup)
				{
					DismountGame.uiManager.ChangeState(UIManager.State.DismountStarting);
				}
				else if (currentState == DismountState.RevingEngine)
				{
					DismountGame.uiManager.ChangeState(UIManager.State.DismountRevingEngine);
				}
				DismountGame.instance.Unpause();
				break;
			case UIManager.State.VideoOptions:
				DismountGame.uiManager.ChangeState(UIManager.State.Paused);
				DismountGame.uiManager.SyncVideoOptions();
				break;
			case UIManager.State.LevelSelect:
				DismountGame.uiManager.ChangeState(previousUIState);
				break;
			case UIManager.State.SetupVehicle:
				DismountGame.uiManager.ChangeState(UIManager.State.DismountStarting);
				break;
			case UIManager.State.SetupCharacter:
				OnSelectCharacter(DismountGame.playerState.currentCharacterItem, DismountGame.playerState.currentCharacterStartPosition);
				DismountGame.uiManager.ChangeState(UIManager.State.DismountStarting);
				break;
			case UIManager.State.SetupHead:
				DismountGame.uiManager.ChangeState(UIManager.State.DismountStarting);
				break;
			case UIManager.State.WebNag:
				DismountGame.uiManager.ChangeState(stateBeforeNag);
				break;
			}
		}
	}
}
