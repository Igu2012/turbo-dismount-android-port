#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using Dismount;
using Dismount.GameStates;
using UnityEngine;
using reLive;

[RequireComponent(typeof(Rigidbody))]
public class BodyPart : MonoBehaviour
{
	public enum Type
	{
		Head = 0,
		Torso = 1,
		Limb = 2
	}

	public enum Hit
	{
		Impact = 0,
		JointLimitBreak = 1,
		Detach = 2,
		Slide = 3
	}

	public enum Pain
	{
		None = 0,
		HeadHit = 1,
		HeadCracked = 2,
		HeadCrackedMajor = 3,
		NeckBroken = 4,
		HeadDetached = 5,
		HeadSlide = 6,
		LimbHit = 7,
		LimbBroken = 8,
		LimbBrokenMajor = 9,
		LimbJointBroken = 10,
		LimbDetached = 11,
		LimbSlide = 12,
		ChestHit = 13,
		ChestBroken = 14,
		ChestBrokenMajor = 15,
		PelvisBrokenMajor = 16,
		SpineBroken = 17,
		SpineDetached = 18,
		TorsoSlide = 19,
		MinorHit = 20,
		RanOver = 21,
		ThroughWindow = 22,
		Flip = 23,
		Airtime = 24
	}

	private const float collisionRepeatThresholdCount = 3f;

	private const float collisionRepeatThresholdTime = 0.03f;

	public GameObject decal;

	public MrDismount.Part part;

	public float minimumPuffImpulse = 10f;

	public Type type;

	public float attachForce;

	public string attachRigidbodyName = string.Empty;

	public float jointDetachForce = float.PositiveInfinity;

	public float jointLimitBreakForce = float.PositiveInfinity;

	private int slideLayer;

	private int slideCounter;

	private MrDismount root;

	private CharacterJoint joint;

	private FixedJoint vehiclePartJoint;

	[HideInInspector]
	public bool isConnectedToVehicle;

	private float impactStartTime = -1f;

	private float impactDisplayTime = 0.3f;

	private bool isDismounted;

	private float scoreMultiplier = 1f;

	private int debugIsInCollision;

	private int flipCount;

	private float flipDot = 1f;

	private Vector3 flipUp = Vector3.up;

	private float lastDamageTime;

	private int lastDamageSeverity;

	private int penetrationCheckIgnoreLayers;

	private int groundContactIgnoreLayers;

	private int vehicleLayer;

	private float headPainStartTime = -1f;

	private float headPainDuration = 1f;

	private float headPainMagnitude;

	[NonSerialized]
	public bool isStatsNode;

	public bool isConnectedToStatsNode = true;

	public BodyPart physicsParent;

	public List<BodyPart> physicsChildren = new List<BodyPart>();

	private static Dismount.GameStates.Dismount gameLogic;

	private Vector4[] lastCollisions = new Vector4[8];

	private int lastCollisionsWriteHead;

	private GameObject impact;

	private float jointBreakStretch = 1f;

	private AverageFloat jointStretch;

	private int jointStretchCoolDownFrames = 100;

	private Transform jointConnectedTransform;

	private float impactScoreWindow = -1f;

	private int prevImpactSeverity;

	private static float previousCraterTime = -1f;

	private Collider penetrationCollider;

	private Vector3 penetrationCollisionNormal;

	private int penetrationCollisionCounter;

	private float lastPenetrationCheckTime = -1f;

	public void SetRoot(MrDismount newRoot)
	{
		root = newRoot;
		gameLogic = root.gameLogic;
	}

	public void SetPhysicsParent(BodyPart part)
	{
		physicsParent = part;
	}

	public void AddPhysicsChild(BodyPart part)
	{
		physicsChildren.Add(part);
	}

	public void RemovePhysicsChild(BodyPart part)
	{
		physicsChildren.Remove(part);
	}

	public bool FindStatsNodeRecursive()
	{
		if (isStatsNode)
		{
			return true;
		}
		foreach (BodyPart physicsChild in physicsChildren)
		{
			if (physicsChild.FindStatsNodeRecursive())
			{
				return true;
			}
		}
		return false;
	}

