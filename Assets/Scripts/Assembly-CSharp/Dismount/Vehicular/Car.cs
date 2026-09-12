#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using Dismount.GameStates;
using UnityEngine;
using reLive;

namespace Dismount.Vehicular
{
	[RequireComponent(typeof(Vehicle))]
	public class Car : MonoBehaviour
	{
		private const float RpmToRadps = (float)Math.PI / 30f;

		private const float RadpsToRpm = 30f / (float)Math.PI;

		private const float turboBoostDuration = 2.5f;

		private const float turboBoostTorqueMultiplier = 1.5f;

		private const float turboBoostMaxRpmMultiplier = 1.2f;

		private const float brakePadDuration = 1.25f;

		public bool nutsAndBolts = true;

		public Vector3 centerOfMass = Vector3.zero;

		public Vector3 engineOrientation = Vector3.forward;

		public AnimationCurve engineOutputTorque = AnimationCurve.Linear(0f, 0.5f, 1f, 0.3f);

		public float curveMaxTorque = 320f;

		public float curveMaxRpm = 6500f;

		public float engineFrictionCoefficient = 0.5f;

		public float idleThrottle = 0.2f;

		public float stallRpm = 400f;

		public float limiterRpm = -1f;

		public float limiterWidth = 10f;

		[Range(0f, 1f)]
		public float antiStallAssist;

		[Range(0.5f, 0.995f)]
		public float engineInertia = 0.9f;

		public float flywheelMass = 20f;

		public float maxCrashGs = 100f;

		public float[] gearRatios;

		public float finalDrive = 0.9f;

		public float minClutchPopDuration = 0.4f;

		public float maxClutchPopDuration = 2f;

		public float gearChangeDuration = 0.2f;

		public bool automaticTransmissionStyle;

		[Range(0f, 1f)]
		public float engineBraking = 1f;

		public bool canStall = true;

		public float fullBrakeTorque = 500f;

		public float constantBrakeTorque = 10f;

		public float turbopadMagicForce = 800f;

		public float brakepadMagicForce = -800f;

		public bool requireRocketsForTurboBoost;

		public float velocitySteerAhead = 0.5f;

		public float steerCorrectAmount = 1f;

		public float steerCorrectSpeed = 1f;

		public float steerLockLimiterSpeed = 30f;

		public float steerLockLimiter = 1f;

		public float counterSteerSpeed = -1f;

		public bool tiltSteering;

		[Range(0f, 1f)]
		public float tiltSteerInputLinearity = 0.7f;

		public float tiltPrediction = 0.1f;

		public float tiltSmoothingRange = 20f;

		public float tiltSpeed1 = 5f;

		public float tiltSpeed2 = 30f;

		public float tiltAngle1 = 30f;

		public float tiltAngle2 = 40f;

		public float tiltForce1 = 300f;

		public float tiltForce2 = 3000f;

		public float tiltRiderForce1 = 0.5f;

		public float tiltRiderForce2 = 0.3f;

		public float tiltConstantForce = 200f;

		public float antiWheelieThrottleDecrease = -1f;

		public float antiWheelieMinAngle;

		public float antiWheelieMaxAngle = 90f;

		public float partBreakforcePropagationMultiplier = 0.9f;

		public bool detachedPartsCanDetachOtherParts = true;

		public bool forceTwowayJointConnection;

		private Wheel[] allWheels;

		private Wheel[] steerWheels;

		private Wheel[] driveWheels;

		private Wheel[] brakeWheels;

		private Wheel[] gyroWheels;

		private float totalWheelDrive;

		private float totalWheelBrake;

		private float controlThrottle;

		private float controlClutch;

		private float controlBrake;

		private float controlSteering;

		private float targetThrottle;

		private float targetClutch;

		private float targetBrake;

		private float targetSteering;

		private float currThrottle;

		private float currClutch;

		private float currBrake;

		private float currSteering;

		private float engineRpm = 1000f;

		private float gearboxRpm;

		private bool clutchPopped;

		private float clutchPopTime = -1f;

		private float clutchPopDuration = 2f;

		private bool isRunning;

		private bool isStalled;

		private bool isBroken;

		private int gear;

		private int prevGear;

		private float gearChangeTime = -1f;

		private float lastGearChangeTime;

		private float prevSteer;

		private float fuelLeft = 10f;

		private float limiterAmount;

		private Dictionary<string, object> debug = new Dictionary<string, object>();

		private float runningTime;

		private ApproximatedCurve engineOutputTorqueCurve;

		private ApproximatedCurve engineRpmForThrottle;

		private Vector3 prevVelocity = Vector3.zero;

		private float prevVelocityMagnitude;

		private float prevSteerSplineParam = 0.5f;

		private float steerSplineParamRadius = 0.5f;

		private float steerSplineParamResolution = 0.01f;

		private float antiWheelieMinDot;

		private float antiWheelieMaxDot = 1f;

		private Dismount.GameStates.Dismount _gameLogic;

		private MrDismount _driver;

		private VehicleAudio vehicleAudio;

		private float maxTorque;

		private float maxTorqueAt;

		private float maxPower;

		private float maxPowerAt;

		private float engineIdleRpm;

		private float engineMaxRpm;

		private float engineLoad;

		private float turboBoostTime = -1f;

		[HideInInspector]
		public bool turboBoost;

		private float brakePadTime = -1f;

		private bool brakePad;

		private Vector3 steerTarget;

		private Vehicle vehicle;

		private Transform sceneDynamic;

		private static GameObject decal;

		private List<RocketEngineFx> rocketEngineFxs = new List<RocketEngineFx>();

		private int liveRocketEngineCount;

		public GameObject[] rocketEngines;

		private CarTelemetry telemetry;

		private Zither zither;

		private Dictionary<string, GameObject> clonedParts = new Dictionary<string, GameObject>();

		private Dictionary<string, GameObject> originalParts = new Dictionary<string, GameObject>();

		private Dictionary<string, bool> connectedToBody = new Dictionary<string, bool>();

		private Dictionary<string, Rigidbody> rootRigidbodyLookup = new Dictionary<string, Rigidbody>();

		private Dictionary<string, List<string>> detachLookup = new Dictionary<string, List<string>>();

		private Dictionary<string, GameObject> clonedWings = new Dictionary<string, GameObject>();

		private float prevTiltAngle;

		private float prevSurfaceLoad;

		private float prevTurboCheck = -1f;

		private List<Transform> visitedParts = new List<Transform>();

		private List<Transform> processList = new List<Transform>();

		public float vehiclePartBreakLimit = 10f;

		[HideInInspector]
		public float previousHitIconTime = -1f;

		private float impactScoreWindow = -1f;

		private float lastSoundEffectTime = -1f;

		private float previousNutsAndBoltsTime = -1f;

		private static float previousCraterTime = -1f;

		public Dismount.GameStates.Dismount gameLogic
		{
			get
			{
				return _gameLogic;
			}
			set
			{
				_gameLogic = value;
			}
		}

		public MrDismount driver
		{
			get
			{
				return _driver;
			}
			set
			{
				_driver = value;
			}
		}

		private bool IsGrayScale(Color color)
		{
			float num = (color.r + color.g + color.b) / 3f;
			if (Mathf.Abs(color.r - color.g) < 0.01f && Mathf.Abs(color.r - color.b) < 0.01f && Mathf.Abs(color.g - color.b) < 0.01f)
			{
				return true;
			}
			return false;
		}

		private bool IsWhite(Color color)
		{
			if (color.grayscale > 0.99f)
			{
				return true;
			}
			return false;
		}

