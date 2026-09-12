#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;
using reLive;

namespace Dismount.Vehicular
{
	public class Wheel : MonoBehaviour
	{
		private const int emitSkipFrames = 1;

		private const float rpmToRadps = (float)Math.PI / 30f;

		private const float radpsToRpm = 30f / (float)Math.PI;

		[Range(0f, 1f)]
		public float driveAmount;

		[Range(0f, 1f)]
		public float brakeAmount = 1f;

		[Range(-90f, 90f)]
		public float steerLock;

		[Range(0f, 1f)]
		public float gyroAmount;

		public float skidOffset;

		public float skidWidth = 0.3f;

		public GameObject skidmarkPrefab;

		private Skidmark skidmark;

		public bool flaming;

		public bool noSmoke;

		public bool showDebug;

		public AnimationCurve originalCurve;

		private Dictionary<string, object> debug = new Dictionary<string, object>();

		private Transform mVisualWheel;

		private ExtendedLocalEulers mVisualWheelEulers;

		private Transform mSuspensionArm;

		private WheelCollider mWheelCollider;

		private float suspensionTravel;

		private float prevSuspensionTravel;

		private float hitContactTime;

		private Vehicle owner;

		private Car ownerCar;

		private Rigidbody connectedBody;

		private int smokeSkipCounter;

		private Transform thisTransform;

		private Vector3 contactPosition = Vector3.zero;

		private Vector3 mContactNormal = Vector3.up;

		private float contactIncidence = 1f;

		private bool contactWithRigidbody;

		private WheelFrictionCurve originalForwardFriction;

		private WheelFrictionCurve originalSidewaysFriction;

		private WheelTelemetry telemetry;

		private float mSlip;

		private float mRpm;

		private float mRotation;

		private float mInertia;

		private float mMotorTorque;

		private float mBrakeTorque;

		private float mFriction;

		private float mLoad;

		private float mMaxSpringLoad = 1f;

		private bool doSkip;

		private int emitSkip = 1;

		private float mSidewaysSlip;

		private int groundHitLayerMask = 512;

		private float airGrip;

		private Vector3 lastFlamePos = Vector3.zero;

		private bool skipSkidMark;

		public float gyroForce
		{
			get
			{
				return mRpm * ((float)Math.PI / 30f) * mWheelCollider.mass * gyroAmount;
			}
		}

		public bool hasContact
		{
			get
			{
				return hitContactTime > 0f;
			}
		}

		public float contactTime
		{
			get
			{
				return hitContactTime;
			}
		}

		public float rpm
		{
			get
			{
				return mRpm;
			}
		}

		public float motorTorque
		{
			get
			{
				return mMotorTorque;
			}
			set
			{
				mMotorTorque = value;
			}
		}

		public float brakeTorque
		{
			get
			{
				return mBrakeTorque;
			}
			set
			{
				mBrakeTorque = value;
			}
		}

		public float slip
		{
			get
			{
				return mSlip;
			}
		}

		public float steerAngle
		{
			get
			{
				return mWheelCollider.steerAngle;
			}
			set
			{
				mWheelCollider.steerAngle = value;
			}
		}

		public float rotation
		{
			get
			{
				return mRotation;
			}
		}

		public float load
		{
			get
			{
				return mLoad;
			}
		}

		public float maxSpringLoad
		{
			get
			{
				return mMaxSpringLoad;
			}
		}

		public float surfaceFriction
		{
			get
			{
				return mFriction;
			}
		}

		public Vector3 contactNormal
		{
			get
			{
				return mContactNormal;
			}
		}

		public float sidewaysSlip
		{
			get
			{
				return mSidewaysSlip;
			}
		}

		private void Awake()
		{
			if (!base.gameObject.GetComponent<RetainHierarchy>())
			{
				base.gameObject.AddComponent<RetainHierarchy>();
			}
			thisTransform = base.transform;
			mSuspensionArm = thisTransform.Find("SuspensionArm");
			mVisualWheel = mSuspensionArm.GetChild(0);
			mVisualWheelEulers = mVisualWheel.gameObject.GetComponent<ExtendedLocalEulers>();
			if (!mVisualWheelEulers)
			{
				mVisualWheelEulers = mVisualWheel.gameObject.AddComponent<ExtendedLocalEulers>();
			}
			mWheelCollider = base.GetComponent<Collider>() as WheelCollider;
			mInertia = mWheelCollider.mass * mWheelCollider.radius;
			groundHitLayerMask = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Character"));
			mMaxSpringLoad = mWheelCollider.suspensionSpring.spring * mWheelCollider.suspensionDistance;
			originalForwardFriction = mWheelCollider.forwardFriction;
			originalSidewaysFriction = mWheelCollider.sidewaysFriction;
			connectedBody = mWheelCollider.attachedRigidbody;
			owner = Utils.FindFirstComponentUpInHierarchy<Vehicle>(base.transform);
			if (owner == null)
			{
				Debug.LogError("Wheel should be under a Vehicle");
			}
			if ((bool)owner)
			{
				ownerCar = owner.GetComponent<Car>();
			}
			if (DismountGame.IsLowPerformanceDevice())
			{
				skidmarkPrefab = null;
			}
			if (DismountGame.IsLowPerformanceDevice())
			{
				doSkip = true;
			}
		}

