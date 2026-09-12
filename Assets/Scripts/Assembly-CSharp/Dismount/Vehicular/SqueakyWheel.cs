#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount.Vehicular
{
	[RequireComponent(typeof(AudioSource))]
	public class SqueakyWheel : MonoBehaviour
	{
		public Wheel wheel;

		public float squeakEveryDegrees = 360f;

		private float squeakEvery = 6.283f;

		private float originalPitch = 1f;

		private AudioSource cachedAudio;

		private float prevSqueakRotation;

		private void Awake()
		{
			prevSqueakRotation = wheel.rotation;
			squeakEvery = squeakEveryDegrees * ((float)Math.PI / 180f);
			cachedAudio = base.GetComponent<AudioSource>();
			originalPitch = cachedAudio.pitch;
		}

		private void Update()
		{
			if (Mathf.Abs(wheel.rotation - prevSqueakRotation) > squeakEvery)
			{
				prevSqueakRotation = wheel.rotation;
				cachedAudio.pitch = originalPitch + wheel.rpm * 0.0005f + UnityEngine.Random.Range(-0.025f, 0.025f);
				cachedAudio.Play();
			}
		}
	}
}
