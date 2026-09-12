#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	public class CopLights : MonoBehaviour
	{
		[Serializable]
		public class BlinkingLight
		{
			public GameObject gameObject;

			public string blinkPattern;

			[HideInInspector]
			public char[] pattern;
		}

		public List<BlinkingLight> lights = new List<BlinkingLight>();

		public float patternDuration = 1f;

		public float speed = 1f;

		private float tick;

		private void Awake()
		{
			for (int i = 0; i < lights.Count; i++)
			{
				BlinkingLight blinkingLight = lights[i];
				blinkingLight.pattern = blinkingLight.blinkPattern.ToCharArray();
				Light[] componentsInChildren = blinkingLight.gameObject.transform.GetComponentsInChildren<Light>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].enabled = false;
				}
			}
			tick = UnityEngine.Random.Range(0f, patternDuration);
		}

		private void FixedUpdate()
		{
			tick += speed * Time.fixedDeltaTime;
			float num = tick / patternDuration;
			for (int i = 0; i < lights.Count; i++)
			{
				BlinkingLight blinkingLight = lights[i];
				int num2 = Mathf.FloorToInt(num * (float)blinkingLight.pattern.Length) % blinkingLight.pattern.Length;
				blinkingLight.gameObject.SetActive(blinkingLight.pattern[num2] != '0');
			}
		}
	}
}
