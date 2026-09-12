#pragma warning disable 0618,0619
using Dismount;
using UnityEngine;

public class ActivateMovingObject : MonoBehaviour
{
	public MovingObject movingObject;

	public float velocity = 30f;

	public void OnLevelLoaded()
	{
		movingObject.gameObject.SetActive(false);
	}

	public void OnDismountStarted()
	{
		movingObject.gameObject.SetActive(true);
		movingObject.StartMoving(velocity);
	}

	public void OnDismountReset()
	{
	}
}
