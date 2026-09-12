#pragma warning disable 0618,0619
using UnityEngine;

public class MultiObjectPool
{
	private ObjectPool[] objectPools;

	public MultiObjectPool(GameObject[] prefabs)
	{
		objectPools = new ObjectPool[prefabs.Length];
		for (int i = 0; i < objectPools.Length; i++)
		{
			objectPools[i] = new ObjectPool();
			objectPools[i].prefab = prefabs[i];
		}
	}

	public GameObject Get()
	{
		int num = Random.Range(0, objectPools.Length);
		return objectPools[num].Get();
	}

	public void Prealloc(int count)
	{
		for (int i = 0; i < objectPools.Length; i++)
		{
			objectPools[i].Prealloc(count);
		}
	}
}
