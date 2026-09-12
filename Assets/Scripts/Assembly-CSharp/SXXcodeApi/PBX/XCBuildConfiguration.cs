#pragma warning disable 0618,0619
using System.Collections.Generic;
using System.Linq;

namespace SXXcodeApi.PBX
{
	internal class XCBuildConfiguration : PBXObject
	{
		protected SortedDictionary<string, BuildConfigEntry> entries = new SortedDictionary<string, BuildConfigEntry>();

		public string name
		{
			get
			{
				return GetPropertyString("name");
			}
		}

		private static string EscapeWithQuotesIfNeeded(string name, string value)
		{
			if (name != "LIBRARY_SEARCH_PATHS")
			{
				return value;
			}
			if (!value.Contains(" "))
			{
				return value;
			}
			if (value.First() == '"' && value.Last() == '"')
			{
				return value;
			}
			return "\"" + value + "\"";
		}

		public void SetProperty(string name, string value)
		{
			entries[name] = BuildConfigEntry.FromNameValue(name, EscapeWithQuotesIfNeeded(name, value));
		}

		public void AddProperty(string name, string value)
		{
			if (entries.ContainsKey(name))
			{
				entries[name].AddValue(EscapeWithQuotesIfNeeded(name, value));
			}
			else
			{
				SetProperty(name, value);
			}
		}

		public void RemoveProperty(string name)
		{
			if (entries.ContainsKey(name))
			{
				entries.Remove(name);
			}
		}

		public void RemovePropertyValue(string name, string value)
		{
			if (entries.ContainsKey(name))
			{
				entries[name].RemoveValue(EscapeWithQuotesIfNeeded(name, value));
			}
		}

		public static XCBuildConfiguration Create(string name)
		{
			XCBuildConfiguration xCBuildConfiguration = new XCBuildConfiguration();
			xCBuildConfiguration.guid = PBXGUID.Generate();
			xCBuildConfiguration.SetPropertyString("isa", "XCBuildConfiguration");
			xCBuildConfiguration.SetPropertyString("name", name);
			return xCBuildConfiguration;
		}

		public override void UpdateProps()
		{
			PBXElementDict pBXElementDict = m_Properties.CreateDict("buildSettings");
			foreach (KeyValuePair<string, BuildConfigEntry> entry in entries)
			{
				if (entry.Value.val.Count == 0)
				{
					continue;
				}
				if (entry.Value.val.Count == 1)
				{
					pBXElementDict.SetString(entry.Key, entry.Value.val[0]);
					continue;
				}
				PBXElementArray pBXElementArray = pBXElementDict.CreateArray(entry.Key);
				foreach (string item in entry.Value.val)
				{
					pBXElementArray.AddString(item);
				}
			}
		}

		public override void UpdateVars()
		{
			entries = new SortedDictionary<string, BuildConfigEntry>();
			if (!m_Properties.Contains("buildSettings"))
			{
				return;
			}
			PBXElementDict pBXElementDict = m_Properties["buildSettings"].AsDict();
			foreach (string key in pBXElementDict.values.Keys)
			{
				PBXElement pBXElement = pBXElementDict[key];
				if (pBXElement is PBXElementString)
				{
					if (entries.ContainsKey(key))
					{
						entries[key].val.Add(pBXElement.AsString());
					}
					else
					{
						entries.Add(key, BuildConfigEntry.FromNameValue(key, pBXElement.AsString()));
					}
				}
				else
				{
					if (!(pBXElement is PBXElementArray))
					{
						continue;
					}
					foreach (PBXElement value in pBXElement.AsArray().values)
					{
						if (value is PBXElementString)
						{
							if (entries.ContainsKey(key))
							{
								entries[key].val.Add(value.AsString());
							}
							else
							{
								entries.Add(key, BuildConfigEntry.FromNameValue(key, value.AsString()));
							}
						}
					}
				}
			}
		}
	}
}
