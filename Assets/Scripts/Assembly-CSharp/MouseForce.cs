#pragma warning disable 0618,0619
using UnityEngine;

public class MouseForce : MonoBehaviour
{
	public float impulseScale = 25f;

	private Rigidbody grabBody;

	private Vector3 grabPoint;

	private float grabDistance;

	public void Update()
	{
		GrabBody();
		ReleaseBody();
	}

	public void FixedUpdate()
	{
		MoveBody();
	}

	private void GrabBody()
	{
		RaycastHit hitInfo;
		if (grabBody == null && Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo) && hitInfo.rigidbody != null)
		{
			grabBody = hitInfo.rigidbody;
			grabPoint = grabBody.transform.InverseTransformPoint(hitInfo.point);
			grabDistance = hitInfo.distance;
		}
	}

	private void ReleaseBody()
	{
		if (grabBody != null && Input.GetMouseButtonUp(0))
		{
			grabBody = null;
		}
	}

	private void MoveBody()
	{
		if (grabBody != null)
		{
			Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, grabDistance);
			Vector3 vector = Camera.main.ScreenToWorldPoint(position);
			Vector3 vector2 = grabBody.transform.TransformPoint(grabPoint);
			Debug.DrawLine(vector, vector2, Color.red);
			Vector3 force = (vector - vector2) * (impulseScale * Time.fixedDeltaTime);
			grabBody.AddForceAtPosition(force, vector2, ForceMode.Impulse);
		}
	}
}
