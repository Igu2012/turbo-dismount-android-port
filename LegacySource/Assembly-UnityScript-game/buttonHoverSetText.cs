using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(GUITexture))]
public class buttonHoverSetText : MonoBehaviour
{
	public GUIText chooseText;

	public string textValue;

	public float offset;

	public buttonHoverSetText()
	{
		offset = 30f;
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		if (guiTexture.HitTest(Input.mousePosition))
		{
			chooseText.text = textValue;
			float x = Input.mousePosition.x + offset;
			Vector2 pixelOffset = chooseText.pixelOffset;
			float num = (pixelOffset.x = x);
			Vector2 vector = (chooseText.pixelOffset = pixelOffset);
			float y = Input.mousePosition.y;
			Vector2 pixelOffset2 = chooseText.pixelOffset;
			float num2 = (pixelOffset2.y = y);
			Vector2 vector3 = (chooseText.pixelOffset = pixelOffset2);
		}
	}

	public virtual void Main()
	{
	}
}
