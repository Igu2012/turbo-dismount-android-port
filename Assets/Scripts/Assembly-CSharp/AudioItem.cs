#pragma warning disable 0618,0619
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioItem : MonoBehaviour
{
	public AudioClip[] clips;

	[Range(0.2f, 5f)]
	public float minPitchMultiplier = 1f;

	[Range(0.2f, 5f)]
	public float maxPitchMultiplier = 1f;
}
