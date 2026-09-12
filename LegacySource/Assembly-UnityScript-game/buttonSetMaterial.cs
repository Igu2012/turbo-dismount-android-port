using System;
using UnityEngine;

[Serializable]
public class buttonSetMaterial : MonoBehaviour
{
	public Material[] pickMaterials;

	public int currentMaterialID;

	public SkinnedMeshRenderer chooseMeshRenderer;

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		if (Input.GetMouseButtonDown(0) && guiTexture.HitTest(Input.mousePosition))
		{
			currentMaterialID++;
			if (currentMaterialID >= pickMaterials.Length)
			{
				currentMaterialID = 0;
			}
			chooseMeshRenderer.material = pickMaterials[currentMaterialID];
		}
	}

	public virtual void Main()
	{
	}
}
