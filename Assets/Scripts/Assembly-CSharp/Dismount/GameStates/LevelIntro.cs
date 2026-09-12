#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.GameStates
{
	public class LevelIntro : MonoBehaviour
	{
		private void EnterState()
		{
			DismountGame.hudManager.Disable();
			DismountGame.challenges.RefreshChallenges();
			if (DismountGame.playerState.isUnpublishedLevel)
			{
				DismountGame.uiManager.ChangeState(UIManager.State.LevelIntroUnpublished);
			}
			else
			{
				DismountGame.uiManager.ChangeState(UIManager.State.LevelIntro);
			}
			DismountGame.cameraManager.SetActiveCamera("Menu", 0f);
		}

		private void ExitState()
		{
			base.gameObject.SetActive(false);
		}

		private void OnSkipIntro()
		{
			DismountGame.stateManager.ChangeState(StateManager.State.Dismount);
		}

		private void OnGotoPreviousUIState()
		{
			OnSkipIntro();
		}
	}
}
