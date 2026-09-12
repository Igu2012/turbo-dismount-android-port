#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	public class NPCWheelRotation : MonoBehaviour
	{
		private float _angle;

		private Transform[] wheelTransforms;

		public float angle
		{
			get
			{
				return _angle;
			}
			set
			{
				_angle = value;
				UpdateWheels();
			}
		}

		private void Awake()
		{
			if (wheelTransforms == null)
			{
				NPCWheel[] componentsInChildren = GetComponentsInChildren<NPCWheel>();
				List<Transform> list = new List<Transform>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					list.Add(componentsInChildren[i].transform);
				}
				wheelTransforms = list.ToArray();
			}
		}

		private void UpdateWheels()
		{
			for (int i = 0; i < wheelTransforms.Length; i++)
			{
				Vector3 localEulerAngles = wheelTransforms[i].localEulerAngles;
				localEulerAngles.x = _angle;
				localEulerAngles.y = 0f;
				localEulerAngles.z = 0f;
				wheelTransforms[i].localEulerAngles = localEulerAngles;
			}
		}
	}
}
