#pragma warning disable 0618,0619
using UnityEngine;

public class SplitTask : IShatterTask
{
	private GameObject gameObject;

	private Vector3[] localPoints;

	private Vector3[] localNormals;

	public SplitTask(GameObject gameObject, Plane[] planes)
	{
		this.gameObject = gameObject;
		localPoints = new Vector3[planes.Length];
		localNormals = new Vector3[planes.Length];
		for (int i = 0; i < planes.Length; i++)
		{
			Plane plane = planes[i];
			localPoints[i] = gameObject.transform.InverseTransformPoint(plane.normal * (0f - plane.distance));
			localNormals[i] = gameObject.transform.InverseTransformDirection(plane.normal);
		}
	}

	public void Run()
	{
		if ((bool)gameObject)
		{
			Plane[] array = new Plane[localPoints.Length];
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 inPoint = gameObject.transform.TransformPoint(localPoints[i]);
				Vector3 inNormal = gameObject.transform.TransformDirection(localNormals[i]);
				array[i] = new Plane(inNormal, inPoint);
			}
			gameObject.SendMessage("Split", array, SendMessageOptions.DontRequireReceiver);
		}
	}
}
