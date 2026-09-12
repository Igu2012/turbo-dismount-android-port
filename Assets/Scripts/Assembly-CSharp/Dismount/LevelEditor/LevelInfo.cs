#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dismount.LevelEditor
{
	public class LevelInfo : MonoBehaviour
	{
		[Serializable]
		public class Info
		{
			[SerializeField]
			private string _title = "Untitled";

			[SerializeField]
			private int environmentIndex;

			[SerializeField]
			private int atmosphereIndex;

			[SerializeField]
			private float _timeLimit = 40f;

			[SerializeField]
			private bool _manualSteering = true;

			private List<string> environmentNames = new List<string>();

			private Dictionary<string, List<string>> environments = new Dictionary<string, List<string>>();

			public string title
			{
				get
				{
					return _title;
				}
				set
				{
					string text = ((!string.IsNullOrEmpty(value)) ? value : "Untitled");
					text = text.Trim();
					_title = ((text.Length >= 1) ? text : "Untitled");
				}
			}

			public int environment
			{
				get
				{
					return environmentIndex;
				}
				set
				{
					environmentIndex = Mathf.Clamp(value, 0, environmentNames.Count - 1);
				}
			}

			public int atmosphere
			{
				get
				{
					return atmosphereIndex;
				}
				set
				{
					atmosphereIndex = Mathf.Clamp(value, 0, environments[GetEnvironmentName()].Count - 1);
				}
			}

			public float timeLimit
			{
				get
				{
					return _timeLimit;
				}
				set
				{
					_timeLimit = Mathf.Clamp(value, 10f, 60f);
				}
			}

			public bool manualSteering
			{
				get
				{
					return _manualSteering;
				}
				set
				{
					_manualSteering = value;
				}
			}

			public Info()
			{
				List<string> value = new List<string> { "Bright Green Day", "Clear Blue Haze", "Cold Light of Day", "Desert Sunset", "Fuzzy Day", "Miami Evening", "Moonlight Long Shadows", "Orange Dusty", "Purple Haze", "Warm Light Cool Fog" };
				environmentNames.Add("City");
				environments.Add("City", value);
				environmentNames.Add("Hills");
				environments.Add("Hills", value);
			}

			public string GetEnvironmentName()
			{
				return environmentNames[environmentIndex];
			}

			public string GetAtmoshphereName()
			{
				return environments[GetEnvironmentName()][atmosphereIndex];
			}

			public string[] GetEnvironmentNames()
			{
				return environmentNames.ToArray();
			}

			public string[] GetAtmosphereNames()
			{
				return environments[GetEnvironmentName()].ToArray();
			}

			public int GetEnvironmentIndex(string environmentName)
			{
				for (int i = 0; i < environmentNames.Count; i++)
				{
					if (environmentNames[i] == environmentName)
					{
						return i;
					}
				}
				return 0;
			}

			public int GetAtmosphereIndex(string atmosphereName)
			{
				List<string> list = environments[GetEnvironmentName()];
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] == atmosphereName)
					{
						return i;
					}
				}
				return 0;
			}

			public bool Equals(Info other)
			{
				return string.Compare(title, other.title) == 0 && environment == other.environment && atmosphere == other.atmosphere && Mathf.Approximately(timeLimit, other.timeLimit) && manualSteering == other.manualSteering;
			}

			public void Set(Info other)
			{
				title = other.title;
				environment = other.environment;
				atmosphere = other.atmosphere;
				timeLimit = other.timeLimit;
				manualSteering = other.manualSteering;
			}
		}

		public Info info = new Info();
	}
}
