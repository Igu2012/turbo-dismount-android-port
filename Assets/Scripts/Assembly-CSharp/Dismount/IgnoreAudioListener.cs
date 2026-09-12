#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(AudioSource))]
	public class IgnoreAudioListener : MonoBehaviour
	{
		private void Start()
		{
			base.GetComponent<AudioSource>().ignoreListenerPause = true;
			base.GetComponent<AudioSource>().ignoreListenerVolume = true;
		}
	}
}
