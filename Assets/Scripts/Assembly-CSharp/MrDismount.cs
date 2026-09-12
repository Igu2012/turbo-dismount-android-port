#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using Dismount;
using Dismount.GameStates;
using Dismount.Vehicular;
using UnityEngine;

public class MrDismount : MonoBehaviour
{
	public enum Part
	{
		Hips = 0,
		Head = 1,
		Neck = 2,
		LeftUpLeg = 3,
		LeftLeg = 4,
		LeftFoot = 5,
		LeftToeBase = 6,
		RightUpLeg = 7,
		RightLeg = 8,
		RightFoot = 9,
		RightToeBase = 10,
		Spine = 11,
		Spine1 = 12,
		LeftArm = 13,
		LeftForeArm = 14,
		LeftHand = 15,
		RightArm = 16,
		RightForeArm = 17,
		RightHand = 18
	}

	protected Animator animator;

	public bool bubbleHead;

	public bool awake;

	public bool kinematicDismountSetup = true;

	public Transform cameraTarget;

	public Transform firstPersonCameraHead;

	public float limitBreakNeck = 800f;

	public float limitBreakHip = 800f;

	public float limitBreakKnee = 800f;

	public float limitBreakAnkle = 800f;

	public float limitBreakSpine = 800f;

	public float limitBreakShoulder = 800f;

	public float limitBreakElbow = 800f;

	public float limitBreakWrist = 800f;

	public float detachNeck = 1800f;

	public float detachHip = 1800f;

	public float detachKnee = 1800f;

	public float detachAnkle = 1800f;

	public float detachSpine = 1800f;

	public float detachShoulder = 1800f;

	public float detachElbow = 1800f;

	public float detachWrist = 1800f;

	private Dictionary<Part, float> jointLimitBreakForces = new Dictionary<Part, float>();

	private Dictionary<Part, float> jointDetachForces = new Dictionary<Part, float>();

	private Dictionary<BodyPart.Pain, string> painToSoundEffectMap = new Dictionary<BodyPart.Pain, string>();

	private Dictionary<BodyPart.Pain, HUDManager.DamageIconType> painToDamageIconMap = new Dictionary<BodyPart.Pain, HUDManager.DamageIconType>();

	public Vehicle vehicle;

	private BodyPart[] bodyParts;

	[NonSerialized]
	public BodyPart rideNode;

	private List<BodyPart> bodyPieces = new List<BodyPart>();

	private int maxBodyPieces;

	[HideInInspector]
	public Dismount.GameStates.Dismount gameLogic;

	private float mLastGroundContactTime;

	private float mLastVehicleAttachTime;

	private float mLastVehicleContactTime;

	private float mAirTime;

	private GameObject neckGO;

	private GameObject headGO;

	private GameObject mr_d_faceGO;

	private GameObject bubbleHeadGO;

	private GameObject customHeadInstance;

	private float lastLimbDamageTime;

	private int limbDamageUsedSlots;

	public float lastGroundContactTime
	{
		get
		{
			return mLastGroundContactTime;
		}
		set
		{
			mLastGroundContactTime = value;
			mAirTime = 0f;
		}
	}

	public float lastVehicleAttachTime
	{
		get
		{
			return mLastVehicleAttachTime;
		}
		set
		{
			mLastVehicleAttachTime = value;
		}
	}

	public float lastVehicleContactTime
	{
		get
		{
			return mLastVehicleContactTime;
		}
		set
		{
			mLastVehicleContactTime = value;
			if (vehicle.airTime < 0.05f)
			{
				mAirTime = vehicle.airTime;
			}
		}
	}

	public float airTime
	{
		get
		{
			return mAirTime;
		}
		set
		{
			mAirTime = value;
		}
	}

	public bool isDriving
	{
		get
		{
			return Time.fixedTime - mLastVehicleAttachTime < 0.02f;
		}
	}

