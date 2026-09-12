#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

public class AdManager
{
	public delegate void VideoFinishedDelegate(bool ad_shown);

	private List<AdPlayer> adPlayers = new List<AdPlayer>();

	public VideoFinishedDelegate OnVideoFinished;

	public AdPlayer.VideoAvailabilityChangedDelegate OnVideoAvailabilityChanged;

	private int maximumShownAdCount;

	private int adsShown;

	public AdManager(int maximumShownAdCount)
	{
		this.maximumShownAdCount = maximumShownAdCount;
	}

	private void SortPlayers()
	{
		adPlayers.Sort((AdPlayer x, AdPlayer y) => (x.priority >= y.priority) ? 1 : (-1));
	}

	private void AddPlayer(AdPlayer player)
	{
		adPlayers.Add(player);
		player.OnVideoFinished = OnPlayerVideoFinished;
		player.OnVideoAvailabilityChanged = OnVideoAvailabilityChanged;
		SortPlayers();
	}

	private void OnPlayerVideoFinished(bool shown)
	{
		if (shown)
		{
			adsShown++;
		}
		if (OnVideoFinished != null)
		{
			OnVideoFinished(shown);
		}
	}

	public void InitializeUnityAds(int priority, int prioritizedAdCount, string iosAppId, string androidAppId, bool testMode = false)
	{
		string appId = iosAppId;
		if (Application.platform == RuntimePlatform.Android)
		{
			appId = androidAppId;
		}
		AddPlayer(new UnityAdAdPlayer(priority, prioritizedAdCount, appId, testMode));
	}

	public void Update()
	{
		for (int i = 0; i < adPlayers.Count; i++)
		{
			adPlayers[i].Update();
		}
	}

	public bool IsAdAvailable()
	{
		if (adsShown >= maximumShownAdCount)
		{
			return false;
		}
		if (FindAnyAdPlayerWithAds() != null)
		{
			return true;
		}
		return false;
	}

	private AdPlayer FindAnyAdPlayerWithAds()
	{
		for (int i = 0; i < adPlayers.Count; i++)
		{
			if (adPlayers[i].adAvailable)
			{
				return adPlayers[i];
			}
		}
		return null;
	}

	private AdPlayer FindPrioritizedAdPlayerWithAds()
	{
		for (int i = 0; i < adPlayers.Count; i++)
		{
			if (adPlayers[i].adAvailable && adPlayers[i].adsShown < adPlayers[i].prioritizedAdCount)
			{
				return adPlayers[i];
			}
		}
		return FindAnyAdPlayerWithAds();
	}

	public void ShowAd()
	{
		AdPlayer adPlayer = FindPrioritizedAdPlayerWithAds();
		if (adPlayer != null)
		{
			adPlayer.ShowAd();
		}
	}
}
