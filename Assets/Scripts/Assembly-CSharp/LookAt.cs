#pragma warning disable 0618,0619
using UnityEngine;

public class LookAt : MonoBehaviour
{
	public Transform target;

	private void Update()
	{
		if ((bool)target)
		{
			base.transform.LookAt(target.position, Vector3.up);
		}
	}
}
