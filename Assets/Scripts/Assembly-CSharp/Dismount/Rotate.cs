#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class Rotate : MonoBehaviour
	{
		public Space space;

		public Vector3 speed;

		private void Update()
		{
			base.transform.Rotate(Time.deltaTime * speed, space);
		}
	}
}
