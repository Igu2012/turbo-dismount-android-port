#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	[RequireComponent(typeof(AudioSource))]
	public class SoundFieldSample : MonoBehaviour
	{
		public float pitchReferenceX = -1f;

		public float x;

		public float y;

		public float width;

		public float height;

		public float distanceBias;

		public float attenuation = 1f;

		[HideInInspector]
		public AudioSource audioSource;

		[HideInInspector]
		public float originalVolume = 1f;

		[HideInInspector]
		public float originalPitch = 1f;

		[HideInInspector]
		public float mix;

		[HideInInspector]
		public float scaledX;

		[HideInInspector]
		public float scaledY;

		[HideInInspector]
		public float scaledWidth;

		[HideInInspector]
		public float scaledHeight;

		private void Awake()
		{
			audioSource = GetComponent<AudioSource>();
			originalPitch = audioSource.pitch;
			originalVolume = audioSource.volume;
			audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
			audioSource.volume = 0f;
		}
	}
}
