#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;
using reLive;

namespace Dismount.GameStates
{
	public class ReplayMode : MonoBehaviour
	{
		private enum EveryplayGUIState
		{
			None = 0,
			RecordingStopped = 1,
			RecordingActive = 2,
			RecordingPaused = 3
		}

		private enum GIFGUIState
		{
			None = 0,
			Ready = 1,
			Capturing = 2,
			Encoding = 3,
			Done = 4
		}

		private const float recordingMaxTime = 120f;

		private GIFGUIState gifGUIState;

		private float gifDoneTimer = -1f;

		private EveryplayGUIState everyplayGUIState = EveryplayGUIState.RecordingStopped;

		private EveryplayHelper everyplay;

		private Replay replay;

		private float[] playbackSpeeds = new float[15]
		{
			-4f, -2f, -1f, -0.5f, -0.25f, -0.125f, -0.0625f, 0f, 0.0625f, 0.125f,
			0.25f, 0.5f, 1f, 2f, 4f
		};

		private float targetPlaybackSpeed = 1f;

		private float playbackSpeed = 1f;

		private bool isRewinding;

		private bool startAfterRewind;

		private bool startNewRecording = true;

		private bool hasRecording;

		private GameObject recordStartButton;

		private GameObject recordPauseButton;

		private GameObject recordResumeButton;

		private GameObject finishRecordingButton;

		private GameObject playVideoButton;

		private GameObject recordingIndicator;

		private GameObject recordingDuration;

		private UILabel recordingDurationLabel;

		private GameObject gifButtonObject;

		private UIImageButton gifButton;

		private GameObject gifStatusObject;

		private UILabel gifStatusLabel;

		private float recordingElapsedTime;

		private bool enableEveryplayRecording;

		private bool enableGIFRecording;

		private bool superSlomo;

		private bool interpolate = true;

		private int replaySpeed = 12;

		private bool slomoNotified;

		private void UpdateLeaderboards()
		{
			DismountGame.uiManager.PopulateLeaderboard(new List<Leaderboards.LeaderboardEntry>(), 0);
			SXJNI.Instance.IsSignedInAsync((bool signedIn, string displayName) =>
			{
				if (signedIn)
				{
					SXJNI.Instance.GetLeaderboardData(DismountGame.playerState.levelItem.GPGSId, 25, DismountGame.playerState.leaderboardTimeSpan, (long[] ranks, string[] names, long[] scores) =>
					{
						int playerIndex = -1;
						List<Leaderboards.LeaderboardEntry> list = new List<Leaderboards.LeaderboardEntry>(ranks.Length);
						for (int i = 0; i < ranks.Length; i++)
						{
							Leaderboards.LeaderboardEntry item = new Leaderboards.LeaderboardEntry
							{
								rank = (int)ranks[i],
								score = (int)scores[i],
								name = names[i]
							};
							if (item.name.Equals(displayName))
							{
								playerIndex = i;
							}
							list.Add(item);
						}
						DismountGame.uiManager.PopulateLeaderboard(list, playerIndex);
					});
				}
			});
		}

