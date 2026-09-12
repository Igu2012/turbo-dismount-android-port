using System;
using UnityEngine;

[Serializable]
public class buttonPicker : MonoBehaviour
{
	public buttonPosition[] buttonList;

	public Vector2 startPosition;

	public float separation;

	public float minButtonSize;

	public float buttonSize;

	public float mouseInfluence;

	public buttonPicker()
	{
		minButtonSize = 0.99f;
		buttonSize = 200f;
		mouseInfluence = 0.7f;
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		float num = 0f;
		for (int i = 0; i < buttonList.Length; i++)
		{
			buttonList[i].buttonPosition.x = startPosition.x;
			buttonList[i].buttonPosition.y = startPosition.y + num;
			buttonList[i].yPositionFromTop = true;
			float num2 = Vector3.Distance(b: new Vector3
			{
				x = buttonList[i].getGuiTexture.pixelInset.x - (float)buttonList[i].getGuiTexture.texture.width * 0.5f,
				y = buttonList[i].getGuiTexture.pixelInset.y + (float)buttonList[i].getGuiTexture.texture.height * 0.5f
			}, a: Input.mousePosition);
			float a = buttonSize / Mathf.Max(num2 / mouseInfluence, buttonSize);
			a = Mathf.Max(a, minButtonSize);
			buttonList[i].buttonSize = a;
			num += separation * a;
		}
	}

	public virtual void Main()
	{
	}
}