	private void Awake()
	{
		InitMaps();
		bodyParts = GetComponentsInChildren<BodyPart>();
		BodyPart[] array = bodyParts;
		foreach (BodyPart bodyPart in array)
		{
			bodyPart.SetRoot(this);
			if (jointLimitBreakForces.ContainsKey(bodyPart.part))
			{
				bodyPart.jointLimitBreakForce = jointLimitBreakForces[bodyPart.part];
			}
			if (jointDetachForces.ContainsKey(bodyPart.part))
			{
				bodyPart.jointDetachForce = jointDetachForces[bodyPart.part];
			}
			if (cameraTarget == bodyPart.transform)
			{
				bodyPart.isStatsNode = true;
			}
			else
			{
				bodyPart.isStatsNode = false;
			}
			if (bodyPart.GetComponent<CharacterJoint>() == null)
			{
				bodyPieces.Add(bodyPart);
			}
			if (bodyPart.part == Part.Spine1)
			{
				rideNode = bodyPart;
			}
		}
		maxBodyPieces = bodyParts.Length;
		mLastGroundContactTime = Time.fixedTime;
		animator = GetComponent<Animator>();
		neckGO = base.transform.Find("MrDismount_Ctrl_Hips/MrDismount_Ctrl_Spine/MrDismount_Ctrl_Spine1/MrDismount_Ctrl_Neck").gameObject;
		headGO = base.transform.Find("MrDismount_Ctrl_Hips/MrDismount_Ctrl_Spine/MrDismount_Ctrl_Spine1/MrDismount_Ctrl_Neck/MrDismount_Ctrl_Head").gameObject;
		firstPersonCameraHead = headGO.transform;
		Transform transform = base.transform.Find("MrDismount_Ctrl_Hips/MrDismount_Ctrl_Spine/MrDismount_Ctrl_Spine1/MrDismount_Ctrl_Neck/MrDismount_Ctrl_Head/Mr_D_Face");
		if ((bool)transform)
		{
			mr_d_faceGO = base.transform.Find("MrDismount_Ctrl_Hips/MrDismount_Ctrl_Spine/MrDismount_Ctrl_Spine1/MrDismount_Ctrl_Neck/MrDismount_Ctrl_Head/Mr_D_Face").gameObject;
		}
		Transform transform2 = base.transform.Find("MrDismount_Ctrl_Hips/MrDismount_Ctrl_Spine/MrDismount_Ctrl_Spine1/MrDismount_Ctrl_Neck/MrDismount_Ctrl_Head/Face");
		if ((bool)transform2)
		{
			bubbleHeadGO = transform2.gameObject;
		}
		if (DismountGame.playerState.IsBubbleHeadMode)
		{
			SetBubbleheadMode();
		}
		else
		{
			SetNormalHeadMode();
		}
		if (DismountGame.playerState.currentHeadItem != null && DismountGame.playerState.currentHeadItem.referenceName.CompareTo("empty") != 0)
		{
			headGO.GetComponent<Renderer>().enabled = false;
			neckGO.GetComponent<Renderer>().enabled = true;
		}
	}

