#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	public class Achievements
	{
		private Dictionary<string, int> achievements = new Dictionary<string, int>();

		public static Dictionary<string, string> achievementsGPGS = new Dictionary<string, string>();

		private void Start()
		{
			Debug.Log("Over here");
		}

		public void Init()
		{
			achievements["com.secretexit.turbodismount.ToSpace"] = 0;
			achievements["com.secretexit.turbodismount.JustAFleshWound"] = 0;
			achievements["com.secretexit.turbodismount.FrenchRevolution"] = 0;
			achievements["com.secretexit.turbodismount.ExWithABazooka"] = 0;
			achievements["com.secretexit.turbodismount.DummyTotalDisintegration"] = 0;
			achievements["com.secretexit.turbodismount.DizzyYet"] = 0;
			achievements["com.secretexit.turbodismount.LifetimeAchievement"] = 0;
			achievements["com.secretexit.turbodismount.TheMillennial"] = 0;
			achievements["com.secretexit.turbodismount.SpeedIsKey"] = 0;
			achievements["com.secretexit.turbodismount.TimmysNightmare"] = 0;
			achievements["com.secretexit.turbodismount.MissionFromGod"] = 0;
			achievements["com.secretexit.turbodismount.BeenAroundTheBlock"] = 0;
			achievements["com.secretexit.turbodismount.TurboLover"] = 0;
			achievements["com.secretexit.turbodismount.BorderIncident"] = 0;
			achievements["com.secretexit.turbodismount.PaulBunyan"] = 0;
			achievements["com.secretexit.turbodismount.DudeAbides"] = 0;
			achievements["com.secretexit.turbodismount.88mph"] = 0;
			achievements["com.secretexit.turbodismount.LookAtHimGo"] = 0;
			achievements["com.secretexit.turbodismount.AdventurousMind"] = 0;
			achievements["com.secretexit.turbodismount.LetsSeeThatAgain"] = 0;
			achievements["com.secretexit.turbodismount.InsultToInjury"] = 0;
			achievements["com.secretexit.turbodismount.Scaramanga"] = 0;
			achievements["com.secretexit.turbodismount.InItToWinIt"] = 0;
			achievements["com.secretexit.turbodismount.ExceptMySister"] = 0;
			achievements["com.secretexit.turbodismount.PingPong"] = 0;
			achievementsGPGS["com.secretexit.turbodismount.ToSpace"] = "CgkIuMvs_sQYEAIQFw";
			achievementsGPGS["com.secretexit.turbodismount.JustAFleshWound"] = "CgkIuMvs_sQYEAIQEg";
			achievementsGPGS["com.secretexit.turbodismount.FrenchRevolution"] = "CgkIuMvs_sQYEAIQJQ";
			achievementsGPGS["com.secretexit.turbodismount.ExWithABazooka"] = "CgkIuMvs_sQYEAIQFA";
			achievementsGPGS["com.secretexit.turbodismount.DummyTotalDisintegration"] = "CgkIuMvs_sQYEAIQFg";
			achievementsGPGS["com.secretexit.turbodismount.DizzyYet"] = "CgkIuMvs_sQYEAIQHA";
			achievementsGPGS["com.secretexit.turbodismount.LifetimeAchievement"] = string.Empty;
			achievementsGPGS["com.secretexit.turbodismount.TheMillennial"] = "CgkIuMvs_sQYEAIQIA";
			achievementsGPGS["com.secretexit.turbodismount.SpeedIsKey"] = "CgkIuMvs_sQYEAIQGA";
			achievementsGPGS["com.secretexit.turbodismount.TimmysNightmare"] = "CgkIuMvs_sQYEAIQJA";
			achievementsGPGS["com.secretexit.turbodismount.MissionFromGod"] = "CgkIuMvs_sQYEAIQJg";
			achievementsGPGS["com.secretexit.turbodismount.BeenAroundTheBlock"] = string.Empty;
			achievementsGPGS["com.secretexit.turbodismount.TurboLover"] = "CgkIuMvs_sQYEAIQHw";
			achievementsGPGS["com.secretexit.turbodismount.BorderIncident"] = "CgkIuMvs_sQYEAIQEw";
			achievementsGPGS["com.secretexit.turbodismount.PaulBunyan"] = "CgkIuMvs_sQYEAIQHg";
			achievementsGPGS["com.secretexit.turbodismount.DudeAbides"] = "CgkIuMvs_sQYEAIQGQ";
			achievementsGPGS["com.secretexit.turbodismount.88mph"] = "CgkIuMvs_sQYEAIQHQ";
			achievementsGPGS["com.secretexit.turbodismount.LookAtHimGo"] = "CgkIuMvs_sQYEAIQIg";
			achievementsGPGS["com.secretexit.turbodismount.AdventurousMind"] = string.Empty;
			achievementsGPGS["com.secretexit.turbodismount.LetsSeeThatAgain"] = "CgkIuMvs_sQYEAIQEQ";
			achievementsGPGS["com.secretexit.turbodismount.InsultToInjury"] = "CgkIuMvs_sQYEAIQFQ";
			achievementsGPGS["com.secretexit.turbodismount.Scaramanga"] = "CgkIuMvs_sQYEAIQGw";
			achievementsGPGS["com.secretexit.turbodismount.InItToWinIt"] = "CgkIuMvs_sQYEAIQIw";
			achievementsGPGS["com.secretexit.turbodismount.ExceptMySister"] = "CgkIuMvs_sQYEAIQIQ";
			achievementsGPGS["com.secretexit.turbodismount.PingPong"] = "CgkIuMvs_sQYEAIQGg";
			List<string> list = new List<string>();
			list.AddRange(achievements.Keys);
			foreach (string item in list)
			{
				achievements[item] = Prefs.GetInt(item, 0);
				if (Steamworks.IsSteamAvailable() && achievements[item] == 1 && !Steamworks.IsAchievementUnlocked(item))
				{
					Steamworks.UnlockAchievement(item);
				}
			}
		}

		public void ReportComplete(string id)
		{
			if (achievements.ContainsKey(id))
			{
				if (achievements[id] == 0)
				{
					achievements[id] = 1;
					Prefs.SetInt(id, 1);
					Prefs.Save();
					DismountGame.instance.OnAchievementComplete(id);
					Steamworks.UnlockAchievement(id);
				}
				SXJNI.Instance.Unlock(achievementsGPGS[id], (bool success) =>
				{
				});
			}
		}
	}
}
