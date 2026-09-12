using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(GUIText))]
public class clearText : MonoBehaviour
{
	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		guiText.text = string.Empty;
	}

	public virtual void Main()
	{
	}
}
