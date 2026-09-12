#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.LevelEditor
{
	public class PathNode : MonoBehaviour
	{
		public float customValue;

		private void OnDrawGizmos()
		{
			OnDrawPathNodeGizmo(false);
		}

		private void OnDrawGizmosSelected()
		{
			OnDrawPathNodeGizmo(true);
		}

		private void OnDrawPathNodeGizmo(bool selected)
		{
			PathInfo.PathType pathType = PathInfo.PathType.Steering;
			if ((bool)base.transform.parent)
			{
				PathInfo component = base.transform.parent.GetComponent<PathInfo>();
				if ((bool)component)
				{
					pathType = component.pathType;
				}
			}
			float a = ((!selected) ? 0.4f : 1f);
			if (pathType == PathInfo.PathType.Steering)
			{
				Gizmos.color = new Color(1f, 0.625f, 0f, a);
			}
			else
			{
				Gizmos.color = new Color(0f, 1f, 0f, a);
			}
			Gizmos.DrawSphere(base.transform.position, 0.5f);
		}
	}
}