		public void CustomizeColor(Color main, Color secondary, Color rim)
		{
			MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
			MeshFilter[] array = componentsInChildren;
			foreach (MeshFilter meshFilter in array)
			{
				if (meshFilter.name.Contains("wheel"))
				{
					continue;
				}
				Mesh mesh = meshFilter.mesh;
				meshFilter.GetComponent<Renderer>().material.SetColor("_RimColor", rim);
				Color[] colors = mesh.colors;
				Color[] array2 = new Color[colors.Length];
				for (int j = 0; j < array2.Length; j++)
				{
					if (IsGrayScale(colors[j]))
					{
						array2[j] = secondary * colors[j].grayscale;
					}
					else
					{
						array2[j] = main * colors[j].grayscale;
					}
				}
				mesh.colors = array2;
			}
		}

		public void WakeUp()
		{
			CloneParts();
			controlBrake = 1f;
			base.GetComponent<Rigidbody>().isKinematic = false;
			DynamicPhysicsPart[] componentsInChildren = base.transform.GetComponentsInChildren<DynamicPhysicsPart>();
			foreach (DynamicPhysicsPart dynamicPhysicsPart in componentsInChildren)
			{
				dynamicPhysicsPart.WakeUp();
			}
			vehicle.lastGroundContactTime = Time.fixedTime;
		}

		private Transform GetWingsRoot()
		{
			Transform transform = Utils.FindChildRecursive("Wings", base.transform);
			if (!transform)
			{
				GameObject gameObject = new GameObject("Wings");
				transform = gameObject.transform;
				transform.parent = base.transform;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
			}
			return transform;
		}

		private void CloneWing(string wingName, Wing originalWing)
		{
			Transform wingsRoot = GetWingsRoot();
			GameObject gameObject = new GameObject(wingName + " clone");
			Wing wing = gameObject.AddComponent<Wing>();
			wing.connectedBody = base.GetComponent<Rigidbody>();
			wing.lift = originalWing.lift;
			wing.frontalDrag = originalWing.frontalDrag;
			wing.surfaceDrag = originalWing.surfaceDrag;
			Transform transform = wing.transform;
			transform.parent = wingsRoot;
			transform.position = originalWing.transform.position;
			transform.rotation = originalWing.transform.rotation;
			clonedWings.Add(wingName, gameObject);
		}

		private void CloneParts()
		{
			List<Transform> children = new List<Transform>();
			Utils.FindChildrenRecursive("Parts", base.transform, ref children);
			foreach (Transform item2 in children)
			{
				if ((bool)item2.parent && (bool)item2.parent.GetComponent<Rigidbody>() && item2.parent.GetComponent<Rigidbody>() != base.GetComponent<Rigidbody>())
				{
					CollisionEventDispatcher collisionEventDispatcher = item2.parent.gameObject.AddComponent<CollisionEventDispatcher>();
					collisionEventDispatcher.target = base.gameObject;
				}
				foreach (Transform item3 in item2)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(item3.gameObject) as GameObject;
					gameObject.AddComponent<SmokeOnImpact>();
					clonedParts.Add(item3.name, gameObject);
					originalParts.Add(item3.name, item3.gameObject);
					gameObject.transform.parent = GameObject.Find("SceneDynamic").transform;
					gameObject.AddComponent<RecordMeInReplay>().Warmup();
					if (!detachedPartsCanDetachOtherParts)
					{
						gameObject.tag = "DetachedPart";
					}
					Wing component = item3.GetComponent<Wing>();
					if ((bool)component)
					{
						CloneWing(item3.name, component);
					}
					Joint[] components = gameObject.GetComponents<Joint>();
					Joint[] array = components;
					foreach (Joint obj in array)
					{
						UnityEngine.Object.Destroy(obj);
					}
					DynamicPhysicsPart component2 = gameObject.GetComponent<DynamicPhysicsPart>();
					if ((bool)component2)
					{
						UnityEngine.Object.Destroy(component2);
					}
					gameObject.SetActive(false);
					Joint[] components2 = item3.GetComponents<Joint>();
					List<string> list = new List<string>();
					Joint[] array2 = components2;
					foreach (Joint joint in array2)
					{
						if (joint.connectedBody.gameObject != item2.parent.gameObject)
						{
							if (!list.Contains(joint.connectedBody.gameObject.name))
							{
								list.Add(joint.connectedBody.gameObject.name);
							}
						}
						else
						{
							connectedToBody[item3.name] = true;
						}
						UnityEngine.Object.Destroy(joint);
					}
					if (!item3.GetComponent<DynamicPhysicsPart>())
					{
						Debug.LogWarning("DynamicPhysicsPart was missing: " + item3.name);
						item3.gameObject.AddComponent<DynamicPhysicsPart>();
					}
					UnityEngine.Object.Destroy(item3.GetComponent<Rigidbody>());
					rootRigidbodyLookup[item3.name] = item2.parent.GetComponent<Rigidbody>();
					if ((bool)component)
					{
						UnityEngine.Object.Destroy(component);
					}
					detachLookup.Add(gameObject.name, list);
				}
			}
			base.GetComponent<Rigidbody>().centerOfMass = centerOfMass;
			if (!forceTwowayJointConnection)
			{
				return;
			}
			foreach (KeyValuePair<string, List<string>> item4 in detachLookup)
			{
				List<string> value = item4.Value;
				foreach (string item5 in value)
				{
					string key = item5 + "(Clone)";
					if (detachLookup.ContainsKey(key))
					{
						string item = item4.Key.Remove(item4.Key.IndexOf("(Clone)"));
						if (!detachLookup[key].Contains(item))
						{
							detachLookup[key].Add(item);
						}
					}
					else
					{
						Debug.Log(item5 + " not found!");
					}
				}
			}
			foreach (KeyValuePair<string, List<string>> item6 in detachLookup)
			{
				if (item6.Value.Count == 0)
				{
					string key2 = item6.Key.Remove(item6.Key.IndexOf("(Clone)"));
					if (!connectedToBody.ContainsKey(key2))
					{
						Debug.LogError(item6.Key + " is floating!");
					}
				}
			}
		}

		public void ActivateRockets()
		{
			if (!isRunning)
			{
				return;
			}
			liveRocketEngineCount = 0;
			for (int i = 0; i < rocketEngineFxs.Count; i++)
			{
				if (rocketEngineFxs[i] != null)
				{
					rocketEngineFxs[i].Activate();
					liveRocketEngineCount++;
				}
			}
			if ((bool)vehicleAudio && liveRocketEngineCount > 0)
			{
				vehicleAudio.StartBoost();
			}
		}

		private void DeactivateRockets()
		{
			liveRocketEngineCount = 0;
			for (int i = 0; i < rocketEngineFxs.Count; i++)
			{
				if (rocketEngineFxs[i] != null)
				{
					rocketEngineFxs[i].Deactivate();
					liveRocketEngineCount++;
				}
			}
			if ((bool)vehicleAudio)
			{
				vehicleAudio.KillBoost();
			}
		}

