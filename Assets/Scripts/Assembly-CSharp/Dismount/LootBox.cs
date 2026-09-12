#pragma warning disable 0618,0619
using Dismount.Vehicular;
using UnityEngine;
using reLive;

namespace Dismount
{
	public class LootBox : MonoBehaviour
	{
		[Header("Timing")]
		public Vector2 lockOpen = new Vector2(0f, 0.2f);

		public Vector2 lidOpen = new Vector2(0.3f, 0.5f);

		public Vector2 shootLoot = new Vector2(0.6f, 0.8f);

		public float activateLoot = 1f;

		[Space(10f)]
		public float triggerRadius = 15f;

		[Space(10f)]
		public Transform lockTransform;

		public HingeJoint lidHinge;

		public Transform lidCheck;

		public Transform boxCheck;

		public LootBoxGlow[] glows;

		[Space(10f)]
		public GameObject[] loots;

		public AudioClip[] lootBoxSounds;

		public AudioSource lootBoxAudio;

		public AudioSource lootPickUpAudio;

		private bool isOpen;

		private Loot[] loot;

		private Transform xf;

		private Transform lootRoot;

		private float tick = -1f;

		private float lastPickUpSoundTick = -1f;

		private Transform magnetTransform;

		private int lootType;

		private void Awake()
		{
			xf = base.transform;
		}

		private void Start()
		{
			SelectLoot();
		}

		private void OnDismountStarted()
		{
			for (int i = 0; i < loot.Length; i++)
			{
				Rigidbody rb = loot[i].rb;
				rb.Sleep();
			}
			magnetTransform = DismountGame.playerState.currentCharacterInstance.GetComponent<MrDismount>().rideNode.transform;
		}

		private void SelectLoot()
		{
			lootType = Random.Range(0, 3);
			GameObject gameObject = loots[lootType];
			lootRoot = gameObject.transform;
			loot = lootRoot.GetComponentsInChildren<Loot>(true);
			for (int i = 0; i < 3; i++)
			{
				if (i == lootType)
				{
					loots[i].SetActive(true);
				}
				else
				{
					Object.Destroy(loots[i]);
				}
			}
			for (int j = 0; j < loot.Length; j++)
			{
				loot[j].owner = this;
				loot[j].rb = loot[j].GetComponent<Rigidbody>();
				loot[j].index = j;
				loot[j].dieTick = -1f;
				loot[j].pickUpLayer = LayerMask.NameToLayer("Character");
			}
		}

		private void FixedUpdate()
		{
			float num = 1f;
			if (!isOpen)
			{
				num = Mathf.InverseLerp(0f, 0.5f, (lidCheck.position - boxCheck.position).magnitude);
				if (num >= 1f)
				{
					isOpen = true;
					Trigger();
				}
			}
			for (int i = 0; i < glows.Length; i++)
			{
				Color tint = glows[i].tint;
				tint.a = 0.25f * (1f - num * num);
				glows[i].tint = tint;
			}
			if (tick < 0f && magnetTransform != null && (magnetTransform.position - xf.position).sqrMagnitude < triggerRadius * triggerRadius)
			{
				Trigger();
			}
			if (tick < 0f)
			{
				return;
			}
			float num2 = Mathf.InverseLerp(lockOpen.x, lockOpen.y, tick);
			if (num2 > 0f)
			{
				Vector3 localEulerAngles = lockTransform.localEulerAngles;
				localEulerAngles.z = num2 * -90f;
				lockTransform.localEulerAngles = localEulerAngles;
			}
			num2 = Mathf.InverseLerp(lidOpen.x, lidOpen.y, tick);
			if (num2 > 0f && lidHinge != null)
			{
				float targetPosition = num2 * -70f;
				JointSpring spring = lidHinge.spring;
				spring.targetPosition = targetPosition;
				lidHinge.spring = spring;
			}
			num2 = Mathf.InverseLerp(shootLoot.x, shootLoot.y, tick);
			float num3 = -1f;
			if (tick >= activateLoot && tick - Time.fixedDeltaTime < activateLoot)
			{
				num3 = 4f;
			}
			for (int j = 0; j < loot.Length; j++)
			{
				if (loot[j] == null)
				{
					continue;
				}
				if (num3 > 0f)
				{
					loot[j].dieTick = num3;
					num3 += 0.05f;
				}
				if (loot[j].dieTick > 0f && tick > loot[j].dieTick)
				{
					Replay.Instance.RemoveRecordedObject(loot[j].gameObject);
					Object.Destroy(loot[j].gameObject);
					loot[j] = null;
					continue;
				}
				Rigidbody rb = loot[j].rb;
				if (num2 > 0f && num2 < 0.99f)
				{
					Vector3 force = (rb.position - xf.position + xf.up).normalized * 200f * (1f - num2);
					rb.AddForce(force, ForceMode.Acceleration);
				}
				if (loot[j].dieTick > 0f && !loot[j].pickedUp)
				{
					Vector3 vector = magnetTransform.position - rb.position;
					float sqrMagnitude = vector.sqrMagnitude;
					if (sqrMagnitude <= 625f)
					{
						float num4 = Mathf.Max(10f, 400f / (1f + sqrMagnitude));
						vector = num4 * vector.normalized;
						rb.AddForce(vector, ForceMode.Acceleration);
					}
				}
			}
			tick += Time.fixedDeltaTime;
		}

		private void Trigger()
		{
			if (!(tick >= 0f))
			{
				lootRoot.parent = null;
				tick = 0f;
				lootBoxAudio.clip = lootBoxSounds[lootType];
				lootBoxAudio.Play();
			}
		}

		public void OnLootPickedUp(int index)
		{
			if (!(loot[index] == null))
			{
				if (tick > lastPickUpSoundTick + 0.1f)
				{
					float num = 1f - (float)lootType * 0.2f;
					lootPickUpAudio.pitch = Random.Range(0.8f, 1.25f) * num;
					OneShotAudioHelper.PlayClipAtPoint(lootPickUpAudio.clip, loot[index].rb.position, 1f, lootPickUpAudio);
					lastPickUpSoundTick = tick;
				}
				loot[index].dieTick = tick + 0.33f;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!(tick >= 0f))
			{
				if (other.GetComponent<Vehicle>() != null)
				{
					Trigger();
				}
				else if (other.gameObject.layer == LayerMask.NameToLayer("Character"))
				{
					Trigger();
				}
			}
		}
	}
}
