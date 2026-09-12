#pragma warning disable 0618,0619
using UnityEngine;

public class PreviewCameraAnimation : MonoBehaviour
{
	private Vector3 startPosition;

	private void Start()
	{
		startPosition = base.transform.position;
	}

	private void Update()
	{
		base.transform.position = new Vector3(startPosition.x, startPosition.y + Mathf.Sin(Time.fixedTime * 1.3f) * 2f, startPosition.z);
	}
}