		private void Awake()
		{
			sceneDynamic = GameObject.Find("SceneDynamic").transform;
			if ((bool)vehicle)
			{
				return;
			}
			if (!decal)
			{
				decal = Resources.Load("Decal") as GameObject;
			}
			for (int i = 0; i < rocketEngines.Length; i++)
			{
				if (rocketEngines[i].transform.childCount == 0)
				{
					RocketEngineFx component = (UnityEngine.Object.Instantiate(Resources.Load("RocketEngineFX")) as GameObject).GetComponent<RocketEngineFx>();
					component.transform.parent = rocketEngines[i].transform;
					component.transform.localScale = Vector3.Scale(component.transform.localScale, rocketEngines[i].transform.localScale);
					component.transform.localPosition = new Vector3(0f, 0f, 0f);
					component.transform.localRotation = Quaternion.identity;
				}
			}
			for (int j = 0; j < rocketEngines.Length; j++)
			{
				RocketEngineFx[] componentsInChildren = rocketEngines[j].GetComponentsInChildren<RocketEngineFx>();
				for (int k = 0; k < componentsInChildren.Length; k++)
				{
					rocketEngineFxs.Add(componentsInChildren[k]);
				}
			}
			DeactivateRockets();
			vehicle = GetComponent<Vehicle>();
			List<Wheel> list = new List<Wheel>();
			List<Wheel> list2 = new List<Wheel>();
			List<Wheel> list3 = new List<Wheel>();
			List<Wheel> list4 = new List<Wheel>();
			allWheels = base.gameObject.GetComponentsInChildren<Wheel>();
			for (int l = 0; l < allWheels.Length; l++)
			{
				Wheel wheel = allWheels[l];
				if (wheel.driveAmount > 0f)
				{
					list.Add(wheel);
					totalWheelDrive += wheel.driveAmount;
				}
				if (wheel.steerLock != 0f)
				{
					list2.Add(wheel);
				}
				if (wheel.brakeAmount > 0f)
				{
					list3.Add(wheel);
					totalWheelBrake += wheel.brakeAmount;
				}
				if (wheel.gyroAmount > 0f)
				{
					list4.Add(wheel);
				}
			}
			driveWheels = list.ToArray();
			steerWheels = list2.ToArray();
			brakeWheels = list3.ToArray();
			gyroWheels = list4.ToArray();
			steerTarget = base.transform.position + base.transform.forward * 1000f;
			base.GetComponent<Rigidbody>().centerOfMass = centerOfMass;
			if (gearRatios.Length < 0)
			{
				Debug.LogError("No gearRatios found, must have at least one gear");
			}
			base.GetComponent<Rigidbody>().isKinematic = true;
			base.GetComponent<Rigidbody>().solverIterations = 6;
			fuelLeft = vehicle.fuel;
			engineOutputTorqueCurve = new ApproximatedCurve(engineOutputTorque, curveMaxRpm, curveMaxTorque);
			CalculateEngineRpmForThrottle();
			engineIdleRpm = Mathf.Max(stallRpm, EngineRPMForThrottle(idleThrottle));
			engineMaxRpm = Mathf.Min(curveMaxRpm, EngineRPMForThrottle(1f));
			prevVelocityMagnitude = 0f;
			prevSteerSplineParam = 0.5f;
			steerSplineParamRadius = 0.5f;
			steerSplineParamResolution = 0.01f;
			vehicleAudio = base.transform.Find("Audio").GetComponent<VehicleAudio>();
			if (stallRpm > 0.8f * engineIdleRpm)
			{
				stallRpm = 0.8f * engineIdleRpm;
			}
			if ((bool)vehicleAudio.engineSound)
			{
				vehicleAudio.engineSound.maxVolumeRpm = engineMaxRpm * 0.8f;
				vehicleAudio.engineSound.muteEndRpm = stallRpm;
				vehicleAudio.engineSound.muteStartRpm = stallRpm * 0.5f;
			}
			antiWheelieThrottleDecrease = Mathf.Clamp01(antiWheelieThrottleDecrease);
			antiWheelieMinAngle = Mathf.Clamp(antiWheelieMinAngle, 0f, 90f);
			antiWheelieMaxAngle = Mathf.Clamp(antiWheelieMaxAngle, antiWheelieMinAngle, 90f);
			antiWheelieMinDot = Mathf.Sin(antiWheelieMinAngle / 180f * (float)Math.PI);
			antiWheelieMaxDot = Mathf.Sin(antiWheelieMaxAngle / 180f * (float)Math.PI);
			zither = GetComponent<Zither>();
		}

		private void Start()
		{
			_gameLogic = UnityEngine.Object.FindObjectOfType<Dismount.GameStates.Dismount>();
			ReportEnginePeaks();
		}

		private void ReportEnginePeaks()
		{
			for (float num = 0f; num < curveMaxRpm; num++)
			{
				float num2 = engineOutputTorqueCurve.Evaluate(num);
				float num3 = num2 * num * ((float)Math.PI / 30f);
				if (num2 > maxTorque)
				{
					maxTorque = num2;
					maxTorqueAt = num;
				}
				if (num3 > maxPower)
				{
					maxPower = num3;
					maxPowerAt = num;
				}
			}
		}

