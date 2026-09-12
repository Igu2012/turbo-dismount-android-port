using System;
using UnityEngine;

[Serializable]
public class cameraViewObject : MonoBehaviour
{
	public GameObject guiObject;

	private Vector2 rotationVelocity;

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		bool flag = false;
		if (guiObject != null)
		{
			GUITexture[] componentsInChildren = guiObject.GetComponentsInChildren<GUITexture>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].HitTest(Input.mousePosition))
				{
					flag = true;
				}
			}
		}
		if (!flag && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
		{
			rotationVelocity.x += Mathf.Pow(Mathf.Abs(Input.GetAxis("Mouse X")), 1.5f) * Mathf.Sign(Input.GetAxis("Mouse X"));
			rotationVelocity.y -= Input.GetAxis("Mouse Y") * 0.04f;
		}
		float y = transform.position.y + rotationVelocity.y;
		Vector3 position = transform.position;
		float num = (position.y = y);
		Vector3 vector = (transform.position = position);
		transform.RotateAround(Vector3.zero, Vector3.up, rotationVelocity.x);
		rotationVelocity = Vector2.Lerp(rotationVelocity, Vector2.zero, Time.deltaTime * 10f);
		float y2 = Mathf.Clamp(transform.position.y, 0f, 5f);
		Vector3 position2 = transform.position;
		float num2 = (position2.y = y2);
		Vector3 vector3 = (transform.position = position2);
		transform.LookAt(new Vector3(0f, 1f, 0f));
	}

	public virtual void Main()
	{
	}
}
