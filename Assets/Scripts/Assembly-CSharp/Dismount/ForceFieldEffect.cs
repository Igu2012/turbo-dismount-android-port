#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class ForceFieldEffect : MonoBehaviour
	{
		public float intensity = 1f;

		public ForceField spawner;

		private float startTime;

		private Vector3 originalScale = Vector3.one;

		private bool resetAndDie;

		private void Awake()
		{
			startTime = Time.fixedTime;
			originalScale = base.transform.localScale;
			base.GetComponent<Renderer>().material.SetFloat("_Intensity", intensity);
		}

		private void OnEnable()
		{
			startTime = Time.fixedTime;
			base.transform.localScale = originalScale;
		}

		private void SetScaleAndIntensity(float phase)
		{
			float num = 1.5f + phase * 3f * intensity;
			num *= num;
			base.transform.localScale = num * originalScale;
			base.GetComponent<Renderer>().material.mainTextureScale = Vector2.one * num;
			base.GetComponent<Renderer>().material.mainTextureOffset = Vector2.one * num * -0.5f;
			float num2 = intensity * (1f - phase);
			if (phase < 0.05f)
			{
				num2 = 0f;
			}
			else if (phase < 0.1f)
			{
				num2 = 20f * num2 * (phase - 0.05f);
			}
			num2 *= num2;
			base.GetComponent<Renderer>().material.SetFloat("_Intensity", num2);
		}

		private void FixedUpdate()
		{
			if (resetAndDie)
			{
				SetScaleAndIntensity(0f);
				base.GetComponent<Renderer>().material.SetFloat("_Intensity", 0f);
				resetAndDie = false;
				if ((bool)spawner)
				{
					spawner.EffectDied(base.gameObject);
				}
			}
			float num = (Time.fixedTime - startTime) * 1.25f;
			if (num > 1f)
			{
				num = 1f;
				resetAndDie = true;
			}
			SetScaleAndIntensity(num);
		}
	}
}
