using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(GUIText))]
public class infoText : MonoBehaviour
{
	private float lastClickTime;

	public float acknowledgeDuration;

	public float provideHelpTime;

	public string helpText;

	public infoText()
	{
		acknowledgeDuration = 0.3f;
		provideHelpTime = 3f;
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			lastClickTime = Time.time;
		}
		if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !(Time.time <= lastClickTime + acknowledgeDuration))
		{
			guiText.text = string.Empty;
			UnityEngine.Object.Destroy(gameObject);
		}
		if (!(Time.time <= provideHelpTime))
		{
			guiText.text = helpText;
		}
		else
		{
			guiText.text = string.Empty;
		}
	}

	public virtual void Main()
	{
	}
}
