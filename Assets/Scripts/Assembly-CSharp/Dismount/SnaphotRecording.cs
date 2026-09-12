#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class SnaphotRecording : MonoBehaviour
	{
		private void OnPreRender()
		{
			if (!DismountGame.IsMetalDevice())
			{
				DismountGame.everyplayHelper.SnapshotRenderbuffer();
			}
		}
	}
}
