#pragma warning disable 0618,0619
using System.Collections.Generic;
using Dismount;
using UnityEngine;

public class StatelessParticleSystem : MonoBehaviour
{
	private struct Particle
	{
		public float startSize;

		public float startTime;

		public float startRotationVelocity;

		public Vector3 startVelocity;

		public Vector3 startPosition;
	}

	public const int maxParticles = 1000;

	public int emitRate = 1;

	public Vector3 startVelocity;

	public float startSize = 1f;

	public float startRotationVelocity = 360f;

	public AnimationCurve velocityModifier;

	public AnimationCurve sizeModifier;

	public AnimationCurve alphaModifier;

	private float startTime;

	private Vector3[] newVertices;

	private Vector2[] newUV;

	private Color[] newColors;

	private int[] indices;

	private Mesh mesh;

	private List<Particle> particles = new List<Particle>();

	private Particle particle = default(Particle);

	private float timeBetweenSpawns;

	private float previousSpawnTime = -1f;

	public bool playback = true;

	public float myTime;

	private void spawnParticle(float startTime)
	{
		particle.startPosition = base.transform.position;
		particle.startTime = startTime + Random.Range(0f, 0.5f);
		particle.startVelocity = startVelocity + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.5f, 0.5f), Random.Range(-0.2f, 0.2f));
		particle.startSize = startSize + Random.Range(-0.3f, 0.3f);
		particle.startRotationVelocity = startRotationVelocity + Random.Range((0f - startRotationVelocity) * 0.3f, startRotationVelocity * 0.3f);
		particles.Add(particle);
	}

	private void Awake()
	{
		mesh = GetComponent<MeshFilter>().mesh;
		mesh.MarkDynamic();
		newVertices = new Vector3[4000];
		newUV = new Vector2[4000];
		newColors = new Color[4000];
		indices = new int[6000];
		for (int i = 0; i < 4000; i++)
		{
			newVertices[i] = default(Vector3);
			newUV[i] = default(Vector2);
			newColors[i] = new Color(1f, 1f, 1f, 1f);
		}
		for (int j = 0; j < 6000; j++)
		{
			indices[j] = 0;
		}
	}

	private void OnEnable()
	{
		startTime = Time.fixedTime;
		myTime = Time.fixedTime;
		timeBetweenSpawns = 1f / (float)emitRate;
		MonoBehaviour.print("timeBetweenSpawns = " + timeBetweenSpawns);
	}

	private void FixedUpdate()
	{
		if (playback && (previousSpawnTime == -1f || Time.fixedTime - previousSpawnTime > timeBetweenSpawns))
		{
			spawnParticle(Time.fixedTime);
			previousSpawnTime = Time.fixedTime;
		}
	}

	private void Update()
	{
		GameObject currentCamera = DismountGame.cameraManager.currentCamera;
		if (!currentCamera)
		{
			return;
		}
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		base.transform.LookAt(base.transform.position + currentCamera.transform.rotation * Vector3.back, currentCamera.transform.rotation * Vector3.up);
		int num = 0;
		Transform transform = new GameObject().transform;
		for (int i = 0; i < particles.Count; i++)
		{
			float num2 = Time.fixedTime - particles[i].startTime;
			if (!playback)
			{
				num2 = myTime - particles[i].startTime;
			}
			if (!(num2 < 0f))
			{
				float num3 = alphaModifier.Evaluate(num2);
				if (num3 >= 0.01f)
				{
					float num4 = particles[i].startSize * sizeModifier.Evaluate(num2);
					Vector3 vector = particles[i].startPosition + particles[i].startVelocity * num2;
					float angle = particles[i].startRotationVelocity * num2;
					transform.rotation = Quaternion.identity;
					transform.Rotate(new Vector3(0f, 0f, 1f), angle);
					newVertices[num * 4] = vector + base.transform.TransformPoint(transform.TransformPoint(new Vector3(0f - num4, num4, 0f)));
					newVertices[num * 4 + 1] = vector + base.transform.TransformPoint(transform.TransformPoint(new Vector3(num4, num4, 0f)));
					newVertices[num * 4 + 2] = vector + base.transform.TransformPoint(transform.TransformPoint(new Vector3(num4, 0f - num4, 0f)));
					newVertices[num * 4 + 3] = vector + base.transform.TransformPoint(transform.TransformPoint(new Vector3(0f - num4, 0f - num4, 0f)));
					newUV[num * 4] = new Vector2(0f, 0f);
					newUV[num * 4 + 1] = new Vector2(1f, 0f);
					newUV[num * 4 + 2] = new Vector2(1f, 1f);
					newUV[num * 4 + 3] = new Vector2(0f, 1f);
					num3 = Mathf.Min(1f, Mathf.Max(0f, num3));
					Color color = new Color(1f, 1f, 1f, num3);
					newColors[num * 4] = (newColors[num * 4 + 1] = (newColors[num * 4 + 2] = (newColors[num * 4 + 3] = color)));
					num++;
				}
			}
		}
		for (int j = 0; j < num; j++)
		{
			indices[j * 6] = j * 4;
			indices[j * 6 + 1] = j * 4 + 1;
			indices[j * 6 + 2] = j * 4 + 2;
			indices[j * 6 + 3] = j * 4;
			indices[j * 6 + 4] = j * 4 + 2;
			indices[j * 6 + 5] = j * 4 + 3;
		}
		for (int k = num; k < 1000; k++)
		{
			indices[k * 6] = 0;
			indices[k * 6 + 1] = 0;
			indices[k * 6 + 2] = 0;
			indices[k * 6 + 3] = 0;
			indices[k * 6 + 4] = 0;
			indices[k * 6 + 5] = 0;
		}
		mesh.Clear();
		mesh.vertices = newVertices;
		mesh.uv = newUV;
		mesh.colors = newColors;
		mesh.triangles = indices;
		base.transform.position = position;
		base.transform.rotation = rotation;
	}
}