	public void SetStatsNodeConnectionRecursive(bool connected)
	{
		isConnectedToStatsNode = connected;
		foreach (BodyPart physicsChild in physicsChildren)
		{
			physicsChild.SetStatsNodeConnectionRecursive(connected);
		}
	}

	public void DetachFromVehicleRecursive()
	{
		if (vehiclePartJoint != null)
		{
			UnityEngine.Object.Destroy(vehiclePartJoint);
			isConnectedToVehicle = false;
		}
		foreach (BodyPart physicsChild in physicsChildren)
		{
			physicsChild.DetachFromVehicleRecursive();
		}
	}

	private void Awake()
	{
		if (!impact)
		{
			impact = Resources.Load("Impact") as GameObject;
		}
		for (int i = 0; i < lastCollisions.Length; i++)
		{
			lastCollisions[i] = Vector4.zero;
		}
		joint = GetComponent<CharacterJoint>();
		if ((bool)joint)
		{
			jointConnectedTransform = joint.connectedBody.transform;
			joint.breakForce = float.PositiveInfinity;
			joint.breakTorque = float.PositiveInfinity;
			if (!GetComponent<JointFriction>())
			{
				JointFriction jointFriction = base.gameObject.AddComponent<JointFriction>();
				jointFriction.dampingTime = 0.1f;
				jointFriction.velocityThreshold = 0.01f;
				if (part == MrDismount.Part.LeftFoot || part == MrDismount.Part.RightFoot || part == MrDismount.Part.LeftHand || part == MrDismount.Part.RightHand)
				{
					jointFriction.dampingTime = 0.05f;
				}
				else if (part == MrDismount.Part.Neck || part == MrDismount.Part.Head)
				{
					jointFriction.dampingTime = 0.05f;
				}
			}
			physicsParent = joint.connectedBody.GetComponent<BodyPart>();
			if ((bool)physicsParent)
			{
				physicsParent.AddPhysicsChild(this);
			}
		}
		jointBreakStretch = 0.25f;
		int frameCount = 5;
		if (part == MrDismount.Part.LeftFoot || part == MrDismount.Part.RightFoot || part == MrDismount.Part.LeftHand || part == MrDismount.Part.RightHand)
		{
			jointBreakStretch = 0.3f;
			frameCount = 5;
		}
		else if (part == MrDismount.Part.Neck)
		{
			jointBreakStretch = 0.2f;
			frameCount = 5;
		}
		else if (part == MrDismount.Part.LeftLeg || part == MrDismount.Part.RightLeg || part == MrDismount.Part.LeftArm || part == MrDismount.Part.RightArm)
		{
			jointBreakStretch = 0.15f;
			frameCount = 4;
		}
		jointStretch = new AverageFloat(frameCount);
		penetrationCheckIgnoreLayers = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Character"));
		groundContactIgnoreLayers = (1 << LayerMask.NameToLayer("Vehicle")) | (1 << LayerMask.NameToLayer("Character"));
		vehicleLayer = LayerMask.NameToLayer("Vehicle");
		slideLayer = LayerMask.NameToLayer("Ground");
		if (scoreMultiplier == 1f)
		{
			if (type == Type.Head)
			{
				scoreMultiplier = 5f;
			}
			else if (type == Type.Torso)
			{
				scoreMultiplier = 3f;
			}
			else if (type == Type.Limb)
			{
				scoreMultiplier = 1f;
			}
		}
	}

	public void SetGameLogic(Dismount.GameStates.Dismount dismount)
	{
		gameLogic = dismount;
	}

	private void SetBreakable()
	{
		if ((bool)joint)
		{
			joint.breakForce = jointLimitBreakForce;
			joint.breakTorque = jointLimitBreakForce;
		}
	}

