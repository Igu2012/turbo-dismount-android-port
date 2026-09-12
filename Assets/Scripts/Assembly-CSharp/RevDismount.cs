#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using Dismount.Vehicular;
using UnityEngine;

public class RevDismount : MonoBehaviour
{
	public EngineSoundField[] engineSounds;

	private int index;

	private int drawLayer;

	private Car car;

	private EngineSoundField engineSound;

	private bool autoThrottle;

	[Range(0f, 1f)]
	private float throttle;

	private float engineRpm;

	private float engineLoad;

	private float engineIdleRpm = 1000f;

	private float engineMaxRpm = 10000f;

	private float slider;

	private float engineInertia = 0.9f;

	private float limiterAmount;

	private ApproximatedCurve engineOutputTorqueCurve;

	private ApproximatedCurve engineRpmForThrottle;

	private float currThrottle;

	private List<Vector2> loadHistory;

	private List<Vector2> rpmHistory;

	private int historyCount;

	private Transform storedAudioParent;

	private void InitHistory()
	{
		rpmHistory = new List<Vector2>();
		for (int i = 0; i < 100; i++)
		{
			rpmHistory.Add(new Vector2(-100 + i, 0f));
		}
		loadHistory = new List<Vector2>();
		for (int j = 0; j < 100; j++)
		{
			loadHistory.Add(new Vector2(-100 + j, 0f));
		}
		historyCount = 0;
	}

	private void ChangeEngine()
	{
		if (index < 0)
		{
			index = engineSounds.Length - 1;
		}
		else if (index >= engineSounds.Length)
		{
			index = 0;
		}
		if ((bool)engineSound)
		{
			engineSound.Stop();
		}
		if ((bool)engineSound && (bool)storedAudioParent)
		{
			engineSound.transform.parent = storedAudioParent;
		}
		engineSound = engineSounds[index];
		storedAudioParent = engineSound.transform.parent;
		car = engineSound.transform.parent.parent.GetComponent<Car>();
		engineSound.transform.parent = base.transform;
		AudioSource[] componentsInChildren = engineSound.GetComponentsInChildren<AudioSource>();
		foreach (AudioSource audioSource in componentsInChildren)
		{
			audioSource.transform.position = base.transform.position;
		}
		throttle = 0f;
		currThrottle = 0f;
		engineOutputTorqueCurve = new ApproximatedCurve(car.engineOutputTorque, car.curveMaxRpm, car.curveMaxTorque);
		CalculateEngineRpmForThrottle();
		engineIdleRpm = EngineRPMForThrottle(car.idleThrottle);
		engineMaxRpm = EngineRPMForThrottle(1f);
		engineRpm = engineIdleRpm;
		slider = 0f;
		engineInertia = car.engineInertia;
		engineLoad = 0f;
		Debug.Log(car.name + " idle: " + engineIdleRpm.ToString("0") + "rpm, max: " + engineMaxRpm.ToString("0") + "rpm");
		if ((bool)engineSound)
		{
			if (engineSound.maxVolumeRpm < 0f)
			{
				engineSound.maxVolumeRpm = engineMaxRpm * 0.75f;
			}
			engineSound.muteEndRpm = car.stallRpm;
			engineSound.muteStartRpm = car.stallRpm * 0.5f;
			engineSound.Play();
		}
		InitHistory();
	}

	private void Awake()
	{
		ChangeEngine();
	}

	private void CalculateEngineRpmForThrottle()
	{
		Vector2[] array = new Vector2[101];
		float num = 0f;
		for (int i = 0; i < 101; i++)
		{
			float num2 = (float)i * 0.01f;
			for (float num3 = num; num3 < car.curveMaxRpm; num3 += 10f)
			{
				float num4 = EngineTorqueAt(num3) * num2;
				float num5 = EngineFrictionTorqueAt(num3);
				if (num5 > num4)
				{
					array[i].x = num2;
					array[i].y = num3;
					num = num3;
					break;
				}
			}
		}
		engineRpmForThrottle = new ApproximatedCurve(array);
	}

