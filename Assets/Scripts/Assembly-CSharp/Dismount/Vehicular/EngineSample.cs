#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	[RequireComponent(typeof(AudioSource))]
	public class EngineSample : MonoBehaviour
	{
		public float referenceRpm = 2000f;

		public Vector2 rpmLoad = new Vector2(0.5f, 0.5f);

		public float distanceBias = 0.02f;

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
		public float edgeLength = 1f;

		[HideInInspector]
		public Vector2 edgeTangent = Vector2.right;

		[HideInInspector]
		public Vector2 edgeNormal = Vector2.up;

		private void Awake()
		{
			audioSource = GetComponent<AudioSource>();
			originalPitch = audioSource.pitch;
			originalVolume = audioSource.volume;
			audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
		}
	}
}
