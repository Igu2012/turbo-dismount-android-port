#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SXXcodeApi.PBX;

namespace SXXcodeApi
{
	public class PBXProject
	{
		private Dictionary<string, SectionBase> m_Section;

		private PBXElementDict m_RootElements;

		private PBXElementDict m_UnknownObjects;

		private string m_ObjectVersion;

		private List<string> m_SectionOrder;

		private Dictionary<string, KnownSectionBase<PBXObject>> m_UnknownSections;

		private KnownSectionBase<PBXBuildFile> buildFiles;

		private KnownSectionBase<PBXFileReference> fileRefs;

		private KnownSectionBase<PBXGroup> groups;

		private KnownSectionBase<PBXContainerItemProxy> containerItems;

		private KnownSectionBase<PBXReferenceProxy> references;

		private KnownSectionBase<PBXSourcesBuildPhase> sources;

		private KnownSectionBase<PBXFrameworksBuildPhase> frameworks;

		private KnownSectionBase<PBXResourcesBuildPhase> resources;

		private KnownSectionBase<PBXCopyFilesBuildPhase> copyFiles;

		private KnownSectionBase<PBXShellScriptBuildPhase> shellScripts;

		private KnownSectionBase<PBXNativeTarget> nativeTargets;

		private KnownSectionBase<PBXTargetDependency> targetDependencies;

		private KnownSectionBase<PBXVariantGroup> variantGroups;

		private KnownSectionBase<XCBuildConfiguration> buildConfigs;

		private KnownSectionBase<XCConfigurationList> configs;

		private PBXProjectSection project;

		private Dictionary<string, Dictionary<string, PBXBuildFile>> m_FileGuidToBuildFileMap;

		private Dictionary<string, PBXFileReference> m_ProjectPathToFileRefMap;

		private Dictionary<string, string> m_FileRefGuidToProjectPathMap;

		private Dictionary<PBXSourceTree, Dictionary<string, PBXFileReference>> m_RealPathToFileRefMap;

		private Dictionary<string, PBXGroup> m_ProjectPathToGroupMap;

		private Dictionary<string, string> m_GroupGuidToProjectPathMap;

		private Dictionary<string, PBXGroup> m_GuidToParentGroupMap;

		private void BuildFilesAdd(string targetGuid, PBXBuildFile buildFile)
		{
			if (!m_FileGuidToBuildFileMap.ContainsKey(targetGuid))
			{
				m_FileGuidToBuildFileMap[targetGuid] = new Dictionary<string, PBXBuildFile>();
			}
			m_FileGuidToBuildFileMap[targetGuid][buildFile.fileRef] = buildFile;
			buildFiles.AddEntry(buildFile);
		}

		private void BuildFilesRemove(string targetGuid, string fileGuid)
		{
			PBXBuildFile pBXBuildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
			if (pBXBuildFile != null)
			{
				m_FileGuidToBuildFileMap[targetGuid].Remove(pBXBuildFile.fileRef);
				buildFiles.RemoveEntry(pBXBuildFile.guid);
			}
		}

		private PBXBuildFile BuildFilesGetForSourceFile(string targetGuid, string fileGuid)
		{
			if (!m_FileGuidToBuildFileMap.ContainsKey(targetGuid))
			{
				return null;
			}
			if (!m_FileGuidToBuildFileMap[targetGuid].ContainsKey(fileGuid))
			{
				return null;
			}
			return m_FileGuidToBuildFileMap[targetGuid][fileGuid];
		}

		private void FileRefsAdd(string realPath, string projectPath, PBXGroup parent, PBXFileReference fileRef)
		{
			fileRefs.AddEntry(fileRef);
			m_ProjectPathToFileRefMap.Add(projectPath, fileRef);
			m_FileRefGuidToProjectPathMap.Add(fileRef.guid, projectPath);
			m_RealPathToFileRefMap[fileRef.tree].Add(realPath, fileRef);
			m_GuidToParentGroupMap.Add(fileRef.guid, parent);
		}

		private PBXFileReference FileRefsGet(string guid)
		{
			return fileRefs[guid];
		}

		private PBXFileReference FileRefsGetByRealPath(string path, PBXSourceTree sourceTree)
		{
			if (m_RealPathToFileRefMap[sourceTree].ContainsKey(path))
			{
				return m_RealPathToFileRefMap[sourceTree][path];
			}
			return null;
		}

		private PBXFileReference FileRefsGetByProjectPath(string path)
		{
			if (m_ProjectPathToFileRefMap.ContainsKey(path))
			{
				return m_ProjectPathToFileRefMap[path];
			}
			return null;
		}

		private void FileRefsRemove(string guid)
		{
			PBXFileReference pBXFileReference = fileRefs[guid];
			fileRefs.RemoveEntry(guid);
			m_ProjectPathToFileRefMap.Remove(m_FileRefGuidToProjectPathMap[guid]);
			m_FileRefGuidToProjectPathMap.Remove(guid);
			foreach (PBXSourceTree item in FileTypeUtils.AllAbsoluteSourceTrees())
			{
				m_RealPathToFileRefMap[item].Remove(pBXFileReference.path);
			}
			m_GuidToParentGroupMap.Remove(guid);
		}

		private PBXGroup GroupsGet(string guid)
		{
			return groups[guid];
		}

		private PBXGroup GroupsGetByChild(string childGuid)
		{
			return m_GuidToParentGroupMap[childGuid];
		}

		private PBXGroup GroupsGetMainGroup()
		{
			return groups[project.project.mainGroup];
		}

		private PBXGroup GroupsGetByProjectPath(string sourceGroup)
		{
			if (m_ProjectPathToGroupMap.ContainsKey(sourceGroup))
			{
				return m_ProjectPathToGroupMap[sourceGroup];
			}
			return null;
		}

		private void GroupsAdd(string projectPath, PBXGroup parent, PBXGroup gr)
		{
			m_ProjectPathToGroupMap.Add(projectPath, gr);
			m_GroupGuidToProjectPathMap.Add(gr.guid, projectPath);
			m_GuidToParentGroupMap.Add(gr.guid, parent);
			groups.AddEntry(gr);
		}

		private void GroupsRemove(string guid)
		{
			m_ProjectPathToGroupMap.Remove(m_GroupGuidToProjectPathMap[guid]);
			m_GroupGuidToProjectPathMap.Remove(guid);
			m_GuidToParentGroupMap.Remove(guid);
			groups.RemoveEntry(guid);
		}

		private void RefreshBuildFilesMapForBuildFileGuidList(Dictionary<string, PBXBuildFile> mapForTarget, FileGUIDListBase list)
		{
			foreach (string item in (IEnumerable<string>)list.files)
			{
				PBXBuildFile pBXBuildFile = buildFiles[item];
				mapForTarget[pBXBuildFile.fileRef] = pBXBuildFile;
			}
		}

