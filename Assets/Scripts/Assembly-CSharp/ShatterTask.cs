#pragma warning disable 0618,0619
using UnityEngine;

public class ShatterTask : IShatterTask
{
	private GameObject gameObject;

	private Vector3 localPoint;

	public ShatterTask(GameObject gameObject, Vector3 point)
	{
		this.gameObject = gameObject;
		localPoint = gameObject.transform.InverseTransformPoint(point);
	}

	public void Run()
	{
		if ((bool)gameObject)
		{
			Vector3 vector = gameObject.transform.TransformPoint(localPoint);
			gameObject.SendMessage("Shatter", vector, SendMessageOptions.DontRequireReceiver);
		}
	}
}
