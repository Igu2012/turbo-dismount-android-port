using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(GUITexture))]
public class buttonSwitchTexture : MonoBehaviour
{
	public Texture[] textureList;

	public int currentTextureID;

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		if (Input.GetMouseButtonDown(0) && guiTexture.HitTest(Input.mousePosition))
		{
			currentTextureID++;
			if (currentTextureID >= textureList.Length)
			{
				currentTextureID = 0;
			}
			guiTexture.texture = textureList[currentTextureID];
		}
	}

	public virtual void Main()
	{
	}
}
