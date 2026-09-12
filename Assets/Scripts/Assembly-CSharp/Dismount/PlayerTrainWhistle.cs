#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class PlayerTrainWhistle : MonoBehaviour
	{
		public AudioSource whistle;

		public Transform smokeEmitter;

		private bool whistleBroken;

		private int emitSkip;

		public void TurboBoostStarted()
		{
			if (!whistleBroken)
			{
				whistle.Play();
			}
		}

		public void TurboBoostEnded()
		{
		}

		public void WhistleBroken()
		{
			whistle.Stop();
			whistleBroken = true;
		}

		public void CarBroken()
		{
			whistle.Stop();
			whistleBroken = true;
		}

		private void FixedUpdate()
		{
			if (whistle.isPlaying && --emitSkip <= 0)
			{
				DismountGame.particleManager.EmitRocketSmoke(smokeEmitter.position, smokeEmitter.forward * 30f);
				emitSkip = 5;
			}
		}
	}
}
