#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class BowlingPin : MonoBehaviour
	{
		private bool pinHit;

		private bool pinToppled;

		private Vector3 originalUp = Vector3.up;

		private Transform cachedTransform;

		private BowlingPinObserver observer;

		private void Awake()
		{
			cachedTransform = base.transform;
			originalUp = cachedTransform.up;
		}

		public void SetObserver(BowlingPinObserver observer)
		{
			this.observer = observer;
		}

		private void FixedUpdate()
		{
			if (pinHit && !pinToppled && !(observer == null) && Vector3.Dot(cachedTransform.up, originalUp) < 0.5f)
			{
				pinToppled = true;
				observer.PinToppled();
			}
		}

		private void OnCollisionEnter()
		{
			pinHit = true;
		}
	}
}