		private void EnterState()
		{
			superSlomo = false;
			UpdateLeaderboards();
			everyplay = DismountGame.everyplayHelper;
			DismountGame.uiManager.RemoveRecordingHelpItems();
			replay = Replay.Instance;
			replay.MessageListener = base.gameObject;
			targetPlaybackSpeed = 1f;
			playbackSpeed = 1f;
			replaySpeed = 12;
			DismountGame.uiManager.SetReplayScrollbarPosition((float)replaySpeed / 14f);
			Transform cameraTarget = DismountGame.cameraManager.GetCameraTarget("Character");
			Transform transform = replay.GetReplayCloneFor(cameraTarget.gameObject).transform;
			DismountGame.cameraManager.SetCameraTarget("Character", transform);
			cameraTarget = DismountGame.cameraManager.GetCameraTarget("Vehicle");
			Transform transform2 = replay.GetReplayCloneFor(cameraTarget.gameObject).transform;
			DismountGame.cameraManager.SetCameraTarget("Vehicle", transform2);
			DismountGame.cameraManager.SetCameraTarget("Scene", transform, transform2);
			DismountGame.cameraManager.EnterReplayMode();
			replay.SetCameraOnPlayback = false;
			if (Prefs.GetInt("showLeaderboard", 1) == 1)
			{
				DismountGame.uiManager.ChangeState(UIManager.State.ReplayWithLeaderboard);
			}
			else
			{
				DismountGame.uiManager.ChangeState(UIManager.State.Replay);
			}
			startAfterRewind = true;
			replay.SeekPlayback(replay.Duration, false);
			if (everyplay.IsSupported())
			{
				Everyplay.RecordingStarted += EveryplayRecordingStarted;
				Everyplay.RecordingStopped += EveryplayRecordingStopped;
			}
			Rewind();
			GameObject.Find("SceneDynamic").SetActive(false);
			DismountGame.playerState.currentCharacterInstance.SetActive(false);
			DismountGame.playerState.currentVehicleInstance.SetActive(false);
			cameraTarget = DismountGame.cameraManager.GetCameraTarget("1stPerson");
			Transform target = replay.GetReplayCloneFor(cameraTarget.gameObject).transform;
			DismountGame.cameraManager.SetCameraTarget("1stPerson", target);
			DismountGame.hudManager.EnableInstantReplay();
			DismountGame.hudManager.SetStatsTargets(transform2, transform);
			DismountGame.hudManager.EnableStats(true);
			everyplayGUIState = EveryplayGUIState.RecordingStopped;
			startNewRecording = true;
			hasRecording = false;
			recordingElapsedTime = 0f;
			gifGUIState = GIFGUIState.None;
			UpdateRecordingControls();
			DismountGame.uiManager.SetDefaultReplaySpeed();
			DismountGame.hudManager.UpdateTrialsLeftLabels();
		}

		private void GetLeaderboardEntriesComplete(List<Leaderboards.LeaderboardEntry> entries, int playerIndex)
		{
			DismountGame.uiManager.PopulateLeaderboard(entries, playerIndex);
		}

		private void EveryplayRecordingStarted()
		{
		}

		private void EveryplayRecordingStopped()
		{
		}

		private void GIFRecordCapturing()
		{
			UpdateGIFGUI(GIFGUIState.Capturing);
		}

		private void GIFRecordEncoding()
		{
			UpdateGIFGUI(GIFGUIState.Encoding);
		}

		private void GIFRecordDone()
		{
			UpdateGIFGUI(GIFGUIState.Done);
		}

		private void ExitState()
		{
			DismountGame.cameraManager.ExitReplayMode();
			DismountGame.cameraManager.EnableRewindEffects(false);
			DismountGame.audioManager.EnableRewindEffects(false);
			Replay.Instance.StopPlayback();
			everyplay.StopRecording();
			if (everyplay.IsSupported())
			{
				Everyplay.RecordingStarted -= EveryplayRecordingStarted;
				Everyplay.RecordingStopped -= EveryplayRecordingStopped;
			}
			base.gameObject.SetActive(false);
			DismountGame.hudManager.DisableInstantReplay();
			DismountGame.hudManager.EnableStats(false);
		}

