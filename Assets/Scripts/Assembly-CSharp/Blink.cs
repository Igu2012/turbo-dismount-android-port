#pragma warning disable 0618,0619
using UnityEngine;

public class Blink : MonoBehaviour
{
	public float delayOff = 3f;

	public float delayOn = 1f;

	private void Start()
	{
		Invoke("Off", delayOff);
	}

	private void Off()
	{
		GetComponent<UISprite>().enabled = false;
		Invoke("On", delayOn);
	}

	private void On()
	{
		GetComponent<UISprite>().enabled = true;
		Invoke("Off", delayOff);
	}
}
