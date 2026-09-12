#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using Dismount;
using UnityEngine;

public class Steamworks : MonoBehaviour
{
	private static bool steamInitialized;

	private static string currentScoreFetchLeaderboard = string.Empty;

	private static string currentGlobalScoreFetchLeaderboard = string.Empty;

	private static string currentFriendScoreFetchLeaderboard = string.Empty;

	private static string currentLeaderboardPostingTo = string.Empty;

	private bool overlayActive;

	private static Action<string> reportScoreCompleteAction;

	private static Action<string, long> getPlayerScoreCompleteAction;

	private static Action<List<Leaderboards.LeaderboardEntry>, int> getGlobalLeaderboardEntriesCompleteAction;

	private static Action<List<Leaderboards.LeaderboardEntry>, int> getFriendLeaderboardEntriesCompleteAction;

	private static bool publishFileInProgress;

	private static Action<bool> publishFileCompleteAction;

	public static bool getSubscribedContentInProgress;

	private static Action<Dictionary<string, object>> getSubscribedContentCompleteAction;

	private static bool updateUserPublishedItemsInProgress;

	private static Action<bool> updateUserPublishedItemsCompleteAction;

	public static bool IsSteamAvailable()
	{
		return false;
	}

	private void Awake()
	{
	}

	public static void Shutdown()
	{
	}

	private void Update()
	{
	}

	public static void ReportScore(int score, string leaderboardId, Action<string> completeAction)
	{
	}

	private void GetPlayerScoreComplete(string leaderboardId, int score)
	{
		getPlayerScoreCompleteAction(leaderboardId, score);
	}

	public static void GetPlayerScore(string leaderboardId, Action<string, long> completeAction)
	{
	}

	public static void GetGlobalLeaderboardEntries(string leaderboardId, Action<List<Leaderboards.LeaderboardEntry>, int> completeAction)
	{
	}

	public static void GetFriendLeaderboardEntries(string leaderboardId, Action<List<Leaderboards.LeaderboardEntry>, int> completeAction)
	{
	}

	public static void PublishFile(string fileName, string previewFileName, int appid, string title, string description, Action<bool> completeAction)
	{
	}

	public static void GetSubscribedContent(int appid, string downloadPath, Action<Dictionary<string, object>> completeAction)
	{
	}

	public static string GetSubscribedContentInfo()
	{
		return string.Empty;
	}

	public static int GetSubscribedContentProgress()
	{
		return 100;
	}

	public static void UpdateUserPublishedItems(Action<bool> completeAction)
	{
	}

	public static void OpenPublishedItemWorkshopPage()
	{
	}

	public static void OpenURL(string url)
	{
	}

	public static void UnlockAchievement(string achievement)
	{
	}

	public static bool IsAchievementUnlocked(string achievementId)
	{
		return false;
	}

	public static void BringApplicationToFront()
	{
	}

	public static bool IsOverlayActive()
	{
		return false;
	}

	public static int GetStatInt(string statId)
	{
		return -1;
	}

	public static bool SetStatInt(string statId, int value)
	{
		return false;
	}

	public static float GetStatFloat(string statId)
	{
		return -1f;
	}

	public static bool SetStatFloat(string statId, float value)
	{
		return false;
	}

	public static void ResetStatsAndAchievements()
	{
	}
}