		public void UpdateRecordingControls()
		{
			Transform transform = DismountGame.uiManager.GetReplayControls().Find("RecordingControls");
			recordStartButton = transform.Find("RecordStartButton").gameObject;
			recordPauseButton = transform.Find("RecordPauseButton").gameObject;
			recordResumeButton = transform.Find("RecordResumeButton").gameObject;
			finishRecordingButton = transform.Find("FinishRecordingButton").gameObject;
			playVideoButton = transform.Find("PlayVideoButton").gameObject;
			recordingIndicator = transform.Find("RecordingStatus/Indicator").gameObject;
			recordingDuration = transform.Find("RecordingStatus/Duration").gameObject;
			recordingDurationLabel = recordingDuration.GetComponent<UILabel>();
			Transform transform2 = DismountGame.uiManager.GetReplayControls().Find("GIFControls");
			gifButtonObject = transform2.Find("GIFCaptureButton").gameObject;
			gifButton = gifButtonObject.GetComponent<UIImageButton>();
			gifStatusObject = transform2.Find("GIFStatusIndicator").gameObject;
			gifStatusLabel = gifStatusObject.GetComponent<UILabel>();
			UpdateGUI(everyplayGUIState);
			UpdateGIFGUI(gifGUIState);
		}

		private void Rewind()
		{
			isRewinding = true;
			replay.PlaybackSpeed = -16f;
			replay.SeekPlayback(replay.PlaybackTime, true);
			DismountGame.cameraManager.EnableRewindEffects(true);
			DismountGame.audioManager.EnableRewindEffects(true, -16f);
		}

		private void ReplayPlaybackReachedBeginning()
		{
			if (isRewinding)
			{
				isRewinding = false;
				playbackSpeed = 0f;
				replay.PlaybackSpeed = 0f;
				DismountGame.cameraManager.EnableRewindEffects(false);
				DismountGame.audioManager.EnableRewindEffects(false);
				replay.StopPlayback();
				UpdateGUI(EveryplayGUIState.RecordingStopped);
				if (enableGIFRecording)
				{
					UpdateGIFGUI(GIFGUIState.Ready);
				}
				if (startAfterRewind)
				{
					startAfterRewind = false;
					replay.StartPlayback();
				}
			}
		}

		private void ReplayPlaybackReachedEnd()
		{
			if (everyplayGUIState == EveryplayGUIState.RecordingActive)
			{
				OnRecordPauseClicked();
			}
		}

		private void SlowDownReplay()
		{
			replaySpeed--;
			if (replaySpeed < 0)
			{
				replaySpeed = 0;
			}
			DismountGame.uiManager.SetReplayScrollbarPosition((float)replaySpeed / 14f);
		}

		private void SpeedUpReplay()
		{
			replaySpeed++;
			if (replaySpeed > 14)
			{
				replaySpeed = 14;
			}
			DismountGame.uiManager.SetReplayScrollbarPosition((float)replaySpeed / 14f);
		}

		private void Update()
		{
			float replayScrollbarPosition = DismountGame.uiManager.GetReplayScrollbarPosition();
			replaySpeed = Mathf.RoundToInt(14f * replayScrollbarPosition);
			float num = playbackSpeeds[replaySpeed];
			float num2 = 0f;
			if (!SXInputManager.GetButton(SXInputManager.Button.L3) && !SXInputManager.GetButton(SXInputManager.Button.R3))
			{
				num2 -= Mathf.Clamp01(SXInputManager.GetAxisValue(SXInputManager.Axis.L2, 0.1f));
				num2 += Mathf.Clamp01(SXInputManager.GetAxisValue(SXInputManager.Axis.R2, 0.1f));
			}
			targetPlaybackSpeed = Mathf.Clamp(num + num2 * 4f, -4f, 4f);
			if (!slomoNotified && targetPlaybackSpeed >= 0f && targetPlaybackSpeed < 1f)
			{
				DismountGame.playerState.achievements.ReportComplete("com.secretexit.turbodismount.LetsSeeThatAgain");
				slomoNotified = true;
			}
			if (gifDoneTimer > 0f && Time.time > gifDoneTimer)
			{
				gifDoneTimer = -1f;
				UpdateGIFGUI(GIFGUIState.Ready);
			}
		}

