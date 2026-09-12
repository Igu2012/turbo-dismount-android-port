#pragma warning disable 0618,0619
using UnityEngine;

[ExecuteInEditMode]
public class CRT : MonoBehaviour
{
	public Shader curShader;

	public float Distortion = 0.1f;

	public float Gamma = 1f;

	public float YExtra = 0.5f;

	public float CurvatureSet1 = 0.5f;

	public float CurvatureSet2 = 1f;

	public float DotWeight = 1f;

	public CRTScanLinesSizes scanSize = CRTScanLinesSizes.S512;

	public Color rgb1 = Color.white;

	public Color rgb2 = Color.white;

	private Material curMaterial;

	private Material material
	{
		get
		{
			if (curMaterial == null)
			{
				curMaterial = new Material(curShader);
				curMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return curMaterial;
		}
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if (curShader != null)
		{
			material.SetFloat("_Distortion", Distortion);
			material.SetFloat("_Gamma", Gamma);
			material.SetFloat("_curvatureSet1", CurvatureSet1);
			material.SetFloat("_curvatureSet2", CurvatureSet2);
			material.SetFloat("_YExtra", YExtra);
			material.SetFloat("_rgb1R", rgb1.r);
			material.SetFloat("_rgb1G", rgb1.g);
			material.SetFloat("_rgb1B", rgb1.b);
			material.SetFloat("_rgb2R", rgb2.r);
			material.SetFloat("_rgb2G", rgb2.g);
			material.SetFloat("_rgb2B", rgb2.b);
			material.SetFloat("_dotWeight", DotWeight);
			material.SetVector("_TextureSize", new Vector2((float)scanSize, (float)scanSize));
			Graphics.Blit(sourceTexture, destTexture, material);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);
		}
	}

	private void Update()
	{
	}

	private void OnDisable()
	{
		if ((bool)curMaterial)
		{
			Object.DestroyImmediate(curMaterial);
		}
	}
}
