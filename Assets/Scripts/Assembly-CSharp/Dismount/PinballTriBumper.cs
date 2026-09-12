#pragma warning disable 0618,0619
using Dismount.GameStates;
using UnityEngine;
using reLive;

namespace Dismount
{
	public class PinballTriBumper : MonoBehaviour
	{
		private const float thresholdVelocity = 2f;

		private const float actDuration = 0.125f;

		private const float returnDuration = 0.375f;

		private const float totalDuration = 0.5f;

		public MeshRenderer bumperMeshRenderer;

		private Material bumperMaterial;

		private Color impactColor = Color.red;

		private float actTime;

		private float targetMove = 1f;

		private Vector3 originalScale = Vector3.one;

		private Vector3 originalPosition = Vector3.zero;

		private Vector3 targetPosition = Vector3.zero;

		private Rigidbody bumperRigidbody;

		private Dismount.GameStates.Dismount gameLogic;

		private void Awake()
		{
			originalPosition = base.transform.position;
			bumperMaterial = bumperMeshRenderer.material;
			impactColor = bumperMaterial.GetColor("_ImpactColor");
			originalScale = bumperMeshRenderer.transform.localScale;
			bumperRigidbody = base.GetComponent<Rigidbody>();
			Transform transform = base.transform.Find("TargetMove");
			targetMove = (transform.position - originalPosition).magnitude;
			if (targetMove > 3f)
			{
				targetMove = 3f;
			}
		}

		public void OnDismountStarted()
		{
			originalPosition = base.transform.position;
			gameLogic = DismountGame.stateManager.currentStateObject.GetComponent<Dismount.GameStates.Dismount>();
		}

		private void FixedUpdate()
		{
			if (actTime <= 0f)
			{
				return;
			}
			if (Time.fixedTime > actTime + 0.5f)
			{
				actTime = 0f;
				impactColor.a = 0f;
				bumperMaterial.SetColor("_ImpactColor", impactColor);
				bumperMeshRenderer.transform.localScale = originalScale;
				bumperRigidbody.MovePosition(originalPosition);
				return;
			}
			float num = Time.fixedTime - actTime;
			float num2 = 1f - num / 0.5f;
			impactColor.a = num2;
			bumperMaterial.SetColor("_ImpactColor", impactColor);
			float num3 = num2 * 0.2f;
			bumperMeshRenderer.transform.localScale = originalScale + new Vector3(num3, 0f, num3);
			if (num < 0.125f)
			{
				bumperRigidbody.MovePosition(Vector3.Lerp(originalPosition, targetPosition, num / 0.125f));
			}
			else
			{
				bumperRigidbody.MovePosition(Vector3.Lerp(targetPosition, originalPosition, (num - 0.125f) / 0.375f));
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (actTime > 0f && Time.fixedTime < actTime + 0.5f)
			{
				return;
			}
			Collider collider = collision.collider;
			if (collider.isTrigger || collision.contacts.Length == 0)
			{
				return;
			}
			Rigidbody attachedRigidbody = collider.attachedRigidbody;
			if (attachedRigidbody == null || attachedRigidbody.mass < 1.1f || collision.relativeVelocity.sqrMagnitude < 4f)
			{
				return;
			}
			float num = Mathf.Sign(Vector3.Dot(collision.contacts[0].point - base.transform.position, base.transform.forward));
			if (!(num < 0f))
			{
				targetPosition = originalPosition + base.transform.forward * targetMove;
				actTime = Time.fixedTime;
				base.GetComponent<AudioSource>().pitch = Random.Range(0.6f, 1.2f);
				OneShotAudioHelper.PlayClipAtPoint(base.GetComponent<AudioSource>().clip, originalPosition + Vector3.up, 1f, base.GetComponent<AudioSource>());
				if ((bool)gameLogic)
				{
					gameLogic.AddImpactScore(0, 1000, Dismount.GameStates.Dismount.ScoreCategory.Ragdoll);
				}
			}
		}
	}
}
