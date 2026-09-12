#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	public class TankGun : MonoBehaviour
	{
		public Transform nozzle;

		public AudioSource fireAudio;

		public GameObject shotPrefab;

		private Rigidbody thisrb;

		public float cycleTime = 1f;

		public float cycleOffset;

		private float lastShotTime = -1f;

		private List<GameObject> inactiveShots = new List<GameObject>();

		private List<GameObject> activeShots = new List<GameObject>();

		private GameObject pool;

		private void Start()
		{
			thisrb = base.GetComponent<Rigidbody>();
			string text = base.gameObject.name;
			string text2 = text + "_ShotPool";
			pool = new GameObject(text2);
			pool.transform.position = Vector3.zero;
			pool.transform.rotation = Quaternion.identity;
			pool.transform.localScale = Vector3.one;
			for (int i = 0; i < 5; i++)
			{
				GameObject gameObject = Object.Instantiate(shotPrefab) as GameObject;
				gameObject.name = text + "_Shot";
				TankGunShell component = gameObject.GetComponent<TankGunShell>();
				if ((bool)component)
				{
					component.owner = this;
				}
				gameObject.SetActive(false);
				gameObject.transform.parent = pool.transform;
				inactiveShots.Add(gameObject);
			}
		}

		private void OnDestroy()
		{
			if ((bool)pool)
			{
				Object.Destroy(pool);
			}
		}

		public void Fire()
		{
			if (!(lastShotTime >= 0f))
			{
				lastShotTime = Time.fixedTime - cycleTime + cycleOffset;
			}
		}

		public void StopFiring()
		{
			lastShotTime = -1f;
		}

		public void DestroyShot(GameObject shot)
		{
			if (activeShots.Contains(shot))
			{
				activeShots.Remove(shot);
				shot.SetActive(false);
				inactiveShots.Add(shot);
			}
		}

		private GameObject NewShot()
		{
			if (inactiveShots.Count < 1)
			{
				return null;
			}
			GameObject gameObject = inactiveShots[inactiveShots.Count - 1];
			inactiveShots.RemoveAt(inactiveShots.Count - 1);
			activeShots.Add(gameObject);
			gameObject.SetActive(true);
			return gameObject;
		}

		private void FixedUpdate()
		{
			if (!(lastShotTime < 0f) && Time.fixedTime > lastShotTime + cycleTime)
			{
				FireShot();
			}
		}

		private void FireShot()
		{
			GameObject gameObject = NewShot();
			if (!(gameObject == null))
			{
				Transform transform = gameObject.transform;
				Vector3 position = nozzle.position;
				Vector3 pointVelocity = thisrb.GetPointVelocity(position);
				transform.position = position;
				transform.rotation = nozzle.rotation;
				gameObject.GetComponent<Rigidbody>().velocity = pointVelocity + nozzle.forward * 50f;
				lastShotTime = Time.fixedTime;
				if ((bool)fireAudio)
				{
					fireAudio.pitch = Random.Range(0.4f, 0.5f);
					fireAudio.Play();
				}
				DismountGame.particleManager.EmitTankGunShot(position, nozzle.forward, pointVelocity + nozzle.forward * 15f);
				thisrb.AddForceAtPosition(nozzle.up * 2000f, position, ForceMode.Impulse);
			}
		}

		private void TurboBoostStarted()
		{
			Fire();
		}

		private void TurboBoostEnded()
		{
			StopFiring();
		}
	}
}
