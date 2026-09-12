#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dismount;
using TurboDismountMiniJSON;
using UnityEngine;

public class GameCenter : MonoBehaviour
{
	private static bool isAuthenticated;

	private static string leaderboardPrefix = "turbodismount.";

	private static Action<bool> authenticateCompleteAction;

	private static Action<string, long> getPlayerScoreCompleteAction;

	private void ReportScoreComplete(string success)
	{
	}

	private static string CreateFullLeaderBoardId(string id)
	{
		return leaderboardPrefix + id;
	}

#if UNITY_IOS
	[DllImport("__Internal")]
	public static extern void gc_authenticate();

	[DllImport("__Internal")]
	public static extern void gc_showLeaderboard(string leaderboardId);

	[DllImport("__Internal")]
	public static extern void gc_showAchievements();

	[DllImport("__Internal")]
	public static extern void gc_reportScore(long score, string leaderboardId);

	[DllImport("__Internal")]
	public static extern void gc_getPlayerScore(string leaderboardId);

	[DllImport("__Internal")]
	public static extern void gc_getScores(string leaderboardId, bool global);

	[DllImport("__Internal")]
	public static extern void gc_reportAchievementProgress(string leaderboardId, double progress);

	[DllImport("__Internal")]
	public static extern double gc_getAchievementProgress(string leaderboardId);

#else
	private static void gc_authenticate() {}
	private static void gc_showLeaderboard(string leaderboardId) {}
	private static void gc_showAchievements() {}
	private static void gc_reportScore(long score, string leaderboardId) {}
	private static void gc_getPlayerScore(string leaderboardId) {}
	private static void gc_getScores(string leaderboardId, bool global) {}
	private static void gc_reportAchievementProgress(string leaderboardId, double progress) {}
	private static double gc_getAchievementProgress(string leaderboardId) { return 0.0; }
#endif
	private void AuthenticateComplete(string success)
	{
		if (success.CompareTo("true") == 0)
		{
			isAuthenticated = true;
		}
		else
		{
			isAuthenticated = false;
		}
		authenticateCompleteAction(isAuthenticated);
	}

	public static void Authenticate(Action<bool> completeAction)
	{
		authenticateCompleteAction = completeAction;
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			gc_authenticate();
		}
	}

	public static void ShowLeaderboard(string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			gc_showLeaderboard(CreateFullLeaderBoardId(leaderboardId));
		}
	}

	public static void ShowAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			gc_showAchievements();
		}
	}

	public static void ReportScore(long score, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer && isAuthenticated)
		{
			gc_reportScore(score, CreateFullLeaderBoardId(leaderboardId));
		}
	}

	private void GetPlayerScoreComplete(string result)
	{
		string[] array = result.Split(',');
		getPlayerScoreCompleteAction(array[1].Remove(0, leaderboardPrefix.Length), long.Parse(array[0]));
	}

	public static void GetPlayerScore(string leaderboardId, Action<string, long> completeAction)
	{
		getPlayerScoreCompleteAction = completeAction;
		if (Application.platform == RuntimePlatform.IPhonePlayer && isAuthenticated)
		{
			gc_getPlayerScore(CreateFullLeaderBoardId(leaderboardId));
		}
	}

	public void GetScoresComplete(string entries)
	{
		List<Leaderboards.LeaderboardEntry> list = new List<Leaderboards.LeaderboardEntry>();
		if (entries != string.Empty)
		{
			Dictionary<string, object> dictionary = Json.Deserialize(entries) as Dictionary<string, object>;
			int num = int.Parse(dictionary["entryCount"].ToString());
			for (int i = 0; i < num; i++)
			{
				list.Add(new Leaderboards.LeaderboardEntry
				{
					name = dictionary["name" + i].ToString(),
					score = int.Parse(dictionary["score" + i].ToString()),
					rank = int.Parse(dictionary["rank" + i].ToString())
				});
			}
			int playerIndex = int.Parse(dictionary["playerIndex"].ToString());
			DismountGame.uiManager.PopulateLeaderboard(list, playerIndex);
		}
	}

	public static void GetScores(string leaderboardId, bool global)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer && isAuthenticated)
		{
			gc_getScores(CreateFullLeaderBoardId(leaderboardId), global);
		}
	}

	public static void UnlockAchievement(string achievementId)
	{
		SetAchievementProgress(achievementId, 100.0);
	}

	public static void SetAchievementProgress(string achievementId, double progress)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer && isAuthenticated)
		{
			gc_reportAchievementProgress(achievementId, progress);
		}
	}

	public static double GetAchievementProgress(string achievementId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer && isAuthenticated)
		{
			return gc_getAchievementProgress(achievementId);
		}
		return 0.0;
	}
}
