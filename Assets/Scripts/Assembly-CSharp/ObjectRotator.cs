#pragma warning disable 0618,0619
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
	public Vector3 axis = new Vector3(0f, 1f, 0f);

	public float angularVelocity = 1f;

	private void FixedUpdate()
	{
		base.transform.Rotate(axis, angularVelocity * Time.fixedDeltaTime);
	}
}
