#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class DigitalDisplayDigit : MonoBehaviour
	{
		private Material material;

		private void Awake()
		{
			material = base.GetComponent<Renderer>().material;
		}

		public void SetIndex(int index)
		{
			int num = index / 8;
			int num2 = index % 8;
			material.mainTextureOffset = new Vector2((float)num2 * 0.125f, (float)(1 - num) * 0.5f);
		}
	}
}