		private void FixedUpdate()
		{
			playbackSpeed = playbackSpeed * 0.9625f + targetPlaybackSpeed * 0.0375f;
			if (Mathf.Abs(targetPlaybackSpeed - playbackSpeed) < 0.01f)
			{
				playbackSpeed = targetPlaybackSpeed;
			}
			if (!isRewinding)
			{
				if (superSlomo)
				{
					replay.PlaybackSpeed = 0.01f;
				}
				else
				{
					replay.PlaybackSpeed = playbackSpeed;
				}
			}
			if (Mathf.Abs(replay.PlaybackSpeed) > 1f)
			{
				DismountGame.audioManager.EnableRewindEffects(true, replay.PlaybackSpeed);
			}
			else
			{
				DismountGame.audioManager.EnableRewindEffects(false);
			}
			if (everyplayGUIState == EveryplayGUIState.RecordingActive)
			{
				recordingElapsedTime += Time.fixedDeltaTime;
				if (recordingElapsedTime > 120f)
				{
					recordingElapsedTime = 120f;
					OnFinishRecordingClicked();
				}
				UpdateRecordingDuration();
			}
		}

		private void OnPause()
		{
			if (everyplayGUIState == EveryplayGUIState.RecordingActive)
			{
				OnRecordPauseClicked();
			}
		}

		private void OnLevelSelect()
		{
			DismountGame.uiManager.ChangeState(UIManager.State.LevelSelect);
		}

		private void DoSelectLevel(GameItem LevelItem)
		{
			DismountGame.instance.OnPause();
			DismountGame.instance.OnLevelSelected(LevelItem);
		}

		private void SelectLevelOverride(GameItem LevelItem)
		{
			DismountGame.instance.OnLevelSelected(LevelItem, true);
		}

		private void CloseDialog()
		{
			OnGotoPreviousUIState();
		}

		private void StartOrResumeRecording()
		{
			if (startNewRecording)
			{
				everyplay.StartRecording();
				recordingElapsedTime = 0f;
			}
			else
			{
				everyplay.ResumeRecording();
			}
			startNewRecording = false;
			hasRecording = true;
		}

		private void OnRecordStartClicked()
		{
			StartOrResumeRecording();
			UpdateGUI(EveryplayGUIState.RecordingActive);
		}

		private void OnRecordPauseClicked()
		{
			everyplay.PauseRecording();
			UpdateGUI(EveryplayGUIState.RecordingPaused);
		}

		private void OnFinishRecordingClicked()
		{
			everyplay.StopRecording();
			startNewRecording = true;
			UpdateGUI(EveryplayGUIState.RecordingStopped);
			everyplay.PlayLastRecording();
		}

		private void OnPlayVideoClicked()
		{
			everyplay.PlayLastRecording();
		}

		private void UpdateRecordingDuration()
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(recordingElapsedTime);
			TimeSpan timeSpan2 = TimeSpan.FromSeconds(120.0);
			recordingDurationLabel.text = "[" + timeSpan.Minutes + ":" + timeSpan.Seconds.ToString("00") + " / " + timeSpan2.Minutes + ":" + timeSpan2.Seconds.ToString("00") + "]";
			if (recordingElapsedTime >= 120f)
			{
				recordingDurationLabel.color = Color.red;
			}
			else
			{
				recordingDurationLabel.color = Color.white;
			}
			int num = Mathf.RoundToInt(recordingElapsedTime * 2f);
			if (everyplayGUIState == EveryplayGUIState.RecordingActive)
			{
				recordingIndicator.SetActive(num % 2 == 0);
			}
			else
			{
				recordingIndicator.SetActive(false);
			}
		}

