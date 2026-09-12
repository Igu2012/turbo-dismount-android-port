#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;
using reLive;

namespace Dismount.Vehicular
{
	public class AltWheel : MonoBehaviour
	{
		[Serializable]
		public class SuspensionParams
		{
			public float Spring = 20000f;

			public float Damper = 2000f;

			public float Distance = 0.3f;
		}

		[Serializable]
		public class FrictionCurve
		{
			public AnimationCurve FrictionOverSlip;

			public float Factor = 1f;

			public float Evaluate(float slip)
			{
				return FrictionOverSlip.Evaluate(Mathf.Abs(slip)) * Factor;
			}
		}

		[Range(0f, 1f)]
		public float DriveAmount;

		[Range(0f, 1f)]
		public float BrakeAmount = 1f;

		[Range(-90f, 90f)]
		public float SteerLock;

		[Range(0f, 1f)]
		public float GyroAmount;

		public float Mass = 10f;

		public float Radius = 0.3f;

		public float Width = 0.2f;

		public SuspensionParams Suspension;

		public FrictionCurve ForwardFriction;

		public FrictionCurve SidewaysFriction;

		public bool ShowDebug;

		private Dictionary<string, object> debug = new Dictionary<string, object>();

		private Color mGizmoColor = Color.green;

		private Transform mVisualWheel;

		private ExtendedLocalEulers mVisualWheelEulers;

		private Transform mSuspensionArm;

		private float mSuspensionCompression;

		private float mPrevSuspensionCompression;

		private float mVisualSuspensionTravel;

		private float mPrevVisualSuspensionTravel;

		private float mOriginalForwardFrictionFactor;

		private float mOriginalSidewaysFrictionFactor;

		private float mHitContactTime;

		private Vehicle owner;

		private int mSmokeSkipCounter;

		private float mRpm;

		private float mAngularVelocity;

		private float mAngularVelocityDifferential;

		private float mRotation;

		private float mInertia;

		private float mMotorTorque;

		private float mBrakeTorque;

		private float mSteerAngle;

		private float mStaticFriction;

		private float mDynamicFriction;

		private float mFriction;

		private float mForwardSlip;

		private float mSidewaysSlip;

		private float mCombinedSlip;

		private float mSpringForce;

		private float mDamperForce;

		private float mAntiRollBarForce;

		private float mLoad;

		private float mMaxSpringLoad = 1f;

		private bool mIsGrounded;

		private Transform mDummy;

		private RaycastHit mGroundHit;

		private int groundHitLayerMask = 512;

		public float gyroForce
		{
			get
			{
				return mAngularVelocity * Mass * GyroAmount;
			}
		}

		public bool hasContact
		{
			get
			{
				return mIsGrounded;
			}
		}

		public float contactTime
		{
			get
			{
				return mHitContactTime;
			}
		}

		public float rpm
		{
			get
			{
				return mRpm;
			}
		}

		public float angularVelocity
		{
			get
			{
				return mAngularVelocity;
			}
		}

		public float angularVelocityDifferential
		{
			get
			{
				return mAngularVelocityDifferential;
			}
			set
			{
				mAngularVelocityDifferential = value;
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
				return mCombinedSlip;
			}
		}