	private float sin(float t)
	{
		return Mathf.Sin(t);
	}

	private float saw(float t)
	{
		return -1f + 2f * (Mathf.Repeat(t, (float)Math.PI) / (float)Math.PI);
	}

	private float triangle(float t)
	{
		return -1f + 2f * Mathf.Abs(saw(t));
	}

	private float square(float t)
	{
		return Mathf.Sign(saw(t));
	}

	private float EngineFrictionTorqueAt(float rpm)
	{
		return car.engineFrictionCoefficient * (rpm / car.curveMaxRpm) * car.curveMaxTorque;
	}

	private float EngineTorqueAt(float rpm)
	{
		return engineOutputTorqueCurve.Evaluate(rpm);
	}

	private float EngineRPMForThrottle(float throttle)
	{
		return engineRpmForThrottle.Evaluate(throttle);
	}

	private void CheckLimiter()
	{
		if (car.limiterRpm < 0f)
		{
			limiterAmount = 0f;
		}
		else
		{
			limiterAmount = Mathf.InverseLerp(car.limiterRpm, car.limiterRpm + car.limiterWidth, engineRpm);
		}
	}

	private void FixedUpdate()
	{
		if (car == null || engineSound == null)
		{
			return;
		}
		if (!autoThrottle)
		{
			float num = (float)Screen.height / 20f;
			if (Input.GetMouseButton(0))
			{
				throttle = Mathf.Clamp01((Input.mousePosition.y - 4f * num) / (15f * num));
			}
			else
			{
				throttle = 0f;
			}
		}
		float num2 = throttle;
		if (autoThrottle)
		{
			float num3 = 0.6f + sin(Time.fixedTime * 1.2f) * 0.4f;
			float num4 = 2f + sin(Time.fixedTime * 0.7f) * 0.5f;
			float num5 = 0.5f + square(Time.fixedTime + num4 * 3f) * 0.5f;
			num2 = num5 * num3;
			num2 = Mathf.Lerp(currThrottle, num2, 0.1f);
		}
		float num6 = 0f;
		CheckLimiter();
		if (engineRpm < engineIdleRpm * 1.5f && num2 < car.idleThrottle)
		{
			num2 = ((!(engineRpm < engineIdleRpm)) ? Mathf.Max(num2, car.idleThrottle * (1f - Mathf.InverseLerp(engineIdleRpm, engineIdleRpm * 1.5f, engineRpm))) : car.idleThrottle);
		}
		if (engineRpm < engineIdleRpm)
		{
			float num7 = 1f - Mathf.InverseLerp(car.stallRpm, engineIdleRpm, engineRpm);
			num6 = (1f - num2) * num7 * car.antiStallAssist;
			num2 += num6;
		}
		float t = 0.3f;
		currThrottle = Mathf.Lerp(currThrottle, num2, t);
		currThrottle *= 1f - limiterAmount;
		if (!car.canStall && engineRpm < car.stallRpm)
		{
			engineRpm = car.stallRpm;
		}
		float num8 = EngineTorqueAt(engineRpm);
		float num9 = 0f;
		float num10 = 0f;
		num10 = EngineFrictionTorqueAt(engineRpm);
		num9 = currThrottle * num8;
		engineRpm += (num9 - num10) * (10f - 10f * engineInertia * engineInertia) * Time.fixedDeltaTime * 60f;
		if (engineRpm < 0f)
		{
			engineRpm = 0f;
		}
		float num11 = 0f;
		num11 = ((!(num9 > num10)) ? (0f - (1f - Mathf.InverseLerp(0f, num10, num9))) : Mathf.InverseLerp(num10, num8, num9));
		float num12 = Mathf.InverseLerp(engineMaxRpm * 0.5f, engineMaxRpm, engineRpm);
		num11 += currThrottle * 0.5f * num12 * num12;
		num11 = Mathf.Clamp(num11, -1f, 1f);
		engineLoad = Mathf.Lerp(engineLoad, num11, 0.8f);
		if ((bool)engineSound)
		{
			engineSound.UpdateSound(currThrottle, engineRpm, engineLoad);
		}
		rpmHistory.RemoveAt(0);
		rpmHistory.Add(new Vector2(historyCount, engineRpm));
		loadHistory.RemoveAt(0);
		loadHistory.Add(new Vector2(historyCount, engineLoad));
		historyCount++;
	}

