#pragma warning disable 0618,0619
using UnityEngine;

[RequireComponent(typeof(ShatterTool))]
[RequireComponent(typeof(Rigidbody))]
public class Shatterer : MonoBehaviour
{
	public ParticleSystem shatterParticleSystem;

	private ParticleSystem.Particle glassParticle = default(ParticleSystem.Particle);

	public ShatterScheduler scheduler;

	public float requiredForce = 1f;

	public float cooldownTime = 0.5f;

	private float startTime;

	private ShatterTool shatterTool;

	public void Start()
	{
		shatterTool = GetComponent<ShatterTool>();
		if (!scheduler)
		{
			Debug.LogWarning("Shatterer has no scheduler set. It's strongly recommended to set one for performance reasons");
		}
		startTime = Time.time;
		glassParticle.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		glassParticle.randomSeed = 5u;
	}

	private void EmitParticle(Vector3 position, Vector3 velocity)
	{
		glassParticle.position = position;
		glassParticle.velocity = velocity;
		glassParticle.remainingLifetime = Random.Range(1, 3);
		glassParticle.startLifetime = glassParticle.remainingLifetime;
		glassParticle.size = (float)Random.Range(20, 50) * 0.01f;
		glassParticle.rotation = Random.Range(0, 360);
		glassParticle.angularVelocity = 0f;
		shatterParticleSystem.Emit(glassParticle);
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (!(Time.time >= startTime + cooldownTime) || !(collision.relativeVelocity.sqrMagnitude >= requiredForce))
		{
			return;
		}
		base.transform.parent = null;
		int num = 6 / shatterTool.Generation;
		float num2 = (float)num * 0.2f;
		ContactPoint[] contacts = collision.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if (!(contactPoint.otherCollider == collision.collider))
			{
				continue;
			}
			if (!shatterTool.IsLastGeneration)
			{
				if (scheduler != null)
				{
					scheduler.AddTask(new ShatterTask(contactPoint.thisCollider.gameObject, contactPoint.point));
				}
				else
				{
					contactPoint.thisCollider.SendMessage("Shatter", contactPoint.point, SendMessageOptions.DontRequireReceiver);
				}
			}
			if ((bool)shatterParticleSystem)
			{
				for (int j = 0; j < num; j++)
				{
					EmitParticle(contactPoint.point + Random.onUnitSphere, base.GetComponent<Rigidbody>().velocity + Random.onUnitSphere * num2);
				}
			}
			break;
		}
		if (shatterTool.IsLastGeneration)
		{
			Object.Destroy(this);
		}
	}
}
