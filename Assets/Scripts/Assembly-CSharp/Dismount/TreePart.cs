#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class TreePart : MonoBehaviour
	{
		private void OnJointBreak()
		{
			base.transform.parent.SendMessage("TreePartBroken");
		}
	}
}