		private void UpdateSlip()
		{
			float num = 0.1f;
			float num2 = 0.1f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			contactWithRigidbody = false;
			WheelHit hit;
			if (mWheelCollider.GetGroundHit(out hit))
			{
				float num6 = originalForwardFriction.extremumSlip * 0.5f;
				float num7 = originalForwardFriction.asymptoteSlip * 0.75f;
				num = (Mathf.Abs(hit.forwardSlip) - num6) / (num7 - num6);
				num = Mathf.Clamp01(num);
				num6 = originalSidewaysFriction.extremumSlip;
				num7 = originalSidewaysFriction.asymptoteSlip;
				num2 = hit.sidewaysSlip;
				mSidewaysSlip = Mathf.Lerp(mSidewaysSlip, num2, 0.2f);
				num2 = (Mathf.Abs(num2) - num6) / (num7 - num6);
				num2 = Mathf.Clamp01(num2);
				if (mRpm < 10f && connectedBody.velocity.sqrMagnitude < 10f)
				{
					num *= 0.2f;
					num2 *= 0.4f;
				}
				num3 = hit.collider.material.staticFriction;
				num4 = hit.collider.material.dynamicFriction;
				num5 = hit.force;
				if (showDebug)
				{
					debug["wheelHit.force"] = hit.force;
					debug["wheelHit.forwardSlip"] = hit.forwardSlip;
					debug["wheelHit.sidewaysSlip"] = hit.sidewaysSlip;
				}
				Vector3 vector = thisTransform.TransformPoint(mWheelCollider.center);
				Vector3 vector2 = vector + Vector3.Dot(-thisTransform.up, hit.point - vector) * -thisTransform.up;
				contactPosition = hit.point + hit.sidewaysDir * skidOffset;
				mContactNormal = hit.normal;
				contactIncidence = Mathf.Abs(Vector3.Dot(thisTransform.up, mContactNormal));
				if (contactIncidence < 0.5f)
				{
					contactIncidence = 0.5f;
				}
				contactIncidence = 0.5f + contactIncidence * 0.5f;
				if ((bool)hit.collider.attachedRigidbody)
				{
					contactWithRigidbody = true;
				}
				airGrip += 0.05f;
				if (airGrip > 1f)
				{
					airGrip = 1f;
				}
			}
			else
			{
				mSidewaysSlip = 0f;
				airGrip = 0.1f;
			}
			float num8 = airGrip * contactIncidence;
			float to = num5 * num8;
			mLoad = Mathf.Lerp(mLoad, to, 0.1f);
			float num9 = Mathf.Max(num2 * 0.75f, num);
			mSlip = Mathf.Lerp(mSlip, num9, 0.1f);
			if (showDebug)
			{
				debug["forwardSlip"] = num;
				debug["sidewaysSlip"] = num2;
				debug["slip"] = num9;
				debug["mSlip"] = mSlip;
			}
			if (driveAmount > 0f)
			{
				float num10 = mMotorTorque / mInertia * Time.fixedDeltaTime;
				float num11 = mBrakeTorque / mInertia * Time.fixedDeltaTime;
				float num12 = mRpm + num10 * (30f / (float)Math.PI);
				float num13 = Mathf.Sign(num12);
				num12 += num13 * (0f - num11) * (30f / (float)Math.PI);
				if (Mathf.Sign(num12) != num13)
				{
					num12 = 0f;
				}
				mRpm = Mathf.Lerp(num12, mWheelCollider.rpm, (1f - mSlip) * mFriction * mFriction);
				if (showDebug)
				{
					debug["mRpm"] = mRpm;
				}
			}
			else
			{
				mRpm = Mathf.Lerp(mWheelCollider.rpm, mRpm, num);
			}
			mRotation += mRpm * ((float)Math.PI / 30f) * Time.fixedDeltaTime;
			mWheelCollider.motorTorque = mMotorTorque;
			mWheelCollider.brakeTorque = mBrakeTorque;
			if (mSlip > 0.1f)
			{
				mFriction = num4;
			}
			else
			{
				mFriction = num3;
			}
			if (mFriction == 0f)
			{
				mFriction = 1f;
			}
			WheelFrictionCurve forwardFriction = mWheelCollider.forwardFriction;
			float num14 = (1f - mSlip * 0.95f) * mFriction * num8;
			if (num14 < 0.05f)
			{
				num14 = 0.05f;
			}
			forwardFriction.stiffness = num14 * originalForwardFriction.stiffness;
			mWheelCollider.forwardFriction = forwardFriction;
			WheelFrictionCurve sidewaysFriction = mWheelCollider.sidewaysFriction;
			num14 = (1f - mSlip * 0.3f) * mFriction * num8;
			if (num14 < 0.05f)
			{
				num14 = 0.05f;
			}
			sidewaysFriction.stiffness = num14 * originalSidewaysFriction.stiffness;
			mWheelCollider.sidewaysFriction = sidewaysFriction;
		}

