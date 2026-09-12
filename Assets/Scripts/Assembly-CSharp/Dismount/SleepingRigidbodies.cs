#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class SleepingRigidbodies : MonoBehaviour
	{
		private void Start()
		{
			Rigidbody[] componentsInChildren = base.transform.GetComponentsInChildren<Rigidbody>();
			Rigidbody[] array = componentsInChildren;
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.Sleep();
			}
		}
	}
}
