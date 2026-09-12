#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount.Vehicular
{
	public class MinecartLever : MonoBehaviour
	{
		public Wheel wheel;

		public float leverCycleWheelRotations = 5f;

		public AudioSource squeakAudio;

		private float originalPitch = 1f;

		private HingeJoint hinge;

		private float prevSqueakPhase;

		private float phase;

		private void Awake()
		{
			phase = wheel.rotation / leverCycleWheelRotations;
			prevSqueakPhase = -0.5f;
			hinge = GetComponent<HingeJoint>();
			originalPitch = squeakAudio.pitch;
		}

		private void Update()
		{
			phase = wheel.rotation / leverCycleWheelRotations;
			JointSpring spring = hinge.spring;
			spring.targetPosition = Mathf.Sin(phase * (float)Math.PI * 2f) * 25f;
			hinge.spring = spring;
			float num = Mathf.Abs(phase - prevSqueakPhase);
			if (num >= 1f)
			{
				prevSqueakPhase = phase - num + 1f;
				squeakAudio.pitch = originalPitch + wheel.rpm * 0.0005f + UnityEngine.Random.Range(-0.025f, 0.025f);
				squeakAudio.Play();
			}
		}
	}
}