		private void RefreshMapsForGroupChildren(string projectPath, string realPath, PBXSourceTree realPathTree, PBXGroup parent)
		{
			List<string> list = new List<string>(parent.children);
			foreach (string item in list)
			{
				PBXFileReference pBXFileReference = fileRefs[item];
				string resPath;
				PBXSourceTree resTree;
				if (pBXFileReference != null)
				{
					string text = Utils.CombinePaths(projectPath, pBXFileReference.name);
					Utils.CombinePaths(realPath, realPathTree, pBXFileReference.path, pBXFileReference.tree, out resPath, out resTree);
					if (!m_ProjectPathToFileRefMap.ContainsKey(text))
					{
						m_ProjectPathToFileRefMap.Add(text, pBXFileReference);
					}
					if (!m_FileRefGuidToProjectPathMap.ContainsKey(pBXFileReference.guid))
					{
						m_FileRefGuidToProjectPathMap.Add(pBXFileReference.guid, text);
					}
					if (!m_RealPathToFileRefMap[resTree].ContainsKey(resPath))
					{
						m_RealPathToFileRefMap[resTree].Add(resPath, pBXFileReference);
					}
					if (!m_GuidToParentGroupMap.ContainsKey(item))
					{
						m_GuidToParentGroupMap.Add(item, parent);
					}
					continue;
				}
				PBXGroup pBXGroup = groups[item];
				if (pBXGroup != null)
				{
					string text = Utils.CombinePaths(projectPath, pBXGroup.name);
					Utils.CombinePaths(realPath, realPathTree, pBXGroup.path, pBXGroup.tree, out resPath, out resTree);
					if (!m_ProjectPathToGroupMap.ContainsKey(text))
					{
						m_ProjectPathToGroupMap.Add(text, pBXGroup);
					}
					if (!m_GroupGuidToProjectPathMap.ContainsKey(pBXGroup.guid))
					{
						m_GroupGuidToProjectPathMap.Add(pBXGroup.guid, text);
					}
					if (!m_GuidToParentGroupMap.ContainsKey(item))
					{
						m_GuidToParentGroupMap.Add(item, parent);
					}
					RefreshMapsForGroupChildren(text, resPath, resTree, pBXGroup);
				}
			}
		}

		private void RefreshAuxMaps()
		{
			foreach (KeyValuePair<string, PBXNativeTarget> entry in nativeTargets.GetEntries())
			{
				Dictionary<string, PBXBuildFile> dictionary = new Dictionary<string, PBXBuildFile>();
				foreach (string item in (IEnumerable<string>)entry.Value.phases)
				{
					if (frameworks.HasEntry(item))
					{
						RefreshBuildFilesMapForBuildFileGuidList(dictionary, frameworks[item]);
					}
					if (resources.HasEntry(item))
					{
						RefreshBuildFilesMapForBuildFileGuidList(dictionary, resources[item]);
					}
					if (sources.HasEntry(item))
					{
						RefreshBuildFilesMapForBuildFileGuidList(dictionary, sources[item]);
					}
					if (copyFiles.HasEntry(item))
					{
						RefreshBuildFilesMapForBuildFileGuidList(dictionary, copyFiles[item]);
					}
				}
				m_FileGuidToBuildFileMap[entry.Key] = dictionary;
			}
			RefreshMapsForGroupChildren(string.Empty, string.Empty, PBXSourceTree.Source, GroupsGetMainGroup());
		}

		private void Clear()
		{
			buildFiles = new KnownSectionBase<PBXBuildFile>("PBXBuildFile");
			fileRefs = new KnownSectionBase<PBXFileReference>("PBXFileReference");
			groups = new KnownSectionBase<PBXGroup>("PBXGroup");
			containerItems = new KnownSectionBase<PBXContainerItemProxy>("PBXContainerItemProxy");
			references = new KnownSectionBase<PBXReferenceProxy>("PBXReferenceProxy");
			sources = new KnownSectionBase<PBXSourcesBuildPhase>("PBXSourcesBuildPhase");
			frameworks = new KnownSectionBase<PBXFrameworksBuildPhase>("PBXFrameworksBuildPhase");
			resources = new KnownSectionBase<PBXResourcesBuildPhase>("PBXResourcesBuildPhase");
			copyFiles = new KnownSectionBase<PBXCopyFilesBuildPhase>("PBXCopyFilesBuildPhase");
			shellScripts = new KnownSectionBase<PBXShellScriptBuildPhase>("PBXShellScriptBuildPhase");
			nativeTargets = new KnownSectionBase<PBXNativeTarget>("PBXNativeTarget");
			targetDependencies = new KnownSectionBase<PBXTargetDependency>("PBXTargetDependency");
			variantGroups = new KnownSectionBase<PBXVariantGroup>("PBXVariantGroup");
			buildConfigs = new KnownSectionBase<XCBuildConfiguration>("XCBuildConfiguration");
			configs = new KnownSectionBase<XCConfigurationList>("XCConfigurationList");
			project = new PBXProjectSection();
			m_UnknownSections = new Dictionary<string, KnownSectionBase<PBXObject>>();
			m_Section = new Dictionary<string, SectionBase>
			{
				{ "PBXBuildFile", buildFiles },
				{ "PBXFileReference", fileRefs },
				{ "PBXGroup", groups },
				{ "PBXContainerItemProxy", containerItems },
				{ "PBXReferenceProxy", references },
				{ "PBXSourcesBuildPhase", sources },
				{ "PBXFrameworksBuildPhase", frameworks },
				{ "PBXResourcesBuildPhase", resources },
				{ "PBXCopyFilesBuildPhase", copyFiles },
				{ "PBXShellScriptBuildPhase", shellScripts },
				{ "PBXNativeTarget", nativeTargets },
				{ "PBXTargetDependency", targetDependencies },
				{ "PBXVariantGroup", variantGroups },
				{ "XCBuildConfiguration", buildConfigs },
				{ "XCConfigurationList", configs },
				{ "PBXProject", project }
			};
			m_RootElements = new PBXElementDict();
			m_UnknownObjects = new PBXElementDict();
			m_ObjectVersion = null;
			m_SectionOrder = new List<string>
			{
				"PBXBuildFile", "PBXContainerItemProxy", "PBXCopyFilesBuildPhase", "PBXFileReference", "PBXFrameworksBuildPhase", "PBXGroup", "PBXNativeTarget", "PBXProject", "PBXReferenceProxy", "PBXResourcesBuildPhase",
				"PBXShellScriptBuildPhase", "PBXSourcesBuildPhase", "PBXTargetDependency", "PBXVariantGroup", "XCBuildConfiguration", "XCConfigurationList"
			};
			m_FileGuidToBuildFileMap = new Dictionary<string, Dictionary<string, PBXBuildFile>>();
			m_ProjectPathToFileRefMap = new Dictionary<string, PBXFileReference>();
			m_FileRefGuidToProjectPathMap = new Dictionary<string, string>();
			m_RealPathToFileRefMap = new Dictionary<PBXSourceTree, Dictionary<string, PBXFileReference>>();
			foreach (PBXSourceTree item in FileTypeUtils.AllAbsoluteSourceTrees())
			{
				m_RealPathToFileRefMap.Add(item, new Dictionary<string, PBXFileReference>());
			}
			m_ProjectPathToGroupMap = new Dictionary<string, PBXGroup>();
			m_GroupGuidToProjectPathMap = new Dictionary<string, string>();
			m_GuidToParentGroupMap = new Dictionary<string, PBXGroup>();
		}

