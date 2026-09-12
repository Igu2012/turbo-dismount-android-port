#pragma warning disable 0618,0619
using UnityEngine;

public class MouseShatter : MonoBehaviour
{
	public ShatterScheduler scheduler;

	public void Update()
	{
		RaycastHit hitInfo;
		if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
		{
			if (scheduler != null)
			{
				scheduler.AddTask(new ShatterTask(hitInfo.collider.gameObject, hitInfo.point));
			}
			else
			{
				hitInfo.collider.SendMessage("Shatter", hitInfo.point, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
