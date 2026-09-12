using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(GUITexture))]
public class buttonPosition : MonoBehaviour
{
	public bool yPositionFromTop;

	public bool xPositionFromRight;

	public Vector2 buttonPosition;

	public float buttonSize;

	public GUITexture getGuiTexture;

	public buttonPosition()
	{
		yPositionFromTop = true;
		buttonSize = 1f;
	}

	public virtual void Start()
	{
		getGuiTexture = (GUITexture)GetComponent(typeof(GUITexture));
		transform.position = Vector3.zero;
	}

	public virtual void Update()
	{
		if (xPositionFromRight)
		{
			float num = (float)Screen.width - buttonPosition.x - (float)guiTexture.texture.width * 0.5f * buttonSize;
			Rect pixelInset = guiTexture.pixelInset;
			float num2 = (pixelInset.x = num);
			Rect rect = (guiTexture.pixelInset = pixelInset);
		}
		else
		{
			float num4 = buttonPosition.x - (float)guiTexture.texture.width * 0.5f * buttonSize;
			Rect pixelInset2 = guiTexture.pixelInset;
			float num5 = (pixelInset2.x = num4);
			Rect rect3 = (guiTexture.pixelInset = pixelInset2);
		}
		if (yPositionFromTop)
		{
			float num7 = (float)Screen.height - buttonPosition.y - (float)guiTexture.texture.height * 0.5f * buttonSize;
			Rect pixelInset3 = guiTexture.pixelInset;
			float num8 = (pixelInset3.y = num7);
			Rect rect5 = (guiTexture.pixelInset = pixelInset3);
		}
		else
		{
			float num10 = buttonPosition.y - (float)guiTexture.texture.height * 0.5f * buttonSize;
			Rect pixelInset4 = guiTexture.pixelInset;
			float num11 = (pixelInset4.y = num10);
			Rect rect7 = (guiTexture.pixelInset = pixelInset4);
		}
		float num13 = (float)guiTexture.texture.width * buttonSize;
		Rect pixelInset5 = guiTexture.pixelInset;
		float num14 = (pixelInset5.width = num13);
		Rect rect9 = (guiTexture.pixelInset = pixelInset5);
		float num16 = (float)guiTexture.texture.height * buttonSize;
		Rect pixelInset6 = guiTexture.pixelInset;
		float num17 = (pixelInset6.height = num16);
		Rect rect11 = (guiTexture.pixelInset = pixelInset6);
	}

	public virtual void Main()
	{
	}
}
