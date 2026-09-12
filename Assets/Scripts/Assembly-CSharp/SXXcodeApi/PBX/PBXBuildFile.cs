#pragma warning disable 0618,0619
namespace SXXcodeApi.PBX
{
	internal class PBXBuildFile : PBXObject
	{
		public string fileRef;

		public string compileFlags;

		public bool weak;

		private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[1] { "fileRef/*" });

		internal override PropertyCommentChecker checker
		{
			get
			{
				return checkerData;
			}
		}

		internal override bool shouldCompact
		{
			get
			{
				return true;
			}
		}

		public static PBXBuildFile CreateFromFile(string fileRefGUID, bool weak, string compileFlags)
		{
			PBXBuildFile pBXBuildFile = new PBXBuildFile();
			pBXBuildFile.guid = PBXGUID.Generate();
			pBXBuildFile.SetPropertyString("isa", "PBXBuildFile");
			pBXBuildFile.fileRef = fileRefGUID;
			pBXBuildFile.compileFlags = compileFlags;
			pBXBuildFile.weak = weak;
			return pBXBuildFile;
		}

		private PBXElementDict GetSettingsDictOptional()
		{
			if (m_Properties.Contains("settings"))
			{
				return m_Properties["settings"].AsDict();
			}
			return null;
		}

		private PBXElementDict GetSettingsDict()
		{
			if (m_Properties.Contains("settings"))
			{
				return m_Properties["settings"].AsDict();
			}
			return m_Properties.CreateDict("settings");
		}

		public override void UpdateProps()
		{
			SetPropertyString("fileRef", fileRef);
			if (compileFlags != null && compileFlags != string.Empty)
			{
				GetSettingsDict().SetString("COMPILER_FLAGS", compileFlags);
			}
			else
			{
				PBXElementDict settingsDictOptional = GetSettingsDictOptional();
				if (settingsDictOptional != null)
				{
					settingsDictOptional.Remove("COMPILER_FLAGS");
				}
			}
			if (weak)
			{
				PBXElementDict settingsDict = GetSettingsDict();
				PBXElementArray pBXElementArray = null;
				pBXElementArray = ((!settingsDict.Contains("ATTRIBUTES")) ? settingsDict.CreateArray("ATTRIBUTES") : settingsDict["ATTRIBUTES"].AsArray());
				bool flag = false;
				foreach (PBXElement value in pBXElementArray.values)
				{
					if (value is PBXElementString && value.AsString() == "Weak")
					{
						flag = true;
					}
				}
				if (!flag)
				{
					pBXElementArray.AddString("Weak");
				}
				return;
			}
			PBXElementDict settingsDictOptional2 = GetSettingsDictOptional();
			if (settingsDictOptional2 != null && settingsDictOptional2.Contains("ATTRIBUTES"))
			{
				PBXElementArray pBXElementArray2 = settingsDictOptional2["ATTRIBUTES"].AsArray();
				pBXElementArray2.values.RemoveAll((PBXElement el) => el is PBXElementString && el.AsString() == "Weak");
				if (pBXElementArray2.values.Count == 0)
				{
					settingsDictOptional2.Remove("ATTRIBUTES");
				}
				if (settingsDictOptional2.values.Count == 0)
				{
					m_Properties.Remove("settings");
				}
			}
		}

		public override void UpdateVars()
		{
			fileRef = GetPropertyString("fileRef");
			compileFlags = null;
			weak = false;
			if (!m_Properties.Contains("settings"))
			{
				return;
			}
			PBXElementDict pBXElementDict = m_Properties["settings"].AsDict();
			if (pBXElementDict.Contains("COMPILER_FLAGS"))
			{
				compileFlags = pBXElementDict["COMPILER_FLAGS"].AsString();
			}
			if (!pBXElementDict.Contains("ATTRIBUTES"))
			{
				return;
			}
			PBXElementArray pBXElementArray = pBXElementDict["ATTRIBUTES"].AsArray();
			foreach (PBXElement value in pBXElementArray.values)
			{
				if (value is PBXElementString && value.AsString() == "Weak")
				{
					weak = true;
				}
			}
		}
	}
}
