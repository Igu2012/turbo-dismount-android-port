#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class CreditsScroller : MonoBehaviour
	{
		public float speed = 100f;

		public float topStartY = -360f;

		public float bottomEndY = 360f;

		private float totalHeight;

		private void Awake()
		{
			Reset();
		}

		private void OnEnable()
		{
			Reset();
		}

		public void Reset()
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition.y = topStartY;
			base.transform.localPosition = localPosition;
			totalHeight = 0f - base.transform.Find("BottomMarker").localPosition.y;
		}

		private void Complete()
		{
			Reset();
		}

		private void Update()
		{
			base.transform.localPosition += Vector3.up * speed * Time.deltaTime;
			if (base.transform.localPosition.y > bottomEndY + totalHeight)
			{
				Complete();
			}
		}
	}
}
