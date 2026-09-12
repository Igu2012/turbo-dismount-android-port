#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.LevelEditor
{
	public class NPCVehicleCatcher : MonoBehaviour
	{
		private void OnDrawGizmos()
		{
			BoxCollider boxCollider = base.GetComponent<Collider>() as BoxCollider;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
			Gizmos.DrawCube(boxCollider.center, boxCollider.size);
		}
	}
}
