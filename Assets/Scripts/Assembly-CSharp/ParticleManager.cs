#pragma warning disable 0618,0619
using System;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
	private struct IncrementalTrig
	{
		public float cos;

		public float sin;

		private float cosd;

		private float sind;

		private float cosn;

		private float sinn;

		public void Init(int divs)
		{
			float f = (float)Math.PI * 2f / (float)divs;
			sin = 0f;
			cos = 1f;
			sind = Mathf.Sin(f);
			cosd = Mathf.Cos(f);
		}

		public void Init(float startAngle, int divs)
		{
			float delta = (float)Math.PI * 2f / (float)divs;
			Init(startAngle, delta);
		}

		public void Init(float startAngle, float delta)
		{
			sin = Mathf.Sin(startAngle);
			cos = Mathf.Cos(startAngle);
			sind = Mathf.Sin(delta);
			cosd = Mathf.Cos(delta);
		}

		public void Inc()
		{
			sinn = sin * cosd + cos * sind;
			cosn = cos * cosd - sin * sind;
			sin = sinn;
			cos = cosn;
		}
	}

	private ParticleSystem dustParticleSystem;

	private ParticleSystem smokeParticleSystem;

	private ParticleSystem sparkParticleSystem;

	private ParticleSystem rocketSmokeParticleSystem;

	private ParticleSystem rockParticleSystem;

	private ParticleSystem shatterParticleSystem;

	private ParticleSystem fireParticleSystem;

	private ParticleSystem flameParticleSystem;

	private GameObject particleSystemRoot;

	private ParticleSystem.Particle particle = default(ParticleSystem.Particle);

	private uint randomSeedCounter;

	private float lastDustRingTick;

	public GameObject GetParticleSystemRoot()
	{
		return particleSystemRoot;
	}

	private void Start()
	{
		particleSystemRoot = base.transform.Find("ParticleSystems").gameObject;
		base.transform.Find("ParticleSystemsLight").gameObject.SetActive(false);
		dustParticleSystem = particleSystemRoot.transform.Find("DustParticleSystem").GetComponent<ParticleSystem>();
		smokeParticleSystem = particleSystemRoot.transform.Find("SmokeParticleSystem").GetComponent<ParticleSystem>();
		sparkParticleSystem = particleSystemRoot.transform.Find("SparkParticleSystem").GetComponent<ParticleSystem>();
		rocketSmokeParticleSystem = particleSystemRoot.transform.Find("RocketSmokeParticleSystem").GetComponent<ParticleSystem>();
		rockParticleSystem = particleSystemRoot.transform.Find("RockParticleSystem").GetComponent<ParticleSystem>();
		shatterParticleSystem = particleSystemRoot.transform.Find("ShatteredGround").GetComponent<ParticleSystem>();
		fireParticleSystem = particleSystemRoot.transform.Find("FireParticleSystem").GetComponent<ParticleSystem>();
		flameParticleSystem = particleSystemRoot.transform.Find("FlameParticleSystem").GetComponent<ParticleSystem>();
	}

	public void Reset()
	{
		dustParticleSystem.Clear();
		smokeParticleSystem.Clear();
		sparkParticleSystem.Clear();
		rocketSmokeParticleSystem.Clear();
		rockParticleSystem.Clear();
		shatterParticleSystem.Clear();
		fireParticleSystem.Clear();
		flameParticleSystem.Clear();
	}

	public void EmitDust(Vector3 position, Vector3 velocity, float intensity = 1f)
	{
		particle.randomSeed = randomSeedCounter++;
		Color32 color = dustParticleSystem.startColor;
		color.a = (byte)(intensity * (float)(int)color.a);
		particle.color = color;
		float startLifetime = dustParticleSystem.startLifetime;
		particle.remainingLifetime = startLifetime;
		particle.startLifetime = startLifetime;
		particle.size = dustParticleSystem.startSize;
		particle.rotation = dustParticleSystem.startRotation;
		particle.angularVelocity = 0f;
		particle.position = position;
		particle.velocity = velocity;
		dustParticleSystem.Emit(particle);
	}

	public void EmitDustRing(Vector3 position, Vector3 normal, float intensity = 1f)
	{
		if (Time.fixedTime < lastDustRingTick + 1f)
		{
			return;
		}
		Vector3 vector = new Vector3(normal.y, 0f - normal.x, 0f);
		Vector3 vector2 = Vector3.Cross(normal, vector);
		int num = (int)(intensity * 16f);
		if (num > 0)
		{
			lastDustRingTick = Time.fixedTime;
			Vector3 zero = Vector3.zero;
			Color32 color = dustParticleSystem.startColor;
			color.a = (byte)(intensity * (float)(int)color.a);
			float num2 = 0f;
			float num3 = (float)Math.PI * 2f / (float)num;
			int num4 = 0;
			while (num4 < num)
			{
				zero = Mathf.Sin(num2) * vector + Mathf.Cos(num2) * vector2;
				zero *= intensity * 4f * UnityEngine.Random.Range(0.5f, 1.5f);
				zero += normal;
				particle.randomSeed = randomSeedCounter++;
				particle.color = color;
				float num5 = dustParticleSystem.startLifetime * 0.5f;
				particle.remainingLifetime = num5;
				particle.startLifetime = num5;
				particle.size = dustParticleSystem.startSize * 1.4f;
				particle.rotation = dustParticleSystem.startRotation;
				particle.angularVelocity = 0f;
				particle.position = position;
				particle.velocity = zero;
				dustParticleSystem.Emit(particle);
				num4++;
				num2 += num3;
			}
		}
	}

	public void EmitSmoke(Vector3 position, Vector3 velocity, float intensity = 1f)
	{
		particle.randomSeed = randomSeedCounter++;
		Color32 color = smokeParticleSystem.startColor;
		color.a = (byte)(intensity * (float)(int)color.a);
		particle.color = color;
		float startLifetime = smokeParticleSystem.startLifetime;
		particle.remainingLifetime = startLifetime;
		particle.startLifetime = startLifetime;
		particle.size = smokeParticleSystem.startSize * (0.5f + 0.5f * intensity);
		particle.rotation = smokeParticleSystem.startRotation;
		particle.angularVelocity = 0f;
		particle.position = position;
		particle.velocity = velocity;
		smokeParticleSystem.Emit(particle);
	}

	public void EmitSpark(Vector3 position, Vector3 velocity, float intensity = 1f)
	{
		particle.randomSeed = randomSeedCounter++;
		Color32 color = sparkParticleSystem.startColor;
		color.a = (byte)(intensity * (float)(int)color.a);
		particle.color = color;
		float startLifetime = sparkParticleSystem.startLifetime;
		particle.remainingLifetime = startLifetime;
		particle.startLifetime = startLifetime;
		particle.size = sparkParticleSystem.startSize;
		particle.rotation = sparkParticleSystem.startRotation;
		particle.angularVelocity = 0f;
		particle.position = position;
		particle.velocity = velocity;
		sparkParticleSystem.Emit(particle);
	}

	public void EmitRocketSmoke(Vector3 position, Vector3 velocity, float intensity = 1f)
	{
		particle.randomSeed = randomSeedCounter++;
		Color32 color = rocketSmokeParticleSystem.startColor;
		color.a = (byte)(intensity * (float)(int)color.a);
		particle.color = color;
		float startLifetime = rocketSmokeParticleSystem.startLifetime;
		particle.remainingLifetime = startLifetime;
		particle.startLifetime = startLifetime;
		particle.size = rocketSmokeParticleSystem.startSize;
		particle.rotation = rocketSmokeParticleSystem.startRotation;
		particle.angularVelocity = 0f;
		particle.position = position;
		particle.velocity = velocity;
		rocketSmokeParticleSystem.Emit(particle);
	}

	public void EmitRocks(Vector3 position, float intensity = 1f)
	{
		Color32 color = rockParticleSystem.startColor;
		particle.color = color;
		float startLifetime = rockParticleSystem.startLifetime;
		particle.remainingLifetime = startLifetime;
		particle.startLifetime = startLifetime;
		particle.size = rockParticleSystem.startSize;
		particle.rotation = rockParticleSystem.startRotation;
		for (int i = 0; (float)i < 15f * intensity; i++)
		{
			particle.randomSeed = randomSeedCounter++;
			particle.position = position;
			particle.size = UnityEngine.Random.Range(rockParticleSystem.startSize / 2f, rockParticleSystem.startSize);
			particle.velocity = intensity * new Vector3(6f * (1f - UnityEngine.Random.Range(0f, 2f)), UnityEngine.Random.Range(1, 6), 6f * (1f - UnityEngine.Random.Range(0f, 2f)));
			rockParticleSystem.Emit(particle);
		}
	}

	public void EmitShatter(Vector3 position, float intensity = 1f)
	{
		particle.randomSeed = randomSeedCounter++;
		Color32 color = shatterParticleSystem.startColor;
		color.a = (byte)(intensity * (float)(int)color.a);
		particle.color = color;
		float startLifetime = shatterParticleSystem.startLifetime;
		particle.remainingLifetime = startLifetime;
		particle.startLifetime = startLifetime;
		particle.size = shatterParticleSystem.startSize;
		particle.rotation = shatterParticleSystem.startRotation;
		particle.angularVelocity = 0f;
		particle.position = position;
		particle.velocity = new Vector3(0f, 0f, 0f);
		shatterParticleSystem.Emit(particle);
	}

	public void EmitExplosion(Vector3 position, Vector3 normal, float intensity = 1f)
	{
		Vector3 vector = new Vector3(normal.y, 0f - normal.x, 0f);
		Vector3 vector2 = Vector3.Cross(normal, vector);
		int num = (int)(intensity * 16f);
		if (num > 0)
		{
			lastDustRingTick = Time.fixedTime;
			Vector3 zero = Vector3.zero;
			Color32 color = dustParticleSystem.startColor;
			color.a = (byte)(intensity * (float)(int)color.a);
			float num2 = 0f;
			float num3 = (float)Math.PI * 2f / (float)num;
			int num4 = 0;
			while (num4 < num)
			{
				zero = Mathf.Sin(num2) * vector + Mathf.Cos(num2) * vector2;
				zero *= intensity * 7f * UnityEngine.Random.Range(0.5f, 1.5f);
				zero += normal;
				particle.randomSeed = randomSeedCounter++;
				particle.color = color;
				float num5 = smokeParticleSystem.startLifetime * 0.5f;
				particle.remainingLifetime = num5;
				particle.startLifetime = num5;
				particle.size = smokeParticleSystem.startSize * 1.4f;
				particle.rotation = smokeParticleSystem.startRotation;
				particle.angularVelocity = 0f;
				particle.position = position;
				particle.velocity = zero;
				smokeParticleSystem.Emit(particle);
				num4++;
				num2 += num3;
			}
			for (int i = 0; i < num; i++)
			{
				zero = new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(10f, 50f), UnityEngine.Random.Range(-3f, 3f));
				particle.randomSeed = randomSeedCounter++;
				particle.color = color;
				float num5 = smokeParticleSystem.startLifetime * 0.5f;
				particle.remainingLifetime = num5;
				particle.startLifetime = num5;
				particle.size = smokeParticleSystem.startSize * 1.4f;
				particle.rotation = smokeParticleSystem.startRotation;
				particle.angularVelocity = 0f;
				particle.position = position;
				particle.velocity = zero;
				smokeParticleSystem.Emit(particle);
			}
			for (int j = 0; j < num * 2; j++)
			{
				zero = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(0f, 15f), UnityEngine.Random.Range(-5f, 5f));
				particle.randomSeed = randomSeedCounter++;
				particle.color = color;
				float num5 = fireParticleSystem.startLifetime * 0.5f;
				particle.remainingLifetime = num5;
				particle.startLifetime = num5;
				particle.size = fireParticleSystem.startSize * 1.4f;
				particle.rotation = fireParticleSystem.startRotation;
				particle.angularVelocity = 0f;
				particle.position = position;
				particle.velocity = zero;
				fireParticleSystem.Emit(particle);
			}
			EmitRocks(position, 2f);
		}
	}

	public void EmitLaserImpact(Vector3 position, Vector3 velocity)
	{
		particle.randomSeed = randomSeedCounter++;
		particle.color = new Color(1f, 0.25f, 0.5f, 1f);
		float num = fireParticleSystem.startLifetime * 0.5f;
		particle.remainingLifetime = num;
		particle.startLifetime = num;
		particle.size = fireParticleSystem.startSize;
		particle.rotation = fireParticleSystem.startRotation;
		particle.angularVelocity = 0f;
		particle.position = position;
		particle.velocity = velocity;
		fireParticleSystem.Emit(particle);
	}

	public void EmitFlame(Vector3 position, Vector3 velocity)
	{
		Vector3 normalized = velocity.normalized;
		Vector3 velocity2 = velocity * 0.15f;
		for (int i = 0; i < 3; i++)
		{
			particle.randomSeed = randomSeedCounter++;
			particle.color = Color.white;
			float startLifetime = flameParticleSystem.startLifetime;
			particle.remainingLifetime = startLifetime;
			particle.startLifetime = startLifetime;
			particle.size = UnityEngine.Random.Range(0.5f, 1.4f) * flameParticleSystem.startSize * 0.4f;
			particle.rotation = flameParticleSystem.startRotation;
			particle.angularVelocity = 0f;
			particle.position = position + new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), 0f, UnityEngine.Random.Range(-0.2f, 0.2f)) + normalized * UnityEngine.Random.Range(-1f, 1f);
			particle.velocity = velocity2;
			flameParticleSystem.Emit(particle);
		}
	}

	public void EmitTankGunShot(Vector3 position, Vector3 direction, Vector3 velocity)
	{
		Vector3 axis = Vector3.left;
		Vector3 axis2 = Vector3.up;
		FindPlaneAxes(direction, out axis, out axis2);
		int num = 16;
		Vector3 zero = Vector3.zero;
		Color32 color = dustParticleSystem.startColor;
		IncrementalTrig incrementalTrig = default(IncrementalTrig);
		incrementalTrig.Init(num);
		for (int i = 0; i < num; i++)
		{
			zero = incrementalTrig.cos * axis + incrementalTrig.sin * axis2;
			zero *= 3f * UnityEngine.Random.Range(0.75f, 1.333f);
			zero += velocity;
			particle.randomSeed = randomSeedCounter++;
			particle.color = color;
			float num2 = smokeParticleSystem.startLifetime * 0.5f;
			particle.remainingLifetime = num2;
			particle.startLifetime = num2;
			particle.size = smokeParticleSystem.startSize * UnityEngine.Random.Range(0.4f, 0.7f);
			particle.rotation = smokeParticleSystem.startRotation;
			particle.angularVelocity = 0f;
			particle.position = position;
			particle.velocity = zero;
			smokeParticleSystem.Emit(particle);
			incrementalTrig.Inc();
		}
		position += direction * 0.5f;
		num = 6;
		for (int j = 0; j < num; j++)
		{
			float num3 = j;
			zero = UnityEngine.Random.Range(-2.5f, 2.5f) * axis + UnityEngine.Random.Range(-2.5f, 2.5f) * axis2 + (2f + num3 * 4f) * direction;
			zero += velocity;
			particle.randomSeed = randomSeedCounter++;
			particle.color = color;
			float num2 = 0.2f + num3 * 0.1f;
			particle.remainingLifetime = num2;
			particle.startLifetime = num2;
			particle.size = fireParticleSystem.startSize * (0.4f + num3 * 0.2f);
			particle.rotation = fireParticleSystem.startRotation;
			particle.angularVelocity = 1f - num3 * 0.1f;
			position -= direction * 0.25f;
			particle.position = position;
			particle.velocity = zero;
			fireParticleSystem.Emit(particle);
		}
	}

	private void FindPlaneAxes(Vector3 normal, out Vector3 axis1, out Vector3 axis2)
	{
		Vector3 vector = new Vector3(normal.y, 0f - normal.x, 0f);
		Vector3 vector2 = new Vector3(0f - normal.z, 0f, normal.x);
		axis1 = (vector + vector2).normalized;
		axis2 = Vector3.Cross(normal.normalized, axis1);
	}
}