		public static string GetPBXProjectPath(string buildPath)
		{
			return Utils.CombinePaths(buildPath, "Unity-iPhone.xcodeproj/project.pbxproj");
		}

		public static string GetUnityTargetName()
		{
			return "Unity-iPhone";
		}

		public static string GetUnityTestTargetName()
		{
			return "Unity-iPhone Tests";
		}

		public string TargetGuidByName(string name)
		{
			foreach (KeyValuePair<string, PBXNativeTarget> entry in nativeTargets.GetEntries())
			{
				if (entry.Value.name == name)
				{
					return entry.Key;
				}
			}
			return null;
		}

		internal string ProjectGuid()
		{
			return project.project.guid;
		}

		private FileGUIDListBase BuildSection(PBXNativeTarget target, string path)
		{
			string extension = Path.GetExtension(path);
			switch (FileTypeUtils.GetFileType(extension))
			{
			case PBXFileType.Framework:
				foreach (string item in (IEnumerable<string>)target.phases)
				{
					if (frameworks.HasEntry(item))
					{
						return frameworks[item];
					}
				}
				break;
			case PBXFileType.Resource:
				foreach (string item2 in (IEnumerable<string>)target.phases)
				{
					if (resources.HasEntry(item2))
					{
						return resources[item2];
					}
				}
				break;
			case PBXFileType.Source:
				foreach (string item3 in (IEnumerable<string>)target.phases)
				{
					if (sources.HasEntry(item3))
					{
						return sources[item3];
					}
				}
				break;
			case PBXFileType.CopyFile:
				foreach (string item4 in (IEnumerable<string>)target.phases)
				{
					if (copyFiles.HasEntry(item4))
					{
						return copyFiles[item4];
					}
				}
				break;
			}
			return null;
		}

		public static bool IsKnownExtension(string ext)
		{
			return FileTypeUtils.IsKnownExtension(ext);
		}

		public static bool IsBuildable(string ext)
		{
			return FileTypeUtils.IsBuildable(ext);
		}

		private string AddFileImpl(string path, string projectPath, PBXSourceTree tree)
		{
			path = Utils.FixSlashesInPath(path);
			projectPath = Utils.FixSlashesInPath(projectPath);
			string extension = Path.GetExtension(path);
			if (extension != Path.GetExtension(projectPath))
			{
				throw new Exception("Project and real path extensions do not match");
			}
			string text = FindFileGuidByProjectPath(projectPath);
			if (text == null)
			{
				text = FindFileGuidByRealPath(path);
			}
			if (text == null)
			{
				PBXFileReference pBXFileReference = PBXFileReference.CreateFromFile(path, Utils.GetFilenameFromPath(projectPath), tree);
				PBXGroup pBXGroup = CreateSourceGroup(Utils.GetDirectoryFromPath(projectPath));
				pBXGroup.children.AddGUID(pBXFileReference.guid);
				FileRefsAdd(path, projectPath, pBXGroup, pBXFileReference);
				text = pBXFileReference.guid;
			}
			return text;
		}

		public string AddFile(string path, string projectPath)
		{
			return AddFileImpl(path, projectPath, PBXSourceTree.Source);
		}

		public string AddFile(string path, string projectPath, PBXSourceTree sourceTree)
		{
			if (sourceTree == PBXSourceTree.Group)
			{
				throw new Exception("sourceTree must not be PBXSourceTree.Group");
			}
			return AddFileImpl(path, projectPath, sourceTree);
		}

		private void AddBuildFileImpl(string targetGuid, string fileGuid, bool weak, string compileFlags)
		{
			PBXNativeTarget target = nativeTargets[targetGuid];
			string extension = Path.GetExtension(fileRefs[fileGuid].path);
			if (FileTypeUtils.IsBuildable(extension) && BuildFilesGetForSourceFile(targetGuid, fileGuid) == null)
			{
				PBXBuildFile pBXBuildFile = PBXBuildFile.CreateFromFile(fileGuid, weak, compileFlags);
				BuildFilesAdd(targetGuid, pBXBuildFile);
				BuildSection(target, extension).files.AddGUID(pBXBuildFile.guid);
			}
		}

		public void AddFileToBuild(string targetGuid, string fileGuid)
		{
			AddBuildFileImpl(targetGuid, fileGuid, false, null);
		}

		public void AddFileToBuildWithFlags(string targetGuid, string fileGuid, string compileFlags)
		{
			AddBuildFileImpl(targetGuid, fileGuid, false, compileFlags);
		}

		public List<string> GetCompileFlagsForFile(string targetGuid, string fileGuid)
		{
			PBXBuildFile pBXBuildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
			if (pBXBuildFile == null)
			{
				return null;
			}
			if (pBXBuildFile.compileFlags == null)
			{
				return new List<string>();
			}
			List<string> list = new List<string>();
			list.Add(pBXBuildFile.compileFlags);
			return list;
		}

		public void SetCompileFlagsForFile(string targetGuid, string fileGuid, List<string> compileFlags)
		{
			PBXBuildFile pBXBuildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
			if (pBXBuildFile != null)
			{
				pBXBuildFile.compileFlags = string.Join(" ", compileFlags.ToArray());
			}
		}

		public bool ContainsFileByRealPath(string path)
		{
			return FindFileGuidByRealPath(path) != null;
		}

		public bool ContainsFileByRealPath(string path, PBXSourceTree sourceTree)
		{
			if (sourceTree == PBXSourceTree.Group)
			{
				throw new Exception("sourceTree must not be PBXSourceTree.Group");
			}
			return FindFileGuidByRealPath(path, sourceTree) != null;
		}

		public bool ContainsFileByProjectPath(string path)
		{
			return FindFileGuidByProjectPath(path) != null;
		}

		public bool HasFramework(string framework)
		{
			return ContainsFileByRealPath("System/Library/Frameworks/" + framework);
		}

		public void AddFrameworkToProject(string targetGuid, string framework, bool weak)
		{
			string fileGuid = AddFile("System/Library/Frameworks/" + framework, "Frameworks/" + framework, PBXSourceTree.Sdk);
			AddBuildFileImpl(targetGuid, fileGuid, weak, null);
		}

		public void RemoveFrameworkFromProject(string targetGuid, string framework)
		{
			string text = FindFileGuidByRealPath("System/Library/Frameworks/" + framework);
			if (text != null)
			{
				RemoveFile(text);
			}
		}

