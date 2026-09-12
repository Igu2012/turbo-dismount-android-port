#pragma warning disable 0618,0619
using UnityEngine;

public class SpinObject : MonoBehaviour
{
	public float speed = 1f;

	private float time;

	private void Update()
	{
		time += Time.deltaTime * speed;
		base.transform.localRotation = Quaternion.AngleAxis(360f * time, Vector3.up);
	}
}