	public void AttachToVehicle(GameObject vehicle)
	{
		if (attachForce <= 0f)
		{
			return;
		}
		Rigidbody rigidbody = vehicle.GetComponent<Rigidbody>();
		if (!string.IsNullOrEmpty(attachRigidbodyName))
		{
			Transform transform = vehicle.transform;
			if (attachRigidbodyName.Contains("Parts/"))
			{
				Transform transform2 = vehicle.transform.Find(attachRigidbodyName);
				if ((bool)transform2)
				{
					Transform transform3 = Utils.FindParent("Parts", transform2);
					if ((bool)transform3 && (bool)transform3.parent)
					{
						transform = transform3.parent;
					}
				}
			}
			else
			{
				transform = vehicle.transform.Find(attachRigidbodyName);
			}
			if ((bool)transform && (bool)transform.GetComponent<Rigidbody>())
			{
				rigidbody = transform.GetComponent<Rigidbody>();
			}
			else
			{
				Debug.LogWarning(root.name + ": BodyPart " + base.name + " could not find rigidbody to attach to: " + attachRigidbodyName);
			}
		}
		isConnectedToVehicle = true;
		vehiclePartJoint = rigidbody.gameObject.AddComponent<FixedJoint>();
		vehiclePartJoint.anchor = rigidbody.transform.InverseTransformPoint(base.transform.position);
		vehiclePartJoint.connectedBody = base.GetComponent<Rigidbody>();
		vehiclePartJoint.breakForce = attachForce;
	}

	public void OnDismountStarted()
	{
		if (isStatsNode)
		{
			flipDot = 1f;
			flipUp = base.transform.up;
		}
		isDismounted = true;
	}

	private void LateUpdate()
	{
		if (impactStartTime > 0f)
		{
			float num = Time.time - impactStartTime;
			float a = 0f;
			if (num < impactDisplayTime)
			{
				a = ((!(num < impactDisplayTime / 2f)) ? (1f - (num - impactDisplayTime / 2f) / (impactDisplayTime / 2f)) : (num / (impactDisplayTime / 2f)));
			}
			else
			{
				impactStartTime = -1f;
			}
			Color color = base.GetComponent<Renderer>().material.GetColor("_ImpactColor");
			color.a = a;
			base.GetComponent<Renderer>().material.SetColor("_ImpactColor", color);
		}
	}

	private void FixedUpdate()
	{
		if (isConnectedToVehicle && (vehiclePartJoint == null || vehiclePartJoint.connectedBody == null))
		{
			isConnectedToVehicle = false;
		}
		if (isConnectedToStatsNode && isConnectedToVehicle)
		{
			root.lastVehicleAttachTime = Time.fixedTime;
			root.lastVehicleContactTime = Time.fixedTime;
		}
		if (base.GetComponent<Rigidbody>().IsSleeping())
		{
			root.lastGroundContactTime = Time.fixedTime;
		}
		if (joint != null && --jointStretchCoolDownFrames < 0)
		{
			Vector3 vector = base.transform.TransformPoint(joint.anchor);
			Vector3 vector2 = jointConnectedTransform.TransformPoint(joint.connectedAnchor);
			float magnitude = (vector - vector2).magnitude;
			float num = jointStretch.Add(magnitude);
			if (num > jointBreakStretch)
			{
				BreakJoint();
				int severity = 0;
				int bonusScore = 0;
				Pain pain = GetPain(0f, Hit.Detach, out severity, out bonusScore);
				if (gameLogic != null)
				{
					gameLogic.AddImpactScore(0, bonusScore, Dismount.GameStates.Dismount.ScoreCategory.Ragdoll);
				}
				AddDamageIcon(pain, severity, bonusScore);
				root.PlaySoundEffect(pain, base.transform);
			}
		}
		if (type == Type.Head && headPainStartTime >= 0f)
		{
			float num2 = Time.fixedTime - headPainStartTime;
			if (num2 > headPainDuration)
			{
				headPainStartTime = -1f;
				headPainMagnitude = 0f;
				DismountGame.playerState.statistics.realtime.headPain = 0f;
			}
			else
			{
				DismountGame.playerState.statistics.realtime.headPain = (1f - num2 / headPainDuration) * headPainMagnitude;
			}
		}
		if (!isStatsNode)
		{
			return;
		}
		float f = Vector3.Dot(base.transform.up, flipUp);
		if (Mathf.Sign(f) != Mathf.Sign(flipDot) && Mathf.Abs(f) > 0.5f)
		{
			flipCount++;
			if (flipCount % 2 == 0 && physicsChildren.Count > 0 && physicsParent != null && (bool)gameLogic && gameLogic.isDismountActive)
			{
				gameLogic.AddImpactScore(0, 50000, Dismount.GameStates.Dismount.ScoreCategory.Ragdoll);
				root.AddDamageIcon(Pain.Flip, 50000);
				DismountGame.playerState.statistics.CharacterFlipped();
			}
			flipDot = f;
		}
	}

