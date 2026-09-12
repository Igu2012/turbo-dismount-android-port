#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Object
{
	private Stack<GameObject> items = new Stack<GameObject>();

	public GameObject prefab;

	private static Transform objectParent;

	private GameObject createInstance()
	{
		GameObject gameObject = Object.Instantiate(prefab) as GameObject;
		gameObject.SetActive(true);
		gameObject.SendMessage("SetPool", this);
		gameObject.SetActive(false);
		if (!objectParent)
		{
			objectParent = GameObject.Find("/DismountGame/ObjectPool").transform;
		}
		gameObject.transform.parent = objectParent;
		return gameObject;
	}

	public GameObject Get()
	{
		if (items.Count == 0)
		{
			return createInstance();
		}
		return items.Pop();
	}

	public void Free(GameObject item)
	{
		items.Push(item);
	}

	public void Prealloc(int numberOfItems)
	{
		int count = items.Count;
		for (int i = 0; i < count; i++)
		{
			Object.Destroy(items.Pop());
		}
		for (int j = 0; j < numberOfItems; j++)
		{
			GameObject t = createInstance();
			items.Push(t);
		}
	}
}
