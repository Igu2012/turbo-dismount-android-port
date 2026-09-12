#pragma warning disable 0618,0619
using UnityEngine;
using reLive;

public class Bolt : MonoBehaviour
{
	private float spawnTime;

	private float maxAge = 4.5f;

	private ObjectPool pool;

	private void Start()
	{
		GetComponent<RecordMeInReplay>().Warmup();
	}

	private void OnEnable()
	{
		maxAge = Random.Range(1f, 3.5f);
		spawnTime = Time.time;
	}

	private void Update()
	{
		float num = Time.time - spawnTime;
		if (num > maxAge)
		{
			base.gameObject.SetActive(false);
			if (pool != null)
			{
				pool.Free(base.gameObject);
			}
		}
	}

	private void SetPool(ObjectPool pool)
	{
		this.pool = pool;
	}
}