	private Pain GetPain(float impactVelocity, Hit hit, out int severity, out int bonusScore)
	{
		Pain result = Pain.MinorHit;
		severity = 1;
		bonusScore = 0;
		switch (hit)
		{
		case Hit.Detach:
			if (part == MrDismount.Part.Neck)
			{
				bonusScore = 50000;
				severity = 5;
				return Pain.HeadDetached;
			}
			switch (type)
			{
			case Type.Head:
				bonusScore = 50000;
				severity = 5;
				return Pain.HeadDetached;
			case Type.Limb:
				bonusScore = 25000;
				severity = 4;
				return Pain.LimbDetached;
			default:
				bonusScore = 50000;
				severity = 5;
				return Pain.SpineDetached;
			}
		case Hit.Slide:
			switch (type)
			{
			case Type.Head:
				severity = 2;
				return Pain.HeadSlide;
			case Type.Limb:
				severity = 1;
				return Pain.LimbSlide;
			case Type.Torso:
				severity = 2;
				return Pain.TorsoSlide;
			default:
				severity = 0;
				return Pain.None;
			}
		case Hit.JointLimitBreak:
			switch (type)
			{
			case Type.Head:
				severity = 4;
				return Pain.NeckBroken;
			case Type.Limb:
				severity = 3;
				return Pain.LimbJointBroken;
			case Type.Torso:
				severity = 3;
				return Pain.SpineBroken;
			default:
				severity = 0;
				return Pain.None;
			}
		default:
			if (impactVelocity > 30f)
			{
				severity = 3;
			}
			else if (impactVelocity > 20f)
			{
				severity = 2;
			}
			else if (impactVelocity > 10f)
			{
				severity = 1;
			}
			switch (type)
			{
			case Type.Head:
				if (impactVelocity > 30f)
				{
					result = Pain.HeadCrackedMajor;
				}
				else if (impactVelocity > 20f)
				{
					result = Pain.HeadCracked;
				}
				else if (impactVelocity > 10f)
				{
					result = Pain.HeadHit;
				}
				severity += 2;
				break;
			case Type.Torso:
				if (impactVelocity > 30f)
				{
					result = ((part != MrDismount.Part.Hips) ? Pain.ChestBrokenMajor : Pain.PelvisBrokenMajor);
				}
				else if (impactVelocity > 20f)
				{
					result = Pain.ChestBroken;
				}
				else if (impactVelocity > 10f)
				{
					result = Pain.ChestHit;
				}
				severity++;
				break;
			case Type.Limb:
				if (impactVelocity > 30f)
				{
					result = Pain.LimbBrokenMajor;
				}
				else if (impactVelocity > 20f)
				{
					result = Pain.LimbBroken;
				}
				else if (impactVelocity > 10f)
				{
					result = Pain.LimbHit;
				}
				break;
			}
			return result;
		}
	}

	private int GetImpactScore(float impactVelocity, Collision collision)
	{
		int result = 0;
		if (impactVelocity > 3f)
		{
			float num = 1000f;
			if ((bool)collision.rigidbody)
			{
				num = Mathf.Clamp(collision.rigidbody.mass, 1f, 1000f);
			}
			if (impactVelocity > 25f)
			{
				impactVelocity = 25f;
			}
			result = (int)(impactVelocity * num * 0.05f * scoreMultiplier) * 10;
		}
		return result;
	}