		public float steerAngle
		{
			get
			{
				return mSteerAngle;
			}
			set
			{
				mSteerAngle = value;
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

		public float antiRollBarForce
		{
			get
			{
				return mAntiRollBarForce;
			}
			set
			{
				mAntiRollBarForce = value;
			}
		}

		public float suspensionCompression
		{
			get
			{
				return mSuspensionCompression;
			}
		}

		public float surfaceFriction
		{
			get
			{
				return mFriction;
			}
		}

		private void Awake()
		{
			if (!base.gameObject.GetComponent<RetainHierarchy>())
			{
				base.gameObject.AddComponent<RetainHierarchy>();
			}
			mDummy = base.transform.Find("Dummy");
			if (!mDummy)
			{
				mDummy = new GameObject("Dummy").transform;
				mDummy.parent = base.transform;
				mDummy.position = base.transform.position;
				mDummy.rotation = base.transform.rotation;
			}
			mSuspensionArm = base.transform.Find("SuspensionArm");
			mVisualWheel = mSuspensionArm.GetChild(0);
			mVisualWheelEulers = mVisualWheel.gameObject.GetComponent<ExtendedLocalEulers>();
			if (!mVisualWheelEulers)
			{
				mVisualWheelEulers = mVisualWheel.gameObject.AddComponent<ExtendedLocalEulers>();
			}
			mInertia = Mass * Radius;
			groundHitLayerMask = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Character"));
			Transform parent = base.transform;
			while (!owner && (bool)parent)
			{
				Vehicle component = parent.GetComponent<Vehicle>();
				if ((bool)component)
				{
					owner = component;
					break;
				}
				parent = parent.parent;
			}
			if (!owner)
			{
				Debug.LogError("Wheel needs to be under a Vehicle");
			}
			mOriginalForwardFrictionFactor = ForwardFriction.Factor;
			mOriginalSidewaysFrictionFactor = SidewaysFriction.Factor;
			mMaxSpringLoad = Suspension.Distance * Suspension.Spring;
		}

		private void FixedUpdate()
		{
			if (!owner.isAwake)
			{
				return;
			}
			mDummy.localPosition = Vector3.zero;
			mDummy.localEulerAngles = new Vector3(0f, mSteerAngle, 0f);
			Vector3 position = base.transform.position;
			Vector3 forward = mDummy.forward;
			Vector3 up = mDummy.up;
			Vector3 right = mDummy.right;
			Vector3[] array = new Vector3[5];
			float num = -20f;
			for (int i = 0; i < 5; i++)
			{
				array[i] = (Mathf.Cos(num) * -up + Mathf.Sin(num) * forward) * Radius;
				num += 10f;
			}
			Vector3[] array2 = new Vector3[8]
			{
				position + array[1] + up * 0.1f,
				position + array[3] + up * 0.1f,
				position + right * Width * 0.5f + array[0] + up * 0.1f,
				position + right * Width * 0.5f + array[2] + up * 0.1f,
				position + right * Width * 0.5f + array[4] + up * 0.1f,
				position - right * Width * 0.5f + array[0] + up * 0.1f,
				position - right * Width * 0.5f + array[2] + up * 0.1f,
				position - right * Width * 0.5f + array[4] + up * 0.1f
			};
			Vector3 vector = position;
			Vector3 vector2 = forward;
			float num2 = Suspension.Distance + 1f;
			for (int j = 0; j < array2.Length; j++)
			{
				RaycastHit hitInfo;
				if (Physics.Raycast(array2[j], -up, out hitInfo, Suspension.Distance + 0.1f, groundHitLayerMask) && !(hitInfo.distance > num2) && !hitInfo.collider.isTrigger)
				{
					mGroundHit = hitInfo;
					num2 = hitInfo.distance;
				}
			}
			if (num2 <= Suspension.Distance + 0.1f)
			{
				float num3 = mGroundHit.distance - 0.1f;
				if (num3 < 0f)
				{
					num3 = 0f;
				}
				vector = mGroundHit.point;
				vector2 = -Vector3.Cross((position - up * num3 - vector).normalized, right);
				mIsGrounded = true;
				mHitContactTime += Time.fixedDeltaTime;
				mSuspensionCompression = Suspension.Distance - num3;
				if (mSuspensionCompression >= Suspension.Distance)
				{
					mSuspensionCompression = Suspension.Distance;
					mGizmoColor = Color.red;
				}
				else if (mSuspensionCompression >= Suspension.Distance * 0.5f)
				{
					float g = (Suspension.Distance - mSuspensionCompression) / (Suspension.Distance * 0.5f);
					mGizmoColor = new Color(1f, g, 0f, 1f);
				}
				else
				{
					float r = mSuspensionCompression / (Suspension.Distance * 0.5f);
					mGizmoColor = new Color(r, 1f, 0f, 1f);
				}
				mSpringForce = mSuspensionCompression * Suspension.Spring;
				mDamperForce = (0f - (mSuspensionCompression - mPrevSuspensionCompression)) / Time.fixedDeltaTime * Suspension.Damper;
				mDynamicFriction = mGroundHit.collider.material.dynamicFriction;
				mStaticFriction = mGroundHit.collider.material.staticFriction;
				float num4 = num3;
				if (num4 < mPrevVisualSuspensionTravel)
				{
					mVisualSuspensionTravel = num4;
				}
				else
				{
					mVisualSuspensionTravel = 0.85f * mPrevVisualSuspensionTravel + 0.17f * Suspension.Distance;
					if (mVisualSuspensionTravel > num4)
					{
						mVisualSuspensionTravel = num4;
					}
				}
			}
			else
			{
				mIsGrounded = false;
				mHitContactTime = 0f;
				mGizmoColor = Color.blue;
				mSuspensionCompression = 0f;
				mSpringForce = 0f;
				mDamperForce = (0f - (mSuspensionCompression - mPrevSuspensionCompression)) / Time.fixedDeltaTime * Suspension.Damper;
				mDynamicFriction = (mStaticFriction = (mFriction = 0f));
				mForwardSlip = (mSidewaysSlip = (mCombinedSlip = 0f));
				mVisualSuspensionTravel = 0.85f * mPrevVisualSuspensionTravel + 0.17f * Suspension.Distance;
				if (mVisualSuspensionTravel > Suspension.Distance)
				{
					mVisualSuspensionTravel = Suspension.Distance;
				}
			}
			mPrevSuspensionCompression = mSuspensionCompression;
			mPrevVisualSuspensionTravel = mVisualSuspensionTravel;
			if (ShowDebug)
			{
				debug["mSuspensionCompression"] = mSuspensionCompression;
				debug["mVisualSuspensionTravel"] = mVisualSuspensionTravel;
				debug["mSpringForce"] = mSpringForce;
				debug["mDamperForce"] = mDamperForce;
				debug["mAntiRollBarForce"] = mAntiRollBarForce;
			}
			mAngularVelocity += mMotorTorque / mInertia * Time.fixedDeltaTime;
			mAngularVelocity += mAngularVelocityDifferential * 50f * Time.fixedDeltaTime;
			mAngularVelocity -= Mathf.Sign(mAngularVelocity) * Mathf.Min(Mathf.Abs(mAngularVelocity), mBrakeTorque / mInertia * Time.fixedDeltaTime);
			mRpm = mAngularVelocity * (30f / (float)Math.PI);
			if (ShowDebug)
			{
				debug["mMotorTorque"] = mMotorTorque;
				debug["mBrakeTorque"] = mBrakeTorque;
				debug["mAngularVelocity1"] = mAngularVelocity;
				debug["mRpm"] = mRpm;
			}
			if (mIsGrounded)
			{
				Vector3 pointVelocity = owner.GetComponent<Rigidbody>().GetPointVelocity(vector);
				pointVelocity -= up * Vector3.Dot(pointVelocity, up);
				float magnitude = pointVelocity.magnitude;
				float num5 = mAngularVelocity * Radius;
				Vector3 vector3 = num5 * -vector2 + pointVelocity;
				if (magnitude < 0.05f)
				{
					magnitude = 0.05f;
				}
				float num6 = Vector3.Dot(vector3, vector2);
				float num7 = Vector3.Dot(vector3, right);
				mForwardSlip = mForwardSlip * 0.5f + num6 * 0.5f;
				mSidewaysSlip = mSidewaysSlip * 0.5f + num7 * 0.5f;
				mCombinedSlip = Mathf.Sqrt(mForwardSlip * mForwardSlip + mSidewaysSlip * mSidewaysSlip);
				if (mCombinedSlip > 0.5f)
				{
					mFriction = mDynamicFriction;
				}
				else
				{
					mFriction = mStaticFriction;
				}
				if (mFriction == 0f)
				{
					mFriction = 1f;
				}
				ForwardFriction.Factor = mFriction * mOriginalForwardFrictionFactor;
				SidewaysFriction.Factor = mFriction * mOriginalSidewaysFrictionFactor;
				if (ShowDebug)
				{
					debug["rigidbodyVelocityAtContact"] = pointVelocity;
					debug["wheelSpeedAtContact"] = num5;
					debug["slipVelocity"] = vector3;
					debug["mForwardSlip"] = mForwardSlip;
					debug["mSidewaysSlip"] = mSidewaysSlip;
					debug["mCombinedSlip"] = mCombinedSlip;
					debug["mFriction"] = mFriction;
				}
				if (mSmokeSkipCounter++ >= 4 && DriveAmount > 0f && mFriction >= 0.5f)
				{
					mSmokeSkipCounter = 0;
					if (mHitContactTime > 0.5f && mCombinedSlip > 0.5f && DismountGame.instance != null)
					{
						Vector3 velocity = Vector3.up * UnityEngine.Random.Range(0.5f, 2f) + 0.25f * vector3 + UnityEngine.Random.Range(-2f, 2f) * right;
						DismountGame.particleManager.EmitSmoke(vector, velocity, Mathf.Clamp(mCombinedSlip * 0.1f, 0.25f, 1f));
					}
				}
				float num8 = mSpringForce - mDamperForce;
				Vector3 vector4 = up * num8 - up * mAntiRollBarForce * 0.25f;
				owner.GetComponent<Rigidbody>().AddForceAtPosition(vector4, base.transform.position, ForceMode.Force);
				float num9 = mSpringForce - mDamperForce - mAntiRollBarForce * 0.3f;
				mLoad = mLoad * 0.5f + num9 * 0.5f;
				float num10 = ForwardFriction.Evaluate(mCombinedSlip);
				Vector3 vector5 = -vector2 * Mathf.Sign(mForwardSlip) * num10 * mLoad;
				float num11 = SidewaysFriction.Evaluate(mCombinedSlip);
				Vector3 vector6 = -right * Mathf.Sign(mSidewaysSlip) * num11 * mLoad;
				owner.GetComponent<Rigidbody>().AddForceAtPosition(vector5 + vector6, vector, ForceMode.Force);
				float num12 = Mathf.Clamp((50f - owner.GetComponent<Rigidbody>().velocity.magnitude) / 25f, 0.5f, 1f);
				float num13 = Mathf.Sign(mForwardSlip) * ForwardFriction.Evaluate(mCombinedSlip) * mLoad * Radius * num12;
				mAngularVelocity += num13 / mInertia * Time.fixedDeltaTime;
				if (ShowDebug)
				{
					debug["springForce"] = num8;
					debug["mLoad"] = mLoad;
					debug["upForce"] = vector4;
					debug["forwardFriction"] = num10;
					debug["forwardFrictionForce"] = vector5;
					debug["sidewaysFriction"] = num11;
					debug["sidewaysFrictionForce"] = vector6;
					debug["frictionTorque"] = num13;
					debug["mAngularVelocity2"] = mAngularVelocity;
				}
			}
			MoveVisuals();
		}

		private void MoveVisuals()
		{
			mRotation += mAngularVelocity * Time.fixedDeltaTime;
			Vector3 vector = new Vector3(mRotation * (180f / (float)Math.PI), 0f, 0f);
			mVisualWheel.localEulerAngles = vector;
			mVisualWheelEulers.localEulers = vector;
			mVisualWheel.position = base.transform.position + mVisualSuspensionTravel * -base.transform.up;
			mSuspensionArm.localEulerAngles = new Vector3(0f, mSteerAngle, 0f);
		}

		private void OnDrawGizmosSelected()
		{
			Transform transform = mDummy;
			if (!transform)
			{
				transform = base.transform;
			}
			Gizmos.color = mGizmoColor;
			Vector3 vector = -transform.up * (Suspension.Distance - mSuspensionCompression);
			Vector3 vector2 = transform.up * Radius;
			Vector3 vector3 = -transform.right * Width * 0.5f;
			Vector3 vector4 = transform.right * Width * 0.5f;
			Gizmos.DrawLine(base.transform.position + vector3 + vector2, base.transform.position + vector3 + vector2 + vector);
			Gizmos.DrawLine(base.transform.position - vector4 + vector2, base.transform.position - vector4 + vector2 + vector);
			Vector3 vector5 = transform.TransformPoint(Radius * new Vector3(0f, Mathf.Sin(0f), Mathf.Cos(0f)));
			for (int i = 1; i <= 32; i++)
			{
				Vector3 vector6 = transform.TransformPoint(Radius * new Vector3(0f, Mathf.Sin((float)i / 32f * (float)Math.PI * 2f), Mathf.Cos((float)i / 32f * (float)Math.PI * 2f)));
				Gizmos.DrawLine(vector5 + vector3, vector6 + vector3);
				Gizmos.DrawLine(vector5 + vector4, vector6 + vector4);
				Gizmos.DrawLine(vector5 + vector3 + vector, vector6 + vector3 + vector);
				Gizmos.DrawLine(vector5 + vector4 + vector, vector6 + vector4 + vector);
				vector5 = vector6;
			}
			Gizmos.color = Color.white;
		}

		private void OnGUI()
		{
			if (!ShowDebug)
			{
				return;
			}
			GUI.Box(new Rect(0f, 40f, 320f, 540f), "Wheel");
			float num = 60f;
			foreach (string key in debug.Keys)
			{
				object obj = debug[key];
				string empty = string.Empty;
				empty = ((obj.GetType() != typeof(float)) ? obj.ToString() : Utils.NumberString((float)obj, 3));
				GUI.Label(new Rect(10f, num, Screen.width - 20, 25f), string.Empty + key + ": " + empty);
				num += 20f;
			}
		}
	}
}
