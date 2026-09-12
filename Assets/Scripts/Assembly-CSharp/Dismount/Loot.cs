#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount
{
	public class Loot : MonoBehaviour
	{
		[NonSerialized]
		public LootBox owner;

		[NonSerialized]
		public Rigidbody rb;

		[NonSerialized]
		public int index = -1;

		[NonSerialized]
		public float dieTick = -1f;

		[NonSerialized]
		public bool pickedUp;

		[NonSerialized]
		public int pickUpLayer = 9;

		private void OnCollisionEnter(Collision collision)
		{
			if (!(dieTick < 0f) || !pickedUp)
			{
				GameObject gameObject = collision.gameObject;
				if (gameObject.layer == pickUpLayer)
				{
					pickedUp = true;
					owner.OnLootPickedUp(index);
				}
			}
		}
	}
}
