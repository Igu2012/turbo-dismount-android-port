#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount
{
	public class DamageFlash : MonoBehaviour
	{
		private Renderer mRenderer;

		private Material mMaterial;

		private Camera mCamera;

		private float lastScreenWidth;

		private float lastScreenHeight;

		private float lastFov;

		private void Start()
		{
			mRenderer = base.GetComponent<Renderer>();
			mMaterial = base.GetComponent<Renderer>().material;
			mCamera = base.transform.parent.GetComponent<Camera>();
			mRenderer.enabled = false;
			lastScreenWidth = Screen.width;
			lastScreenHeight = Screen.height;
			lastFov = -1f;
		}

		private void OnEnable()
		{
			lastFov = -1f;
		}

		private void FitToCamera(Camera c)
		{
			float num = c.fieldOfView / 180f * (float)Math.PI;
			float num2 = c.nearClipPlane + 0.001f;
			float num3 = Mathf.Tan(num * 0.5f) * num2 * 2f;
			float num4 = (float)Screen.width / (float)Screen.height * num3;
			Vector3 localScale = new Vector3(num4 * 1.05f, num3 * 1.05f, 1f);
			Vector3 localPosition = new Vector3(0f, 0f, num2);
			base.transform.localScale = localScale;
			base.transform.localPosition = localPosition;
		}

		private void Update()
		{
			float headPain = DismountGame.playerState.statistics.realtime.headPain;
			if (headPain < 0.01f)
			{
				mRenderer.enabled = false;
			}
			else
			{
				mRenderer.enabled = true;
				mMaterial.SetFloat("_Alpha", headPain * headPain);
			}
			if (lastScreenWidth != (float)Screen.width || lastScreenHeight != (float)Screen.height || lastFov != mCamera.fieldOfView)
			{
				FitToCamera(mCamera);
				lastFov = mCamera.fieldOfView;
				lastScreenWidth = Screen.width;
				lastScreenHeight = Screen.height;
			}
		}
	}
}