		public string FindFileGuidByRealPath(string path, PBXSourceTree sourceTree)
		{
			if (sourceTree == PBXSourceTree.Group)
			{
				throw new Exception("sourceTree must not be PBXSourceTree.Group");
			}
			path = Utils.FixSlashesInPath(path);
			PBXFileReference pBXFileReference = FileRefsGetByRealPath(path, sourceTree);
			if (pBXFileReference != null)
			{
				return pBXFileReference.guid;
			}
			return null;
		}

		public string FindFileGuidByRealPath(string path)
		{
			path = Utils.FixSlashesInPath(path);
			foreach (PBXSourceTree item in FileTypeUtils.AllAbsoluteSourceTrees())
			{
				string text = FindFileGuidByRealPath(path, item);
				if (text != null)
				{
					return text;
				}
			}
			return null;
		}

		public string FindFileGuidByProjectPath(string path)
		{
			path = Utils.FixSlashesInPath(path);
			PBXFileReference pBXFileReference = FileRefsGetByProjectPath(path);
			if (pBXFileReference != null)
			{
				return pBXFileReference.guid;
			}
			return null;
		}

		public void RemoveFileFromBuild(string targetGuid, string fileGuid)
		{
			PBXBuildFile pBXBuildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
			if (pBXBuildFile == null)
			{
				return;
			}
			BuildFilesRemove(targetGuid, fileGuid);
			string guid = pBXBuildFile.guid;
			if (guid == null)
			{
				return;
			}
			foreach (KeyValuePair<string, PBXSourcesBuildPhase> entry in sources.GetEntries())
			{
				entry.Value.files.RemoveGUID(guid);
			}
			foreach (KeyValuePair<string, PBXResourcesBuildPhase> entry2 in resources.GetEntries())
			{
				entry2.Value.files.RemoveGUID(guid);
			}
			foreach (KeyValuePair<string, PBXCopyFilesBuildPhase> entry3 in copyFiles.GetEntries())
			{
				entry3.Value.files.RemoveGUID(guid);
			}
			foreach (KeyValuePair<string, PBXFrameworksBuildPhase> entry4 in frameworks.GetEntries())
			{
				entry4.Value.files.RemoveGUID(guid);
			}
		}

		public void RemoveFile(string fileGuid)
		{
			if (fileGuid == null)
			{
				return;
			}
			PBXGroup pBXGroup = GroupsGetByChild(fileGuid);
			if (pBXGroup != null)
			{
				pBXGroup.children.RemoveGUID(fileGuid);
			}
			RemoveGroupIfEmpty(pBXGroup);
			foreach (KeyValuePair<string, PBXNativeTarget> entry in nativeTargets.GetEntries())
			{
				RemoveFileFromBuild(entry.Value.guid, fileGuid);
			}
			FileRefsRemove(fileGuid);
		}

		private void RemoveGroupIfEmpty(PBXGroup gr)
		{
			if (gr.children.Count == 0 && gr != GroupsGetMainGroup())
			{
				PBXGroup pBXGroup = GroupsGetByChild(gr.guid);
				pBXGroup.children.RemoveGUID(gr.guid);
				RemoveGroupIfEmpty(pBXGroup);
				GroupsRemove(gr.guid);
			}
		}

		private void RemoveGroupChildrenRecursive(PBXGroup parent)
		{
			List<string> list = new List<string>(parent.children);
			parent.children.Clear();
			foreach (string item in list)
			{
				PBXFileReference pBXFileReference = fileRefs[item];
				if (pBXFileReference != null)
				{
					foreach (KeyValuePair<string, PBXNativeTarget> entry in nativeTargets.GetEntries())
					{
						RemoveFileFromBuild(entry.Value.guid, item);
					}
					FileRefsRemove(item);
					continue;
				}
				PBXGroup pBXGroup = groups[item];
				if (pBXGroup != null)
				{
					RemoveGroupChildrenRecursive(pBXGroup);
					GroupsRemove(pBXGroup.guid);
				}
			}
		}

		internal void RemoveFilesByProjectPathRecursive(string projectPath)
		{
			projectPath = Utils.FixSlashesInPath(projectPath);
			PBXGroup pBXGroup = GroupsGetByProjectPath(projectPath);
			if (pBXGroup != null)
			{
				RemoveGroupChildrenRecursive(pBXGroup);
				RemoveGroupIfEmpty(pBXGroup);
			}
		}

		internal List<string> GetGroupChildrenFiles(string projectPath)
		{
			projectPath = Utils.FixSlashesInPath(projectPath);
			PBXGroup pBXGroup = GroupsGetByProjectPath(projectPath);
			if (pBXGroup == null)
			{
				return null;
			}
			List<string> list = new List<string>();
			foreach (string item in (IEnumerable<string>)pBXGroup.children)
			{
				list.Add(FileRefsGet(item).name);
			}
			return list;
		}

		private PBXGroup GetPBXGroupChildByName(PBXGroup group, string name)
		{
			foreach (string item in (IEnumerable<string>)group.children)
			{
				PBXGroup pBXGroup = groups[item];
				if (pBXGroup != null && pBXGroup.name == name)
				{
					return pBXGroup;
				}
			}
			return null;
		}

		private PBXGroup CreateSourceGroup(string sourceGroup)
		{
			sourceGroup = Utils.FixSlashesInPath(sourceGroup);
			if (sourceGroup == null || sourceGroup == string.Empty)
			{
				return GroupsGetMainGroup();
			}
			PBXGroup pBXGroup = GroupsGetByProjectPath(sourceGroup);
			if (pBXGroup != null)
			{
				return pBXGroup;
			}
			pBXGroup = GroupsGetMainGroup();
			string[] array = sourceGroup.Trim('/').Split('/');
			string text = null;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				text = ((text != null) ? (text + "/" + text2) : text2);
				PBXGroup pBXGroupChildByName = GetPBXGroupChildByName(pBXGroup, text2);
				if (pBXGroupChildByName != null)
				{
					pBXGroup = pBXGroupChildByName;
					continue;
				}
				PBXGroup pBXGroup2 = PBXGroup.Create(text2, text2, PBXSourceTree.Group);
				pBXGroup.children.AddGUID(pBXGroup2.guid);
				GroupsAdd(text, pBXGroup, pBXGroup2);
				pBXGroup = pBXGroup2;
			}
			return pBXGroup;
		}

		public void AddExternalProjectDependency(string path, string projectPath, PBXSourceTree sourceTree)
		{
			if (sourceTree == PBXSourceTree.Group)
			{
				throw new Exception("sourceTree must not be PBXSourceTree.Group");
			}
			path = Utils.FixSlashesInPath(path);
			projectPath = Utils.FixSlashesInPath(projectPath);
			PBXGroup pBXGroup = PBXGroup.CreateRelative("Products");
			groups.AddEntry(pBXGroup);
			PBXFileReference pBXFileReference = PBXFileReference.CreateFromFile(path, Path.GetFileName(projectPath), sourceTree);
			FileRefsAdd(path, projectPath, null, pBXFileReference);
			CreateSourceGroup(Utils.GetDirectoryFromPath(projectPath)).children.AddGUID(pBXFileReference.guid);
			project.project.AddReference(pBXGroup.guid, pBXFileReference.guid);
		}