		private void CalculateEngineRpmForThrottle()
		{
			Vector2[] array = new Vector2[101];
			float num = 0f;
			for (int i = 0; i < 101; i++)
			{
				float num2 = (float)i * 0.01f;
				for (float num3 = num; num3 < curveMaxRpm; num3 += 10f)
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

		private void CheckClutchPopped()
		{
			if (clutchPopTime >= 0f)
			{
				clutchPopTime += Time.fixedDeltaTime;
				if (clutchPopTime > clutchPopDuration)
				{
					targetClutch = 1f;
					return;
				}
				float num = Mathf.Lerp(0f, 1f, clutchPopTime / clutchPopDuration);
				targetClutch = 0.25f + num * 0.75f;
				lastGearChangeTime = Time.fixedTime - 0.5f;
			}
		}

		private float EngineFrictionTorqueAt(float rpm)
		{
			return engineFrictionCoefficient * (rpm / curveMaxRpm) * curveMaxTorque;
		}

		private float EngineTorqueAt(float rpm)
		{
			float num = curveMaxRpm;
			float num2 = 1f;
			if (turboBoost)
			{
				num = curveMaxRpm * 1.2f;
				num2 = 1.5f;
			}
			float x = rpm / num * curveMaxRpm;
			return engineOutputTorqueCurve.Evaluate(x) * num2;
		}

		private float NetEngineTorqueAt(float rpm, float throttle = 1f)
		{
			return EngineTorqueAt(rpm) * throttle - EngineFrictionTorqueAt(rpm);
		}

		private float EnginePowerAt(float rpm)
		{
			return EngineTorqueAt(rpm) * rpm * ((float)Math.PI / 30f);
		}

		private float NetEnginePowerAt(float rpm, float throttle = 1f)
		{
			return NetEngineTorqueAt(rpm, throttle) * rpm * ((float)Math.PI / 30f);
		}

		private float EngineRPMOnGear(int aGear)
		{
			if (aGear == gear)
			{
				return engineRpm;
			}
			aGear = Mathf.Clamp(aGear, 0, gearRatios.Length - 1);
			float num = gearRatios[aGear] / gearRatios[gear];
			return engineRpm * num;
		}

		private float EngineRPMForThrottle(float throttle)
		{
			float num = 1f;
			if (turboBoost)
			{
				num = 1.2f;
			}
			return num * engineRpmForThrottle.Evaluate(throttle);
		}

		private void CheckLimiter()
		{
			if (limiterRpm < 0f || turboBoost)
			{
				limiterAmount = 0f;
			}
			else
			{
				limiterAmount = Mathf.InverseLerp(limiterRpm, limiterRpm + limiterWidth, engineRpm);
			}
		}

		private float CheckGear(float wheelContactTime, float wheelSlip)
		{
			float result = gearRatios[gear];
			float num = gearRatios[Mathf.Clamp(gear + 1, 0, gearRatios.Length - 1)] / gearRatios[gear];
			float num2 = gearChangeDuration;
			if (turboBoost)
			{
				num2 *= 0.9f;
			}
			if (gearChangeTime >= 0f)
			{
				gearChangeTime += Time.fixedDeltaTime;
				if (!(gearChangeTime > num2))
				{
					float num3 = gearChangeTime / num2;
					if (automaticTransmissionStyle)
					{
						result = Mathf.Lerp(gearRatios[prevGear], gearRatios[gear], num3);
						if (prevGear < gear)
						{
							targetThrottle *= 0.75f;
						}
						else
						{
							targetThrottle *= 0.5f;
						}
					}
					else
					{
						result = ((num3 > 0.5f) ? gearRatios[gear] : (result = gearRatios[prevGear]));
						if (prevGear < gear)
						{
							targetThrottle = 0f;
							targetClutch = 0f;
						}
						else
						{
							float num4 = ((!(num3 > 0.2f) || !(num3 < 0.65f)) ? 0f : 1f);
							targetThrottle = num4 * Mathf.Clamp(targetThrottle, 0.6f, 1f);
							targetClutch = 0f;
						}
					}
					return result;
				}
				result = gearRatios[gear];
				gearChangeTime = -1f;
				lastGearChangeTime = Time.fixedTime;
			}
			if (vehicle.overrideGear == -2)
			{
				vehicle.overrideGear = gear;
				return result;
			}
			if (vehicle.overrideGear >= 0)
			{
				vehicle.overrideGear = Mathf.Clamp(vehicle.overrideGear, 0, gearRatios.Length - 1);
				if (gear == vehicle.overrideGear)
				{
					return result;
				}
				prevGear = gear;
				gear = vehicle.overrideGear;
				gearChangeTime = 0f;
				return result;
			}
			if (!isRunning || wheelContactTime < 0.25f || targetClutch < 0.95f)
			{
				return result;
			}
			if (Time.fixedTime < lastGearChangeTime + 0.5f)
			{
				return result;
			}
			if (gear > 0 && engineRpm < engineIdleRpm)
			{
				prevGear = gear;
				gear--;
				gearChangeTime = 0f;
				return result;
			}
			float num5 = Mathf.Clamp(targetThrottle, idleThrottle, 1f);
			if (targetBrake > 0.05f)
			{
				float min = Mathf.Max(0.5f, num5);
				num5 = Mathf.Clamp(2f * targetBrake, min, 1f);
			}
			float num6 = NetEnginePowerAt(engineRpm, targetThrottle);
			float num7 = NetEngineTorqueAt(engineRpm, targetThrottle);
			float num8 = 1f;
			float num9 = 1.2f / num8;
			if (gear > 0)
			{
				float num10 = EngineRPMOnGear(gear - 1);
				if (num10 < engineMaxRpm * 0.9f && NetEnginePowerAt(num10, num5) > num6 * num9)
				{
					prevGear = gear;
					gear--;
					gearChangeTime = 0f;
					return result;
				}
			}
			if (wheelSlip > 0.35f || targetThrottle < idleThrottle)
			{
				return result;
			}
			if (Time.fixedTime < lastGearChangeTime + 0.9f)
			{
				return result;
			}
			if (gear < gearRatios.Length - 1)
			{
				float num11 = EngineRPMOnGear(gear + 1);
				if (num11 < engineIdleRpm * 1.1f)
				{
					return result;
				}
				if (NetEnginePowerAt(num11, num5) > num6 * num8)
				{
					if (NetEngineTorqueAt(num11, targetThrottle) * num < num7 * 0.85f)
					{
						return result;
					}
					prevGear = gear;
					gear++;
					gearChangeTime = 0f;
					return result;
				}
			}
			return result;
		}

		private void Stall()
		{
			if (!isStalled)
			{
				KillEngine();
				if (canStall && clutchPopTime < clutchPopDuration + 0.5f)
				{
					isStalled = true;
					_gameLogic.VehicleStalled();
				}
			}
		}

		private void Break()
		{
			BroadcastMessage("CarBroken", SendMessageOptions.DontRequireReceiver);
			KillEngine();
			isBroken = true;
		}

		private void KillEngine()
		{
			if (turboBoost)
			{
				KillTurboBoost();
			}
			isRunning = false;
			RockerForce[] componentsInChildren = GetComponentsInChildren<RockerForce>();
			if (componentsInChildren != null)
			{
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i]);
				}
			}
		}

		private void DebugInfo(string name, object obj)
		{
			if (vehicle.showDebug)
			{
				debug[name] = obj;
			}
		}

		public void TurboBoost()
		{
			if (isRunning)
			{
				turboBoost = true;
				turboBoostTime = Time.fixedTime;
				DismountGame.playerState.statistics.TurboPadActivated();
				BroadcastMessage("TurboBoostStarted", SendMessageOptions.DontRequireReceiver);
				if (zither != null)
				{
					zither.PlayBoostSound();
				}
				if (brakePad)
				{
					KillBrakePad();
				}
			}
		}

		public void BrakePad()
		{
			brakePad = true;
			brakePadTime = Time.fixedTime;
			DismountGame.playerState.statistics.BrakePadActivated();
			if (turboBoost)
			{
				KillTurboBoost();
			}
		}

		private void KillTurboBoost()
		{
			turboBoost = false;
			turboBoostTime = -1f;
			DeactivateRockets();
			BroadcastMessage("TurboBoostEnded", SendMessageOptions.DontRequireReceiver);
		}

		private void KillBrakePad()
		{
			brakePad = false;
			brakePadTime = -1f;
		}

		private void FixedUpdate()
		{
			Transform transform = base.transform;
			if (!vehicle.isAwake)
			{
				return;
			}
			if (turboBoost)
			{
				if (Time.fixedTime > turboBoostTime + 2.5f)
				{
					KillTurboBoost();
				}
				if (requireRocketsForTurboBoost && Time.fixedTime > prevTurboCheck + 0.1f)
				{
					liveRocketEngineCount = 0;
					for (int i = 0; i < rocketEngineFxs.Count; i++)
					{
						if (rocketEngineFxs[i] != null)
						{
							liveRocketEngineCount++;
						}
					}
					if (liveRocketEngineCount <= 0)
					{
						KillTurboBoost();
					}
					prevTurboCheck = Time.fixedTime;
				}
			}
			if (brakePad && Time.fixedTime > brakePadTime + 1.25f)
			{
				KillBrakePad();
			}
			base.GetComponent<Rigidbody>().WakeUp();
			Vector3 velocity = base.GetComponent<Rigidbody>().velocity;
			float magnitude = velocity.magnitude;
			if (isRunning && !isBroken)
			{
				vehicle.isMoving = magnitude > 1f;
			}
			else
			{
				vehicle.isMoving = magnitude > 3f;
			}
			float num = engineRpm;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			for (int j = 0; j < driveWheels.Length; j++)
			{
				Wheel wheel = driveWheels[j];
				num2 += wheel.rpm;
				if (wheel.contactTime > num3)
				{
					num3 = wheel.contactTime;
				}
				if (wheel.slip > num4)
				{
					num4 = wheel.slip;
				}
			}
			num2 /= (float)driveWheels.Length;
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 0f;
			Vector3 rhs = Vector3.zero;
			int num8 = 0;
			for (int k = 0; k < allWheels.Length; k++)
			{
				Wheel wheel2 = allWheels[k];
				if (wheel2.contactTime > 0f)
				{
					if (wheel2.contactTime > num7)
					{
						num7 = wheel2.contactTime;
					}
					float num9 = wheel2.slip * (wheel2.load / wheel2.maxSpringLoad) * wheel2.surfaceFriction;
					num6 += num9;
					if (num9 > num5)
					{
						num5 = num9;
					}
					rhs += wheel2.contactNormal;
					num8++;
				}
			}
			vehicleAudio.UpdateSkid(num6);
			if (num8 > 0)
			{
				rhs /= (float)num8;
			}
			else
			{
				rhs = Vector3.up;
			}
			if (num7 > 0.05f)
			{
				vehicleAudio.UpdateRoll(magnitude);
			}
			else
			{
				vehicleAudio.UpdateRoll(0f);
			}
			if (vehicle.overrideThrottleBrake)
			{
				controlThrottle = Mathf.Clamp01(vehicle.throttleBrake);
				controlBrake = 0f - Mathf.Clamp(vehicle.throttleBrake, -1f, 0f);
			}
			else
			{
				controlThrottle = Mathf.Clamp01(vehicle.inputThrottle);
				controlBrake = Mathf.Clamp01(vehicle.inputBrake);
			}
			controlClutch = Mathf.Clamp01(vehicle.inputClutch);
			controlSteering = Mathf.Clamp(vehicle.inputSteering, -1f, 1f);
			targetThrottle = controlThrottle;
			targetClutch = controlClutch;
			if (clutchPopTime >= 0f)
			{
				targetBrake = controlBrake;
			}
			else
			{
				targetBrake = 1f;
			}
			targetSteering = controlSteering;
			if (turboBoost)
			{
				targetThrottle = 1f;
			}
			if (brakePad)
			{
				targetBrake = 1f;
				if (!turboBoost)
				{
					targetThrottle = 0f;
				}
			}
			if (!isRunning)
			{
				targetThrottle = 0f;
			}
			CheckClutchPopped();
			CheckLimiter();
			float num10 = CheckGear(num3, num4);
			float num11 = 0f;
			if (isRunning)
			{
				if (engineRpm < engineIdleRpm * 1.5f && targetThrottle < idleThrottle)
				{
					if (engineRpm < engineIdleRpm)
					{
						targetThrottle = idleThrottle;
					}
					else
					{
						targetThrottle = Mathf.Max(targetThrottle, idleThrottle * (1f - Mathf.InverseLerp(engineIdleRpm, engineIdleRpm * 1.5f, engineRpm)));
					}
				}
				if (engineRpm < engineIdleRpm)
				{
					float num12 = 1f - Mathf.InverseLerp(stallRpm, engineIdleRpm, engineRpm);
					num11 = (1f - targetThrottle) * num12 * antiStallAssist;
					targetThrottle += num11;
				}
			}
			if (antiWheelieThrottleDecrease > 0f && num8 > 0 && _driver != null && _driver.isDriving)
			{
				float num13 = Vector3.Dot(transform.forward, rhs);
				float num14 = Vector3.Dot(transform.up, rhs);
				if (num13 > antiWheelieMinDot && num14 > 0f)
				{
					float num15 = 1f - Mathf.InverseLerp(antiWheelieMinDot, antiWheelieMaxDot, num13) * antiWheelieThrottleDecrease;
					targetThrottle *= num15;
				}
			}
			float t = 0.3f;
			currThrottle = Mathf.Lerp(currThrottle, targetThrottle, t);
			currClutch = Mathf.Lerp(currClutch, targetClutch, t);
			currBrake = Mathf.Lerp(currBrake, targetBrake, t);
			currSteering = Mathf.Lerp(currSteering, targetSteering, t);
			if (fuelLeft < 3f)
			{
				currThrottle -= UnityEngine.Random.Range(0f, 0.4f) * currThrottle;
			}
			currThrottle *= 1f - limiterAmount;
			gearboxRpm = num2 * num10 / finalDrive;
			float num16 = engineInertia * engineRpm + (1f - engineInertia) * gearboxRpm;
			engineRpm = (1f - currClutch) * engineRpm + currClutch * num16;
			if (canStall && clutchPopped && engineRpm < stallRpm)
			{
				Stall();
			}
			float f = magnitude - prevVelocityMagnitude;
			float num17 = Mathf.Abs(f) / Time.fixedDeltaTime / 9.81f;
			if (isRunning && num17 > maxCrashGs)
			{
				Break();
			}
			prevVelocityMagnitude = magnitude;
			Vector3 vector = velocity - prevVelocity;
			prevVelocity = velocity;
			Vector3 vector2 = vector / Time.fixedDeltaTime;
			if (fuelLeft < 0f)
			{
				Stall();
			}
			if (isRunning && !canStall && clutchPopped && engineRpm < stallRpm)
			{
				engineRpm = stallRpm;
			}
			float num18 = EngineTorqueAt(engineRpm);
			float num19 = 0f;
			float num20 = 0f;
			num20 = EngineFrictionTorqueAt(engineRpm);
			if (!isRunning)
			{
				num20 = curveMaxTorque * engineFrictionCoefficient;
			}
			else if (isBroken)
			{
				num20 = curveMaxTorque;
			}
			float num21 = engineRpm;
			if (isRunning)
			{
				num19 = currThrottle * num18;
				engineRpm += (num19 - num20) * (1f - currClutch) * (10f - 10f * engineInertia * engineInertia) * Time.fixedDeltaTime * 60f;
				if (engineRpm < 0f)
				{
					engineRpm = 0f;
				}
			}
			else
			{
				num19 = 0f;
			}
			if (clutchPopped && isRunning)
			{
				float num22 = ((!(fuelLeft < 3f)) ? currThrottle : 1f);
				fuelLeft -= num22 * Time.fixedDeltaTime;
			}
			float num23 = currClutch * num19;
			float num24 = currClutch * num20 * engineBraking;
			for (int l = 0; l < allWheels.Length; l++)
			{
				Wheel wheel3 = allWheels[l];
				wheel3.brakeTorque = constantBrakeTorque;
			}
			float num25 = currBrake * fullBrakeTorque;
			for (int m = 0; m < brakeWheels.Length; m++)
			{
				Wheel wheel4 = brakeWheels[m];
				wheel4.brakeTorque += num25 * wheel4.brakeAmount;
			}
			float num26 = num23 * num10 / finalDrive / totalWheelDrive;
			float num27 = num24 * num10 / finalDrive / totalWheelDrive;
			for (int n = 0; n < driveWheels.Length; n++)
			{
				Wheel wheel5 = driveWheels[n];
				wheel5.motorTorque = num26 * wheel5.driveAmount;
				wheel5.brakeTorque += num27 * wheel5.driveAmount;
			}
			float num28 = flywheelMass;
			if (clutchPopped && clutchPopTime > minClutchPopDuration * 2f && isRunning)
			{
				num28 = flywheelMass * 0.2f;
			}
			float num29 = num21 - num;
			float num30 = num28 * num29;
			base.GetComponent<Rigidbody>().AddRelativeTorque(engineOrientation * num30, ForceMode.Force);
			Spline steerSpline = vehicle.steerSpline;
			if ((bool)steerSpline)
			{
				float num31 = prevSteerSplineParam - steerSplineParamRadius;
				float num32 = prevSteerSplineParam + steerSplineParamRadius;
				if (steerSpline.AutoClose)
				{
					num31 = Mathf.Repeat(num31, 1f);
					num32 = Mathf.Repeat(num32, 1f);
				}
				else
				{
					num31 = Mathf.Clamp01(num31);
					num32 = Mathf.Clamp01(num32);
				}
				float num33 = 0f;
				if (num31 > num32)
				{
					float closestPointParamFast = steerSpline.GetClosestPointParamFast(transform.position, num31, 1f, steerSplineParamResolution);
					float closestPointParamFast2 = steerSpline.GetClosestPointParamFast(transform.position, 0f, num32, steerSplineParamResolution);
					float sqrMagnitude = (steerSpline.GetPositionOnSplineFast(closestPointParamFast) - transform.position).sqrMagnitude;
					float sqrMagnitude2 = (steerSpline.GetPositionOnSplineFast(closestPointParamFast2) - transform.position).sqrMagnitude;
					num33 = ((!(sqrMagnitude < sqrMagnitude2)) ? closestPointParamFast2 : closestPointParamFast);
				}
				else
				{
					num33 = steerSpline.GetClosestPointParamFast(transform.position, num31, num32, steerSplineParamResolution);
				}
				prevSteerSplineParam = num33;
				steerSplineParamRadius = 5f / steerSpline.Length;
				steerSplineParamResolution = 0.5f / steerSpline.Length;
				float num34 = 2.5f + velocitySteerAhead * magnitude;
				num33 += num34 / steerSpline.Length;
				num33 = ((!steerSpline.AutoClose) ? Mathf.Clamp01(num33) : Mathf.Repeat(num33, 1f));
				steerTarget = steerSpline.GetPositionOnSplineFast(num33);
			}
			float num35 = targetSteering;
			if ((bool)steerSpline)
			{
				Vector3 normalized = (steerTarget - transform.position).normalized;
				num35 = Vector3.Dot(transform.right, normalized) * steerCorrectAmount;
			}
			num35 *= Mathf.Lerp(1f, steerLockLimiter, Mathf.InverseLerp(0f, steerLockLimiterSpeed, magnitude));
			if (!steerSpline && counterSteerSpeed > 0f)
			{
				float num36 = Mathf.InverseLerp(0f, counterSteerSpeed, magnitude);
				float num37 = Vector3.Dot(transform.right, velocity.normalized) * num36;
				num35 = 0.5f * (num35 + num37);
			}
			num35 = Mathf.Clamp(num35, -1f, 1f);
			float num38 = 2f * steerCorrectSpeed * Time.fixedDeltaTime;
			float f2 = num35 - prevSteer;
			if (Mathf.Abs(f2) > num38)
			{
				num35 = prevSteer + num38 * Mathf.Sign(f2);
			}
			for (int num39 = 0; num39 < steerWheels.Length; num39++)
			{
				Wheel wheel6 = steerWheels[num39];
				wheel6.steerAngle = num35 * wheel6.steerLock;
			}
			prevSteer = num35;
			if (tiltSteering && num8 > 0 && _driver != null && _driver.isDriving)
			{
				float num40 = tiltSteerInputLinearity * num35 + (1f - tiltSteerInputLinearity) * (Mathf.Sign(num35) * num35 * num35);
				float num41 = tiltAngle2;
				float num42 = tiltForce2;
				float num43 = tiltRiderForce2;
				if (magnitude < tiltSpeed1)
				{
					float t2 = Mathf.InverseLerp(0f, tiltSpeed1, magnitude);
					num41 = Mathf.Lerp(0f, tiltAngle1, t2);
					num42 = Mathf.Lerp(0f, tiltForce1, t2);
					num43 = Mathf.Lerp(0f, tiltRiderForce1, t2);
				}
				else
				{
					float t3 = Mathf.InverseLerp(tiltSpeed1, tiltSpeed2, magnitude);
					num41 = Mathf.Lerp(tiltAngle1, tiltAngle2, t3);
					num42 = Mathf.Lerp(tiltForce1, tiltForce2, t3);
					num43 = Mathf.Lerp(tiltRiderForce1, tiltRiderForce2, t3);
				}
				float num44 = num41 * (0f - num40);
				float value = Vector3.Dot(-Physics.gravity, rhs) / 9.81f;
				float value2 = Vector3.Dot(vector2 - Physics.gravity * 0.25f, rhs) / 9.81f;
				value = Mathf.Clamp(value, 0f, 1f);
				value2 = Mathf.Clamp(value2, 0f, 1f);
				value2 = (prevSurfaceLoad = Mathf.Lerp(prevSurfaceLoad, value2, 0.05f));
				num44 = value2 * num44 + (1f - value2) * value * num44;
				if (driveWheels.Length > 0)
				{
					float num45 = Mathf.Clamp(driveWheels[0].sidewaysSlip / Mathf.Max(magnitude, 10f), -1f, 1f);
					float num46 = num45 * num41 * 0.5f;
					if (Mathf.Sign(num44) != Mathf.Sign(num46) || !(Mathf.Abs(num44) > Mathf.Abs(num46)))
					{
						num44 = Mathf.Lerp(num44, num46, Mathf.Abs(num45));
					}
				}
				float f3 = Vector3.Dot(transform.right, Vector3.up);
				float f4 = Vector3.Dot(transform.right, rhs);
				float num47 = Mathf.Asin(f3) / (float)Math.PI * 180f;
				float num48 = Mathf.Asin(f4) / (float)Math.PI * 180f;
				float value3 = value2 * num48 + (1f - value2) * value * num47;
				value3 = Mathf.Clamp(value3, -90f, 90f);
				float num49 = (value3 - prevTiltAngle) / Time.fixedDeltaTime;
				prevTiltAngle = value3;
				float num50 = tiltPrediction * (1f - 0.9f * Mathf.InverseLerp(0f, 60f, magnitude));
				value3 += num49 * num50;
				float value4 = (num44 - value3) / tiltSmoothingRange;
				value4 = Mathf.Clamp(value4, -1f, 1f);
				float num51 = Mathf.Abs(value4);
				float num52 = 1f - num51;
				value4 = Mathf.Sign(value4) * (1f - num52 * num52 * num52);
				if (!clutchPopped || (clutchPopped && isRunning))
				{
					base.GetComponent<Rigidbody>().AddRelativeTorque(0f, 0f, tiltConstantForce * Mathf.Sign(value4), ForceMode.Force);
				}
				if (clutchPopped && !isStalled)
				{
					base.GetComponent<Rigidbody>().AddTorque(transform.forward * (num42 * value4), ForceMode.Force);
					_driver.AddRiderForce(transform.right * (0f - num42 * value4 * num43));
				}
			}
			Vector3 angularVelocity = base.GetComponent<Rigidbody>().angularVelocity;
			for (int num53 = 0; num53 < gyroWheels.Length; num53++)
			{
				Wheel wheel7 = gyroWheels[num53];
				float gyroForce = wheel7.gyroForce;
				base.GetComponent<Rigidbody>().AddTorque(gyroForce * wheel7.transform.up * (0f - Vector3.Dot(angularVelocity, wheel7.transform.up)), ForceMode.Force);
				base.GetComponent<Rigidbody>().AddTorque(gyroForce * wheel7.transform.forward * (0f - Vector3.Dot(angularVelocity, wheel7.transform.forward)), ForceMode.Force);
			}
			if (clutchPopped)
			{
				runningTime += Time.fixedDeltaTime;
			}
			if ((bool)vehicleAudio.engineSound && clutchPopped && !isRunning && num21 <= vehicleAudio.engineSound.muteStartRpm)
			{
				vehicleAudio.KillEngine();
			}
			float num54 = 0f;
			num54 = ((!(num19 > num20)) ? (0f - (1f - Mathf.InverseLerp(0f, num20, num19))) : Mathf.InverseLerp(num20, num18, num19));
			if (turboBoost)
			{
				num54 = 1f;
			}
			engineLoad = Mathf.Lerp(engineLoad, num54, 0.8f);
			vehicleAudio.UpdateEngine(currThrottle, num21, engineLoad);
			float num55 = 1f;
			if (num7 < 0.1f)
			{
				num55 = 0.25f;
			}
			if (turboBoost && isRunning && (!requireRocketsForTurboBoost || liveRocketEngineCount > 0))
			{
				base.GetComponent<Rigidbody>().AddForce(transform.forward * turbopadMagicForce * num55 * Time.fixedDeltaTime, ForceMode.Acceleration);
			}
			Vector3 normalized2 = velocity.normalized;
			if (brakePad && magnitude > 1f)
			{
				base.GetComponent<Rigidbody>().AddForce(normalized2 * brakepadMagicForce * num55 * Time.fixedDeltaTime, ForceMode.Acceleration);
			}
		}

		public void StartEngine()
		{
			isRunning = true;
			engineRpm = engineIdleRpm;
			vehicleAudio.UpdateEngine(currThrottle, 0f, engineRpm);
			vehicleAudio.StartEngine();
		}

		public void OnDismountStarted()
		{
			if (clonedParts.Count == 0)
			{
				DismountGame.playerState.statistics.NonDestructibleVehicle();
			}
			clutchPopped = true;
			clutchPopDuration = Mathf.Lerp(maxClutchPopDuration, minClutchPopDuration, controlThrottle);
			clutchPopTime = 0f;
			runningTime = 0f;
			fuelLeft = vehicle.fuel;
		}

		private bool IsVisited(Transform part)
		{
			for (int i = 0; i < visitedParts.Count; i++)
			{
				if (visitedParts[i] == part)
				{
					return true;
				}
			}
			return false;
		}

		private bool RecurseFillParts(Transform part)
		{
			if (connectedToBody.ContainsKey(part.name) && clonedParts.ContainsKey(part.name))
			{
				return true;
			}
			List<string> list = detachLookup[part.name + "(Clone)"];
			for (int i = 0; i < list.Count; i++)
			{
				if (connectedToBody.ContainsKey(list[i]) && clonedParts.ContainsKey(list[i]))
				{
					return true;
				}
				if (!clonedParts.ContainsKey(list[i]))
				{
					continue;
				}
				Transform transform = originalParts[list[i]].transform;
				if (!IsVisited(transform))
				{
					visitedParts.Add(transform);
					if (RecurseFillParts(transform))
					{
						return true;
					}
				}
			}
			return false;
		}

		private void DetachFloatingParts()
		{
			processList.Clear();
			foreach (KeyValuePair<string, GameObject> originalPart in originalParts)
			{
				processList.Add(originalPart.Value.transform);
			}
			while (processList.Count > 0)
			{
				visitedParts.Clear();
				bool flag = false;
				Transform transform = processList[0];
				visitedParts.Add(transform);
				if (!RecurseFillParts(transform))
				{
					flag = true;
				}
				for (int i = 0; i < visitedParts.Count; i++)
				{
					if (flag)
					{
						DetachPart(visitedParts[i].gameObject, 0f, false);
					}
					processList.Remove(visitedParts[i]);
				}
			}
		}

		private void DetachPart(GameObject part, float impactVelocitySqr, bool detachNeighbours)
		{
			if (!part.transform.parent)
			{
				return;
			}
			while (part.GetComponent<DynamicPhysicsPart>() == null)
			{
				part = part.transform.parent.gameObject;
				if (!part.transform.parent)
				{
					return;
				}
			}
			if (!clonedParts.ContainsKey(part.name))
			{
				return;
			}
			GameObject gameObject = clonedParts[part.name];
			Rigidbody rigidbody = rootRigidbodyLookup[part.name];
			gameObject.transform.position = part.transform.position;
			gameObject.transform.rotation = part.transform.rotation;
			gameObject.transform.localScale = part.transform.localScale;
			gameObject.GetComponent<Rigidbody>().velocity = rigidbody.GetPointVelocity(part.transform.position);
			gameObject.GetComponent<Rigidbody>().angularVelocity = base.GetComponent<Rigidbody>().angularVelocity;
			gameObject.SetActive(true);
			clonedParts.Remove(part.name);
			originalParts.Remove(part.name);
			rootRigidbodyLookup.Remove(part.name);
			if (clonedWings.ContainsKey(part.name))
			{
				GameObject obj = clonedWings[part.name];
				clonedWings.Remove(part.name);
				UnityEngine.Object.Destroy(obj);
			}
			Replay instance = Replay.Instance;
			instance.RecordSnapshotFrame();
			List<string> list = detachLookup[gameObject.name];
			if (detachNeighbours)
			{
				foreach (string item in list)
				{
					if ((!(item == "top") || !(DismountGame.playerState.currentVehicleItemId == "vehicle.londonbus")) && clonedParts.ContainsKey(item) && UnityEngine.Random.Range(0, 100) > 50)
					{
						GameObject gameObject2 = clonedParts[item];
						GameObject gameObject3 = originalParts[item];
						Rigidbody rigidbody2 = rootRigidbodyLookup[item];
						gameObject2.transform.position = gameObject3.transform.position;
						gameObject2.transform.rotation = gameObject3.transform.rotation;
						gameObject2.transform.localScale = gameObject3.transform.localScale;
						gameObject2.GetComponent<Rigidbody>().velocity = rigidbody2.GetPointVelocity(gameObject3.transform.position);
						gameObject2.GetComponent<Rigidbody>().angularVelocity = base.GetComponent<Rigidbody>().angularVelocity;
						gameObject2.SetActive(true);
						FixedJoint fixedJoint = gameObject.AddComponent<FixedJoint>();
						fixedJoint.connectedBody = gameObject2.GetComponent<Rigidbody>();
						fixedJoint.breakForce = 200f;
						fixedJoint.breakTorque = 500f;
						instance.RemoveRecordedObject(gameObject3);
						UnityEngine.Object.Destroy(gameObject3);
						clonedParts.Remove(item);
						originalParts.Remove(item);
						rootRigidbodyLookup.Remove(item);
					}
				}
			}
			instance.RemoveRecordedObject(part);
			UnityEngine.Object.Destroy(part);
			if (part.name == "SirenLights" && DismountGame.playerState.currentVehicleItemId == "vehicle.copcar")
			{
				BroadcastMessage("CopLightsBroken", SendMessageOptions.DontRequireReceiver);
			}
			if ((part.name == "Light" || part.name == "Roof") && DismountGame.playerState.currentVehicleItemId == "vehicle.ambulance")
			{
				BroadcastMessage("CopLightsBroken", SendMessageOptions.DontRequireReceiver);
			}
			if ((part.name == "Trunk" || part.name == "TurboLid") && DismountGame.playerState.currentVehicleItemId == "vehicle.agentcar")
			{
				BroadcastMessage("TurboLidBroken", SendMessageOptions.DontRequireReceiver);
			}
			if (part.name == "Whistle" && DismountGame.playerState.currentVehicleItemId == "vehicle.train")
			{
				BroadcastMessage("WhistleBroken", SendMessageOptions.DontRequireReceiver);
			}
			if (zither != null && (part.name.StartsWith("String") || part.name.StartsWith("Tooth")))
			{
				zither.PlayBreakSound();
			}
			DismountGame.playerState.currentCharacterInstance.GetComponent<MrDismount>().VehiclePartDetached(part.name, gameObject.GetComponent<Rigidbody>());
			float num = impactVelocitySqr * partBreakforcePropagationMultiplier;
			if (num > vehiclePartBreakLimit)
			{
				foreach (string item2 in list)
				{
					if ((!(item2 == "top") || !(DismountGame.playerState.currentVehicleItemId == "vehicle.londonbus")) && clonedParts.ContainsKey(item2))
					{
						DetachPart(originalParts[item2], num, true);
					}
				}
			}
			instance.RecordSnapshotFrame();
		}

		private Vector3 randomVector3()
		{
			float x = 1f - UnityEngine.Random.Range(0f, 2f);
			float y = 1f - UnityEngine.Random.Range(0f, 2f);
			float z = 1f - UnityEngine.Random.Range(0f, 2f);
			return new Vector3(x, y, z).normalized;
		}

		private GameObject FindPhysicsPart(Collider c)
		{
			Transform parent = c.transform;
			while ((bool)parent.parent && parent.parent != base.transform)
			{
				if ((bool)parent.GetComponent<DynamicPhysicsPart>())
				{
					return parent.gameObject;
				}
				parent = parent.parent;
			}
			return null;
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.contacts.Length <= 0)
			{
				return;
			}
			Collider collider = collision.contacts[0].thisCollider;
			Collider collider2 = collision.collider;
			Vector3 vector = collision.contacts[0].normal;
			if (collider == collider2)
			{
				collider = collision.contacts[0].otherCollider;
				vector = collision.contacts[0].normal * -1f;
			}
			bool flag = collider is WheelCollider;
			int layer = collider2.gameObject.layer;
			if (layer == 12)
			{
				return;
			}
			vehicle.lastGroundContactTime = Time.fixedTime;
			bool flag2 = false;
			float num = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, vector));
			float num2 = num * num;
			float num3 = 10f;
			float num4 = 0.1f;
			float volumeMultiplier = 0f;
			string text = string.Empty;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			if (num2 > num3 && num > 0f && !flag)
			{
				flag2 = true;
				volumeMultiplier = Mathf.Min(1f, (num2 - num3) / 40f) * 0.3f + 0.7f;
				text = Utils.RandomPick("AutoBumpHard", "AutoCrash") as string;
				flag3 = true;
				flag5 = true;
			}
			else if (num2 > num4)
			{
				volumeMultiplier = Mathf.Min(1f, (num2 - num4) / (num3 - num4)) * 0.7f;
				text = Utils.RandomPick("AutoBumpSoft", "AutoCrashSoft") as string;
				flag3 = true;
			}
			if (flag3 && (lastSoundEffectTime == -1f || Time.time - lastSoundEffectTime > 0.1f))
			{
				DismountGame.audioManager.PlaySoundEffect(text, base.transform, volumeMultiplier);
				lastSoundEffectTime = Time.time;
				if (zither != null)
				{
					zither.PlayCollisionSound(volumeMultiplier);
				}
			}
			if (flag2)
			{
				Vector3 normalized = Vector3.Reflect(base.GetComponent<Rigidbody>().velocity, vector).normalized;
				DismountGame.particleManager.EmitDust(collision.contacts[0].point, normalized * 2f);
				DismountGame.particleManager.EmitSpark(collision.contacts[0].point + normalized, normalized * 4f, 3f);
				if (nutsAndBolts && Time.time - previousNutsAndBoltsTime > 0.025f)
				{
					if (collider2.tag != "Bolts")
					{
						int num5 = (int)Mathf.Min(num2 / 30f, 15f);
						for (int i = 0; i < num5; i++)
						{
							GameObject gameObject = DismountGame.instance.boltObjectPool.Get();
							if (gameObject != null)
							{
								gameObject.transform.position = collision.contacts[0].point;
								gameObject.transform.rotation = Quaternion.identity;
								gameObject.SetActive(true);
								gameObject.GetComponent<Rigidbody>().velocity = (base.GetComponent<Rigidbody>().velocity + base.GetComponent<Rigidbody>().velocity.magnitude * randomVector3() * 0.5f) * UnityEngine.Random.Range(0.8f, 1f);
								gameObject.GetComponent<Rigidbody>().angularVelocity = randomVector3() * UnityEngine.Random.Range(0f, 30f);
								gameObject.transform.parent = sceneDynamic;
							}
						}
					}
					previousNutsAndBoltsTime = Time.time;
				}
				if (layer == 9 && Time.fixedTime - previousCraterTime > 0.1f)
				{
					DismountGame.particleManager.EmitRocks(collision.contacts[0].point);
					Vector3 vector2 = vector;
					if (Mathf.Abs(vector2.y) > Mathf.Abs(vector2.x) && Mathf.Abs(vector2.y) > Mathf.Abs(vector2.z))
					{
						GameObject gameObject2 = UnityEngine.Object.Instantiate(decal, collision.contacts[0].point + vector2 * 0.08f, Quaternion.LookRotation(vector2)) as GameObject;
						gameObject2.transform.parent = collider2.transform;
						gameObject2.AddComponent<RecordLossyScale>();
						gameObject2.AddComponent<RecordMeInReplay>();
					}
					previousCraterTime = Time.fixedTime;
				}
			}
			if (num2 > vehiclePartBreakLimit && collider2.tag != "DetachedPart")
			{
				GameObject gameObject3 = FindPhysicsPart(collider);
				if ((bool)gameObject3 && clonedParts.ContainsKey(gameObject3.name))
				{
					if (!(gameObject3.name == "top") || !(DismountGame.playerState.currentVehicleItemId == "vehicle.londonbus"))
					{
						DetachPart(gameObject3, num2, true);
					}
					flag4 = true;
				}
			}
			if (!_gameLogic || !_gameLogic.isDismountActive)
			{
				return;
			}
			int impactScore = GetImpactScore(num, collision);
			if (flag4)
			{
				DismountGame.hudManager.AddDamageIcon(HUDManager.DamageIconType.VehicleDetach);
				_gameLogic.AddImpactScore(impactScore, 10000, Dismount.GameStates.Dismount.ScoreCategory.Vehicle);
				DetachFloatingParts();
				if (DismountGame.playerState.currentVehicleItemId == "vehicle.londonbus" && originalParts.ContainsKey("top"))
				{
					int num6 = 0;
					List<string> list = detachLookup["top(Clone)"];
					for (int j = 0; j < list.Count; j++)
					{
						if (!clonedParts.ContainsKey(list[j]))
						{
							continue;
						}
						visitedParts.Clear();
						visitedParts.Add(originalParts["top"].transform);
						for (int k = 0; k < list.Count; k++)
						{
							if (list[j] != list[k] && originalParts.ContainsKey(list[k]))
							{
								visitedParts.Add(originalParts[list[k]].transform);
							}
						}
						if (RecurseFillParts(originalParts[list[j]].transform))
						{
							num6++;
						}
					}
					if (num6 == 1)
					{
						DetachPart(originalParts["top"], 0f, true);
						DetachFloatingParts();
					}
				}
				if (clonedParts.Count == 0)
				{
					DismountGame.playerState.statistics.VehicleDisintegrated();
				}
			}
			else if (flag5 && Time.fixedTime > impactScoreWindow)
			{
				float num7 = Time.time - previousHitIconTime;
				if (num7 > 0.2f)
				{
					DismountGame.hudManager.AddDamageIcon(HUDManager.DamageIconType.Vehicle);
					previousHitIconTime = Time.time;
				}
				_gameLogic.AddImpactScore(impactScore, 0, Dismount.GameStates.Dismount.ScoreCategory.Vehicle);
				impactScoreWindow = Time.fixedTime + 2.5f * Time.fixedDeltaTime;
			}
		}

		private int GetImpactScore(float impactVelocity, Collision collision)
		{
			int num = 0;
			float num2 = 1000f;
			if ((bool)collision.rigidbody)
			{
				num2 = Mathf.Clamp(collision.rigidbody.mass, 1f, 10000f);
			}
			if (impactVelocity > 50f)
			{
				impactVelocity = 50f;
			}
			return (int)(impactVelocity * num2 * 0.01f) * 10;
		}

		private void OnCollisionStay(Collision collision)
		{
			if (collision.contacts.Length > 0)
			{
				ContactPoint contactPoint = collision.contacts[0];
				if (contactPoint.otherCollider.gameObject.layer != 12 && contactPoint.thisCollider.gameObject.layer != 12)
				{
					vehicle.lastGroundContactTime = Time.fixedTime;
				}
			}
		}

		private void Update()
		{
			if (base.GetComponent<Rigidbody>().centerOfMass != centerOfMass)
			{
				base.GetComponent<Rigidbody>().centerOfMass = centerOfMass;
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawSphere(centerOfMass, 0.25f);
		}
	}
}
