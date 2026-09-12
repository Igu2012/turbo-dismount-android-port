#pragma warning disable 0618,0619
using System.Collections.Generic;
using Dismount;
using Dismount.Vehicular;
using UnityEngine;

namespace CarEngineTest
{
	public class CarEngineTestMain : MonoBehaviour
	{
		public List<GameObject> tracks = new List<GameObject>();

		private GameObject vehicle;

		private GameObject character;

		public Transform cameraTarget;

		private float dragTimeStart;

		private List<string> dragMessages = new List<string>();

		private float throttle;

		private float countdownTime = -1f;

		private PersistentState persistentState;

		private bool reached100;

		private bool reached400;

		private bool reached1000;

		private void Start()
		{
			persistentState = GameObject.Find("PersistentState").GetComponent<PersistentState>();
			if (persistentState.trackIndex >= tracks.Count)
			{
				persistentState.trackIndex = 0;
			}
			for (int i = 0; i < tracks.Count; i++)
			{
				if (i == persistentState.trackIndex)
				{
					tracks[i].SetActive(true);
				}
				else
				{
					tracks[i].SetActive(false);
				}
			}
			List<GameObject> list = new List<GameObject>();
			Vehicle[] array = Object.FindObjectsOfType(typeof(Vehicle)) as Vehicle[];
			Vehicle[] array2 = array;
			foreach (Vehicle vehicle in array2)
			{
				list.Add(vehicle.gameObject);
			}
			Vehicle[] array3 = Object.FindObjectsOfType(typeof(Vehicle)) as Vehicle[];
			Vehicle[] array4 = array3;
			foreach (Vehicle vehicle2 in array4)
			{
				list.Add(vehicle2.gameObject);
			}
			if (persistentState.vehicleIndex >= list.Count)
			{
				persistentState.vehicleIndex = 0;
			}
			for (int l = 0; l < list.Count; l++)
			{
				if (l == persistentState.vehicleIndex)
				{
					this.vehicle = list[l];
				}
				else
				{
					list[l].SetActive(false);
				}
			}
			MrDismount[] array5 = Object.FindObjectsOfType(typeof(MrDismount)) as MrDismount[];
			MrDismount[] array6 = array5;
			foreach (MrDismount mrDismount in array6)
			{
				if (mrDismount.transform.parent.name == this.vehicle.name)
				{
					character = mrDismount.gameObject;
				}
				else
				{
					mrDismount.gameObject.SetActive(false);
				}
			}
			if ((bool)character)
			{
				character.transform.parent = this.vehicle.transform;
			}
			if (cameraTarget == null || !cameraTarget.gameObject.activeInHierarchy)
			{
				cameraTarget = this.vehicle.transform;
			}
			TVCamera[] array7 = Object.FindObjectsOfType(typeof(TVCamera)) as TVCamera[];
			TVCamera[] array8 = array7;
			foreach (TVCamera tVCamera in array8)
			{
				tVCamera.SetTarget(cameraTarget);
			}
			array7[0].gameObject.SendMessage("ChangeCamera", cameraTarget, SendMessageOptions.DontRequireReceiver);
			GameObject gameObject = GameObject.FindGameObjectWithTag("SteerPath");
			if ((bool)gameObject)
			{
				this.vehicle.SendMessage("set_steerSpline", gameObject.GetComponent<Spline>(), SendMessageOptions.DontRequireReceiver);
			}
			this.vehicle.SendMessage("WakeUp", SendMessageOptions.DontRequireReceiver);
			this.vehicle.SendMessage("StartEngine", SendMessageOptions.DontRequireReceiver);
		}

		private void Update()
		{
			if (countdownTime >= 0f && countdownTime < 1f)
			{
				countdownTime += TimeManager.deltaTime;
				if (countdownTime >= 1f)
				{
					OnDismount();
				}
			}
		}

		private void FixedUpdate()
		{
			if (!reached100 && (bool)vehicle && vehicle.GetComponent<Rigidbody>().velocity.magnitude * 3.6f >= 100f)
			{
				On0To100();
				reached100 = true;
			}
		}

		private void OnDismount()
		{
			vehicle.SendMessage("OnDismountStarted", SendMessageOptions.DontRequireReceiver);
			dragTimeStart = TimeManager.fixedTime;
			if ((bool)character)
			{
				character.transform.parent = null;
				character.SendMessage("OnDismountStarted", SendMessageOptions.DontRequireReceiver);
			}
		}

