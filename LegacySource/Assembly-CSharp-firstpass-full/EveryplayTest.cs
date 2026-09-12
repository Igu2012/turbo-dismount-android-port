using UnityEngine;

public class EveryplayTest : MonoBehaviour
{
	private bool isRecording;

	private bool isPaused;

	private bool isRecordingFinished;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		Everyplay.RecordingStarted += RecordingStarted;
		Everyplay.RecordingStopped += RecordingStopped;
	}

	private void Destroy()
	{
		Everyplay.RecordingStarted -= RecordingStarted;
		Everyplay.RecordingStopped -= RecordingStopped;
	}

	private void RecordingStarted()
	{
		isRecording = true;
		isPaused = false;
		isRecordingFinished = false;
	}

	private void RecordingStopped()
	{
		isRecording = false;
		isRecordingFinished = true;
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 10f, 138f, 48f), "Everyplay"))
		{
		}
		if (isRecording && GUI.Button(new Rect(10f, 64f, 138f, 48f), "Stop Recording"))
		{
			Everyplay.StopRecording();
		}
		else if (!isRecording && GUI.Button(new Rect(10f, 64f, 138f, 48f), "Start Recording"))
		{
			Everyplay.StartRecording();
		}
		if (isRecording)
		{
			if (!isPaused && GUI.Button(new Rect(160f, 64f, 138f, 48f), "Pause Recording"))
			{
				Everyplay.PauseRecording();
				isPaused = true;
			}
			else if (isPaused && GUI.Button(new Rect(160f, 64f, 138f, 48f), "Resume Recording"))
			{
				Everyplay.ResumeRecording();
				isPaused = false;
			}
		}
		if (isRecordingFinished && GUI.Button(new Rect(10f, 118f, 138f, 48f), "Play Last Recording"))
		{
			Everyplay.PlayLastRecording();
		}
		if (isRecording && GUI.Button(new Rect(10f, 118f, 138f, 48f), "Take Thumbnail"))
		{
			Everyplay.TakeThumbnail();
		}
		if (isRecordingFinished && GUI.Button(new Rect(10f, 172f, 138f, 48f), "Show sharing modal"))
		{
			Everyplay.ShowSharingModal();
		}
	}
}
