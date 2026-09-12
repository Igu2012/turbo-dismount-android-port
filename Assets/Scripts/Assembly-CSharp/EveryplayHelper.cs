#pragma warning disable 0618,0619
public class EveryplayHelper
{
	private bool isSupported;

	public void Init()
	{
		isSupported = false;
	}

	public bool IsSupported()
	{
		return isSupported;
	}

	public bool IsRecording()
	{
		if (isSupported)
		{
			return Everyplay.IsRecording();
		}
		return false;
	}

	public bool IsPaused()
	{
		if (isSupported)
		{
			return Everyplay.IsPaused();
		}
		return false;
	}

	public void StartRecording()
	{
		if (isSupported)
		{
			Everyplay.StartRecording();
		}
	}

	public void StopRecording()
	{
		if (isSupported)
		{
			Everyplay.StopRecording();
		}
	}

	public void PauseRecording()
	{
		if (isSupported)
		{
			Everyplay.PauseRecording();
		}
	}

	public void ResumeRecording()
	{
		if (isSupported)
		{
			Everyplay.ResumeRecording();
		}
	}

	public bool SnapshotRenderbuffer()
	{
		return false;
	}

	public void PlayLastRecording()
	{
		if (isSupported)
		{
			Everyplay.PlayLastRecording();
		}
	}

	public void SetMetadata(string key, object val)
	{
	}

	public void Show()
	{
	}

	public void ShowSharingModal()
	{
		if (isSupported)
		{
			Everyplay.ShowSharingModal();
		}
	}

	public void ShowWithPath(string path)
	{
	}
}
