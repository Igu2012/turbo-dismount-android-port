using UnityEngine;

public class EveryplayHudCamera : MonoBehaviour
{
	private const int EPSR = 1162892114;

	private bool subscribed;

	private bool readyForRecording;

	private void Awake()
	{
		Debug.Log("Everyplay HUD-less functionality is no longer maintained and may not function properly.");
		Subscribe(true);
		readyForRecording = Everyplay.IsReadyForRecording();
	}

	private void OnDestroy()
	{
		Subscribe(false);
	}

	private void OnEnable()
	{
		Subscribe(true);
	}

	private void OnDisable()
	{
		Subscribe(false);
	}

	private void Subscribe(bool subscribe)
	{
		if (!subscribed && subscribe)
		{
			Everyplay.ReadyForRecording += ReadyForRecording;
		}
		else if (subscribed && !subscribe)
		{
			Everyplay.ReadyForRecording -= ReadyForRecording;
		}
		subscribed = subscribe;
	}

	private void ReadyForRecording(bool ready)
	{
		readyForRecording = ready;
	}

	private void OnPreRender()
	{
		if (readyForRecording)
		{
			Everyplay.SnapshotRenderbuffer();
		}
	}
}
