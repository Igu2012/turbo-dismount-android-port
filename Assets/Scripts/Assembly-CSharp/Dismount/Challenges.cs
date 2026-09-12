#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	public class Challenges : MonoBehaviour
	{
		public List<Challenge> challenges;

		private List<Challenge> currentChallenges = new List<Challenge>();

		private void AddChallenge(string id, string displayText, params Challenge.Rule[] rules)
		{
			AddChallenge(id, displayText, new Challenge.Prerequisites(string.Empty, string.Empty, string.Empty, -1), rules);
		}

		private void AddChallenge(string id, string displayText, Challenge.Prerequisites prerequisites, params Challenge.Rule[] rules)
		{
			Challenge challenge = new Challenge(id, displayText);
			challenge.prerequisites = prerequisites;
			for (int i = 0; i < rules.Length; i++)
			{
				challenge.rules.Add(rules[i]);
			}
			challenges.Add(challenge);
		}

		private void CreateChallenges()
		{
			AddChallenge("score10k", "Achieve a score of 10,000 points", new Challenge.Rule(Challenge.TrackedData.Score, Challenge.Operation.MoreThanOrEquals, 10000, Challenge.Evaluate.RealTime));
			AddChallenge("score50k", "Achieve a score of 50,000 points", new Challenge.Rule(Challenge.TrackedData.Score, Challenge.Operation.MoreThanOrEquals, 50000, Challenge.Evaluate.RealTime));
			AddChallenge("score100k", "Achieve a score of 100,000 points", new Challenge.Rule(Challenge.TrackedData.Score, Challenge.Operation.MoreThanOrEquals, 100000, Challenge.Evaluate.RealTime));
			AddChallenge("score1M", "Achieve a score of 1,000,000 points", new Challenge.Rule(Challenge.TrackedData.Score, Challenge.Operation.MoreThanOrEquals, 1000000, Challenge.Evaluate.RealTime));
			AddChallenge("scoreLessThan10k", "Achieve a score of less than 10,000 points", new Challenge.Rule(Challenge.TrackedData.Score, Challenge.Operation.LessThan, 10000, Challenge.Evaluate.AtDismountComplete));
			AddChallenge("trikeSpeed30kph", "Achieve a speed of 30kph with the tricycle", new Challenge.Prerequisites(string.Empty, "Tricycle", string.Empty, -1), new Challenge.Rule(Challenge.TrackedData.VehicleSpeed, Challenge.Operation.MoreThanOrEquals, 8, Challenge.Evaluate.RealTime));
			AddChallenge("trikeAltitude5m", "Achieve an altitude of 5m with the tricycle,\n   on Classic level,\n   scoring less than 50,000 points", new Challenge.Prerequisites("LevelDevelopment", "Tricycle", string.Empty, -1), new Challenge.Rule(Challenge.TrackedData.VehicleAltitude, Challenge.Operation.MoreThanOrEquals, 5, Challenge.Evaluate.RealTime), new Challenge.Rule(Challenge.TrackedData.Score, Challenge.Operation.LessThan, 50000, Challenge.Evaluate.AtDismountComplete));
		}

		private void Awake()
		{
			ValidateChallenges();
			UpdateChallengeCompleteFlags();
		}

		private void ValidateChallenges()
		{
			foreach (Challenge challenge in challenges)
			{
				foreach (Challenge challenge2 in challenges)
				{
					if (challenge != challenge2 && challenge.id == challenge2.id)
					{
						Debug.LogError("Two challenges found with the same id: " + challenge.id);
					}
				}
			}
		}

		private void UpdateChallengeCompleteFlags()
		{
			foreach (Challenge challenge in challenges)
			{
				if (Prefs.GetInt(challenge.id, 0) > 0)
				{
					challenge.isComplete = true;
				}
			}
		}

		public void RefreshChallenges()
		{
			foreach (Challenge currentChallenge in currentChallenges)
			{
				if (currentChallenge.isComplete)
				{
					Prefs.SetInt(currentChallenge.id, 1);
				}
			}
			Prefs.Save();
			currentChallenges.Clear();
			int num = 0;
			foreach (Challenge challenge in challenges)
			{
				if (!challenge.isComplete)
				{
					currentChallenges.Add(challenge);
					if (++num >= 3)
					{
						break;
					}
				}
			}
			if (currentChallenges.Count <= 0)
			{
				DismountGame.uiManager.SetChallenges(string.Empty);
				return;
			}
			string text = "CHALLENGES\n";
			foreach (Challenge currentChallenge2 in currentChallenges)
			{
				text = text + "* " + currentChallenge2.displayText + "\n";
			}
			DismountGame.uiManager.SetChallenges(string.Empty);
		}

		public void ResetCurrentChallenges()
		{
			foreach (Challenge currentChallenge in currentChallenges)
			{
				currentChallenge.ResetRulesAndPrerequisites();
			}
		}

		public void CheckRealtime(ref Statistics.RealtimeStatistics currentData)
		{
			foreach (Challenge currentChallenge in currentChallenges)
			{
				currentChallenge.EvaluateRealtimeRules(ref currentData);
			}
		}

		public void CheckDismountComplete(ref Statistics.RealtimeStatistics currentData)
		{
			foreach (Challenge currentChallenge in currentChallenges)
			{
				currentChallenge.EvaluateDismountCompleteRules(ref currentData);
			}
		}

		public void ReportChallengeEvent(string id)
		{
			foreach (Challenge currentChallenge in currentChallenges)
			{
				currentChallenge.TriggerEvent(id);
				if (!currentChallenge.isComplete)
				{
					currentChallenge.EvaluateEventRules(ref DismountGame.playerState.statistics.realtime);
				}
			}
		}
	}
}
