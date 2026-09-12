#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[ExecuteInEditMode]
	public class RenderSettingsOverride : MonoBehaviour
	{
		public bool fog;

		public Color fogColor = Color.black;

		public FogMode fogMode = FogMode.Exponential;

		public float fogDensity = 0.01f;

		public float linearFogStart = 5f;

		public float linearFogEnd = 50f;

		public Color ambientLight = Color.black;

		public Material customSkybox;

		private void Start()
		{
			UpdateRenderSettings();
		}

		private void UpdateRenderSettings()
		{
			RenderSettings.fog = fog;
			RenderSettings.fogColor = fogColor;
			RenderSettings.fogMode = fogMode;
			RenderSettings.fogDensity = fogDensity;
			RenderSettings.fogStartDistance = linearFogStart;
			RenderSettings.fogEndDistance = linearFogEnd;
			RenderSettings.ambientLight = ambientLight;
		}
	}
}
