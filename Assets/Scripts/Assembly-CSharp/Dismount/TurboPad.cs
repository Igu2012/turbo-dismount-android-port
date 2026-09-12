#pragma warning disable 0618,0619
using Dismount.Vehicular;
using UnityEngine;

namespace Dismount
{
	public class TurboPad : MonoBehaviour
	{
		private const float duration = 2f;

		private float boostTime = -1f;

		private GameObject visual;

		private void Awake()
		{
			visual = base.transform.Find("Visual").gameObject;
		}

		private void FixedUpdate()
		{
			if (!(boostTime < 0f) && Time.fixedTime > boostTime + 2f)
			{
				boostTime = -1f;
				base.GetComponent<Collider>().enabled = true;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!(boostTime >= 0f) && other.isTrigger && !(other.GetComponent<Vehicle>() == null))
			{
				other.gameObject.SendMessage("TurboBoost");
				other.gameObject.SendMessage("ActivateRockets");
				boostTime = Time.fixedTime;
				base.GetComponent<Collider>().enabled = false;
			}
		}
	}
}
