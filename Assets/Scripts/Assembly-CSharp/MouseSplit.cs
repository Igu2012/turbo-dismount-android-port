#pragma warning disable 0618,0619
using UnityEngine;

public class MouseSplit : MonoBehaviour
{
	public ShatterScheduler scheduler;

	public int raycastCount = 5;

	private bool started;

	private Vector3 start;

	private Vector3 end;

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			start = Input.mousePosition;
			started = true;
		}
		if (!Input.GetMouseButtonUp(0) || !started)
		{
			return;
		}
		end = Input.mousePosition;
		Camera mainCamera = Camera.main;
		float near = mainCamera.nearClipPlane;
		Vector3 lhs = mainCamera.ScreenToWorldPoint(new Vector3(end.x, end.y, near)) - mainCamera.ScreenToWorldPoint(new Vector3(start.x, start.y, near));
		for (int i = 0; i < raycastCount; i++)
		{
			Ray ray = mainCamera.ScreenPointToRay(Vector3.Lerp(start, end, (float)i / (float)raycastCount));
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo))
			{
				Plane plane = new Plane(Vector3.Normalize(Vector3.Cross(lhs, ray.direction)), hitInfo.point);
				if (scheduler != null)
				{
					scheduler.AddTask(new SplitTask(hitInfo.collider.gameObject, new Plane[1] { plane }));
				}
				else
				{
					hitInfo.collider.SendMessage("Split", new Plane[1] { plane }, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		started = false;
	}
}
