#pragma warning disable 0618,0619
using UnityEngine;

public class ShatterOnCollision : MonoBehaviour
{
	public ShatterScheduler scheduler;

	public float requiredForce = 1f;

	public float cooldownTime = 0.5f;

	private float timeSinceInstantiated;

	public void Update()
	{
		timeSinceInstantiated += Time.deltaTime;
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (!(timeSinceInstantiated >= cooldownTime) || !(collision.relativeVelocity.magnitude >= requiredForce))
		{
			return;
		}
		ContactPoint[] contacts = collision.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if (contactPoint.otherCollider == collision.collider)
			{
				if (scheduler != null)
				{
					scheduler.AddTask(new ShatterTask(contactPoint.thisCollider.gameObject, contactPoint.point));
				}
				else
				{
					contactPoint.thisCollider.SendMessage("Shatter", contactPoint.point, SendMessageOptions.DontRequireReceiver);
				}
				break;
			}
		}
	}
}