	private void OnRenderObject()
	{
		if (Application.isPlaying)
		{
			GLDrawGizmos();
		}
	}

	private void GLDrawGizmos()
	{
		float num = (float)Screen.height / 20f;
		GLDraw.Begin();
		GL.LoadPixelMatrix();
		engineSound.Draw(drawLayer, new Vector3(2f * num, 8f * num, 0f), Vector3.right * num * 10f, Vector3.up * num * 10f);
		GLDraw.End();
		if (rpmHistory == null)
		{
			InitHistory();
		}
		float limiterRpm = engineMaxRpm;
		if (car != null && car.limiterRpm > 0f)
		{
			limiterRpm = car.limiterRpm;
		}
		limiterRpm = Mathf.Ceil(limiterRpm / 1000f) * 1000f;
		Vector2[] array = new Vector2[engineOutputTorqueCurve.keys.Length];
		engineOutputTorqueCurve.keys.CopyTo(array, 0);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].y *= currThrottle;
		}
		GLDraw.Begin();
		GL.LoadPixelMatrix();
		GLDraw.VerticalMeter(new Vector3((float)Screen.width - 3f * num, 4f * num, 0f), Vector3.right * 2f * num, Vector3.up * 15f * num, 0f, 1f, 0.2f, 0f, true, new Color(0.125f, 0.125f, 0.125f, 1f), new Color(1f, 1f, 1f, 0.3f), new Color(0.125f, 1f, 0.25f, 0.75f), currThrottle);
		Vector3 vector = new Vector3((float)Screen.width - 10f * num, 11f * num, 0f);
		Vector3 vector2 = 6f * num * Vector3.right;
		Vector3 vector3 = 3f * num * Vector3.up;
		GLDraw.Graph(minLimits: new Vector2(historyCount - 100, -1f), maxLimits: new Vector2(historyCount - 1, 1f), position: vector, right: vector2, up: vector3, ruleSteps: Vector2.zero, labelMultipliers: Vector2.up, background: new Color(0.125f, 0.125f, 0.125f, 1f), rule: new Color(1f, 1f, 1f, 0.3f), type: GLDraw.GraphType.Line, graph: new Color(1f, 0.4f, 0.2f, 1f), values: loadHistory.ToArray());
		vector += 4f * num * Vector3.up;
		Vector2 zero = Vector2.zero;
		Vector2 maxLimits = new Vector2(car.curveMaxRpm, car.curveMaxTorque * 1.05f);
		GLDraw.Graph(vector, vector2, vector3, zero, maxLimits, new Vector2(0f, 50f), Vector2.one, new Color(0f, 0f, 0f, 0.2f), new Color(1f, 1f, 1f, 0.2f), GLDraw.GraphType.Line, new Color(0.1f, 1f, 0.2f, 1f), engineOutputTorqueCurve.keys);
		GLDraw.GraphValues(vector, vector2, vector3, zero, maxLimits, GLDraw.GraphType.Area, new Color(0.1f, 1f, 0.2f, 0.2f), array);
		GLDraw.Line(vector + vector2 * (engineRpm / car.curveMaxRpm), vector + vector2 * (engineRpm / car.curveMaxRpm) + vector3 / 1.05f * currThrottle * EngineTorqueAt(engineRpm) / car.curveMaxTorque, new Color(0.1f, 1f, 0.2f, 1f));
		GLDraw.RadialMeter(new Vector3((float)Screen.width - 7f * num, 7f * num, 0f), Vector3.right * 3f * num, Vector3.up * 3f * num, 0f, limiterRpm, 1000f, 0.001f, new Color(0.125f, 0.125f, 0.125f, 1f), new Color(1f, 1f, 1f, 0.3f), new Color(1f, 0.25f, 0.125f, 0.75f), engineRpm);
		GLDraw.String(new Vector3((float)Screen.width - 7f * num, 5f * num, 0f), Vector3.right, Vector3.up, 0.5f * num, 4, true, 5, new Color(1f, 1f, 1f, 1f), engineRpm.ToString("0000"));
		GLDraw.End();
	}

	private void OnGUI()
	{
		if (!(car == null) && !(engineSound == null))
		{
			float num = (float)Screen.height / 20f;
			GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
			gUIStyle.fontSize = (int)num;
			GUIStyle gUIStyle2 = new GUIStyle(GUI.skin.button);
			gUIStyle2.fontSize = (int)num;
			float num2 = (float)Screen.height - num * 3f;
			float num3 = num;
			float num4 = num2;
			GUI.Label(new Rect(num3 + 3f * num, num4, 13f * num, 2f * num), engineSound.name, gUIStyle);
			if (GUI.Button(new Rect(num, num4, 2f * num, 2f * num), "<", gUIStyle2))
			{
				index--;
				ChangeEngine();
			}
			if (GUI.Button(new Rect(num3 + 16f * num, num4, 2f * num, 2f * num), ">", gUIStyle2))
			{
				index++;
				ChangeEngine();
			}
			num4 -= 3f * num;
			SoundField component = engineSound.GetComponent<SoundField>();
			drawLayer = Mathf.Clamp(drawLayer, 0, component.layers.Length - 1);
			string text = component.layers[drawLayer].name;
			if ((bool)component && GUI.Button(new Rect(num3, num4, 2f * num, 2f * num), "-", gUIStyle2))
			{
				drawLayer--;
				drawLayer = Mathf.Clamp(drawLayer, 0, component.layers.Length - 1);
			}
			num3 += 3f * num;
			if ((bool)component)
			{
				GUI.Label(new Rect(num3, num4, 13f * num, 2f * num), text, gUIStyle);
			}
			num3 += 13f * num;
			if ((bool)component && GUI.Button(new Rect(num3, num4, 2f * num, 2f * num), "+", gUIStyle2))
			{
				drawLayer++;
				drawLayer = Mathf.Clamp(drawLayer, 0, component.layers.Length - 1);
			}
			num3 = (float)Screen.width - 3f * num;
			num4 = (float)Screen.height - 3f * num;
			if (GUI.Button(new Rect(num3, num4, 2f * num, 2f * num), (!autoThrottle) ? string.Empty : "X", gUIStyle2))
			{
				autoThrottle = !autoThrottle;
			}
			num3 = (float)Screen.width - 10f * num;
			num4 = (float)Screen.height - 3f * num;
			GUIStyle gUIStyle3 = new GUIStyle(GUI.skin.horizontalSlider);
			gUIStyle3.fixedHeight = num * 2f;
			GUIStyle gUIStyle4 = new GUIStyle(GUI.skin.verticalSliderThumb);
			gUIStyle4.fixedWidth = num * 2f;
			gUIStyle4.fixedHeight = num * 2f;
			slider = GUI.HorizontalSlider(new Rect(num3, num4, 6f * num, num * 2f), slider, 0f, 1f, gUIStyle3, gUIStyle4);
			float num5 = 1f - slider;
			num5 = 1f - num5 * num5;
			engineInertia = Mathf.Lerp(car.engineInertia, 0.999f, num5);
		}
	}
}