		private void UpdateWheelHit()
		{
			contactWithRigidbody = false;
			WheelHit hit;
			if (mWheelCollider.GetGroundHit(out hit))
			{
				Vector3 vector = thisTransform.TransformPoint(mWheelCollider.center);
				Vector3 vector2 = vector + Vector3.Dot(-thisTransform.up, hit.point - vector) * -thisTransform.up;
				contactPosition = hit.point + hit.sidewaysDir * skidOffset;
				mContactNormal = hit.normal;
				if ((bool)hit.collider.attachedRigidbody)
				{
					contactWithRigidbody = true;
				}
			}
		}

		private void UpdateSuspensionTravel()
		{
			float radius = mWheelCollider.radius;
			bool flag = false;
			RaycastHit hitInfo;
			if (mWheelCollider.isGrounded && Physics.Raycast(thisTransform.position + mWheelCollider.center.x * thisTransform.right, -thisTransform.up, out hitInfo, radius + mWheelCollider.suspensionDistance, groundHitLayerMask) && !hitInfo.collider.isTrigger)
			{
				flag = true;
				hitContactTime += Time.fixedDeltaTime;
				float num = hitInfo.distance - radius;
				if (num < prevSuspensionTravel)
				{
					suspensionTravel = num;
				}
				else
				{
					suspensionTravel = 0.85f * prevSuspensionTravel + 0.17f * mWheelCollider.suspensionDistance;
					if (suspensionTravel > num)
					{
						suspensionTravel = num;
					}
				}
			}
			if (!flag)
			{
				hitContactTime = 0f;
				suspensionTravel = 0.85f * prevSuspensionTravel + 0.17f * mWheelCollider.suspensionDistance;
				if (suspensionTravel > mWheelCollider.suspensionDistance)
				{
					suspensionTravel = mWheelCollider.suspensionDistance;
				}
			}
			prevSuspensionTravel = suspensionTravel;
		}

		public void RecycleSkidmark()
		{
		}

		private void AddSkidMark(Vector3 rigidbodyVelocityAtContact)
		{
			float num = contactIncidence * contactIncidence;
			Vector3 normalized = rigidbodyVelocityAtContact.normalized;
			if (skidmarkPrefab != null && skidmark == null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(skidmarkPrefab) as GameObject;
				skidmark = gameObject.GetComponent<Skidmark>();
				skidmark.transform.parent = GameObject.Find("SceneDynamic").transform;
			}
			bool isGrounded = mWheelCollider.isGrounded;
			if (skidmark != null)
			{
				Vector3 vector = Vector3.Cross(normalized, mContactNormal);
				Vector3 vector2 = contactPosition + mContactNormal * 0.02f;
				if (!isGrounded)
				{
					vector2 += rigidbodyVelocityAtContact * Time.fixedDeltaTime;
				}
				Vector3 leftEdge = vector2 - vector * skidWidth * 0.5f * num;
				Vector3 rightEdge = vector2 + vector * skidWidth * 0.5f * num;
				float num2 = 0f;
				if (isGrounded && !contactWithRigidbody)
				{
					num2 = mSlip;
					num2 = ((!(num2 < 0.4f)) ? (1.6f * (num2 - 0.4f)) : 0f);
					num2 *= Mathf.Clamp01(mLoad / mMaxSpringLoad);
				}
				skidmark.AddSegment(leftEdge, rightEdge, mContactNormal, num2 * mFriction * mFriction, num);
				if (flaming && (bool)ownerCar && ownerCar.turboBoost && (vector2 - lastFlamePos).sqrMagnitude > 1f)
				{
					DismountGame.particleManager.EmitFlame(vector2, rigidbodyVelocityAtContact);
					lastFlamePos = vector2;
				}
			}
		}

