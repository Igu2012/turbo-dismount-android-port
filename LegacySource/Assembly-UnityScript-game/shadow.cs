using System;
using UnityEngine;

[Serializable]
public class shadow : MonoBehaviour
{
	public string[] ignoreRootName;

	public float distanceTolerance;

	public float maxOpacity;

	private float multiplier;

	private float opacity;

	private Transform castingPoint;

	private float buffer;

	public shadow()
	{
		distanceTolerance = 0.4f;
		maxOpacity = 1f;
		multiplier = 1.2f;
		opacity = 1f;
		buffer = 0.02f;
	}

	public virtual void Start()
	{
		renderer.enabled = true;
		castingPoint = transform.Find("castingPoint");
		castingPoint.parent = transform.parent;
		transform.parent = transform.root;
	}

	public virtual void LateUpdate()
	{
		transform.position = castingPoint.position;
		RaycastHit[] array = null;
		array = Physics.RaycastAll(transform.position + Vector3.up * 0.5f, -Vector3.up);
		float num = -999999f;
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			string text = raycastHit.transform.root.name;
			bool flag = true;
			for (int j = 0; j < ignoreRootName.Length; j++)
			{
				string text2 = ignoreRootName[j];
				if (text == text2)
				{
					flag = false;
				}
			}
			if (flag && !(raycastHit.point.y + buffer <= num))
			{
				num = raycastHit.point.y + buffer;
				float y = raycastHit.point.y + buffer;
				Vector3 position = transform.position;
				float num2 = (position.y = y);
				Vector3 vector = (transform.position = position);
				transform.LookAt(transform.position + raycastHit.normal);
			}
		}
		float num3 = Vector3.Distance(transform.position, castingPoint.position);
		opacity = Mathf.Lerp(maxOpacity, 0f, num3 * (1f / distanceTolerance));
		if (array.Length == 0)
		{
			opacity = 0f;
		}
		opacity *= multiplier;
		float a = opacity;
		Color color = renderer.material.color;
		float num4 = (color.a = a);
		Color color2 = (renderer.material.color = color);
	}

	public virtual void Main()
	{
	}
}
