#pragma warning disable 0618,0619
using System;
using UnityEngine;

public class CustomLayer : MonoBehaviour
{
	[Flags]
	public enum Layers
	{
		Layer0 = 0,
		Layer1 = 1,
		Layer2 = 2,
		Layer3 = Layer1 | Layer2,
		Layer4 = 4,
		Layer5 = Layer1 | Layer4,
		Layer6 = Layer2 | Layer4,
		Layer7 = Layer3 | Layer4,
		Layer8 = 8,
		Layer9 = Layer1 | Layer8,
		Layer10 = Layer2 | Layer8,
		Layer11 = Layer3 | Layer8,
		Layer12 = Layer4 | Layer8,
		Layer13 = Layer5 | Layer8,
		Layer14 = Layer6 | Layer8,
		Layer15 = Layer7 | Layer8
	}

	[HideInInspector]
	public int layers;

	public bool applyToChildren = true;

	private void Awake()
	{
		if (applyToChildren)
		{
			spawnCustomLayerOnChildren(base.transform);
		}
	}

	private void spawnCustomLayerOnChildren(Transform root)
	{
		foreach (Transform item in root)
		{
			CustomLayer component = item.gameObject.GetComponent<CustomLayer>();
			if (!component)
			{
				CustomLayer customLayer = item.gameObject.AddComponent<CustomLayer>();
				customLayer.layers = layers;
				spawnCustomLayerOnChildren(item);
			}
		}
	}
}
