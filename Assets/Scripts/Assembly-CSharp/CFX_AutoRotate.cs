#pragma warning disable 0618,0619
using UnityEngine;

public class CFX_AutoRotate : MonoBehaviour
{
	public Vector3 rotation;

	public Space space = Space.Self;

	private void Update()
	{
		base.transform.Rotate(rotation * Time.deltaTime, space);
	}
}
