#pragma warning disable 0618,0619
using UnityEngine;

public class MouseInstantiate : MonoBehaviour
{
	public GameObject prefabToInstantiate;

	public float speed = 7f;

	public void Update()
	{
		if (Input.GetMouseButtonDown(0) && prefabToInstantiate != null)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			GameObject gameObject = (GameObject)Object.Instantiate(prefabToInstantiate, ray.origin, Quaternion.identity);
			if (gameObject.GetComponent<Rigidbody>() != null)
			{
				gameObject.GetComponent<Rigidbody>().velocity = ray.direction * speed;
			}
		}
	}
}
