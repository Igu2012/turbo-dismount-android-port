#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(MeshRenderer))]
	public class TextureOffsetAnimator : MonoBehaviour
	{
		public int materialIndex;

		public Vector2 speed = Vector2.up;

		private Material material;

		private void Start()
		{
			material = base.GetComponent<Renderer>().materials[materialIndex];
		}

		private void FixedUpdate()
		{
			Vector2 vector = material.mainTextureOffset + speed * Time.fixedDeltaTime;
			vector = new Vector2(Mathf.Repeat(vector.x, 20f), Mathf.Repeat(vector.y, 20f));
			material.mainTextureOffset = vector;
		}
	}
}
