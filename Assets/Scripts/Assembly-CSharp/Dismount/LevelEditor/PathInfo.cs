#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.LevelEditor
{
	public class PathInfo : MonoBehaviour
	{
		public enum PathType
		{
			Steering = 0,
			NPC = 1
		}

		public PathType pathType;

		public bool loop;
	}
}
