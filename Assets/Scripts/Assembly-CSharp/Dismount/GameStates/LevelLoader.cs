#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.GameStates
{
	public class LevelLoader : MonoBehaviour
	{
		private void EnterState()
		{
			DismountGame.level.LoadLevel(true);
		}

		private void ExitState()
		{
			base.gameObject.SetActive(false);
		}
	}
}
