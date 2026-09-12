#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class GameItem : ScriptableObject
	{
		public enum ItemCategory
		{
			Obstacle = 0,
			Vehicle = 1,
			Character = 2,
			Level = 3,
			Bundle = 4,
			Head = 5
		}

		public ItemCategory itemCategory;

		public string itemId = string.Empty;

		public string itemName = string.Empty;

		public string itemDescription = string.Empty;

		public bool isLocked = true;

		public string IAPProductId = string.Empty;

		public GameObject ingamePrefab;

		public GameObject menuPrefab;

		public string referenceName = string.Empty;

		public string GPGSId = string.Empty;
	}
}
