#pragma warning disable 0618,0619
using UnityEngine;

public class StartAlarm : MonoBehaviour
{
	public AudioSource alarm;

	private void OnDismountStarted()
	{
		alarm.Play();
	}
}
