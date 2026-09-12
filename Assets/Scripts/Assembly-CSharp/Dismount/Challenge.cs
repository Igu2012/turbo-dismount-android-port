#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	[Serializable]
	public class Challenge
	{
		public enum TrackedData
		{
			Score = 0,
			CharacterAltitude = 1,
			CharacterAirTime = 2,
			VehicleSpeed = 3,
			VehicleAltitude = 4,
			VehicleAirTime = 5
		}

		public enum Operation
		{
			Equals = 0,
			NotEquals = 1,
			LessThan = 2,
			LessThanOrEquals = 3,
			MoreThan = 4,
			MoreThanOrEquals = 5
		}

		public enum Evaluate
		{
			RealTime = 0,
			AtDismountComplete = 1,
			AtEvent = 2
		}

		[Serializable]
		public class Prerequisites
		{
			public string level = string.Empty;

			public string vehicle = string.Empty;

			public string character = string.Empty;

			public int startPosition = -1;

			public Prerequisites(string level, string vehicle, string character, int startPosition)
			{
				this.level = level;
				this.vehicle = vehicle;
				this.character = character;
				this.startPosition = startPosition;
			}
		}

		[Serializable]
		public class Rule
		{
			public TrackedData trackedData;

			public Operation operation = Operation.MoreThan;

			public int compareValue = 1000;

			public Evaluate evaluate;

			[NonSerialized]
			public bool isFulfilled;

			public Rule(TrackedData trackedData, Operation operation, int compareValue, Evaluate evaluate)
			{
				this.trackedData = trackedData;
				this.operation = operation;
				this.compareValue = compareValue;
				this.evaluate = evaluate;
				isFulfilled = false;
			}
		}

		[Serializable]
		public class ChallengeEvent
		{
			public string id = string.Empty;

			[NonSerialized]
			public bool isTriggered;

			public ChallengeEvent(string id)
			{
				this.id = id;
				isTriggered = false;
			}
		}

		public string id = string.Empty;

		public string displayText = string.Empty;

		public Prerequisites prerequisites;

		public bool isComplete;

		private bool prerequisitesOk;

		private bool prerequisitesChecked;

		public List<Rule> rules = new List<Rule>();

		public List<ChallengeEvent> events = new List<ChallengeEvent>();

		public Challenge(string id, string displayText)
		{
			this.id = id;
			this.displayText = displayText;
		}

		public void ResetRulesAndPrerequisites()
		{
			prerequisitesChecked = false;
			foreach (Rule rule in rules)
			{
				rule.isFulfilled = false;
			}
			foreach (ChallengeEvent @event in events)
			{
				@event.isTriggered = false;
			}
		}

		private bool CheckPrerequisites()
		{
			if (prerequisitesChecked)
			{
				return prerequisitesOk;
			}
			PlayerState playerState = DismountGame.playerState;
			prerequisitesOk = true;
			if (prerequisites.level != string.Empty && playerState.currentLevel != prerequisites.level)
			{
				prerequisitesOk = false;
			}
			if (prerequisites.vehicle != string.Empty && playerState.currentVehicleName != prerequisites.vehicle)
			{
				prerequisitesOk = false;
			}
			if (prerequisites.character != string.Empty && playerState.currentCharacterName != prerequisites.character)
			{
				prerequisitesOk = false;
			}
			if (prerequisites.startPosition >= 0 && playerState.currentCharacterStartPosition != prerequisites.startPosition)
			{
				prerequisitesOk = false;
			}
			prerequisitesChecked = true;
			return prerequisitesOk;
		}

		public void EvaluateRealtimeRules(ref Statistics.RealtimeStatistics currentData)
		{
			if (rules.Count == 0 || !CheckPrerequisites())
			{
				return;
			}
			foreach (Rule rule in rules)
			{
				if (rule.evaluate == Evaluate.RealTime)
				{
					EvaluateRule(rule, ref currentData);
				}
			}
			CheckChallengeComplete(true);
		}

		public void EvaluateDismountCompleteRules(ref Statistics.RealtimeStatistics currentData)
		{
			if (rules.Count == 0 || !CheckPrerequisites())
			{
				return;
			}
			foreach (Rule rule in rules)
			{
				if (rule.evaluate == Evaluate.AtDismountComplete)
				{
					EvaluateRule(rule, ref currentData);
				}
			}
			CheckChallengeComplete(true);
		}

		public void EvaluateEventRules(ref Statistics.RealtimeStatistics currentData)
		{
			if (!CheckPrerequisites())
			{
				return;
			}
			foreach (Rule rule in rules)
			{
				if (rule.evaluate == Evaluate.AtEvent)
				{
					EvaluateRule(rule, ref currentData);
				}
			}
			CheckChallengeComplete(true);
		}

		public void TriggerEvent(string id)
		{
			if (isComplete || events.Count == 0 || !CheckPrerequisites())
			{
				return;
			}
			foreach (ChallengeEvent @event in events)
			{
				if (!@event.isTriggered && @event.id == id)
				{
					@event.isTriggered = true;
					break;
				}
			}
		}

		private bool CheckChallengeComplete(bool trigger)
		{
			if (isComplete)
			{
				return isComplete;
			}
			if (rules.Count == 0 && events.Count == 0)
			{
				return false;
			}
			int num = 0;
			foreach (Rule rule in rules)
			{
				if (!rule.isFulfilled)
				{
					break;
				}
				num++;
			}
			if (rules.Count > 0 && num < rules.Count)
			{
				return false;
			}
			int num2 = 0;
			foreach (ChallengeEvent @event in events)
			{
				if (!@event.isTriggered)
				{
					break;
				}
				num2++;
			}
			if (events.Count > 0 && num2 < events.Count)
			{
				return false;
			}
			if (trigger)
			{
				DismountGame.instance.OnChallengeComplete(this);
			}
			isComplete = true;
			return isComplete;
		}

		private bool EvaluateRule(Rule rule, ref Statistics.RealtimeStatistics currentData)
		{
			if (rule.isFulfilled)
			{
				return true;
			}
			int num = 0;
			switch (rule.trackedData)
			{
			case TrackedData.Score:
				num = currentData.score;
				break;
			case TrackedData.CharacterAltitude:
				num = (int)currentData.characterAltitude;
				break;
			case TrackedData.CharacterAirTime:
				num = (int)(currentData.characterAirTime * 1000f);
				break;
			case TrackedData.VehicleSpeed:
				num = (int)currentData.vehicleSpeed;
				break;
			case TrackedData.VehicleAltitude:
				num = (int)currentData.vehicleAltitude;
				break;
			case TrackedData.VehicleAirTime:
				num = (int)(currentData.vehicleAirTime * 1000f);
				break;
			default:
				Debug.LogWarning("Challenge rule tracked data not implemented: " + rule.trackedData);
				break;
			}
			switch (rule.operation)
			{
			case Operation.Equals:
				rule.isFulfilled = num == rule.compareValue;
				break;
			case Operation.NotEquals:
				rule.isFulfilled = num != rule.compareValue;
				break;
			case Operation.LessThan:
				rule.isFulfilled = num < rule.compareValue;
				break;
			case Operation.LessThanOrEquals:
				rule.isFulfilled = num <= rule.compareValue;
				break;
			case Operation.MoreThan:
				rule.isFulfilled = num > rule.compareValue;
				break;
			case Operation.MoreThanOrEquals:
				rule.isFulfilled = num >= rule.compareValue;
				break;
			default:
				Debug.LogWarning("Challenge rule operation not available: " + rule.operation);
				break;
			}
			return rule.isFulfilled;
		}
	}
}
