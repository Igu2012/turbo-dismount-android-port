#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class Tree : MonoBehaviour
	{
		private bool treeBroken;

		private void TreePartBroken()
		{
			if (!treeBroken)
			{
				DismountGame.playerState.statistics.TreeCut();
			}
			treeBroken = true;
		}
	}
}
