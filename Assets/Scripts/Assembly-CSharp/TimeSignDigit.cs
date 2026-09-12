#pragma warning disable 0618,0619
using UnityEngine;

public class TimeSignDigit : MonoBehaviour
{
	private Material material;

	private void Start()
	{
		material = base.GetComponent<Renderer>().material;
		SetNumber(10);
	}

	public void SetNumber(int number)
	{
		int num = number / 8;
		int num2 = number % 8;
		material.mainTextureOffset = new Vector2((float)num2 * 0.125f, (float)(1 - num) * 0.5f);
	}
}
