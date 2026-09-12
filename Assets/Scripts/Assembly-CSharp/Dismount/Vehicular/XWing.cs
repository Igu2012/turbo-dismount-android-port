#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	public class XWing : MonoBehaviour
	{
		public AttackWing[] wings;

		public TurboLaserGun[] guns;

		private void TurboBoostStarted()
		{
			for (int i = 0; i < wings.Length; i++)
			{
				wings[i].AttackPosition();
			}
			for (int j = 0; j < guns.Length; j++)
			{
				guns[j].Fire();
			}
		}

		private void TurboBoostEnded()
		{
			for (int i = 0; i < wings.Length; i++)
			{
				wings[i].NormalPosition();
			}
			for (int j = 0; j < guns.Length; j++)
			{
				guns[j].StopFiring();
			}
		}
	}
}
