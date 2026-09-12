#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class EndZoneChecker : MonoBehaviour
	{
		private int characterLayer;

		private int characterObjects;

		public bool HasCharacter()
		{
			return characterObjects > 0;
		}

		private void Awake()
		{
			characterLayer = LayerMask.NameToLayer("Character");
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.layer == characterLayer)
			{
				characterObjects++;
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.gameObject.layer == characterLayer)
			{
				characterObjects--;
			}
		}
	}
}