		public void AddExternalLibraryDependency(string targetGuid, string filename, string remoteFileGuid, string projectPath, string remoteInfo)
		{
			PBXNativeTarget target = nativeTargets[targetGuid];
			filename = Utils.FixSlashesInPath(filename);
			projectPath = Utils.FixSlashesInPath(projectPath);
			string text = FindFileGuidByRealPath(projectPath);
			if (text == null)
			{
				throw new Exception("No such project");
			}
			string text2 = null;
			foreach (ProjectReference projectReference in project.project.projectReferences)
			{
				if (projectReference.projectRef == text)
				{
					text2 = projectReference.group;
					break;
				}
			}
			if (text2 == null)
			{
				throw new Exception("Malformed project: no project in project references");
			}
			PBXGroup pBXGroup = groups[text2];
			string extension = Path.GetExtension(filename);
			if (!FileTypeUtils.IsBuildable(extension))
			{
				throw new Exception("Wrong file extension");
			}
			PBXContainerItemProxy pBXContainerItemProxy = PBXContainerItemProxy.Create(text, "2", remoteFileGuid, remoteInfo);
			containerItems.AddEntry(pBXContainerItemProxy);
			string typeName = FileTypeUtils.GetTypeName(extension);
			PBXReferenceProxy pBXReferenceProxy = PBXReferenceProxy.Create(filename, typeName, pBXContainerItemProxy.guid, "BUILT_PRODUCTS_DIR");
			references.AddEntry(pBXReferenceProxy);
			PBXBuildFile pBXBuildFile = PBXBuildFile.CreateFromFile(pBXReferenceProxy.guid, false, null);
			BuildFilesAdd(targetGuid, pBXBuildFile);
			BuildSection(target, extension).files.AddGUID(pBXBuildFile.guid);
			pBXGroup.children.AddGUID(pBXReferenceProxy.guid);
		}

		private void SetDefaultAppExtensionReleaseBuildFlags(XCBuildConfiguration config, string infoPlistPath)
		{
			config.AddProperty("ALWAYS_SEARCH_USER_PATHS", "NO");
			config.AddProperty("CLANG_CXX_LANGUAGE_STANDARD", "gnu++0x");
			config.AddProperty("CLANG_CXX_LIBRARY", "libc++");
			config.AddProperty("CLANG_ENABLE_MODULES", "YES");
			config.AddProperty("CLANG_ENABLE_OBJC_ARC", "YES");
			config.AddProperty("CLANG_WARN_BOOL_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_CONSTANT_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_DIRECT_OBJC_ISA_USAGE", "YES_ERROR");
			config.AddProperty("CLANG_WARN_EMPTY_BODY", "YES");
			config.AddProperty("CLANG_WARN_ENUM_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_INT_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_OBJC_ROOT_CLASS", "YES_ERROR");
			config.AddProperty("CLANG_WARN_UNREACHABLE_CODE", "YES");
			config.AddProperty("CLANG_WARN__DUPLICATE_METHOD_MATCH", "YES");
			config.AddProperty("COPY_PHASE_STRIP", "YES");
			config.AddProperty("ENABLE_NS_ASSERTIONS", "NO");
			config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
			config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
			config.AddProperty("GCC_WARN_64_TO_32_BIT_CONVERSION", "YES");
			config.AddProperty("GCC_WARN_ABOUT_RETURN_TYPE", "YES_ERROR");
			config.AddProperty("GCC_WARN_UNDECLARED_SELECTOR", "YES");
			config.AddProperty("GCC_WARN_UNINITIALIZED_AUTOS", "YES_AGGRESSIVE");
			config.AddProperty("GCC_WARN_UNUSED_FUNCTION", "YES");
			config.AddProperty("INFOPLIST_FILE", infoPlistPath);
			config.AddProperty("IPHONEOS_DEPLOYMENT_TARGET", "8.0");
			config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks @executable_path/../../Frameworks");
			config.AddProperty("MTL_ENABLE_DEBUG_INFO", "NO");
			config.AddProperty("PRODUCT_NAME", "$(TARGET_NAME)");
			config.AddProperty("SKIP_INSTALL", "YES");
			config.AddProperty("VALIDATE_PRODUCT", "YES");
		}

