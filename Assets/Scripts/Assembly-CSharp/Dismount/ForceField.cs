#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	public class ForceField : MonoBehaviour
	{
		public GameObject effectPrefab;

		private List<GameObject> inactiveEffects = new List<GameObject>();

		private List<GameObject> activeEffects = new List<GameObject>();

		private void Awake()
		{
			GameObject gameObject = GameObject.Find("ForceFieldEffectPool");
			if (gameObject == null)
			{
				gameObject = new GameObject("ForceFieldEffectPool");
				gameObject.transform.position = Vector3.zero;
				gameObject.transform.rotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
			}
			for (int i = 0; i < 10; i++)
			{
				GameObject gameObject2 = Object.Instantiate(effectPrefab) as GameObject;
				gameObject2.name = base.gameObject.name + "_Effect";
				gameObject2.SetActive(false);
				gameObject2.transform.parent = gameObject.transform;
				inactiveEffects.Add(gameObject2);
			}
		}

		public void EffectDied(GameObject effect)
		{
			if (activeEffects.Contains(effect))
			{
				activeEffects.Remove(effect);
				effect.SetActive(false);
				inactiveEffects.Add(effect);
			}
		}

		private GameObject ActivateEffect()
		{
			if (inactiveEffects.Count < 1)
			{
				return null;
			}
			GameObject gameObject = inactiveEffects[inactiveEffects.Count - 1];
			inactiveEffects.RemoveAt(inactiveEffects.Count - 1);
			activeEffects.Add(gameObject);
			return gameObject;
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!effectPrefab || inactiveEffects.Count <= 0 || collision.contacts.Length <= 0)
			{
				return;
			}
			Vector3 point = collision.contacts[0].point;
			Vector3 normal = collision.contacts[0].normal;
			float num = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, normal));
			if (!(num < 0.2f))
			{
				float num2 = 100f;
				if ((bool)collision.rigidbody)
				{
					num2 = collision.rigidbody.mass;
				}
				float intensity = Mathf.Clamp01(num2 * num * num * 0.005f);
				point.x = Mathf.Round(2f * point.x) * 0.5f;
				point.y = Mathf.Round(2f * point.y) * 0.5f;
				point.z = Mathf.Round(2f * point.z) * 0.5f;
				Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);
				GameObject gameObject = ActivateEffect();
				if ((bool)gameObject)
				{
					gameObject.transform.position = point;
					gameObject.transform.rotation = rotation;
					ForceFieldEffect component = gameObject.GetComponent<ForceFieldEffect>();
					component.intensity = intensity;
					component.spawner = this;
					gameObject.SetActive(true);
				}
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(1f, 0f, 0.5f, 0.125f);
			Gizmos.DrawCube(base.GetComponent<Collider>().bounds.center, base.GetComponent<Collider>().bounds.size);
		}
	}
}
