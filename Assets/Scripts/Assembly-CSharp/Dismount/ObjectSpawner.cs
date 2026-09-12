#pragma warning disable 0618,0619
using System.Collections.Generic;
using Dismount.LevelEditor;
using UnityEngine;
using reLive;

namespace Dismount
{
	public class ObjectSpawner : MonoBehaviour
	{
		public GameObject[] objectPrefabs;

		public int numObjectsToSpawn = 1;

		public float spawnInterval = 1f;

		public float overrideObjectVelocity = -1f;

		public Spline followPath;

		private float previousSpawnTick = -1f;

		private int numObjectSpawned;

		private int occupiers;

		private List<GameObject> respawnObjects = new List<GameObject>();

		private void Start()
		{
			SpawnObject();
		}

		public void Respawn(GameObject obj)
		{
			if (!respawnObjects.Contains(obj))
			{
				respawnObjects.Add(obj);
				obj.transform.position += Vector3.up * -10000f;
			}
		}

		private bool RespawnObject()
		{
			if (occupiers > 0)
			{
				return false;
			}
			GameObject gameObject = respawnObjects[0];
			gameObject.transform.position = base.transform.position;
			gameObject.transform.rotation = base.transform.rotation;
			MovingObject component = gameObject.GetComponent<MovingObject>();
			if ((bool)component && (bool)followPath)
			{
				component.SetFollowPath(followPath);
			}
			NotifyCop(gameObject);
			respawnObjects.Remove(gameObject);
			return true;
		}

		private void NotifyCop(GameObject obj)
		{
			Cop component = obj.GetComponent<Cop>();
			if (component != null)
			{
				component.isTrafficCop = true;
				if (DismountGame.level.isDismountActive)
				{
					component.Chase(new Rigidbody[2]
					{
						DismountGame.playerState.currentVehicleInstance.GetComponent<Rigidbody>(),
						DismountGame.playerState.currentCharacterInstance.GetComponent<MrDismount>().cameraTarget.GetComponent<Rigidbody>()
					});
				}
			}
		}

		private void SpawnObject()
		{
			if (numObjectSpawned >= numObjectsToSpawn || occupiers > 0 || objectPrefabs == null || objectPrefabs.Length < 0)
			{
				return;
			}
			previousSpawnTick = Time.fixedTime;
			GameObject gameObject = Object.Instantiate(objectPrefabs[Random.Range(0, objectPrefabs.Length)], base.transform.position, base.transform.rotation) as GameObject;
			MovingObject component = gameObject.GetComponent<MovingObject>();
			NPCVehicleData component2 = gameObject.GetComponent<NPCVehicleData>();
			if ((bool)component)
			{
				component.GetComponent<Rigidbody>().isKinematic = true;
				if ((bool)followPath)
				{
					component.SetFollowPath(followPath);
				}
				if (overrideObjectVelocity > 0f)
				{
					component2.velocity = overrideObjectVelocity;
				}
				component.SetSpawner(this);
			}
			NotifyCop(gameObject);
			Replay instance = Replay.Instance;
			if (instance.Activity == Replay.ReplayActivity.Record && instance.IsRunning)
			{
				instance.AddObjectForRecording(gameObject);
			}
			gameObject.transform.parent = base.transform.parent;
			numObjectSpawned++;
		}

		private void FixedUpdate()
		{
			bool flag = false;
			if (respawnObjects.Count > 0)
			{
				flag = RespawnObject();
			}
			if (!flag && Time.fixedTime - previousSpawnTick > spawnInterval)
			{
				SpawnObject();
			}
		}

		private void OnTriggerEnter()
		{
			occupiers++;
		}

		private void OnTriggerExit()
		{
			occupiers--;
		}

		private void OnDrawGizmos()
		{
			if (occupiers > 0)
			{
				Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
			}
			else
			{
				Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
			}
			Gizmos.matrix = base.transform.localToWorldMatrix;
			BoxCollider boxCollider = base.GetComponent<Collider>() as BoxCollider;
			Gizmos.DrawCube(boxCollider.center, boxCollider.size);
		}
	}
}