		private void FixedUpdate()
		{
			if (owner != null && !owner.isAwake)
			{
				return;
			}
			UpdateSlip();
			UpdateSuspensionTravel();
			MoveVisuals();
			Vector3 pointVelocity = connectedBody.GetPointVelocity(contactPosition);
			if (!skipSkidMark)
			{
				AddSkidMark(pointVelocity);
			}
			skipSkidMark = false;
			if (noSmoke || smokeSkipCounter++ < 4 || driveAmount <= 0f || mFriction < 0.5f)
			{
				return;
			}
			smokeSkipCounter = 0;
			float num = mRpm / 60f * 2f * (float)Math.PI * mWheelCollider.radius;
			Vector3 vector = num * -mSuspensionArm.forward;
			Vector3 vector2 = pointVelocity + vector;
			if (hitContactTime > 0.5f && mSlip > 0.25f && mFriction > 0.75f && DismountGame.instance != null)
			{
				if (doSkip)
				{
					if (emitSkip++ < 1)
					{
						return;
					}
					emitSkip = 0;
				}
				Vector3 velocity = Vector3.up * UnityEngine.Random.Range(0.5f, 2f) + 0.25f * vector2 + UnityEngine.Random.Range(-2f, 2f) * mSuspensionArm.right;
				DismountGame.particleManager.EmitSmoke(contactPosition, velocity, Mathf.Clamp(mSlip * 1.25f, 0.25f, 1f));
			}
			else
			{
				emitSkip = 0;
			}
		}

		private void LateUpdate()
		{
			if (!(owner != null) || owner.isAwake)
			{
				UpdateWheelHit();
				Vector3 pointVelocity = connectedBody.GetPointVelocity(contactPosition);
				AddSkidMark(pointVelocity);
				skipSkidMark = true;
			}
		}

		private void MoveVisuals()
		{
			Vector3 vector = new Vector3(mRotation * (180f / (float)Math.PI), 0f, 0f);
			mVisualWheel.localEulerAngles = vector;
			mVisualWheelEulers.localEulers = vector;
			mVisualWheel.position = thisTransform.position + suspensionTravel * -thisTransform.up;
			mSuspensionArm.localEulerAngles = new Vector3(0f, mWheelCollider.steerAngle, 0f);
		}

		public void DebugDraw()
		{
			GLDraw.Begin();
			GL.MultMatrix(base.transform.localToWorldMatrix);
			Vector3 center = mWheelCollider.center;
			float suspensionDistance = mWheelCollider.suspensionDistance;
			float radius = mWheelCollider.radius;
			float f = mWheelCollider.steerAngle / 180f * (float)Math.PI;
			Color color = new Color(1f, 0.2f, 0.1f, 0.5f);
			Color color2 = new Color(1f, 1f, 0f, 0.5f);
			Color b = new Color(0.1f, 1f, 0.2f, 0.5f);
			Color color3 = color;
			float num = suspensionTravel / suspensionDistance;
			color3 = ((!(num < 0.5f)) ? Color.Lerp(color2, b, 2f * (num - 0.5f)) : Color.Lerp(color, color2, 2f * num));
			Vector3 vector = new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
			GLDraw.Sector(center, vector * radius, Vector3.up * radius, 0f, 180f, new Color(1f, 1f, 1f, 0.5f), false, 16);
			GLDraw.Sector(center + Vector3.down * suspensionDistance, vector * radius, Vector3.up * radius, 180f, 360f, new Color(1f, 1f, 1f, 0.5f), false, 16);
			GLDraw.Line(center - vector * radius, center - vector * radius + Vector3.down * suspensionDistance, new Color(1f, 1f, 1f, 0.5f));
			GLDraw.Line(center + vector * radius, center + vector * radius + Vector3.down * suspensionDistance, new Color(1f, 1f, 1f, 0.5f));
			float num2 = 0.05f;
			int num3 = (int)(suspensionDistance / num2);
			if ((num3 & 1) != 0)
			{
				num3++;
			}
			num2 = suspensionTravel / (float)num3;
			GLDraw.Line(center, center + vector * 0.05f + 0.5f * num2 * Vector3.down, color3);
			float num4 = 1f;
			for (int i = 0; i < num3 - 1; i++)
			{
				GLDraw.Line(center + num4 * vector * 0.05f + ((float)i + 0.5f) * num2 * Vector3.down, center - num4 * vector * 0.05f + ((float)i + 1.5f) * num2 * Vector3.down, color3);
				num4 *= -1f;
			}
			GLDraw.Line(center + suspensionTravel * Vector3.down, center - vector * 0.05f + (suspensionTravel - 0.5f * num2) * Vector3.down, color3);
			color3.a = 0.25f;
			GLDraw.Ellipse(center + Vector3.down * suspensionTravel, vector * radius, Vector3.up * radius, color3, true, 32);
			GLDraw.Ellipse(center + Vector3.down * suspensionTravel, -vector * radius, Vector3.up * radius, color3, true, 32);
			color3.a = 0.5f;
			GLDraw.Ellipse(center + Vector3.down * suspensionTravel, vector * radius, Vector3.up * radius, color3, false, 32);
			GLDraw.End();
		}
	}
}
