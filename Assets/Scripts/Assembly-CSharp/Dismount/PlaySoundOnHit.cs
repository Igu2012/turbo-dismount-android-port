#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class PlaySoundOnHit : MonoBehaviour
	{
		public AudioItem soundSet;

		public float impactLimit = 5f;

		public float miminumTimeBetweenEffect = 0.1f;

		private float lastPlayTime = -1f;

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.contacts.Length > 0)
			{
				float num = Vector3.Dot(collision.relativeVelocity, collision.contacts[0].normal);
				float num2 = num * num;
				if (num2 > impactLimit && Time.fixedTime - lastPlayTime >= 0.1f)
				{
					DismountGame.audioManager.PlaySoundEffect(soundSet, base.transform);
					lastPlayTime = Time.fixedTime;
				}
			}
		}
	}
}