	private void BreakJoint()
	{
		if ((bool)joint)
		{
			if ((bool)physicsParent)
			{
				physicsParent.RemovePhysicsChild(this);
			}
			physicsParent = null;
			UnityEngine.Object.Destroy(joint);
			JointFriction component = GetComponent<JointFriction>();
			if ((bool)component)
			{
				UnityEngine.Object.Destroy(component);
			}
			base.transform.parent = root.transform;
			root.BodyPartDetached(this);
			if (!isConnectedToStatsNode)
			{
				DetachFromVehicleRecursive();
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (base.GetComponent<Rigidbody>().isKinematic || !isDismounted || collision.contacts.Length <= 0)
		{
			return;
		}
		Collider collider = collision.collider;
		Vector3 vector = collision.contacts[0].normal;
		if (collision.contacts[0].thisCollider == collider)
		{
			vector = collision.contacts[0].normal * -1f;
		}
		int layer = collider.gameObject.layer;
		int num = 1 << layer;
		if (isConnectedToStatsNode)
		{
			if ((num & groundContactIgnoreLayers) == 0)
			{
				root.lastGroundContactTime = Time.fixedTime;
			}
			if (num == vehicleLayer)
			{
				root.lastVehicleContactTime = Time.fixedTime;
			}
		}
		float num2 = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, collision.contacts[0].normal));
		bool flag = CheckRepeatedCollisionEnter(collision, num, num2);
		if (collision.collider == penetrationCollider)
		{
			CheckCollisionForPenetration(collision);
		}
		else if (collision.relativeVelocity.sqrMagnitude > 100f)
		{
			RegisterCollisionForPenetrationCheck(collision, num);
		}
		float num3 = num2 * num2;
		if (num3 > minimumPuffImpulse)
		{
			Vector3 point = collision.contacts[0].point;
			Vector3 velocity = Vector3.up * UnityEngine.Random.Range(0.25f, 1f) + 0.025f * base.GetComponent<Rigidbody>().GetPointVelocity(point);
			DismountGame.particleManager.EmitDust(point, velocity);
			if (type == Type.Head)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(impact, base.transform.position, Quaternion.LookRotation(collision.contacts[0].normal)) as GameObject;
				gameObject.transform.parent = base.transform;
				gameObject.AddComponent<RecordLossyScale>();
				gameObject.AddComponent<RecordMeInReplay>();
			}
			if (layer == 9 && Time.fixedTime - previousCraterTime > 0.1f)
			{
				DismountGame.particleManager.EmitRocks(collision.contacts[0].point);
				Vector3 vector2 = vector;
				if (Mathf.Abs(vector2.y) > Mathf.Abs(vector2.x) && Mathf.Abs(vector2.y) > Mathf.Abs(vector2.z))
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(decal, collision.contacts[0].point + vector2 * 0.08f, Quaternion.LookRotation(vector2)) as GameObject;
					gameObject2.transform.parent = collider.transform;
					gameObject2.AddComponent<RecordLossyScale>();
					gameObject2.AddComponent<RecordMeInReplay>();
				}
				previousCraterTime = Time.fixedTime;
			}
		}
		if (impactStartTime < 0f && num3 > 5f)
		{
			impactStartTime = Time.time;
		}
		if (type == Type.Head && num3 > 10f)
		{
			headPainStartTime = Time.fixedTime;
			float num4 = Mathf.Clamp01((num3 + 20f) / 100f);
			if (num4 > headPainMagnitude)
			{
				headPainMagnitude = num4;
			}
			headPainDuration = headPainMagnitude * 3f;
			DismountGame.playerState.statistics.realtime.headPain = headPainMagnitude;
		}
		if (num3 < 5f)
		{
			return;
		}
		Hit hit = Hit.Impact;
		Pain pain = Pain.None;
		if (num3 > jointLimitBreakForce && joint != null)
		{
			SoftJointLimit swing1Limit = new SoftJointLimit
			{
				limit = 160f
			};
			joint.swing1Limit = swing1Limit;
			hit = Hit.JointLimitBreak;
		}
		if ((bool)gameLogic)
		{
			int num5 = 0;
			if (!flag)
			{
				num5 = GetImpactScore(num2, collision);
			}
			int severity = 0;
			int bonusScore = 0;
			pain = GetPain(num3, hit, out severity, out bonusScore);
			if (Time.fixedTime > impactScoreWindow)
			{
				prevImpactSeverity = -1;
				AddDamageIcon(pain, severity, num5 + bonusScore);
				root.PlaySoundEffect(pain, base.transform);
			}
			else if (severity <= prevImpactSeverity)
			{
				num5 = 0;
			}
			else if (severity > prevImpactSeverity || bonusScore > 0)
			{
				AddDamageIcon(pain, severity, num5 + bonusScore);
				root.PlaySoundEffect(pain, base.transform);
			}
			prevImpactSeverity = severity;
			impactScoreWindow = Time.fixedTime + 2.5f * Time.fixedDeltaTime;
			gameLogic.AddImpactScore(num5, bonusScore, Dismount.GameStates.Dismount.ScoreCategory.Ragdoll);
			if (severity >= 2 && type == Type.Torso && num != vehicleLayer && num3 > minimumPuffImpulse)
			{
				DismountGame.particleManager.EmitDustRing(collision.contacts[0].point, vector);
			}
		}
	}

	private void AddDamageIcon(Pain pain, int severity, int score)
	{
		if (severity < 2 || score < 5000 || !gameLogic.isDismountActive)
		{
			return;
		}
		float fixedTime = Time.fixedTime;
		if (severity > lastDamageSeverity)
		{
			lastDamageTime = fixedTime;
			lastDamageSeverity = severity;
			root.AddDamageIcon(pain, score);
			return;
		}
		if (fixedTime > lastDamageTime + 2f)
		{
			lastDamageSeverity = 0;
		}
		if (fixedTime > lastDamageTime + 2f)
		{
			lastDamageTime = fixedTime;
			lastDamageSeverity = severity;
			root.AddDamageIcon(pain, score);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (base.GetComponent<Rigidbody>().isKinematic || !isDismounted || collision.contacts.Length <= 0)
		{
			return;
		}
		int layer = collision.collider.gameObject.layer;
		int num = 1 << layer;
		if (isConnectedToStatsNode)
		{
			if ((num & groundContactIgnoreLayers) == 0)
			{
				root.lastGroundContactTime = Time.fixedTime;
			}
			if (layer == vehicleLayer)
			{
				root.lastVehicleContactTime = Time.fixedTime;
			}
		}
		if (collision.collider == penetrationCollider)
		{
			CheckCollisionForPenetration(collision);
		}
		Vector3 point = collision.contacts[0].point;
		Vector3 pointVelocity = base.GetComponent<Rigidbody>().GetPointVelocity(point);
		float magnitude = pointVelocity.magnitude;
		float num2 = magnitude * magnitude;
		int num3 = (int)(0.02f / Time.fixedDeltaTime);
		if (num3 < 1)
		{
			num3 = 1;
		}
		if (collision.collider.gameObject.layer != slideLayer || !(magnitude > 3f))
		{
			return;
		}
		slideCounter++;
		int num4 = 5 * num3;
		int num5 = 10 * num3;
		if (type == Type.Head)
		{
			num4 = 5 * num3;
			num5 = 10 * num3;
		}
		else if (type == Type.Limb)
		{
			num4 = 10 * num3;
			num5 = 20 * num3;
		}
		int num6 = 0;
		if (slideCounter % num4 == 0)
		{
			Vector3 velocity = Vector3.up * UnityEngine.Random.Range(0.25f, 1f) + 0.5f * pointVelocity;
			DismountGame.particleManager.EmitDust(point, velocity);
		}
		if (slideCounter % num5 == 0)
		{
			num6 = (int)(magnitude * scoreMultiplier) * 10;
			float num7 = 9f;
			float num8 = 0.2f + 0.8f * ((num2 - num7) / (100f - num7));
			if (num8 > 1f)
			{
				num8 = 1f;
			}
			int severity = 0;
			int bonusScore = 0;
			Pain pain = GetPain(0f, Hit.Slide, out severity, out bonusScore);
			gameLogic.AddSlideScore(num6, bonusScore, Dismount.GameStates.Dismount.ScoreCategory.Ragdoll);
			AddDamageIcon(pain, severity, num6 + bonusScore);
			root.PlaySoundEffect(pain, base.transform, num8);
		}
	}

	public bool GetPenetration(out Collider collider, out Vector3 normal)
	{
		if ((float)penetrationCollisionCounter >= 3f)
		{
			collider = penetrationCollider;
			normal = penetrationCollisionNormal;
			return true;
		}
		collider = null;
		normal = Vector3.up;
		return false;
	}

	private void RegisterCollisionForPenetrationCheck(Collision collision, int layer)
	{
		if ((layer & penetrationCheckIgnoreLayers) == 0)
		{
			penetrationCollider = collision.collider;
			penetrationCollisionCounter = 1;
			lastPenetrationCheckTime = Time.fixedTime;
		}
	}

	private void CheckCollisionForPenetration(Collision collision)
	{
		if (Time.fixedTime - lastPenetrationCheckTime <= 0.03f && collision.contacts.Length > 0)
		{
			penetrationCollisionNormal = collision.contacts[0].normal;
			penetrationCollisionCounter++;
			lastPenetrationCheckTime = Time.fixedTime;
		}
		CheckPenetration();
	}

	private void CheckPenetration()
	{
		Collider collider;
		Vector3 normal;
		if (!(joint == null) && !(physicsParent == null) && !((float)penetrationCollisionCounter < 3f) && physicsParent.GetPenetration(out collider, out normal) && collider == penetrationCollider && Vector3.Dot(penetrationCollisionNormal, normal) < -0.95f)
		{
			BreakJoint();
			int severity = 0;
			int bonusScore = 0;
			Pain pain = GetPain(0f, Hit.Detach, out severity, out bonusScore);
			AddDamageIcon(pain, severity, bonusScore);
			root.PlaySoundEffect(pain, base.transform);
		}
	}

	private bool CheckRepeatedCollisionEnter(Collision collision, int layer, float impactVelocity)
	{
		if (impactVelocity < 0.25f)
		{
			return false;
		}
		Vector3 position = base.GetComponent<Rigidbody>().position;
		float fixedTime = Time.fixedTime;
		Vector4 vector = new Vector4(position.x, position.y, position.z, fixedTime);
		lastCollisions[lastCollisionsWriteHead] = vector;
		lastCollisionsWriteHead = (lastCollisionsWriteHead + 1) & (lastCollisions.Length - 1);
		Vector4 vector2 = lastCollisions[lastCollisionsWriteHead];
		if (vector2.w < 1f)
		{
			return false;
		}
		if (vector.w - vector2.w < 3f)
		{
			return true;
		}
		return false;
	}

	private void OnDrawGizmos()
	{
		if (debugIsInCollision > 0)
		{
			Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
			Gizmos.DrawSphere(base.GetComponent<Collider>().bounds.center, base.GetComponent<Collider>().bounds.size.magnitude * 0.5f);
		}
	}

	public void VehiclePartDetached(string partName, Rigidbody newRigidBody)
	{
		if (attachRigidbodyName.Contains("Parts/") && attachRigidbodyName.EndsWith(partName) && vehiclePartJoint != null && (bool)vehiclePartJoint.connectedBody)
		{
			vehiclePartJoint.connectedBody = null;
			FixedJoint fixedJoint = newRigidBody.gameObject.AddComponent<FixedJoint>();
			fixedJoint.anchor = newRigidBody.gameObject.transform.InverseTransformPoint(base.transform.position);
			fixedJoint.breakForce = attachForce;
			fixedJoint.connectedBody = base.GetComponent<Rigidbody>();
			isConnectedToVehicle = false;
		}
	}
}
