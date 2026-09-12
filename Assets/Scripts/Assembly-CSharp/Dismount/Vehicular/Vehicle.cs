#pragma warning disable 0618,0619
using System.Collections.Generic;
using Dismount.GameStates;
using UnityEngine;

namespace Dismount.Vehicular
{
	[RequireComponent(typeof(Rigidbody))]
	public class Vehicle : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float inputThrottle;

		[Range(0f, 1f)]
		public float inputClutch;

		[Range(0f, 1f)]
		public float inputBrake;

		public bool showDebug;

		[Range(-1f, 1f)]
		public float inputSteering;

		public int overrideGear = -1;

		[HideInInspector]
		public bool isMoving;

		[HideInInspector]
		public Spline steerSpline;

		[HideInInspector]
		public float fuel = 30f;

		[HideInInspector]
		public float lastGroundContactTime;

		[HideInInspector]
		public bool isAwake;

		[HideInInspector]
		public bool overrideThrottleBrake;

		[Range(-1f, 1f)]
		[HideInInspector]
		public float throttleBrake;

		private List<GameObject> billboards = new List<GameObject>();

		public Transform cameraTarget
		{
			get
			{
				return base.transform.Find("OrbitCameraTarget");
			}
		}

		public float airTime
		{
			get
			{
				return Time.fixedTime - lastGroundContactTime;
			}
		}

		private void Awake()
		{
			Transform transform = Utils.FindChildRecursive("Billboard", base.transform);
			while (transform != null)
			{
				billboards.Add(transform.gameObject);
				transform.name = "Billboard_processed";
				transform = Utils.FindChildRecursive("Billboard", base.transform);
			}
			ShowBillboards(DismountGame.instance.vehicleLogoOverridden);
		}

		public void SetGameLogic(Dismount.GameStates.Dismount dismount)
		{
			Car component = GetComponent<Car>();
			if ((bool)component)
			{
				component.gameLogic = dismount;
			}
		}

		public void SetCharacter(MrDismount character)
		{
			Car component = GetComponent<Car>();
			if ((bool)component)
			{
				component.driver = character;
			}
		}

		public void ShowBillboards(bool visible)
		{
			foreach (GameObject billboard in billboards)
			{
				if (billboard != null)
				{
					billboard.SetActive(visible);
				}
			}
		}

		public bool IsMoving()
		{
			return isMoving;
		}

		public void WakeUp()
		{
			isAwake = true;
			inputBrake = 1f;
		}

		public void OnBombed(Bomb bomb)
		{
			base.GetComponent<Rigidbody>().AddExplosionForce(10000f, bomb.transform.position, 10f);
		}

		public void RecycleSkidmarks()
		{
			Wheel[] componentsInChildren = GetComponentsInChildren<Wheel>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].RecycleSkidmark();
			}
		}
	}
}
