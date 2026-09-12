#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.LevelEditor
{
	public class EndZone : MonoBehaviour
	{
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(1f, 0.25f, 0.75f, 0.25f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
			Gizmos.color = new Color(1f, 0.25f, 0.75f, 1f);
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
}
