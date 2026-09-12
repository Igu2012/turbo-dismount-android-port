#pragma warning disable 0618,0619
public abstract class AdPlayer
{
	public delegate void VideoFinishedDelegate(bool ad_shown);

	public delegate void VideoAvailabilityChangedDelegate(bool available, string zone);

	public VideoFinishedDelegate OnVideoFinished;

	public VideoAvailabilityChangedDelegate OnVideoAvailabilityChanged;

	public bool adAvailable;

	public int priority;

	public int prioritizedAdCount;

	public int adsShown;

	public AdPlayer(int priority, int prioritizedAdCount)
	{
		this.priority = priority;
		this.prioritizedAdCount = prioritizedAdCount;
	}

	public abstract void ShowAd();

	public virtual void Update()
	{
	}
}