	public void SetGameLogic(Dismount.GameStates.Dismount dismount)
	{
		gameLogic = dismount;
		BodyPart componentInChildren = GetComponentInChildren<BodyPart>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SetGameLogic(dismount);
		}
	}

	public void SetBubbleheadMode()
	{
		DismountGame.playerState.IsBubbleHeadMode = true;
		neckGO.GetComponent<Renderer>().enabled = false;
		headGO.GetComponent<Renderer>().enabled = false;
		if (mr_d_faceGO != null)
		{
			mr_d_faceGO.SetActive(false);
		}
		if (bubbleHeadGO != null)
		{
			bubbleHeadGO.SetActive(true);
		}
		if (customHeadInstance != null)
		{
			customHeadInstance.SetActive(false);
		}
	}

	public void SetNormalHeadMode()
	{
		DismountGame.playerState.IsBubbleHeadMode = false;
		if (!(DismountGame.playerState.currentHeadItem != null) || DismountGame.playerState.currentHeadItem.referenceName.CompareTo("empty") == 0)
		{
			neckGO.GetComponent<Renderer>().enabled = true;
			headGO.GetComponent<Renderer>().enabled = true;
			if (mr_d_faceGO != null)
			{
				mr_d_faceGO.SetActive(true);
			}
			if (bubbleHeadGO != null)
			{
				bubbleHeadGO.SetActive(false);
			}
			if (customHeadInstance != null)
			{
				customHeadInstance.SetActive(false);
			}
		}
	}

	public void SetCustomHead(GameObject prefab)
	{
		if (!(prefab == null))
		{
			if (customHeadInstance != null)
			{
				UnityEngine.Object.Destroy(customHeadInstance);
			}
			customHeadInstance = UnityEngine.Object.Instantiate(prefab) as GameObject;
			customHeadInstance.transform.parent = headGO.transform;
			customHeadInstance.transform.localPosition = Vector3.zero;
			customHeadInstance.transform.localRotation = Quaternion.identity;
			if (base.name.StartsWith("MrEgo"))
			{
				customHeadInstance.transform.localScale = customHeadInstance.transform.localScale * 2f;
			}
			neckGO.GetComponent<Renderer>().enabled = true;
			headGO.GetComponent<Renderer>().enabled = false;
			if (mr_d_faceGO != null)
			{
				mr_d_faceGO.SetActive(false);
			}
			if (bubbleHeadGO != null)
			{
				bubbleHeadGO.SetActive(false);
			}
		}
	}

	public void AttachToVehicle(GameObject vehicleObject)
	{
		bodyParts = GetComponentsInChildren<BodyPart>();
		BodyPart[] array = bodyParts;
		foreach (BodyPart bodyPart in array)
		{
			bodyPart.AttachToVehicle(vehicleObject);
		}
		vehicle = vehicleObject.GetComponent<Vehicle>();
	}

	public void SetKinematic(bool kinematic)
	{
		bodyParts = GetComponentsInChildren<BodyPart>();
		BodyPart[] array = bodyParts;
		foreach (BodyPart bodyPart in array)
		{
			bodyPart.GetComponent<Rigidbody>().isKinematic = kinematic;
		}
	}

	public void BodyPartDetached(BodyPart part)
	{
		bodyPieces.Add(part);
		if (part.part == Part.Head || part.part == Part.Neck)
		{
			DismountGame.playerState.statistics.CharacterDecapitated();
		}
		else if (part.part == Part.LeftUpLeg || part.part == Part.RightUpLeg || part.part == Part.LeftArm || part.part == Part.RightArm)
		{
			DismountGame.playerState.statistics.CharacterLimbDetached();
		}
		DismountGame.playerState.statistics.CharacterBodyPartDetached();
		int num = -1;
		for (int i = 0; i < bodyPieces.Count; i++)
		{
			BodyPart bodyPart = bodyPieces[i];
			if (bodyPart.FindStatsNodeRecursive())
			{
				num = i;
				break;
			}
		}
		if (bodyPieces.Count == maxBodyPieces)
		{
			DismountGame.playerState.statistics.CharacterDisintegrated();
		}
		if (num >= 0)
		{
			for (int j = 0; j < bodyPieces.Count; j++)
			{
				BodyPart bodyPart2 = bodyPieces[j];
				bodyPart2.SetStatsNodeConnectionRecursive(j == num);
			}
		}
	}

	private void InitMaps()
	{
		jointLimitBreakForces.Add(Part.Head, float.PositiveInfinity);
		jointLimitBreakForces.Add(Part.Neck, limitBreakNeck);
		jointLimitBreakForces.Add(Part.LeftUpLeg, limitBreakHip);
		jointLimitBreakForces.Add(Part.LeftLeg, limitBreakKnee);
		jointLimitBreakForces.Add(Part.LeftFoot, limitBreakAnkle);
		jointLimitBreakForces.Add(Part.LeftToeBase, float.PositiveInfinity);
		jointLimitBreakForces.Add(Part.RightUpLeg, limitBreakHip);
		jointLimitBreakForces.Add(Part.RightLeg, limitBreakKnee);
		jointLimitBreakForces.Add(Part.RightFoot, limitBreakAnkle);
		jointLimitBreakForces.Add(Part.RightToeBase, float.PositiveInfinity);
		jointLimitBreakForces.Add(Part.Spine, limitBreakSpine);
		jointLimitBreakForces.Add(Part.Spine1, limitBreakSpine);
		jointLimitBreakForces.Add(Part.LeftArm, limitBreakShoulder);
		jointLimitBreakForces.Add(Part.LeftForeArm, limitBreakElbow);
		jointLimitBreakForces.Add(Part.LeftHand, limitBreakWrist);
		jointLimitBreakForces.Add(Part.RightArm, limitBreakShoulder);
		jointLimitBreakForces.Add(Part.RightForeArm, limitBreakElbow);
		jointLimitBreakForces.Add(Part.RightHand, limitBreakWrist);
		jointDetachForces.Add(Part.Head, float.PositiveInfinity);
		jointDetachForces.Add(Part.Neck, detachNeck);
		jointDetachForces.Add(Part.LeftUpLeg, detachHip);
		jointDetachForces.Add(Part.LeftLeg, detachKnee);
		jointDetachForces.Add(Part.LeftFoot, detachAnkle);
		jointDetachForces.Add(Part.LeftToeBase, float.PositiveInfinity);
		jointDetachForces.Add(Part.RightUpLeg, detachHip);
		jointDetachForces.Add(Part.RightLeg, detachKnee);
		jointDetachForces.Add(Part.RightFoot, detachAnkle);
		jointDetachForces.Add(Part.RightToeBase, float.PositiveInfinity);
		jointDetachForces.Add(Part.Spine, detachSpine);
		jointDetachForces.Add(Part.Spine1, detachSpine);
		jointDetachForces.Add(Part.LeftArm, detachShoulder);
		jointDetachForces.Add(Part.LeftForeArm, detachElbow);
		jointDetachForces.Add(Part.LeftHand, detachWrist);
		jointDetachForces.Add(Part.RightArm, detachShoulder);
		jointDetachForces.Add(Part.RightForeArm, detachElbow);
		jointDetachForces.Add(Part.RightHand, detachWrist);
		painToSoundEffectMap.Add(BodyPart.Pain.HeadHit, "ThumpHard");
		painToSoundEffectMap.Add(BodyPart.Pain.HeadCracked, "BreakageSoft");
		painToSoundEffectMap.Add(BodyPart.Pain.HeadCrackedMajor, "BreakageHard");
		painToSoundEffectMap.Add(BodyPart.Pain.NeckBroken, "LimitBreak");
		painToSoundEffectMap.Add(BodyPart.Pain.HeadDetached, "Detach");
		painToSoundEffectMap.Add(BodyPart.Pain.HeadSlide, "Slide");
		painToSoundEffectMap.Add(BodyPart.Pain.LimbHit, "ThumpHard");
		painToSoundEffectMap.Add(BodyPart.Pain.LimbBroken, "BreakageSoft");
		painToSoundEffectMap.Add(BodyPart.Pain.LimbBrokenMajor, "BreakageHard");
		painToSoundEffectMap.Add(BodyPart.Pain.LimbJointBroken, "LimitBreak");
		painToSoundEffectMap.Add(BodyPart.Pain.LimbDetached, "Detach");
		painToSoundEffectMap.Add(BodyPart.Pain.LimbSlide, "Slide");
		painToSoundEffectMap.Add(BodyPart.Pain.ChestHit, "ThumpHard");
		painToSoundEffectMap.Add(BodyPart.Pain.ChestBroken, "BreakageSoft");
		painToSoundEffectMap.Add(BodyPart.Pain.ChestBrokenMajor, "BreakageHard");
		painToSoundEffectMap.Add(BodyPart.Pain.PelvisBrokenMajor, "BreakageHard");
		painToSoundEffectMap.Add(BodyPart.Pain.SpineBroken, "LimitBreak");
		painToSoundEffectMap.Add(BodyPart.Pain.TorsoSlide, "Slide");
		painToSoundEffectMap.Add(BodyPart.Pain.MinorHit, "ThumpSoft");
		painToSoundEffectMap.Add(BodyPart.Pain.RanOver, "BreakageSoft");
		painToSoundEffectMap.Add(BodyPart.Pain.ThroughWindow, "ThumpHard");
		painToDamageIconMap.Add(BodyPart.Pain.HeadCrackedMajor, HUDManager.DamageIconType.BreakSkull);
		painToDamageIconMap.Add(BodyPart.Pain.LimbBrokenMajor, HUDManager.DamageIconType.BreakBone);
		painToDamageIconMap.Add(BodyPart.Pain.ChestBrokenMajor, HUDManager.DamageIconType.BreakRibs);
		painToDamageIconMap.Add(BodyPart.Pain.PelvisBrokenMajor, HUDManager.DamageIconType.BreakHip);
		painToDamageIconMap.Add(BodyPart.Pain.SpineBroken, HUDManager.DamageIconType.BreakSpine);
		painToDamageIconMap.Add(BodyPart.Pain.LimbDetached, HUDManager.DamageIconType.DetachLimbJoint);
		painToDamageIconMap.Add(BodyPart.Pain.HeadDetached, HUDManager.DamageIconType.DetachNeck);
		painToDamageIconMap.Add(BodyPart.Pain.SpineDetached, HUDManager.DamageIconType.DetachSpine);
		painToDamageIconMap.Add(BodyPart.Pain.Flip, HUDManager.DamageIconType.Flip);
		painToDamageIconMap.Add(BodyPart.Pain.Airtime, HUDManager.DamageIconType.Airtime);
	}

	public void SetVisible(bool visible)
	{
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		MeshRenderer[] array = componentsInChildren;
		foreach (MeshRenderer meshRenderer in array)
		{
			meshRenderer.enabled = visible;
		}
	}

	public void WakeUp()
	{
		if (!awake)
		{
			UnityEngine.Object.Destroy(animator);
			BodyPart[] array = bodyParts;
			foreach (BodyPart bodyPart in array)
			{
				bodyPart.GetComponent<Rigidbody>().isKinematic = false;
			}
			awake = true;
		}
	}

	private void FixedUpdate()
	{
		if (!awake || !gameLogic || !gameLogic.isDismountActive)
		{
			return;
		}
		float num = mAirTime;
		mAirTime += Time.fixedDeltaTime;
		if (mAirTime < 1f)
		{
			return;
		}
		float num2 = 1f;
		int num3 = 1;
		while (num3 <= 10)
		{
			if (num < num2 && mAirTime >= num2)
			{
				gameLogic.AddImpactScore(0, num3 * 2500, Dismount.GameStates.Dismount.ScoreCategory.Ragdoll);
				AddDamageIcon(BodyPart.Pain.Airtime, num3 * 2500);
				break;
			}
			num3++;
			num2++;
		}
	}

	private void OnDismountStarted()
	{
		WakeUp();
		BodyPart[] array = bodyParts;
		foreach (BodyPart bodyPart in array)
		{
			bodyPart.OnDismountStarted();
		}
	}

	public bool IsMoving()
	{
		BodyPart[] array = bodyParts;
		foreach (BodyPart bodyPart in array)
		{
			Rigidbody rigidbody = bodyPart.GetComponent<Rigidbody>();
			if (!rigidbody.isKinematic && !rigidbody.IsSleeping() && (rigidbody.velocity.sqrMagnitude > 0.06f || rigidbody.angularVelocity.sqrMagnitude > 4.5f))
			{
				return true;
			}
		}
		return false;
	}

	public void PlaySoundEffect(BodyPart.Pain pain, Transform transform, float volumeMultiplier = 1f)
	{
		if ((bool)DismountGame.instance && painToSoundEffectMap.ContainsKey(pain))
		{
			DismountGame.audioManager.PlaySoundEffect(painToSoundEffectMap[pain], transform, volumeMultiplier);
		}
	}

	public void AddDamageIcon(BodyPart.Pain pain, int score)
	{
		if (!DismountGame.instance || !awake)
		{
			return;
		}
		float fixedTime = Time.fixedTime;
		if (!painToDamageIconMap.ContainsKey(pain))
		{
			return;
		}
		if (pain == BodyPart.Pain.LimbBrokenMajor)
		{
			if (fixedTime > lastLimbDamageTime + 0.6f)
			{
				limbDamageUsedSlots -= 3;
			}
			else if (fixedTime > lastLimbDamageTime + 0.4f)
			{
				limbDamageUsedSlots -= 2;
			}
			else if (fixedTime > lastLimbDamageTime + 0.2f)
			{
				limbDamageUsedSlots--;
			}
			if (limbDamageUsedSlots < 0)
			{
				limbDamageUsedSlots = 0;
			}
			if (limbDamageUsedSlots < 3)
			{
				lastLimbDamageTime = fixedTime;
				limbDamageUsedSlots++;
				DismountGame.hudManager.AddDamageIcon(painToDamageIconMap[pain]);
			}
		}
		else
		{
			DismountGame.hudManager.AddDamageIcon(painToDamageIconMap[pain]);
		}
	}

	public void VehiclePartDetached(string partName, Rigidbody newRigidBody)
	{
		BodyPart[] array = bodyParts;
		foreach (BodyPart bodyPart in array)
		{
			bodyPart.VehiclePartDetached(partName, newRigidBody);
		}
	}

	public void AddRiderForce(Vector3 force)
	{
		if (isDriving && rideNode != null)
		{
			rideNode.GetComponent<Rigidbody>().AddForce(force, ForceMode.Force);
		}
	}
}