		private void StartCountdown()
		{
			countdownTime = 0f;
		}

		private void OnTimerStart()
		{
		}

		private void On0To100()
		{
			if (dragTimeStart > 0f)
			{
				dragMessages.Add("0-100km/h: " + Utils.NumberString(TimeManager.fixedTime - dragTimeStart, 3) + "s");
			}
		}

		private void OnTimer400()
		{
			if (!reached400)
			{
				reached400 = true;
				float number = TimeManager.fixedTime - dragTimeStart;
				float number2 = vehicle.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
				dragMessages.Add("400m: " + Utils.NumberString(number, 3) + "s @ " + Utils.NumberString(number2, 1) + "km/h");
			}
		}

		private void OnTimer1000()
		{
			if (!reached1000)
			{
				reached1000 = true;
				float number = TimeManager.fixedTime - dragTimeStart;
				float number2 = vehicle.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
				dragMessages.Add("1000m: " + Utils.NumberString(number, 3) + "s @ " + Utils.NumberString(number2, 1) + "km/h");
			}
		}

		private void OnGUI()
		{
			Rect position = new Rect((float)(Screen.width - 200) * 0.5f, Screen.height - 90, 200f, 25f);
			Rect position2 = new Rect((float)(Screen.width - 100) * 0.5f, Screen.height - 50, 100f, 40f);
			persistentState.showDebug = GUI.Toggle(new Rect(10f, 10f, 200f, 25f), persistentState.showDebug, "Show vehicle debug");
			if ((bool)vehicle)
			{
				if ((bool)vehicle.GetComponent<Vehicle>())
				{
					vehicle.GetComponent<Vehicle>().showDebug = persistentState.showDebug;
				}
				else if ((bool)vehicle.GetComponent<Vehicle>())
				{
					vehicle.GetComponent<Vehicle>().showDebug = persistentState.showDebug;
				}
				GUI.Label(new Rect((float)Screen.width * 0.5f - 50f, 10f, 200f, 40f), vehicle.name);
			}
			if (GUI.Button(new Rect(Screen.width - 110, 10f, 100f, 40f), "Reset"))
			{
				Application.LoadLevel(Application.loadedLevel);
			}
			if (GUI.Button(new Rect(Screen.width - 110, 60f, 100f, 40f), "Track"))
			{
				persistentState.trackIndex++;
				Application.LoadLevel(Application.loadedLevel);
			}
			if (GUI.Button(new Rect(Screen.width - 110, 110f, 100f, 40f), "Vehicle"))
			{
				persistentState.vehicleIndex++;
				Application.LoadLevel(Application.loadedLevel);
			}
			persistentState.dismountControls = GUI.Toggle(new Rect(Screen.width - 110, 160f, 100f, 25f), persistentState.dismountControls, "Auto throttle");
			float num = 0f;
			if (persistentState.dismountControls)
			{
				float num2 = 1f - Mathf.Abs(Mathf.Sin(TimeManager.time * 3f));
				if (countdownTime < 0f)
				{
					throttle = num2;
					throttle = GUI.HorizontalSlider(position, throttle, 0f, 1f);
				}
				else
				{
					GUI.HorizontalSlider(position, throttle, 0f, 1f);
				}
			}
			else
			{
				throttle = GUI.HorizontalSlider(position, throttle, -1f, 1f);
				if (throttle < 0f)
				{
					num = 0f - throttle;
				}
			}
			vehicle.SendMessage("set_inputThrottle", Mathf.Clamp01(throttle), SendMessageOptions.DontRequireReceiver);
			vehicle.SendMessage("set_inputBrake", num, SendMessageOptions.DontRequireReceiver);
			float num3 = Screen.height - 80;
			foreach (string dragMessage in dragMessages)
			{
				GUI.Label(new Rect(10f, num3, 200f, 40f), dragMessage);
				num3 += 25f;
			}
			if (countdownTime < 0f && GUI.Button(position2, "Dismount"))
			{
				StartCountdown();
			}
			else if (countdownTime >= 0f && countdownTime < 1f)
			{
				int num4 = 2 - (int)(countdownTime / 0.5f);
				GUI.Button(position2, string.Empty + num4);
			}
		}
	}
}
