#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class SmokeOnImpact : MonoBehaviour
	{
		public float minimumImpact = 10f;

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.contacts.Length <= 0)
			{
				return;
			}
			float num = Vector3.Dot(collision.relativeVelocity, collision.contacts[0].normal);
			float num2 = num * num;
			if (num2 >= minimumImpact)
			{
				float num3 = -1f;
				if (collision.contacts[0].thisCollider.GetComponent<Rigidbody>() != base.GetComponent<Rigidbody>())
				{
					num3 = 1f;
				}
				Vector3 normalized = Vector3.Reflect(base.GetComponent<Rigidbody>().velocity, collision.contacts[0].normal * num3).normalized;
				DismountGame.particleManager.EmitDust(collision.contacts[0].point, normalized * 2f);
				DismountGame.particleManager.EmitSpark(collision.contacts[0].point + normalized, normalized * 4f, 3f);
			}
		}
	}
}
