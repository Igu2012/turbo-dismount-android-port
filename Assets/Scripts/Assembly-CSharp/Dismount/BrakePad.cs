#pragma warning disable 0618,0619
using Dismount.Vehicular;
using UnityEngine;

namespace Dismount
{
	public class BrakePad : MonoBehaviour
	{
		private const float duration = 2f;

		private float brakeTime = -1f;

		private GameObject visual;

		private void Awake()
		{
			visual = base.transform.Find("Visual").gameObject;
		}

		private void FixedUpdate()
		{
			if (!(brakeTime < 0f) && Time.fixedTime > brakeTime + 2f)
			{
				brakeTime = -1f;
				base.GetComponent<Collider>().enabled = true;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!(brakeTime >= 0f) && other.isTrigger && !(other.GetComponent<Vehicle>() == null))
			{
				other.gameObject.SendMessage("BrakePad");
				brakeTime = Time.fixedTime;
				base.GetComponent<Collider>().enabled = false;
			}
		}
	}
}
