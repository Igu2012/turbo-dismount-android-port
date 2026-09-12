#pragma warning disable 0618,0619
using System.Collections.Generic;
using Dismount;
using UnityEngine;

public class Bomb : MonoBehaviour
{
	private bool activated;

	private GameObject visual;

	private void Start()
	{
		visual = base.transform.Find("Visual").gameObject;
	}

	private void OnTriggerEnter()
	{
		if (activated)
		{
			return;
		}
		activated = true;
		Vector3 position = base.transform.position;
		float num = 15f;
		Collider[] array = Physics.OverlapSphere(position, num);
		List<Rigidbody> list = new List<Rigidbody>();
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].isTrigger)
			{
				Rigidbody attachedRigidbody = array[i].attachedRigidbody;
				if ((bool)attachedRigidbody && !list.Contains(attachedRigidbody))
				{
					list.Add(attachedRigidbody);
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Rigidbody rigidbody = list[j];
			if (rigidbody.useGravity)
			{
				rigidbody.isKinematic = false;
			}
			float mass = rigidbody.mass;
			float num2 = 0f;
			num2 = ((!(mass < 10f)) ? ((!(mass < 100f)) ? ((!(mass < 1000f)) ? ((!(mass < 10000f)) ? ((!(mass < 100000f)) ? 111120f : (21120f + (mass - 10000f))) : (3120f + 2f * (mass - 1000f))) : (420f + 3f * (mass - 100f))) : (60f + 4f * (mass - 10f))) : (6f * mass));
			num2 += (float)Random.Range(25, 100);
			rigidbody.AddExplosionForce(num2, position, num, 3f, ForceMode.Impulse);
		}
		DismountGame.audioManager.PlaySoundEffect("Explosion", base.transform, 2f);
		DismountGame.particleManager.EmitExplosion(position, new Vector3(0f, 1f, 0f));
		DismountGame.playerState.statistics.MineDetonated();
		visual.SetActive(false);
	}
}
