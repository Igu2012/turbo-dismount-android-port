#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace Dismount.LevelEditor
{
	public class NPCVehicleSpawner : MonoBehaviour
	{
		public NPCVehicleData[] vehicles;

		public int numVehiclesToSpawn = 1;

		public float spawnInterval = 1f;

		public float vehicleVelocity = 20f;

		public string followPathName = string.Empty;

		[HideInInspector]
		public List<string> vehicleIds;

		private void OnDrawGizmos()
		{
			BoxCollider boxCollider = base.GetComponent<Collider>() as BoxCollider;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
			Gizmos.DrawCube(boxCollider.center, boxCollider.size);
		}
	}
}
