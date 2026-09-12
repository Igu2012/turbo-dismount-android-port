using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(buttonPosition))]
[RequireComponent(typeof(GUITexture))]
public class buttonClickFlash : MonoBehaviour
{
	private float flashStartTime;

	private AnimationCurve changeCurve;

	public virtual void Start()
	{
		changeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.1f, 0.3f), new Keyframe(0.3f, -0.1f), new Keyframe(0.6f, 0f));
	}

	public virtual void Update()
	{
		if (Input.GetMouseButtonDown(0) && guiTexture.HitTest(Input.mousePosition))
		{
			flashStartTime = Time.time;
		}
		buttonPosition buttonPosition2 = (buttonPosition)GetComponent(typeof(buttonPosition));
		float num = default(float);
		if (!(Time.time <= flashStartTime) && !(Time.time >= flashStartTime + (float)changeCurve.length))
		{
			float time = Time.time - flashStartTime;
			num = changeCurve.Evaluate(time);
			buttonPosition2.buttonSize += num;
			float num2 = 0.5f + changeCurve.Evaluate(time) * 0.3f;
			float b = 0.5f + changeCurve.Evaluate(time) * 0.5f;
			guiTexture.color = new Color(num2, num2, b);
		}
	}

	public virtual void Main()
	{
	}
}
