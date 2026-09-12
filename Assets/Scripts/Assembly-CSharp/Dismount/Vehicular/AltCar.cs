#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dismount.Vehicular
{
	[RequireComponent(typeof(Vehicle))]
	public class AltCar : MonoBehaviour
	{
		private const float RpmToRadps = (float)Math.PI / 30f;

		public Vector3 centerOfMass = Vector3.zero;

		public Vector3 engineOrientation = Vector3.forward;

		public AnimationCurve engineOutputTorque = AnimationCurve.Linear(0f, 0.5f, 1f, 0.3f);

		public float curveMaxTorque = 320f;

		public float curveMaxRpm = 6500f;

		public float engineFrictionCoefficient = 0.5f;

		public float idleThrottle = 0.2f;

		public float stallRpm = 400f;

		[Range(0.8f, 0.99f)]
		public float engineInertia = 0.9f;

		public float flywheelMass = 20f;

		public float maxCrashGs = 100f;

		public float[] gearRatios;

		public float finalDrive = 0.9f;

		public float minClutchPopDuration = 0.4f;

		public float maxClutchPopDuration = 2f;

		public float gearChangeDuration = 0.2f;

		public float steerSpeed = 1f;

		public float reducedSteeringAt = 27.8f;

		public float reducedSteeringAmount = 0.5f;

		public bool canStall = true;

		private List<AltWheel> allWheels = new List<AltWheel>();

		private List<AltWheel> steerWheels = new List<AltWheel>();

		private List<AltWheel> driveWheels = new List<AltWheel>();

		private List<AltWheel> brakeWheels = new List<AltWheel>();

		private List<AltWheel> gyroWheels = new List<AltWheel>();

		private float totalWheelDrive;

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

		private bool isBroken;

		private int gear;

		private int prevGear;

		private float gearChangeTime = -1f;

		private float lastGearChangeTime;

		private float prevSteer;

		private int collisionStayCounter;

		private float fuelLeft = 30f;

		private Dictionary<string, object> debug = new Dictionary<string, object>();

		private float runningTime;

		private VehicleAudio vehicleAudio;

		private float maxTorque;

		private float maxTorqueAt;

		private float maxPower;

		private float maxPowerAt;

		private float engineLoad;

		public float[] upShiftRpm;

		public float[] downShiftRpm;

		private float prevVelocityMagnitude;

		private float prevSteerSplineParam = 0.5f;

		private float steerSplineParamRadius = 0.5f;

		private float steerSplineParamResolution = 0.01f;

		private Vector3 steerTarget;

		private Vehicle vehicle;

		private float lastSoundEffectTime = -1f;

		public void WakeUp()
		{
			base.GetComponent<Rigidbody>().isKinematic = false;
			controlBrake = 1f;
		}

		public void StartEngine()
		{
			isRunning = true;
			engineRpm = stallRpm * 2f;
			vehicleAudio.UpdateEngine(currThrottle, 0f, engineRpm);
			vehicleAudio.StartEngine();
		}

		public void OnDismountStarted()
		{
			clutchPopped = true;
			clutchPopDuration = Mathf.Lerp(maxClutchPopDuration, minClutchPopDuration, controlThrottle);
			clutchPopTime = 0f;
			runningTime = 0f;
		}

		private void Awake()
		{
			if (vehicle != null)
			{
				return;
			}
			vehicle = GetComponent<Vehicle>();
			AltWheel[] componentsInChildren = base.gameObject.GetComponentsInChildren<AltWheel>();
			AltWheel[] array = componentsInChildren;
			foreach (AltWheel altWheel in array)
			{
				allWheels.Add(altWheel);
				if (altWheel.DriveAmount > 0f)
				{
					driveWheels.Add(altWheel);
					totalWheelDrive += altWheel.DriveAmount;
				}
				if (altWheel.SteerLock != 0f)
				{
					steerWheels.Add(altWheel);
				}
				if (altWheel.BrakeAmount > 0f)
				{
					brakeWheels.Add(altWheel);
				}
				if (altWheel.GyroAmount > 0f)
				{
					gyroWheels.Add(altWheel);
				}
			}
			steerTarget = base.transform.position + base.transform.forward * 1000f;
			base.GetComponent<Rigidbody>().centerOfMass = centerOfMass;
			if (gearRatios.Length < 0)
			{
				Debug.LogError("No gearRatios found, must have at least one gear");
			}
			base.GetComponent<Rigidbody>().isKinematic = true;
			base.GetComponent<Rigidbody>().solverIterations = 6;
			fuelLeft = vehicle.fuel;
			vehicleAudio = base.transform.Find("Audio").GetComponent<VehicleAudio>();
			prevVelocityMagnitude = 0f;
			prevSteerSplineParam = 0.5f;
			steerSplineParamRadius = 0.5f;
			steerSplineParamResolution = 0.01f;
			upShiftRpm = new float[gearRatios.Length];
			downShiftRpm = new float[gearRatios.Length];
		}

		private void Start()
		{
			ReportEnginePeaks();
		}

		private void ReportEnginePeaks()
		{
			for (float num = 0f; num < curveMaxRpm; num++)
			{
				float time = num / curveMaxRpm;
				float num2 = engineOutputTorque.Evaluate(time) * curveMaxTorque;
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
			debug["peaks"] = string.Empty + Utils.NumberString(maxTorque, 1) + "Nm @ " + maxTorqueAt + "rpm / " + Utils.NumberString(maxPower * 0.001f, 1) + "kW @ " + maxPowerAt + "rpm";
			downShiftRpm[0] = -1f;
			upShiftRpm[gearRatios.Length - 1] = -1f;
			for (int i = 0; i < gearRatios.Length; i++)
			{
				if (i > 0)
				{
					for (float num4 = curveMaxRpm; num4 >= 0f; num4--)
					{
						float rpm = EngineRPMOnAnyGear(i - 1, i, num4);
						if (EnginePowerAt(rpm) > EnginePowerAt(num4))
						{
							downShiftRpm[i] = num4;
							break;
						}
					}
				}
				if (i >= gearRatios.Length - 1)
				{
					continue;
				}
				for (float num5 = 0f; num5 < curveMaxRpm; num5++)
				{
					float rpm2 = EngineRPMOnAnyGear(i + 1, i, num5);
					if (EnginePowerAt(rpm2) > EnginePowerAt(num5))
					{
						upShiftRpm[i] = num5;
						break;
					}
				}
			}
		}

		private void CheckClutchPopped()
		{
			if (clutchPopTime >= 0f)
			{
				targetBrake = controlBrake;
				clutchPopTime += Time.fixedDeltaTime;
				if (clutchPopTime > clutchPopDuration)
				{
					targetClutch = 1f;
					return;
				}
				float num = Mathf.Lerp(0f, 1f, clutchPopTime / clutchPopDuration);
				targetClutch = 1f - (1f - num) * (1f - num);
				lastGearChangeTime = Time.fixedTime;
			}
		}

		private float EngineTorqueAt(float rpm)
		{
			float time = rpm / curveMaxRpm;
			return engineOutputTorque.Evaluate(time) * curveMaxTorque;
		}

		private float EnginePowerAt(float rpm)
		{
			return EngineTorqueAt(rpm) * rpm * ((float)Math.PI / 30f);
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

		private float EngineRPMOnAnyGear(int checkGear, int currGear, float currRPM)
		{
			if (checkGear == currGear)
			{
				return currRPM;
			}
			checkGear = Mathf.Clamp(checkGear, 0, gearRatios.Length - 1);
			float num = gearRatios[checkGear] / gearRatios[currGear];
			return currRPM * num;
		}

		private float CheckGear(float wheelContactTime, float wheelSlip)
		{
			float result = gearRatios[gear];
			if (gearChangeTime >= 0f)
			{
				gearChangeTime += Time.fixedDeltaTime;
				if (!(gearChangeTime > gearChangeDuration))
				{
					result = Mathf.Lerp(gearRatios[prevGear], gearRatios[gear], gearChangeTime / gearChangeDuration);
					targetThrottle *= 0.25f;
					targetClutch = 0.5f;
					return result;
				}
				result = gearRatios[gear];
				gearChangeTime = -1f;
				lastGearChangeTime = Time.fixedTime;
			}
			if (!isRunning || wheelContactTime < 0.25f || targetClutch < 0.95f || Time.fixedTime < lastGearChangeTime + gearChangeDuration)
			{
				return result;
			}
			if (gear > 0 && engineRpm < stallRpm * 1.5f)
			{
				prevGear = gear;
				gear--;
				gearChangeTime = 0f;
				return result;
			}
			if (targetThrottle < 0.25f && gear > 0)
			{
				float rpm = EngineRPMOnGear(gear - 1);
				if (EngineTorqueAt(rpm) > EngineTorqueAt(engineRpm))
				{
					prevGear = gear;
					gear--;
					gearChangeTime = 0f;
					return result;
				}
			}
			float num = EnginePowerAt(engineRpm);
			float num2 = 0.6f + (targetThrottle - 0.1f) * 0.44f;
			float num3 = 1.15f / num2;
			if (gear > 0)
			{
				float rpm2 = EngineRPMOnGear(gear - 1);
				if (EnginePowerAt(rpm2) > num * num3)
				{
					prevGear = gear;
					gear--;
					gearChangeTime = 0f;
					return result;
				}
			}
			if (targetThrottle < 0.25f || wheelSlip > 1.5f)
			{
				return result;
			}
			if (gear < gearRatios.Length - 1)
			{
				float rpm3 = EngineRPMOnGear(gear + 1);
				if (EnginePowerAt(rpm3) > num * num2)
				{
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
			isRunning = false;
		}

		private void Break()
		{
			isRunning = false;
			isBroken = true;
		}

		private void DebugInfo(string name, object obj)
		{
			if (vehicle.showDebug)
			{
				debug[name] = obj;
			}
		}

		private void FixedUpdate()
		{
			if (!vehicle.isAwake)
			{
				return;
			}
			base.GetComponent<Rigidbody>().WakeUp();
			float magnitude = base.GetComponent<Rigidbody>().velocity.magnitude;
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
			foreach (AltWheel driveWheel in driveWheels)
			{
				if (driveWheel.rpm > num2)
				{
					num2 = driveWheel.rpm;
				}
				if (driveWheel.contactTime > num3)
				{
					num3 = driveWheel.contactTime;
				}
				if (driveWheel.slip > num4)
				{
					num4 = driveWheel.slip;
				}
			}
			if (num3 > 0.05f)
			{
				vehicleAudio.UpdateRoll(magnitude);
			}
			else
			{
				vehicleAudio.UpdateRoll(0f);
			}
			float num5 = 0f;
			float num6 = 0f;
			foreach (AltWheel allWheel in allWheels)
			{
				if (allWheel.contactTime > 0f)
				{
					float num7 = allWheel.slip * (allWheel.load / allWheel.maxSpringLoad) * allWheel.surfaceFriction;
					num6 += num7;
					if (num7 > num5)
					{
						num5 = num7;
					}
				}
			}
			vehicleAudio.UpdateSkid(num6);
			controlThrottle = Mathf.Clamp01(vehicle.inputThrottle);
			controlClutch = Mathf.Clamp01(vehicle.inputClutch);
			controlBrake = Mathf.Clamp01(vehicle.inputBrake);
			controlSteering = Mathf.Clamp01(vehicle.inputSteering);
			targetThrottle = idleThrottle + controlThrottle * (1f - idleThrottle);
			targetClutch = controlClutch;
			targetBrake = controlBrake;
			targetSteering = controlSteering;
			if (targetThrottle < idleThrottle)
			{
				targetThrottle = idleThrottle;
			}
			if (clutchPopped)
			{
				targetThrottle += idleThrottle * (1f - targetThrottle);
			}
			CheckClutchPopped();
			float num8 = CheckGear(num3, num4);
			float num9 = 0.9f;
			currThrottle = currThrottle * num9 + targetThrottle * (1f - num9);
			currClutch = currClutch * num9 + targetClutch * (1f - num9);
			currBrake = currBrake * num9 + targetBrake * (1f - num9);
			currSteering = currSteering * num9 + targetSteering * (1f - num9);
			if (fuelLeft < 1f)
			{
				currThrottle -= UnityEngine.Random.Range(0f, 0.5f) * currThrottle;
			}
			float num10 = 0.001f;
			if (currThrottle < targetThrottle && currThrottle > targetThrottle - num10)
			{
				currThrottle = targetThrottle;
			}
			else if (currThrottle > targetThrottle && currThrottle < targetThrottle + num10)
			{
				currThrottle = targetThrottle;
			}
			if (currClutch < targetClutch && currClutch > targetClutch - num10)
			{
				currClutch = targetClutch;
			}
			else if (currClutch > targetClutch && currClutch < targetClutch + num10)
			{
				currClutch = targetClutch;
			}
			if (currBrake < targetBrake && currBrake > targetBrake - num10)
			{
				currBrake = targetBrake;
			}
			else if (currBrake > targetBrake && currBrake < targetBrake + num10)
			{
				currBrake = targetBrake;
			}
			DebugInfo("throttle", currThrottle);
			DebugInfo("clutch", currClutch);
			DebugInfo("brake", currBrake);
			DebugInfo("steering", currSteering);
			DebugInfo("gear", gear + 1);
			gearboxRpm = num2 * num8 / finalDrive;
			float num11 = engineInertia * engineRpm + (1f - engineInertia) * gearboxRpm;
			engineRpm = (1f - currClutch) * engineRpm + currClutch * num11;
			if (canStall && clutchPopped && engineRpm < stallRpm * 0.75f)
			{
				Stall();
			}
			float f = magnitude - prevVelocityMagnitude;
			float num12 = Mathf.Abs(f) / Time.fixedDeltaTime / 9.81f;
			if (isRunning && num12 > maxCrashGs)
			{
				Break();
			}
			prevVelocityMagnitude = magnitude;
			if (fuelLeft < 0f)
			{
				Stall();
			}
			if (isRunning && !canStall && clutchPopped && engineRpm < stallRpm)
			{
				engineRpm = stallRpm;
			}
			float num13 = 0f;
			float num14 = 0f;
			num14 = engineFrictionCoefficient * (engineRpm / curveMaxRpm) * curveMaxTorque;
			if (!isRunning)
			{
				num14 = ((!isBroken) ? (0.5f * engineFrictionCoefficient * curveMaxTorque) : curveMaxTorque);
			}
			if (isRunning)
			{
				num13 = currThrottle * EngineTorqueAt(engineRpm);
				engineRpm += (num13 - num14) * (1f - currClutch) * (10f - 10f * engineInertia) * Time.fixedDeltaTime * 60f;
				if (engineRpm < 0f)
				{
					engineRpm = 0f;
				}
			}
			else
			{
				currThrottle = 0f;
				num13 = 0f;
			}
			if (clutchPopped)
			{
				fuelLeft -= currThrottle * Time.fixedDeltaTime;
			}
			DebugInfo("engine rpm", engineRpm);
			DebugInfo("gearbox rpm", gearboxRpm);
			DebugInfo("output torque, Nm", num13);
			float num15 = num13 * engineRpm * ((float)Math.PI / 30f);
			DebugInfo("output power, kW", num15 * 0.001f);
			float num16 = currClutch * num13;
			float num17 = currClutch * num14;
			foreach (AltWheel allWheel2 in allWheels)
			{
				allWheel2.brakeTorque = 10f;
			}
			float num18 = currBrake * base.GetComponent<Rigidbody>().mass * 2f;
			foreach (AltWheel brakeWheel in brakeWheels)
			{
				brakeWheel.brakeTorque += num18;
			}
			float num19 = num16 * num8 / finalDrive / totalWheelDrive;
			float num20 = num17 * num8 / finalDrive / totalWheelDrive;
			foreach (AltWheel driveWheel2 in driveWheels)
			{
				driveWheel2.motorTorque = num19 * driveWheel2.DriveAmount;
				driveWheel2.brakeTorque += num20 * driveWheel2.DriveAmount;
			}
			float num21 = flywheelMass;
			if (clutchPopped && clutchPopTime > minClutchPopDuration * 2f && isRunning)
			{
				num21 = flywheelMass * 0.2f;
			}
			float num22 = engineRpm - num;
			float num23 = num21 * num22;
			base.GetComponent<Rigidbody>().AddRelativeTorque(engineOrientation * num23, ForceMode.Force);
			Spline steerSpline = vehicle.steerSpline;
			if ((bool)steerSpline)
			{
				float num24 = prevSteerSplineParam - steerSplineParamRadius;
				float num25 = prevSteerSplineParam + steerSplineParamRadius;
				if (steerSpline.AutoClose)
				{
					num24 = Mathf.Repeat(num24, 1f);
					num25 = Mathf.Repeat(num25, 1f);
				}
				else
				{
					num24 = Mathf.Clamp01(num24);
					num25 = Mathf.Clamp01(num25);
				}
				float num26 = 0f;
				if (num24 > num25)
				{
					float closestPointParamFast = steerSpline.GetClosestPointParamFast(base.transform.position, num24, 1f, steerSplineParamResolution);
					float closestPointParamFast2 = steerSpline.GetClosestPointParamFast(base.transform.position, 0f, num25, steerSplineParamResolution);
					float sqrMagnitude = (steerSpline.GetPositionOnSplineFast(closestPointParamFast) - base.transform.position).sqrMagnitude;
					float sqrMagnitude2 = (steerSpline.GetPositionOnSplineFast(closestPointParamFast2) - base.transform.position).sqrMagnitude;
					num26 = ((!(sqrMagnitude < sqrMagnitude2)) ? closestPointParamFast2 : closestPointParamFast);
				}
				else
				{
					num26 = steerSpline.GetClosestPointParamFast(base.transform.position, num24, num25, steerSplineParamResolution);
				}
				prevSteerSplineParam = num26;
				steerSplineParamRadius = 0.05f;
				steerSplineParamResolution = 0.002f;
				float num27 = 2.5f + 0.5f * magnitude;
				num26 += num27 / steerSpline.Length;
				num26 = ((!steerSpline.AutoClose) ? Mathf.Clamp01(num26) : Mathf.Repeat(num26, 1f));
				steerTarget = steerSpline.GetPositionOnSplineFast(num26);
			}
			float value = targetSteering;
			if ((bool)steerSpline)
			{
				Vector3 normalized = (steerTarget - base.transform.position).normalized;
				value = Vector3.Dot(base.transform.right, normalized);
			}
			value = Mathf.Clamp(value, -1f, 1f);
			float num28 = 2f * steerSpeed * Time.fixedDeltaTime;
			float f2 = value - prevSteer;
			if (Mathf.Abs(f2) > num28)
			{
				value = prevSteer + num28 * Mathf.Sign(f2);
			}
			float num29 = Mathf.Abs(Vector3.Dot(base.GetComponent<Rigidbody>().velocity, base.transform.forward));
			float num30 = reducedSteeringAmount + (1f - reducedSteeringAmount) * ((reducedSteeringAt - num29) / reducedSteeringAt);
			if (num30 < reducedSteeringAmount)
			{
				num30 = reducedSteeringAmount;
			}
			value *= num30;
			foreach (AltWheel steerWheel in steerWheels)
			{
				steerWheel.steerAngle = value * steerWheel.SteerLock;
			}
			prevSteer = value;
			Vector3 angularVelocity = base.GetComponent<Rigidbody>().angularVelocity;
			foreach (AltWheel gyroWheel in gyroWheels)
			{
				float gyroForce = gyroWheel.gyroForce;
				base.GetComponent<Rigidbody>().AddRelativeTorque(gyroForce * gyroWheel.transform.up * (0f - Vector3.Dot(angularVelocity, gyroWheel.transform.up)), ForceMode.Force);
				base.GetComponent<Rigidbody>().AddRelativeTorque(gyroForce * gyroWheel.transform.forward * (0f - Vector3.Dot(angularVelocity, gyroWheel.transform.forward)), ForceMode.Force);
			}
			if (clutchPopped)
			{
				runningTime += Time.fixedDeltaTime;
			}
			DebugInfo("velocity, km/h", magnitude * 3.6f);
			DebugInfo("steer", value);
			DebugInfo("max wheel slip", num4);
			DebugInfo("position", base.GetComponent<Rigidbody>().position);
			DebugInfo("running time", runningTime);
			DebugInfo("fuel left", fuelLeft);
			if (clutchPopped && engineRpm <= 200f)
			{
				vehicleAudio.KillEngine();
			}
			engineLoad = currThrottle * (num15 / maxPower);
			DebugInfo("engine load", engineLoad);
			vehicleAudio.UpdateEngine(currThrottle, engineLoad, engineRpm);
		}

		private void OnGUI()
		{
			if (!vehicle.showDebug)
			{
				return;
			}
			float num = 40f;
			foreach (string key in debug.Keys)
			{
				object obj = debug[key];
				string empty = string.Empty;
				empty = ((obj.GetType() != typeof(float)) ? obj.ToString() : Utils.NumberString((float)obj, 3));
				GUI.Label(new Rect(10f, num, Screen.width - 20, 25f), string.Empty + key + ": " + empty);
				num += 20f;
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.contacts.Length <= 0)
			{
				return;
			}
			int layer = collision.collider.gameObject.layer;
			if (layer == 12)
			{
				return;
			}
			vehicle.lastGroundContactTime = Time.fixedTime;
			float num = Vector3.Dot(collision.relativeVelocity, collision.contacts[0].normal);
			float num2 = num * num;
			float num3 = 10f;
			float num4 = 0.1f;
			float volumeMultiplier = 0f;
			string text = string.Empty;
			bool flag = false;
			if (num2 > num3)
			{
				volumeMultiplier = Mathf.Min(1f, (num2 - num3) / 40f) * 0.3f + 0.7f;
				text = "AutoBumpHard";
				flag = true;
			}
			else if (num2 > num4)
			{
				volumeMultiplier = Mathf.Min(1f, (num2 - num4) / (num3 - num4)) * 0.7f;
				text = "AutoBumpSoft";
				flag = true;
			}
			if (flag && (lastSoundEffectTime == -1f || Time.time - lastSoundEffectTime > 0.1f))
			{
				if (DismountGame.instance != null)
				{
					DismountGame.audioManager.PlaySoundEffect(text, base.transform, volumeMultiplier);
				}
				lastSoundEffectTime = Time.time;
			}
		}

		private void OnCollisionStay(Collision collision)
		{
			if (collision.contacts.Length <= 0)
			{
				return;
			}
			ContactPoint contactPoint = collision.contacts[0];
			if (contactPoint.otherCollider.gameObject.layer != 12 && contactPoint.thisCollider.gameObject.layer != 12)
			{
				vehicle.lastGroundContactTime = Time.fixedTime;
			}
			Vector3 point = contactPoint.point;
			Vector3 pointVelocity = base.GetComponent<Rigidbody>().GetPointVelocity(point);
			float magnitude = pointVelocity.magnitude;
			if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground") && magnitude > 3f)
			{
				collisionStayCounter++;
				int num = (int)(0.02f / Time.fixedDeltaTime);
				if (num < 1)
				{
					num = 1;
				}
				int num2 = 2 * num;
				if (collisionStayCounter % num2 == 0 && DismountGame.instance != null)
				{
					Vector3 normalized = Vector3.Cross(pointVelocity, contactPoint.normal).normalized;
					Vector3 velocity = contactPoint.normal * UnityEngine.Random.Range(0.1f, 0.5f) * magnitude + UnityEngine.Random.Range(-0.2f, 0.2f) * magnitude * normalized - 0.5f * pointVelocity;
					DismountGame.particleManager.EmitSpark(point, velocity);
				}
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.75f);
			Gizmos.DrawSphere(base.GetComponent<Rigidbody>().worldCenterOfMass, 0.25f);
		}
	}
}
