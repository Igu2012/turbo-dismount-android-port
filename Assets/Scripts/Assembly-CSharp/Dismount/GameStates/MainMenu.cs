#pragma warning disable 0618,0619
using UnityEngine;
using reLive;

namespace Dismount.GameStates
{
	public class MainMenu : MonoBehaviour
	{
		private bool firstTime = true;

		private UIManager.State uiStateBeforePoster;

		public static bool isAuthentic()
		{
			return true;
		}

		private void EnterState()
		{
			if (firstTime)
			{
				DismountGame.playerState.statistics.ResetSession();
				if (DismountGame.IsNotchDevice())
				{
					DismountGame.instance.EnableNotchUI(true);
				}
			}
			DismountGame.hudManager.Disable();
			DismountGame.cameraManager.SetActiveCamera("Menu");
			DismountGame.challenges.RefreshChallenges();
			DismountGame.uiManager.ChangeState(UIManager.State.MainMenu);
			if (firstTime)
			{
				DismountGame.uiManager.LoadNewsPoster(Application.persistentDataPath + "/poster.jpg");
				DismountGame.instance.RequestReview();
				uiStateBeforePoster = DismountGame.uiManager.CurrentState;
				DismountGame.uiManager.ChangeState(UIManager.State.NewsPoster);
			}
			firstTime = false;
			bool musicButtonState = Prefs.GetInt("musicEnabled", 1) != 0;
			DismountGame.uiManager.SetMusicButtonState(musicButtonState);
			musicButtonState = Prefs.GetInt("sfxEnabled", 1) != 0;
			DismountGame.uiManager.SetSFXButtonState(musicButtonState);
			musicButtonState = Prefs.GetInt("controllerEnabled", 1) != 0;
			DismountGame.uiManager.SetControllerButtonState(musicButtonState);
			SXInputManager.controllerEnabled = musicButtonState;
			if (DismountGame.IsLowPerformanceDevice())
			{
				Replay.Instance.RequestedRecordInterval = 0.1f;
			}
			else
			{
				Replay.Instance.RequestedRecordInterval = 0.05f;
			}
		}

		private void ExitState()
		{
			base.gameObject.SetActive(false);
		}

		private void OnLevelSelect()
		{
			DismountGame.uiManager.ChangeState(UIManager.State.LevelSelect);
		}

		private void SelectLevelOverride(GameItem LevelItem)
		{
			if (!DismountGame.instance.OnLevelSelected(LevelItem, true))
			{
				CloseDialog();
			}
		}

		private void DoSelectLevel(GameItem LevelItem)
		{
			if (!LevelItem.isLocked)
			{
				SelectLevelOverride(LevelItem);
			}
			else
			{
				DismountGame.uiManager.ChangeState(UIManager.State.WebNag);
			}
		}

		private void CloseDialog()
		{
			OnGotoPreviousUIState();
		}

		private void OnChangeName()
		{
			DismountGame.uiManager.ChangeState(UIManager.State.NameEntry);
		}

		private void OnNameOK()
		{
			DismountGame.playerState.playerName = DismountGame.uiManager.GetPlayerName();
			CloseDialog();
		}

		private void Update()
		{
			if (SXInputManager.GetMouseButtonDown(0) && !DismountGame.uiManager.IsMouseObstructedByUI() && (DismountGame.uiManager.CurrentState == UIManager.State.NewsPoster || DismountGame.uiManager.CurrentState == UIManager.State.NewContentPoster))
			{
				OnGotoPreviousUIState();
			}
		}

		private void OnCustomizeFace()
		{
			DismountGame.uiManager.ChangeState(UIManager.State.CustomizeCharacter);
		}

		private void OnGotoPreviousUIState()
		{
			switch (DismountGame.uiManager.CurrentState)
			{
			case UIManager.State.MainMenu:
				DismountGame.uiManager.ChangeState(UIManager.State.MainMenuOptions);
				break;
			case UIManager.State.MainMenuOptions:
				DismountGame.uiManager.ChangeState(UIManager.State.MainMenu);
				break;
			case UIManager.State.LevelSelect:
				DismountGame.uiManager.ChangeState(UIManager.State.MainMenu);
				break;
			case UIManager.State.CustomizeCharacter:
				DismountGame.uiManager.ChangeState(UIManager.State.MainMenu);
				break;
			case UIManager.State.NameEntry:
				DismountGame.uiManager.ChangeState(UIManager.State.MainMenu);
				break;
			case UIManager.State.VideoOptions:
				DismountGame.uiManager.ChangeState(UIManager.State.MainMenuOptions);
				DismountGame.uiManager.SyncVideoOptions();
				break;
			case UIManager.State.WebNag:
				DismountGame.uiManager.ChangeState(UIManager.State.LevelSelect);
				break;
			case UIManager.State.NewsPoster:
				if (DismountGame.inventory.HasPremiumBeenPurchased())
				{
					if (Prefs.GetInt("NewContentPosterShown", 0) == 0 || Utils.StripMinorMinorFromVersion(Prefs.GetString("NewContentPosterShown" + Prefs.versionPostfix)) != Utils.StripMinorMinorFromVersion(UIManager.version))
					{
						DismountGame.uiManager.ChangeState(UIManager.State.NewContentPoster);
					}
					else
					{
						DismountGame.uiManager.ChangeState(UIManager.State.MainMenu);
					}
					break;
				}
				if (DismountGame.inventory.freeWithAnyPurchasesUnlocked)
				{
					DismountGame.instance.ShowFreeWithAnyPurchasesUnlocked();
				}
				DismountGame.uiManager.ChangeState(UIManager.State.MainMenu);
				Prefs.SetInt("NewContentPosterShown", 1);
				Prefs.Save();
				break;
			case UIManager.State.NewContentPoster:
				DismountGame.uiManager.ChangeState(UIManager.State.MainMenu);
				Prefs.SetInt("NewContentPosterShown", 1);
				Prefs.Save();
				break;
			}
		}
	}
}
