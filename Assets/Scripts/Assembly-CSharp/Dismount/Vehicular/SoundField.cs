#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	public class SoundField : MonoBehaviour
	{
		public SoundFieldLayer[] layers;

		public float minX;

		public float maxX = 1f;

		public float xScale = 1f;

		public float minY;

		public float maxY = 1f;

		public float yScale = 1f;

		[HideInInspector]
		public Vector2 node = Vector2.zero;

		[HideInInspector]
		public Vector2 scaledNode = Vector2.zero;

		public void Play()
		{
			if (layers != null && layers.Length >= 1)
			{
				for (int i = 0; i < layers.Length; i++)
				{
					layers[i].Play();
				}
			}
		}

		public void Stop()
		{
			if (layers != null && layers.Length >= 1)
			{
				for (int i = 0; i < layers.Length; i++)
				{
					layers[i].Stop();
				}
			}
		}

		public void UpdateSound(float x, float y, float volumeMultiplier)
		{
			node.x = x;
			node.y = y;
			scaledNode.x = Mathf.Clamp(node.x, minX, maxX) / xScale;
			scaledNode.y = Mathf.Clamp(node.y, minY, maxY) / yScale;
			if (layers != null && layers.Length >= 1)
			{
				for (int i = 0; i < layers.Length; i++)
				{
					SoundFieldLayer soundFieldLayer = layers[i];
					soundFieldLayer.volumeMultiplier = volumeMultiplier;
					soundFieldLayer.UpdateSound(node, scaledNode);
				}
			}
		}
	}
}