		private void SetDefaultAppExtensionDebugBuildFlags(XCBuildConfiguration config, string infoPlistPath)
		{
			config.AddProperty("ALWAYS_SEARCH_USER_PATHS", "NO");
			config.AddProperty("CLANG_CXX_LANGUAGE_STANDARD", "gnu++0x");
			config.AddProperty("CLANG_CXX_LIBRARY", "libc++");
			config.AddProperty("CLANG_ENABLE_MODULES", "YES");
			config.AddProperty("CLANG_ENABLE_OBJC_ARC", "YES");
			config.AddProperty("CLANG_WARN_BOOL_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_CONSTANT_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_DIRECT_OBJC_ISA_USAGE", "YES_ERROR");
			config.AddProperty("CLANG_WARN_EMPTY_BODY", "YES");
			config.AddProperty("CLANG_WARN_ENUM_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_INT_CONVERSION", "YES");
			config.AddProperty("CLANG_WARN_OBJC_ROOT_CLASS", "YES_ERROR");
			config.AddProperty("CLANG_WARN_UNREACHABLE_CODE", "YES");
			config.AddProperty("CLANG_WARN__DUPLICATE_METHOD_MATCH", "YES");
			config.AddProperty("COPY_PHASE_STRIP", "NO");
			config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
			config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
			config.AddProperty("GCC_DYNAMIC_NO_PIC", "NO");
			config.AddProperty("GCC_OPTIMIZATION_LEVEL", "0");
			config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "DEBUG=1");
			config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "$(inherited)");
			config.AddProperty("GCC_SYMBOLS_PRIVATE_EXTERN", "NO");
			config.AddProperty("GCC_WARN_64_TO_32_BIT_CONVERSION", "YES");
			config.AddProperty("GCC_WARN_ABOUT_RETURN_TYPE", "YES_ERROR");
			config.AddProperty("GCC_WARN_UNDECLARED_SELECTOR", "YES");
			config.AddProperty("GCC_WARN_UNINITIALIZED_AUTOS", "YES_AGGRESSIVE");
			config.AddProperty("GCC_WARN_UNUSED_FUNCTION", "YES");
			config.AddProperty("INFOPLIST_FILE", infoPlistPath);
			config.AddProperty("IPHONEOS_DEPLOYMENT_TARGET", "8.0");
			config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
			config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
			config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/../../Frameworks");
			config.AddProperty("MTL_ENABLE_DEBUG_INFO", "YES");
			config.AddProperty("ONLY_ACTIVE_ARCH", "YES");
			config.AddProperty("PRODUCT_NAME", "$(TARGET_NAME)");
			config.AddProperty("SKIP_INSTALL", "YES");
		}

		internal string AddAppExtension(string mainTarget, string name, string infoPlistPath)
		{
			string text = ".appex";
			string text2 = name + text;
			PBXFileReference pBXFileReference = PBXFileReference.CreateFromFile("Products/" + text2, "Products/" + text2, PBXSourceTree.Group);
			XCBuildConfiguration xCBuildConfiguration = XCBuildConfiguration.Create("Release");
			buildConfigs.AddEntry(xCBuildConfiguration);
			SetDefaultAppExtensionReleaseBuildFlags(xCBuildConfiguration, infoPlistPath);
			XCBuildConfiguration xCBuildConfiguration2 = XCBuildConfiguration.Create("Debug");
			buildConfigs.AddEntry(xCBuildConfiguration2);
			SetDefaultAppExtensionDebugBuildFlags(xCBuildConfiguration2, infoPlistPath);
			XCConfigurationList xCConfigurationList = XCConfigurationList.Create();
			configs.AddEntry(xCConfigurationList);
			xCConfigurationList.buildConfigs.AddGUID(xCBuildConfiguration.guid);
			xCConfigurationList.buildConfigs.AddGUID(xCBuildConfiguration2.guid);
			PBXNativeTarget pBXNativeTarget = PBXNativeTarget.Create(name, pBXFileReference.guid, "com.apple.product-type.app-extension", xCConfigurationList.guid);
			nativeTargets.AddEntry(pBXNativeTarget);
			project.project.targets.Add(pBXNativeTarget.guid);
			PBXSourcesBuildPhase pBXSourcesBuildPhase = PBXSourcesBuildPhase.Create();
			sources.AddEntry(pBXSourcesBuildPhase);
			pBXNativeTarget.phases.AddGUID(pBXSourcesBuildPhase.guid);
			PBXResourcesBuildPhase pBXResourcesBuildPhase = PBXResourcesBuildPhase.Create();
			resources.AddEntry(pBXResourcesBuildPhase);
			pBXNativeTarget.phases.AddGUID(pBXResourcesBuildPhase.guid);
			PBXFrameworksBuildPhase pBXFrameworksBuildPhase = PBXFrameworksBuildPhase.Create();
			frameworks.AddEntry(pBXFrameworksBuildPhase);
			pBXNativeTarget.phases.AddGUID(pBXFrameworksBuildPhase.guid);
			PBXCopyFilesBuildPhase pBXCopyFilesBuildPhase = PBXCopyFilesBuildPhase.Create("Embed App Extensions", "13");
			copyFiles.AddEntry(pBXCopyFilesBuildPhase);
			nativeTargets[mainTarget].phases.AddGUID(pBXCopyFilesBuildPhase.guid);
			PBXContainerItemProxy pBXContainerItemProxy = PBXContainerItemProxy.Create(project.project.guid, "1", pBXNativeTarget.guid, name);
			containerItems.AddEntry(pBXContainerItemProxy);
			PBXTargetDependency pBXTargetDependency = PBXTargetDependency.Create(pBXNativeTarget.guid, pBXContainerItemProxy.guid);
			targetDependencies.AddEntry(pBXTargetDependency);
			nativeTargets[mainTarget].dependencies.AddGUID(pBXTargetDependency.guid);
			AddFile(text2, "Products/" + text2, PBXSourceTree.Build);
			PBXBuildFile pBXBuildFile = PBXBuildFile.CreateFromFile(FindFileGuidByProjectPath("Products/" + text2), false, string.Empty);
			BuildFilesAdd(mainTarget, pBXBuildFile);
			pBXCopyFilesBuildPhase.files.AddGUID(pBXBuildFile.guid);
			AddFile(infoPlistPath, name + "/Supporting Files/Info.plist", PBXSourceTree.Group);
			return pBXNativeTarget.guid;
		}

		public string BuildConfigByName(string targetGuid, string name)
		{
			PBXNativeTarget pBXNativeTarget = nativeTargets[targetGuid];
			foreach (string item in (IEnumerable<string>)configs[pBXNativeTarget.buildConfigList].buildConfigs)
			{
				XCBuildConfiguration xCBuildConfiguration = buildConfigs[item];
				if (xCBuildConfiguration != null && xCBuildConfiguration.name == name)
				{
					return xCBuildConfiguration.guid;
				}
			}
			return null;
		}

		private string GetConfigListForTarget(string targetGuid)
		{
			if (targetGuid == project.project.guid)
			{
				return project.project.buildConfigList;
			}
			return nativeTargets[targetGuid].buildConfigList;
		}

		public void AddBuildProperty(string targetGuid, string name, string value)
		{
			foreach (string item in (IEnumerable<string>)configs[GetConfigListForTarget(targetGuid)].buildConfigs)
			{
				AddBuildPropertyForConfig(item, name, value);
			}
		}

		public void AddBuildProperty(string[] targetGuids, string name, string value)
		{
			foreach (string targetGuid in targetGuids)
			{
				AddBuildProperty(targetGuid, name, value);
			}
		}

		public void AddBuildPropertyForConfig(string configGuid, string name, string value)
		{
			buildConfigs[configGuid].AddProperty(name, value);
		}

		public void AddBuildPropertyForConfig(string[] configGuids, string name, string value)
		{
			foreach (string configGuid in configGuids)
			{
				AddBuildPropertyForConfig(configGuid, name, value);
			}
		}

		public void SetBuildProperty(string targetGuid, string name, string value)
		{
			foreach (string item in (IEnumerable<string>)configs[GetConfigListForTarget(targetGuid)].buildConfigs)
			{
				SetBuildPropertyForConfig(item, name, value);
			}
		}

		public void SetBuildProperty(string[] targetGuids, string name, string value)
		{
			foreach (string targetGuid in targetGuids)
			{
				SetBuildProperty(targetGuid, name, value);
			}
		}

		public void SetBuildPropertyForConfig(string configGuid, string name, string value)
		{
			buildConfigs[configGuid].SetProperty(name, value);
		}

		public void SetBuildPropertyForConfig(string[] configGuids, string name, string value)
		{
			foreach (string configGuid in configGuids)
			{
				SetBuildPropertyForConfig(configGuid, name, value);
			}
		}

		internal void RemoveBuildProperty(string targetGuid, string name)
		{
			foreach (string item in (IEnumerable<string>)configs[GetConfigListForTarget(targetGuid)].buildConfigs)
			{
				RemoveBuildPropertyForConfig(item, name);
			}
		}

		internal void RemoveBuildProperty(string[] targetGuids, string name)
		{
			foreach (string targetGuid in targetGuids)
			{
				RemoveBuildProperty(targetGuid, name);
			}
		}

		internal void RemoveBuildPropertyForConfig(string configGuid, string name)
		{
			buildConfigs[configGuid].RemoveProperty(name);
		}

		internal void RemoveBuildPropertyForConfig(string[] configGuids, string name)
		{
			foreach (string configGuid in configGuids)
			{
				RemoveBuildPropertyForConfig(configGuid, name);
			}
		}

		public void UpdateBuildProperty(string targetGuid, string name, string[] addValues, string[] removeValues)
		{
			foreach (string item in (IEnumerable<string>)configs[GetConfigListForTarget(targetGuid)].buildConfigs)
			{
				UpdateBuildPropertyForConfig(item, name, addValues, removeValues);
			}
		}

		public void UpdateBuildProperty(string[] targetGuids, string name, string[] addValues, string[] removeValues)
		{
			foreach (string targetGuid in targetGuids)
			{
				UpdateBuildProperty(targetGuid, name, addValues, removeValues);
			}
		}

		public void UpdateBuildPropertyForConfig(string configGuid, string name, string[] addValues, string[] removeValues)
		{
			XCBuildConfiguration xCBuildConfiguration = buildConfigs[configGuid];
			if (xCBuildConfiguration == null)
			{
				return;
			}
			if (removeValues != null)
			{
				foreach (string value in removeValues)
				{
					xCBuildConfiguration.RemovePropertyValue(name, value);
				}
			}
			if (addValues != null)
			{
				foreach (string value2 in addValues)
				{
					xCBuildConfiguration.AddProperty(name, value2);
				}
			}
		}

		public void UpdateBuildPropertyForConfig(string[] configGuids, string name, string[] addValues, string[] removeValues)
		{
			foreach (string targetGuid in configGuids)
			{
				UpdateBuildProperty(targetGuid, name, addValues, removeValues);
			}
		}

		private void BuildCommentMapForBuildFiles(GUIDToCommentMap comments, List<string> guids, string sectName)
		{
			foreach (string guid in guids)
			{
				PBXBuildFile pBXBuildFile = buildFiles[guid];
				if (pBXBuildFile == null)
				{
					continue;
				}
				PBXFileReference pBXFileReference = fileRefs[pBXBuildFile.fileRef];
				if (pBXFileReference != null)
				{
					comments.Add(guid, string.Format("{0} in {1}", pBXFileReference.name, sectName));
					continue;
				}
				PBXReferenceProxy pBXReferenceProxy = references[pBXBuildFile.fileRef];
				if (pBXReferenceProxy != null)
				{
					comments.Add(guid, string.Format("{0} in {1}", pBXReferenceProxy.path, sectName));
				}
			}
		}

		private GUIDToCommentMap BuildCommentMap()
		{
			GUIDToCommentMap gUIDToCommentMap = new GUIDToCommentMap();
			foreach (PBXGroup @object in groups.GetObjects())
			{
				gUIDToCommentMap.Add(@object.guid, @object.name);
			}
			foreach (PBXContainerItemProxy object2 in containerItems.GetObjects())
			{
				gUIDToCommentMap.Add(object2.guid, "PBXContainerItemProxy");
			}
			foreach (PBXReferenceProxy object3 in references.GetObjects())
			{
				gUIDToCommentMap.Add(object3.guid, object3.path);
			}
			foreach (PBXSourcesBuildPhase object4 in sources.GetObjects())
			{
				gUIDToCommentMap.Add(object4.guid, "Sources");
				BuildCommentMapForBuildFiles(gUIDToCommentMap, object4.files, "Sources");
			}
			foreach (PBXResourcesBuildPhase object5 in resources.GetObjects())
			{
				gUIDToCommentMap.Add(object5.guid, "Resources");
				BuildCommentMapForBuildFiles(gUIDToCommentMap, object5.files, "Resources");
			}
			foreach (PBXFrameworksBuildPhase object6 in frameworks.GetObjects())
			{
				gUIDToCommentMap.Add(object6.guid, "Frameworks");
				BuildCommentMapForBuildFiles(gUIDToCommentMap, object6.files, "Frameworks");
			}
			foreach (PBXCopyFilesBuildPhase object7 in copyFiles.GetObjects())
			{
				string text = object7.name;
				if (text == null)
				{
					text = "CopyFiles";
				}
				gUIDToCommentMap.Add(object7.guid, text);
				BuildCommentMapForBuildFiles(gUIDToCommentMap, object7.files, text);
			}
			foreach (PBXShellScriptBuildPhase object8 in shellScripts.GetObjects())
			{
				gUIDToCommentMap.Add(object8.guid, "ShellScript");
			}
			foreach (PBXTargetDependency object9 in targetDependencies.GetObjects())
			{
				gUIDToCommentMap.Add(object9.guid, "PBXTargetDependency");
			}
			foreach (PBXNativeTarget object10 in nativeTargets.GetObjects())
			{
				gUIDToCommentMap.Add(object10.guid, object10.name);
				gUIDToCommentMap.Add(object10.buildConfigList, string.Format("Build configuration list for PBXNativeTarget \"{0}\"", object10.name));
			}
			foreach (PBXVariantGroup object11 in variantGroups.GetObjects())
			{
				gUIDToCommentMap.Add(object11.guid, object11.name);
			}
			foreach (XCBuildConfiguration object12 in buildConfigs.GetObjects())
			{
				gUIDToCommentMap.Add(object12.guid, object12.name);
			}
			foreach (PBXProjectObject object13 in project.GetObjects())
			{
				gUIDToCommentMap.Add(object13.guid, "Project object");
				gUIDToCommentMap.Add(object13.buildConfigList, "Build configuration list for PBXProject \"Unity-iPhone\"");
			}
			foreach (PBXFileReference object14 in fileRefs.GetObjects())
			{
				gUIDToCommentMap.Add(object14.guid, object14.name);
			}
			if (m_RootElements.Contains("rootObject") && m_RootElements["rootObject"] is PBXElementString)
			{
				gUIDToCommentMap.Add(m_RootElements["rootObject"].AsString(), "Project object");
			}
			return gUIDToCommentMap;
		}

		public void ReadFromFile(string path)
		{
			ReadFromString(File.ReadAllText(path));
		}

		public void ReadFromString(string src)
		{
			TextReader sr = new StringReader(src);
			ReadFromStream(sr);
		}

		private static PBXElementDict ParseContent(string content)
		{
			TokenList tokens = Lexer.Tokenize(content);
			Parser parser = new Parser(tokens);
			TreeAST ast = parser.ParseTree();
			return Serializer.ParseTreeAST(ast, tokens, content);
		}

		public void ReadFromStream(TextReader sr)
		{
			Clear();
			m_RootElements = ParseContent(sr.ReadToEnd());
			if (!m_RootElements.Contains("objects"))
			{
				throw new Exception("Invalid PBX project file: no objects element");
			}
			PBXElementDict pBXElementDict = m_RootElements["objects"].AsDict();
			m_RootElements.Remove("objects");
			m_RootElements.SetString("objects", "OBJMARKER");
			if (m_RootElements.Contains("objectVersion"))
			{
				m_ObjectVersion = m_RootElements["objectVersion"].AsString();
				m_RootElements.Remove("objectVersion");
			}
			List<string> list = new List<string>();
			string prevSectionName = null;
			foreach (KeyValuePair<string, PBXElement> value2 in pBXElementDict.values)
			{
				list.Add(value2.Key);
				PBXElement value = value2.Value;
				if (!(value is PBXElementDict) || !value.AsDict().Contains("isa"))
				{
					m_UnknownObjects.values.Add(value2.Key, value);
					continue;
				}
				PBXElementDict pBXElementDict2 = value.AsDict();
				string text = pBXElementDict2["isa"].AsString();
				if (m_Section.ContainsKey(text))
				{
					SectionBase sectionBase = m_Section[text];
					sectionBase.AddObject(value2.Key, pBXElementDict2);
				}
				else
				{
					KnownSectionBase<PBXObject> knownSectionBase;
					if (m_UnknownSections.ContainsKey(text))
					{
						knownSectionBase = m_UnknownSections[text];
					}
					else
					{
						knownSectionBase = new KnownSectionBase<PBXObject>(text);
						m_UnknownSections.Add(text, knownSectionBase);
					}
					knownSectionBase.AddObject(value2.Key, pBXElementDict2);
					if (!m_SectionOrder.Contains(text))
					{
						int index = 0;
						if (prevSectionName != null)
						{
							index = m_SectionOrder.FindIndex((string x) => x == prevSectionName);
							index++;
						}
						m_SectionOrder.Insert(index, text);
					}
				}
				prevSectionName = text;
			}
			RepairStructure(list);
			RefreshAuxMaps();
		}

		public void WriteToFile(string path)
		{
			File.WriteAllText(path, WriteToString());
		}

		public void WriteToStream(TextWriter sw)
		{
			sw.Write(WriteToString());
		}

		public string WriteToString()
		{
			GUIDToCommentMap comments = BuildCommentMap();
			PropertyCommentChecker checker = new PropertyCommentChecker();
			GUIDToCommentMap comments2 = new GUIDToCommentMap();
			StringBuilder stringBuilder = new StringBuilder();
			if (m_ObjectVersion != null)
			{
				stringBuilder.AppendFormat("objectVersion = {0};\n\t", m_ObjectVersion);
			}
			stringBuilder.Append("objects = {");
			foreach (string item in m_SectionOrder)
			{
				if (m_Section.ContainsKey(item))
				{
					m_Section[item].WriteSection(stringBuilder, comments);
				}
				else if (m_UnknownSections.ContainsKey(item))
				{
					m_UnknownSections[item].WriteSection(stringBuilder, comments);
				}
			}
			foreach (KeyValuePair<string, PBXElement> value in m_UnknownObjects.values)
			{
				Serializer.WriteDictKeyValue(stringBuilder, value.Key, value.Value, 2, false, checker, comments2);
			}
			stringBuilder.Append("\n\t};");
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append("// !$*UTF8*$!\n");
			Serializer.WriteDict(stringBuilder2, m_RootElements, 0, false, new PropertyCommentChecker(new string[1] { "rootObject/*" }), comments);
			stringBuilder2.Append("\n");
			string text = stringBuilder2.ToString();
			return text.Replace("objects = OBJMARKER;", stringBuilder.ToString());
		}

		private void RepairStructure(List<string> allGuids)
		{
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			foreach (string allGuid in allGuids)
			{
				dictionary.Add(allGuid, false);
			}
			while (RepairStructureImpl(dictionary))
			{
			}
		}

		private static void RemoveMissingGuidsFromGuidList(GUIDList guidList, Dictionary<string, bool> allGuids)
		{
			List<string> list = null;
			foreach (string item in (IEnumerable<string>)guidList)
			{
				if (!allGuids.ContainsKey(item))
				{
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(item);
				}
			}
			if (list == null)
			{
				return;
			}
			foreach (string item2 in list)
			{
				guidList.RemoveGUID(item2);
			}
		}

		private static bool RemoveObjectsFromSection<T>(KnownSectionBase<T> section, Dictionary<string, bool> allGuids, Func<T, bool> checker) where T : PBXObject, new()
		{
			List<string> list = null;
			foreach (KeyValuePair<string, T> entry in section.GetEntries())
			{
				if (checker(entry.Value))
				{
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(entry.Key);
				}
			}
			if (list != null)
			{
				foreach (string item in list)
				{
					section.RemoveEntry(item);
					allGuids.Remove(item);
				}
				return true;
			}
			return false;
		}

		private bool RepairStructureImpl(Dictionary<string, bool> allGuids)
		{
			bool flag = false;
			flag |= RemoveObjectsFromSection(buildFiles, allGuids, (PBXBuildFile o) => o.fileRef == null || !allGuids.ContainsKey(o.fileRef));
			flag |= RemoveObjectsFromSection(groups, allGuids, (PBXGroup o) => o.children == null);
			foreach (PBXGroup @object in groups.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(@object.children, allGuids);
			}
			flag |= RemoveObjectsFromSection(sources, allGuids, (PBXSourcesBuildPhase o) => o.files == null);
			foreach (PBXSourcesBuildPhase object2 in sources.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object2.files, allGuids);
			}
			flag |= RemoveObjectsFromSection(frameworks, allGuids, (PBXFrameworksBuildPhase o) => o.files == null);
			foreach (PBXFrameworksBuildPhase object3 in frameworks.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object3.files, allGuids);
			}
			flag |= RemoveObjectsFromSection(resources, allGuids, (PBXResourcesBuildPhase o) => o.files == null);
			foreach (PBXResourcesBuildPhase object4 in resources.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object4.files, allGuids);
			}
			flag |= RemoveObjectsFromSection(copyFiles, allGuids, (PBXCopyFilesBuildPhase o) => o.files == null);
			foreach (PBXCopyFilesBuildPhase object5 in copyFiles.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object5.files, allGuids);
			}
			flag |= RemoveObjectsFromSection(shellScripts, allGuids, (PBXShellScriptBuildPhase o) => o.files == null);
			foreach (PBXShellScriptBuildPhase object6 in shellScripts.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object6.files, allGuids);
			}
			flag |= RemoveObjectsFromSection(nativeTargets, allGuids, (PBXNativeTarget o) => o.phases == null);
			foreach (PBXNativeTarget object7 in nativeTargets.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object7.phases, allGuids);
			}
			flag |= RemoveObjectsFromSection(variantGroups, allGuids, (PBXVariantGroup o) => o.children == null);
			foreach (PBXVariantGroup object8 in variantGroups.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object8.children, allGuids);
			}
			flag |= RemoveObjectsFromSection(configs, allGuids, (XCConfigurationList o) => o.buildConfigs == null);
			foreach (XCConfigurationList object9 in configs.GetObjects())
			{
				RemoveMissingGuidsFromGuidList(object9.buildConfigs, allGuids);
			}
			return flag;
		}
	}
}
