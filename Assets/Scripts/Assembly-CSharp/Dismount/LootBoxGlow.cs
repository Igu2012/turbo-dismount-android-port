#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class LootBoxGlow : MonoBehaviour
	{
		public Vector3 rotateSpeed = Vector3.zero;

		public Vector3 scaleSpeed = Vector3.zero;

		public Vector3 scaleOffset = Vector3.zero;

		public Vector3 scaleSize = Vector3.one;

		public Vector3 scaleAmplitude = Vector3.zero;

		public Color tint = new Color(1f, 0.6f, 0.2f, 0.25f);

		private Material glowMat;

		private Transform xf;

		private void Awake()
		{
			xf = base.transform;
			MeshRenderer component = GetComponent<MeshRenderer>();
			glowMat = component.material;
			glowMat.SetColor("_TintColor", tint);
		}

		private void Update()
		{
			xf.Rotate(Time.deltaTime * rotateSpeed, Space.Self);
			Vector3 localScale = scaleSize;
			float num = Mathf.Sin(Time.time * scaleSpeed.x + scaleOffset.x);
			localScale.x += scaleAmplitude.x * Mathf.Sin(Time.time * scaleSpeed.x + scaleOffset.x);
			localScale.y += scaleAmplitude.y * Mathf.Sin(Time.time * scaleSpeed.y + scaleOffset.y);
			localScale.z += scaleAmplitude.z * Mathf.Sin(Time.time * scaleSpeed.z + scaleOffset.z);
			float z = localScale.z;
			Color color = tint;
			color.a *= z;
			xf.localScale = localScale;
			glowMat.SetColor("_TintColor", color);
		}
	}
}
