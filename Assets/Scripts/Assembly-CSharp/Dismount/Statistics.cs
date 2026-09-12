#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	public class Statistics
	{
		public class RealtimeStatistics
		{
			private int _score;

			private int _displayScore;

			public int displayMultiplier;

			public float characterAltitude;

			public float characterAirTime;

			public float vehicleSpeed;

			public float vehicleAltitude;

			public float vehicleAirTime;

			public float headPain;

			public int score
			{
				get
				{
					return _score ^ 0x23454321;
				}
				set
				{
					_score = value ^ 0x23454321;
				}
			}

			public int displayScore
			{
				get
				{
					return _displayScore ^ 0x6438F638;
				}
				set
				{
					_displayScore = value ^ 0x6438F638;
				}
			}

			public void Reset()
			{
				score = 0;
				displayScore = 0;
				displayMultiplier = 0;
				characterAltitude = 0f;
				characterAirTime = 0f;
				vehicleSpeed = 0f;
				vehicleAltitude = 0f;
				vehicleAirTime = 0f;
				headPain = 0f;
			}
		}

		public RealtimeStatistics realtime = new RealtimeStatistics();

		private Dictionary<string, int> stats = new Dictionary<string, int>();

		private string characterKey = "MrDismount";

		private string vehicleKey = "MilkVan";

		private string levelKey = "LevelDevelopment";

		private bool isSpaceProgram;

		private PlayerState playerState;

		private List<string> allCharacters = new List<string>();

		private List<string> allVehicles = new List<string>();

		private List<string> allLevels = new List<string>();

		private List<string> allObstacles = new List<string>();

		private List<int> dismountNPCVehiclesHit = new List<int>();

		private Dictionary<string, int> serverStats = new Dictionary<string, int>();

		private Dictionary<string, string> statsToAchievementMapping = new Dictionary<string, string>();

		private float prevCharacterAltitude = -1f;

		public void Init()
		{
			Inventory inventory = DismountGame.inventory;
			List<GameItem> list = new List<GameItem>();
			list = inventory.GetItemsOfCategory(GameItem.ItemCategory.Character);
			foreach (GameItem item in list)
			{
				allCharacters.Add(item.referenceName);
			}
			list = inventory.GetItemsOfCategory(GameItem.ItemCategory.Vehicle);
			foreach (GameItem item2 in list)
			{
				allVehicles.Add(item2.referenceName);
			}
			list = inventory.GetItemsOfCategory(GameItem.ItemCategory.Level);
			foreach (GameItem item3 in list)
			{
				allLevels.Add(item3.referenceName);
			}
			list = inventory.GetItemsOfCategory(GameItem.ItemCategory.Obstacle);
			foreach (GameItem item4 in list)
			{
				allObstacles.Add(item4.referenceName);
			}
			stats["cumulative.sessions"] = 0;
			stats["cumulative.dismountsStarted"] = 0;
			stats["cumulative.dismountsCompleted"] = 0;
			stats["cumulative.dismountsNailed"] = 0;
			stats["cumulative.score"] = 0;
			stats["cumulative.characterDecapitations"] = 0;
			stats["cumulative.characterDisintegrations"] = 0;
			stats["cumulative.vehicleDisintegrations"] = 0;
			foreach (string allCharacter in allCharacters)
			{
				stats["cumulative.dismountsStarted." + allCharacter] = 0;
				stats["cumulative.dismountsCompleted." + allCharacter] = 0;
				stats["cumulative.score." + allCharacter] = 0;
				stats["cumulative.characterDisintegrations." + allCharacter] = 0;
				stats["cumulative.characterDecapitations." + allCharacter] = 0;
			}
			foreach (string allVehicle in allVehicles)
			{
				stats["cumulative.dismountsStarted." + allVehicle] = 0;
				stats["cumulative.dismountsCompleted." + allVehicle] = 0;
				stats["cumulative.score." + allVehicle] = 0;
				stats["cumulative.vehicleDisintegrations." + allVehicle] = 0;
			}
			foreach (string allLevel in allLevels)
			{
				stats["cumulative.dismountsStarted." + allLevel] = 0;
				stats["cumulative.dismountsCompleted." + allLevel] = 0;
			}
			foreach (string allObstacle in allObstacles)
			{
				stats["cumulative.obstaclesPlaced." + allObstacle] = 0;
			}
			stats["cumulative.minesDetonated"] = 0;
			stats["cumulative.turbopadsActivated"] = 0;
			stats["cumulative.brakepadsActivated"] = 0;
			stats["cumulative.treesCut"] = 0;
			stats["cumulative.bowlingStrikes"] = 0;
			serverStats["cumulative.characterDecapitations"] = 1000;
			serverStats["cumulative.dismountsStarted"] = 1000;
			serverStats["cumulative.dismountsCompleted.Tricycle"] = 100;
			serverStats["cumulative.turbopadsActivated"] = 100;
			serverStats["cumulative.treesCut"] = 100;
			serverStats["cumulative.dismountsNailed"] = 100;
			serverStats["cumulative.bowlingStrikes"] = 10;
			statsToAchievementMapping["cumulative.characterDecapitations"] = "com.secretexit.turbodismount.FrenchRevolution";
			statsToAchievementMapping["cumulative.dismountsStarted"] = "com.secretexit.turbodismount.TheMillennial";
			statsToAchievementMapping["cumulative.dismountsCompleted.Tricycle"] = "com.secretexit.turbodismount.TimmysNightmare";
			statsToAchievementMapping["cumulative.turbopadsActivated"] = "com.secretexit.turbodismount.TurboLover";
			statsToAchievementMapping["cumulative.treesCut"] = "com.secretexit.turbodismount.PaulBunyan";
			statsToAchievementMapping["cumulative.dismountsNailed"] = "com.secretexit.turbodismount.InItToWinIt";
			statsToAchievementMapping["cumulative.bowlingStrikes"] = "com.secretexit.turbodismount.DudeAbides";
			Load();
		}

		public void ResetDismount()
		{
			playerState = DismountGame.playerState;
			List<string> list = new List<string>();
			foreach (string key in stats.Keys)
			{
				if (key.StartsWith("dismount."))
				{
					list.Add(key);
				}
			}
			foreach (string item in list)
			{
				Set(item, 0);
			}
			realtime.Reset();
			characterKey = playerState.currentCharacterName;
			vehicleKey = playerState.currentVehicleName;
			levelKey = playerState.currentLevel;
			isSpaceProgram = levelKey == "SpaceProgram";
			if (playerState.isCustomLevel && !playerState.isUnpublishedLevel)
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.AdventurousMind");
			}
			dismountNPCVehiclesHit.Clear();
		}

		public void ResetSession()
		{
			Set("session.consequtiveFullThrottles", 0);
			Increment("cumulative.sessions");
		}

		public void EvaluateRealtime()
		{
			if (isSpaceProgram)
			{
				if (realtime.characterAltitude >= 304.8f && prevCharacterAltitude < 304.8f)
				{
					playerState.achievements.ReportComplete("com.secretexit.turbodismount.ToSpace");
				}
				prevCharacterAltitude = realtime.characterAltitude;
			}
		}

		public void DismountStarted(float throttle)
		{
			if (Increment("cumulative.dismountsStarted") >= serverStats["cumulative.dismountsStarted"])
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.TheMillennial");
			}
			Increment("cumulative.dismountsStarted." + characterKey);
			Increment("cumulative.dismountsStarted." + vehicleKey);
			Increment("cumulative.dismountsStarted." + levelKey);
			bool flag = true;
			foreach (string allCharacter in allCharacters)
			{
				if (Get("cumulative.dismountsStarted." + allCharacter) <= 0)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				foreach (string allVehicle in allVehicles)
				{
					if (Get("cumulative.dismountsStarted." + allVehicle) <= 0)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				foreach (string allLevel in allLevels)
				{
					if (Get("cumulative.dismountsStarted." + allLevel) <= 0)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.ExceptMySister");
			}
			if (throttle >= 0.98f)
			{
				if (Increment("session.consequtiveFullThrottles") == 3)
				{
					playerState.achievements.ReportComplete("com.secretexit.turbodismount.SpeedIsKey");
				}
			}
			else
			{
				Set("session.consequtiveFullThrottles", 0);
			}
		}

		public void DismountComplete(bool nailedIt)
		{
			Increment("cumulative.dismountsCompleted");
			Increment("cumulative.dismountsCompleted." + characterKey);
			Increment("cumulative.dismountsCompleted." + vehicleKey);
			Increment("cumulative.dismountsCompleted." + levelKey);
			if (nailedIt && Increment("cumulative.dismountsNailed") >= serverStats["cumulative.dismountsNailed"])
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.InItToWinIt");
			}
			if (vehicleKey == "Tricycle" && Get("cumulative.dismountsCompleted.Tricycle") >= serverStats["cumulative.dismountsCompleted.Tricycle"])
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.TimmysNightmare");
			}
			if (Get("dismount.limbDetaches") == 1)
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.JustAFleshWound");
			}
			if (dismountNPCVehiclesHit.Count >= 5 && levelKey == "Sideon")
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.PingPong");
			}
			Save();
		}

		public void VehicleDisintegrated()
		{
			playerState.achievements.ReportComplete("com.secretexit.turbodismount.ExWithABazooka");
			Increment("cumulative.vehicleDisintegrations");
			Increment("cumulative.vehicleDisintegrations." + vehicleKey);
			foreach (string allVehicle in allVehicles)
			{
				if (Get("cumulative.vehicleDisintegrations." + allVehicle) <= 0)
				{
					return;
				}
			}
			playerState.achievements.ReportComplete("com.secretexit.turbodismount.MissionFromGod");
		}

		public void NonDestructibleVehicle()
		{
			Increment("cumulative.vehicleDisintegrations." + vehicleKey);
			foreach (string allVehicle in allVehicles)
			{
				if (Get("cumulative.vehicleDisintegrations." + allVehicle) <= 0)
				{
					return;
				}
			}
			playerState.achievements.ReportComplete("com.secretexit.turbodismount.MissionFromGod");
		}

		public void CharacterDecapitated()
		{
			if (Increment("cumulative.characterDecapitations") >= serverStats["cumulative.characterDecapitations"])
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.FrenchRevolution");
			}
			Increment("cumulative.characterDecapitations." + characterKey);
		}

		public void CharacterLimbDetached()
		{
			Increment("dismount.limbDetaches");
		}

		public void CharacterBodyPartDetached()
		{
			Increment("dismount.bodyPartDetaches");
		}

		public void CharacterDisintegrated()
		{
			Increment("cumulative.characterDisintegrations");
			Increment("cumulative.characterDisintegrations." + characterKey);
			playerState.achievements.ReportComplete("com.secretexit.turbodismount.DummyTotalDisintegration");
		}

		public void CharacterFlipped()
		{
			if (Increment("dismount.characterFlips") >= 10)
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.DizzyYet");
			}
		}

		public void TurboPadActivated()
		{
			if (Increment("cumulative.turbopadsActivated") >= 1000)
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.TurboLover");
			}
		}

		public void BrakePadActivated()
		{
			Increment("cumulative.brakepadsActivated");
		}

		public void MineDetonated()
		{
			Increment("cumulative.minesDetonated");
			int num = Increment("dismount.minesDetonated");
			if (num >= 10 && levelKey == "Minefield")
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.BorderIncident");
			}
		}

		public void TreeCut()
		{
			if (Increment("cumulative.treesCut") >= serverStats["cumulative.treesCut"])
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.PaulBunyan");
			}
		}

		public void BowlingStrike()
		{
			if (Increment("cumulative.bowlingStrikes") >= serverStats["cumulative.bowlingStrikes"])
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.DudeAbides");
			}
		}

		public void DragStripFinishLineCrossed(float time, float speed)
		{
			if (Mathf.FloorToInt(speed * 2.23694f) == 88)
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.88mph");
			}
			if (time <= 9.13f)
			{
				playerState.achievements.ReportComplete("com.secretexit.turbodismount.LookAtHimGo");
			}
		}

		public void NPCVehicleHit(int instanceId)
		{
			if (!dismountNPCVehiclesHit.Contains(instanceId))
			{
				dismountNPCVehiclesHit.Add(instanceId);
			}
		}

		public int Increment(string id)
		{
			return Add(id, 1);
		}

		public int Add(string id, int amount)
		{
			int num = Get(id);
			num += amount;
			if (serverStats.ContainsKey(id))
			{
				SXJNI.Instance.Increment(Achievements.achievementsGPGS[statsToAchievementMapping[id]], amount, (bool success) =>
				{
				});
			}
			Set(id, num);
			return num;
		}

		public void Set(string id, int amount)
		{
			stats[id] = amount;
		}

		public int Get(string id)
		{
			if (stats.ContainsKey(id))
			{
				return stats[id];
			}
			return 0;
		}

		public void Load()
		{
			List<string> list = new List<string>();
			foreach (string key in stats.Keys)
			{
				if (key.StartsWith("cumulative."))
				{
					list.Add(key);
				}
			}
			foreach (string item in list)
			{
				Set(item, Prefs.GetInt(item, 0));
			}
		}

		public void Save()
		{
			foreach (string key in stats.Keys)
			{
				if (key.StartsWith("cumulative."))
				{
					Prefs.SetInt(key, Get(key));
				}
			}
			Prefs.Save();
		}
	}
}
