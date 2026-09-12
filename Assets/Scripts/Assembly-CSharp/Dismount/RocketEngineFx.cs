#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class RocketEngineFx : MonoBehaviour
	{
		private const int emitSkipFrames = 3;

		private Vector3 previousPosition;

		private bool emitSmoke;

		public GameObject[] visuals;

		private Vector3 originalScale;

		private bool doSkip;

		private int emitSkip = 3;

		private void Start()
		{
			originalScale = base.transform.localScale;
			previousPosition = base.transform.position;
			if (DismountGame.IsLowPerformanceDevice())
			{
				doSkip = true;
			}
		}

		private void FixedUpdate()
		{
			Transform transform = base.transform;
			if (emitSmoke)
			{
				if (doSkip)
				{
					if (emitSkip++ < 3)
					{
						goto IL_0117;
					}
					emitSkip = 0;
				}
				Vector3 velocity = (transform.position - previousPosition) / Time.fixedDeltaTime * 0.75f;
				velocity += transform.forward * Random.Range(12f, 16f) + 2f * Random.insideUnitSphere + Vector3.up;
				DismountGame.particleManager.EmitRocketSmoke(transform.position, velocity);
				float num = 0.7f + 0.3f * ((Mathf.Sin(Time.fixedTime * 70f) + 1f) / 2f);
				base.transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z * num);
			}
			goto IL_0117;
			IL_0117:
			previousPosition = transform.position;
		}

		public void Activate()
		{
			emitSmoke = true;
			for (int i = 0; i < visuals.Length; i++)
			{
				visuals[i].SetActive(true);
			}
		}

		public void Deactivate()
		{
			emitSmoke = false;
			for (int i = 0; i < visuals.Length; i++)
			{
				visuals[i].SetActive(false);
			}
		}
	}
}
