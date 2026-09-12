#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class PalmtreeTrunk : MonoBehaviour
	{
		private bool trunkHit;

		private bool trunkToppled;

		private Vector3 originalUp = Vector3.up;

		private Transform cachedTransform;

		private int characterLayer;

		private int vehicleLayer;

		private void Awake()
		{
			characterLayer = LayerMask.NameToLayer("Character");
			vehicleLayer = LayerMask.NameToLayer("Vehicle");
			cachedTransform = base.transform;
			originalUp = cachedTransform.up;
		}

		private void FixedUpdate()
		{
			if (trunkHit && !trunkToppled && Vector3.Dot(cachedTransform.up, originalUp) < 0.707f)
			{
				trunkToppled = true;
				DismountGame.playerState.statistics.TreeCut();
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!trunkHit)
			{
				int layer = collision.gameObject.layer;
				if (layer == characterLayer || layer == vehicleLayer)
				{
					trunkHit = true;
				}
			}
		}
	}
}
