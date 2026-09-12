#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class CastToGround : MonoBehaviour
	{
		public enum CastDirection
		{
			LocalY = 0,
			WorldY = 1
		}

		public CastDirection castDirection;

		public float rayStartOffset = 1f;

		public float rayLength = 1000f;

		public float hitYOffset;

		public bool orientWithHitNormal;

		public LayerMask layerMask = 512;

		public bool castAtAwake;

		private void Awake()
		{
			if (castAtAwake)
			{
				Cast();
			}
		}

		public void Cast()
		{
			Vector3 up = Vector3.up;
			if (castDirection == CastDirection.LocalY)
			{
				up = base.transform.up;
			}
			RaycastHit hitInfo;
			if (Physics.Raycast(base.transform.position + up * rayStartOffset, -up, out hitInfo, rayLength, layerMask))
			{
				base.transform.position = hitInfo.point + up * hitYOffset;
				if (orientWithHitNormal)
				{
					base.transform.rotation = Quaternion.LookRotation(Vector3.Cross(base.transform.right, hitInfo.normal), Vector3.up);
				}
			}
		}
	}
}
