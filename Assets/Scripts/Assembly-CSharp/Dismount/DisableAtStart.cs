#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class DisableAtStart : MonoBehaviour
	{
		private void Start()
		{
			base.gameObject.SetActive(false);
		}
	}
}
