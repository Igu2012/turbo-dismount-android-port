#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	public class Axle : MonoBehaviour
	{
		public AltWheel wheel1;

		public AltWheel wheel2;

		[Range(0f, 1f)]
		public float antiRoll = 0.5f;

		[Range(0f, 1f)]
		public float differentialLocking;

		private void FixedUpdate()
		{
			if (wheel2.load > wheel1.load)
			{
				wheel1.antiRollBarForce = antiRoll * (wheel2.load - wheel1.load);
			}
			else
			{
				wheel1.antiRollBarForce = 0f;
			}
			wheel1.angularVelocityDifferential = differentialLocking * (wheel2.angularVelocity - wheel1.angularVelocity);
			if (wheel1.load > wheel2.load)
			{
				wheel2.antiRollBarForce = antiRoll * (wheel1.load - wheel2.load);
			}
			else
			{
				wheel2.antiRollBarForce = 0f;
			}
			wheel2.angularVelocityDifferential = differentialLocking * (wheel1.angularVelocity - wheel2.angularVelocity);
		}
	}
}