		private void UpdateGUI(EveryplayGUIState state)
		{
			if (state == EveryplayGUIState.None || isRewinding || !enableEveryplayRecording)
			{
				recordStartButton.SetActive(false);
				recordPauseButton.SetActive(false);
				recordResumeButton.SetActive(false);
				finishRecordingButton.SetActive(false);
				playVideoButton.SetActive(false);
				recordingIndicator.SetActive(false);
				recordingDuration.SetActive(false);
			}
			else
			{
				switch (state)
				{
				case EveryplayGUIState.RecordingStopped:
					recordStartButton.SetActive(true);
					recordPauseButton.SetActive(false);
					recordResumeButton.SetActive(false);
					finishRecordingButton.SetActive(false);
					playVideoButton.SetActive(hasRecording);
					recordingIndicator.SetActive(false);
					recordingDuration.SetActive(hasRecording);
					break;
				case EveryplayGUIState.RecordingPaused:
					recordStartButton.SetActive(false);
					recordPauseButton.SetActive(false);
					if (recordingElapsedTime < 120f)
					{
						recordResumeButton.SetActive(true);
					}
					else
					{
						recordResumeButton.SetActive(false);
					}
					finishRecordingButton.SetActive(true);
					playVideoButton.SetActive(false);
					recordingIndicator.SetActive(false);
					recordingDuration.SetActive(true);
					break;
				case EveryplayGUIState.RecordingActive:
					recordStartButton.SetActive(false);
					recordPauseButton.SetActive(true);
					recordResumeButton.SetActive(false);
					finishRecordingButton.SetActive(true);
					playVideoButton.SetActive(false);
					recordingIndicator.SetActive(true);
					recordingDuration.SetActive(true);
					break;
				}
			}
			everyplayGUIState = state;
			UpdateRecordingDuration();
		}

		private void UpdateGIFGUI(GIFGUIState state)
		{
			switch (state)
			{
			case GIFGUIState.None:
				gifButtonObject.SetActive(false);
				gifStatusObject.SetActive(false);
				gifDoneTimer = -1f;
				break;
			case GIFGUIState.Ready:
				gifButtonObject.SetActive(true);
				gifButton.isEnabled = true;
				gifStatusObject.SetActive(true);
				gifStatusLabel.text = string.Empty;
				break;
			case GIFGUIState.Capturing:
				gifButtonObject.SetActive(true);
				gifButton.isEnabled = false;
				gifStatusObject.SetActive(true);
				gifStatusLabel.text = "Capturing...";
				gifDoneTimer = -1f;
				break;
			case GIFGUIState.Encoding:
				gifButtonObject.SetActive(true);
				gifButton.isEnabled = false;
				gifStatusObject.SetActive(true);
				gifStatusLabel.text = "Encoding...";
				break;
			case GIFGUIState.Done:
				gifButtonObject.SetActive(true);
				gifButton.isEnabled = true;
				gifStatusObject.SetActive(true);
				gifStatusLabel.text = "Done!";
				gifDoneTimer = Time.time + 3f;
				break;
			}
			gifGUIState = state;
		}

		private void OnGotoPreviousUIState()
		{
			switch (DismountGame.uiManager.CurrentState)
			{
			case UIManager.State.Replay:
			case UIManager.State.ReplayWithLeaderboard:
				DismountGame.uiManager.ChangeState(UIManager.State.Paused);
				DismountGame.instance.Pause();
				break;
			case UIManager.State.Paused:
				if (Prefs.GetInt("showLeaderboard", 1) == 1)
				{
					DismountGame.uiManager.ChangeState(UIManager.State.ReplayWithLeaderboard);
				}
				else
				{
					DismountGame.uiManager.ChangeState(UIManager.State.Replay);
				}
				DismountGame.instance.Unpause();
				break;
			case UIManager.State.VideoOptions:
				DismountGame.uiManager.ChangeState(UIManager.State.Paused);
				DismountGame.uiManager.SyncVideoOptions();
				break;
			case UIManager.State.LevelSelect:
				DismountGame.uiManager.ChangeState(UIManager.State.Paused);
				break;
			}
		}

		private void OnSwitchLeaderboard()
		{
			DismountGame.playerState.leaderboardTimeSpan = (SXJNI.TimeSpan)((int)(DismountGame.playerState.leaderboardTimeSpan + 1) % 3);
			UpdateLeaderboards();
			DismountGame.uiManager.UpdateLeaderboardSwitchButtonLabel();
		}
	}
}
