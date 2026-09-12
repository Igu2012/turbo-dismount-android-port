#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class PlayerCopCarEffects : MonoBehaviour
	{
		public CopLights lights;

		public AudioSource siren;

		private bool sirensBroken;

		private bool carBroken;

		private void Start()
		{
			lights.gameObject.SetActive(false);
		}

		private void FixedUpdate()
		{
			if (carBroken && siren.isPlaying)
			{
				siren.pitch *= 0.99f;
				siren.volume *= 0.99f;
				lights.speed *= 0.99f;
				if (siren.volume < 0.05f)
				{
					siren.Stop();
					lights.gameObject.SetActive(false);
					sirensBroken = true;
				}
			}
		}

		public void TurboBoostStarted()
		{
			if (!sirensBroken && !siren.isPlaying)
			{
				siren.Play();
				lights.gameObject.SetActive(true);
			}
		}

		public void TurboBoostEnded()
		{
			if (!carBroken)
			{
				siren.Stop();
				lights.gameObject.SetActive(false);
			}
		}

		public void CopLightsBroken()
		{
			siren.Stop();
			lights.gameObject.SetActive(false);
			sirensBroken = true;
		}

		public void CarBroken()
		{
			carBroken = true;
		}
	}
}
