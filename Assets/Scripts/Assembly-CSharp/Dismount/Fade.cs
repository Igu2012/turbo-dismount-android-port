#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class Fade : MonoBehaviour
	{
		private Material material;

		private void Start()
		{
			material = base.GetComponent<Renderer>().material;
		}

		private void Update()
		{
			Color color = material.color;
			material.color = new Color(color.r, color.g, color.b, (Mathf.Cos(Time.time * 3f) + 1f) / 2f);
		}
	}
}
