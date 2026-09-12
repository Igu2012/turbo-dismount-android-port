#pragma warning disable 0618,0619
using System;
using UnityEngine;

public class Analytics : MonoBehaviour
{
	public string trackingID = "UA-8989161-8";

	public string appName = "GoogleUniversalAnalyticsForUnityExample";

	public string appVersion = "1.1.3";

	public string newLevelAnalyticsEventPrefix = "level-";

	public bool useHTTPS;

	public static GoogleUniversalAnalytics gua;

	private static bool instanceExists;

	private string sceneName = string.Empty;

	private int getPOSIXTime()
	{
		return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
	}

	public static void changeScreen(string newScreenName)
	{
		gua.sendAppScreenHit(newScreenName);
	}

	private void Awake()
	{
	}

	private void OnLevelWasLoaded(int level)
	{
		if (!sceneName.Equals(Application.loadedLevelName))
		{
			sceneName = Application.loadedLevelName;
			GoogleUniversalAnalytics instance = GoogleUniversalAnalytics.Instance;
			instance.sendAppScreenHit(newLevelAnalyticsEventPrefix + sceneName);
		}
	}

	private void Start()
	{
	}
}
