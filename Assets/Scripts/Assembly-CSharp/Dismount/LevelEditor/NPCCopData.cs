#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount.LevelEditor
{
	public class NPCCopData : MonoBehaviour
	{
		public float observeSectorAngle = 45f;

		public float observeSectorRadius = 50f;

		public float observeOmniRadius = 10f;

		private void OnDrawGizmosSelected()
		{
			if (base.enabled)
			{
				observeSectorAngle = Mathf.Clamp(observeSectorAngle, 0f, 180f);
				observeSectorRadius = Mathf.Clamp(observeSectorRadius, 0f, 1000f);
				observeOmniRadius = Mathf.Clamp(observeOmniRadius, 0f, 1000f);
				Gizmos.color = new Color(0.25f, 0.5f, 1f, 1f);
				Gizmos.matrix = base.transform.localToWorldMatrix;
				int num = 72;
				float num2 = (float)Math.PI * 2f / (float)num;
				for (int i = 0; i < num; i++)
				{
					float f = (float)i * num2;
					float f2 = (float)(i + 1) * num2;
					Gizmos.DrawLine(new Vector3(Mathf.Sin(f) * observeOmniRadius, 1f, Mathf.Cos(f) * observeOmniRadius), new Vector3(Mathf.Sin(f2) * observeOmniRadius, 1f, Mathf.Cos(f2) * observeOmniRadius));
				}
				num = Mathf.FloorToInt(observeSectorAngle / 5f);
				num = Mathf.Max(num, 2);
				float num3 = observeSectorAngle / 180f * (float)Math.PI;
				num2 = num3 / (float)num;
				float f3 = num3 * 0.5f;
				Gizmos.DrawLine(new Vector3(0f, 1f, 0f), new Vector3(Mathf.Sin(f3) * observeSectorRadius, 1f, Mathf.Cos(f3) * observeSectorRadius));
				f3 = num3 * -0.5f;
				Gizmos.DrawLine(new Vector3(0f, 1f, 0f), new Vector3(Mathf.Sin(f3) * observeSectorRadius, 1f, Mathf.Cos(f3) * observeSectorRadius));
				for (int j = 0; j < num; j++)
				{
					float f4 = f3 + (float)j * num2;
					float f5 = f3 + (float)(j + 1) * num2;
					Gizmos.DrawLine(new Vector3(Mathf.Sin(f4) * observeSectorRadius, 1f, Mathf.Cos(f4) * observeSectorRadius), new Vector3(Mathf.Sin(f5) * observeSectorRadius, 1f, Mathf.Cos(f5) * observeSectorRadius));
				}
			}
		}
	}
}
