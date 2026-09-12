#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount
{
	[ExecuteInEditMode]
	public class LevelSettings : MonoBehaviour
	{
		[Serializable]
		public class PhysicsOverride
		{
			public bool doOverride;

			public Vector3 gravity = new Vector3(0f, -9.81f, 0f);
		}

		public PhysicsOverride physics;

		public float groundLevel;

		public float dismountTimeLimit = 40f;

		public float vehicleFuelAmount = 30f;

		public bool manualSteering;

		private void Start()
		{
			UpdatePhysics();
		}

		private void UpdatePhysics()
		{
			if (physics.doOverride)
			{
				Physics.gravity = physics.gravity;
			}
		}
	}
}
