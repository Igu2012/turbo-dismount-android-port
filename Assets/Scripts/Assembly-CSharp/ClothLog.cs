#pragma warning disable 0618,0619
using UnityEngine;

public class ClothLog : MonoBehaviour
{
	private InteractiveCloth cloth;

	private Vector3 prevPos;

	private int counter = 1;

	private void Start()
	{
		cloth = GetComponent<InteractiveCloth>();
		prevPos = base.transform.position;
	}

	private void Update()
	{
		if (counter % 100 == 0)
		{
			Vector3[] vertices = cloth.vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				Debug.Log("[" + i + "]   " + vertices[i]);
			}
		}
		Vector3 position = base.transform.position;
		Vector3 vector = (position - prevPos) / Time.deltaTime;
		prevPos = position;
		cloth.externalAcceleration = -5f * vector;
	}
}
