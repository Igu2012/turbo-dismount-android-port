#pragma warning disable 0618,0619
using System;
using UnityEngine;

public class GLLines : MonoBehaviour
{
	public int lineCount = 100;

	public float radius = 1f;

	public Material lineMaterial;

	private void Update()
	{
		base.transform.Rotate(new Vector3(6f, 13f, 27f) * 2f * Time.deltaTime);
	}

	public void OnRenderObject()
	{
		if ((bool)lineMaterial)
		{
			GL.PushMatrix();
			GL.MultMatrix(base.transform.localToWorldMatrix);
			lineMaterial.SetPass(0);
			GL.Begin(1);
			for (int i = 0; i < lineCount; i++)
			{
				float num = (float)i / (float)lineCount;
				float num2 = num * (float)Math.PI;
				GL.Color(new Color(0.25f, 1f - num, 1f, 0f));
				GL.Vertex3(Mathf.Cos(num2) * radius, Mathf.Sin(num2) * radius, 0f);
				GL.Color(new Color(0.25f, 1f - num, 1f, 0.8f));
				GL.Vertex3(Mathf.Cos(num2 * 2f) * radius, Mathf.Sin(num2 * 2f) * radius, 0f);
			}
			GL.End();
			GL.PopMatrix();
		}
	}
}
