#pragma warning disable 0618,0619
using System.IO;

namespace SXXcodeApi.PBX
{
	internal class PBXFileReference : PBXObject
	{
		private string m_Path;

		private string m_ExplicitFileType;

		private string m_LastKnownFileType;

		public string name;

		public PBXSourceTree tree;

		public string path
		{
			get
			{
				return m_Path;
			}
			set
			{
				m_ExplicitFileType = null;
				m_LastKnownFileType = null;
				m_Path = value;
			}
		}

		internal override bool shouldCompact
		{
			get
			{
				return true;
			}
		}

		public static PBXFileReference CreateFromFile(string path, string projectFileName, PBXSourceTree tree)
		{
			string text = PBXGUID.Generate();
			PBXFileReference pBXFileReference = new PBXFileReference();
			pBXFileReference.SetPropertyString("isa", "PBXFileReference");
			pBXFileReference.guid = text;
			pBXFileReference.path = path;
			pBXFileReference.name = projectFileName;
			pBXFileReference.tree = tree;
			return pBXFileReference;
		}

		public override void UpdateProps()
		{
			string text = null;
			if (m_ExplicitFileType != null)
			{
				SetPropertyString("explicitFileType", m_ExplicitFileType);
			}
			else if (m_LastKnownFileType != null)
			{
				SetPropertyString("lastKnownFileType", m_LastKnownFileType);
			}
			else
			{
				if (name != null)
				{
					text = Path.GetExtension(name);
				}
				else if (m_Path != null)
				{
					text = Path.GetExtension(m_Path);
				}
				if (text != null)
				{
					if (FileTypeUtils.IsFileTypeExplicit(text))
					{
						SetPropertyString("explicitFileType", FileTypeUtils.GetTypeName(text));
					}
					else
					{
						SetPropertyString("lastKnownFileType", FileTypeUtils.GetTypeName(text));
					}
				}
			}
			if (m_Path == name)
			{
				SetPropertyString("name", null);
			}
			else
			{
				SetPropertyString("name", name);
			}
			if (m_Path == null)
			{
				SetPropertyString("path", string.Empty);
			}
			else
			{
				SetPropertyString("path", m_Path);
			}
			SetPropertyString("sourceTree", FileTypeUtils.SourceTreeDesc(tree));
		}

		public override void UpdateVars()
		{
			name = GetPropertyString("name");
			m_Path = GetPropertyString("path");
			if (name == null)
			{
				name = m_Path;
			}
			if (m_Path == null)
			{
				m_Path = string.Empty;
			}
			tree = FileTypeUtils.ParseSourceTree(GetPropertyString("sourceTree"));
			m_ExplicitFileType = GetPropertyString("explicitFileType");
			m_LastKnownFileType = GetPropertyString("lastKnownFileType");
		}
	}
}
