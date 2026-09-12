#pragma warning disable 0618,0619
using UnityEngine;

public class Crusher : MonoBehaviour
{
	private enum State
	{
		Dropping = 0,
		Waiting = 1,
		Rising = 2
	}

	private Vector3 startPosition;

	private State currentState = State.Waiting;

	public float startDelay;

	private void Start()
	{
		startPosition = base.transform.position;
		base.GetComponent<Rigidbody>().useGravity = false;
		Invoke("Drop", startDelay);
	}

	private void Drop()
	{
		currentState = State.Dropping;
		base.GetComponent<Rigidbody>().useGravity = true;
		base.GetComponent<Rigidbody>().velocity = new Vector3(0f, -15f, 0f);
		Invoke("Rise", 1f);
	}

	private void Rise()
	{
		currentState = State.Rising;
		base.GetComponent<Rigidbody>().useGravity = false;
		base.GetComponent<Rigidbody>().velocity = new Vector3(0f, 15f, 0f);
	}

	private void Update()
	{
		if (base.transform.position.y >= startPosition.y && currentState == State.Rising)
		{
			base.transform.position = startPosition;
			Drop();
		}
	}
}
