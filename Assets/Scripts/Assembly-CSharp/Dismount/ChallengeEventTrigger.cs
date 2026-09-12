#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class ChallengeEventTrigger : MonoBehaviour
	{
		public string id = string.Empty;

		public LayerMask layerFilter = 0;

		private void OnTriggerEnter(Collider other)
		{
			if ((layerFilter.value & (1 << other.gameObject.layer)) != 0)
			{
				DismountGame.challenges.ReportChallengeEvent(id);
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(0.25f, 0.75f, 1f, 0.125f);
			Gizmos.DrawCube(base.GetComponent<Collider>().bounds.center, base.GetComponent<Collider>().bounds.size);
		}
	}
}
