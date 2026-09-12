#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using System.IO;
using Dismount.LevelEditor;
using TurboDismountMiniJSON;
using UnityEngine;

namespace Dismount
{
	public class CustomLevelImporter
	{
		private Dictionary<string, GameObject> tdResourceDict = new Dictionary<string, GameObject>();

		private string filename;

		private string levelTitle = "Level";

		private Dictionary<string, object> levelData;

		private static CustomLevelImporter _instance;

		private Action<GameObject, string> importCompleteAction;

		private PartialLevelLoader partialLoader;

		private ManagedCamera[] defaultManagedCameras;

		public static CustomLevelImporter instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new CustomLevelImporter();
					_instance.Init();
				}
				return _instance;
			}
		}

		private void Init()
		{
			GetResources();
			GameObject gameObject = new GameObject("PartialLevelLoader");
			partialLoader = gameObject.AddComponent<PartialLevelLoader>();
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}

		private void GetResources()
		{
			TextAsset textAsset = Resources.Load("TDResourceList") as TextAsset;
			string[] array = textAsset.text.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (array.Length < 2)
			{
				Debug.LogWarning("Invalid resource list");
				return;
			}
			Debug.Log("Resource list: " + array[0]);
			char[] separator = new char[1] { ',' };
			for (int i = 1; i < array.Length; i++)
			{
				string text = array[i];
				string[] array2 = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length != 2)
				{
					Debug.LogWarning("Invalid line in resource list: " + text);
					continue;
				}
				string key = array2[0];
				string path = array2[1];
				GameObject gameObject = Resources.Load<GameObject>(path);
				TDResource component = gameObject.GetComponent<TDResource>();
				if ((bool)component)
				{
					tdResourceDict.Add(key, gameObject);
				}
			}
		}

		public void ImportLevel(string filename, Action<GameObject, string> completeAction)
		{
			importCompleteAction = completeAction;
			if (string.IsNullOrEmpty(filename))
			{
				Debug.LogWarning("Level import failed, no filename given.");
				completeAction(null, string.Empty);
				return;
			}
			string json = string.Empty;
			try
			{
				using (StreamReader streamReader = new StreamReader(filename))
				{
					json = streamReader.ReadToEnd();
				}
			}
			catch
			{
				Debug.LogWarning("Level import failed; failed to read file contents: " + filename + ".");
				completeAction(null, string.Empty);
				return;
			}
			this.filename = filename;
			levelData = Json.Deserialize(json) as Dictionary<string, object>;
			int num = Convert.ToInt32(levelData["version"]);
			if (num < 1 || num > 9)
			{
				Debug.LogWarning("Level import failed; invalid package version: filename, got " + num + ", expected 1 - " + 9 + ".");
				completeAction(null, string.Empty);
			}
			else
			{
				LoadLevel(num);
			}
		}

		private void LoadLevel(int version)
		{
			levelTitle = levelData["title"] as string;
			string text = levelData["environment"] as string;
			string atmosphere = levelData["atmosphere"] as string;
			float timeLimit = 40f;
			if (levelData.ContainsKey("timeLimit"))
			{
				timeLimit = Mathf.Clamp(Convert.ToSingle(levelData["timeLimit"]), 10f, 60f);
			}
			bool manualSteering = false;
			if (levelData.ContainsKey("manualSteering"))
			{
				manualSteering = Convert.ToBoolean(levelData["manualSteering"]);
			}
			if (Application.isPlaying)
			{
				partialLoader.LoadLevelAdditive("Env" + text, (bool success) =>
				{
					if (success)
					{
						Transform transform = GameObject.Find("Atmospheres").transform;
						if ((bool)transform)
						{
							for (int i = 0; i < transform.childCount; i++)
							{
								Transform child = transform.GetChild(i);
								if (child.name == atmosphere)
								{
									child.gameObject.SetActive(true);
								}
								else
								{
									child.gameObject.SetActive(false);
								}
							}
						}
						LevelSettings levelSettings = UnityEngine.Object.FindObjectOfType<LevelSettings>();
						if (levelSettings == null)
						{
							GameObject gameObject = new GameObject("LevelSettings");
							levelSettings = gameObject.AddComponent<LevelSettings>();
						}
						levelSettings.groundLevel = 0f;
						levelSettings.dismountTimeLimit = timeLimit;
						levelSettings.vehicleFuelAmount = timeLimit * 0.75f;
						levelSettings.manualSteering = manualSteering;
					}
					if (version == 1)
					{
						LoadLevelTilesV1();
					}
					else
					{
						LoadLevelTilesV2AndUp();
					}
				});
			}
			else if (version == 1)
			{
				LoadLevelTilesV1();
			}
			else
			{
				LoadLevelTilesV2AndUp();
			}
		}

		private void RemoveManagedCamera(string groupId)
		{
			for (int i = 0; i < defaultManagedCameras.Length; i++)
			{
				if (defaultManagedCameras[i].groupId.CompareTo(groupId) == 0)
				{
					defaultManagedCameras[i].gameObject.SetActive(false);
				}
			}
		}

		private int GetManagedCameraCullingMask(string groupId)
		{
			for (int i = 0; i < defaultManagedCameras.Length; i++)
			{
				if (defaultManagedCameras[i].groupId.CompareTo(groupId) == 0)
				{
					return defaultManagedCameras[i].gameObject.GetComponent<Camera>().cullingMask;
				}
			}
			return 0;
		}

		private Transform FindOrCreateSteeringPathsRoot()
		{
			GameObject gameObject = GameObject.Find("/SteeringPaths");
			if (!gameObject)
			{
				gameObject = new GameObject("SteeringPaths");
			}
			return gameObject.transform;
		}

		private void MoveUnderSceneDynamic(Transform node)
		{
			GameObject gameObject = GameObject.Find("/SceneDynamic");
			if (!gameObject)
			{
				gameObject = new GameObject("SceneDynamic");
			}
			node.parent = gameObject.transform;
		}

		private void LoadLevelTilesV1()
		{
			Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
			Dictionary<string, List<GameObject>> dictionary2 = new Dictionary<string, List<GameObject>>();
			defaultManagedCameras = UnityEngine.Object.FindObjectsOfType<ManagedCamera>();
			List<object> list = levelData["nodeList"] as List<object>;
			int count = list.Count;
			if (count < 1)
			{
				Debug.LogWarning("Level import failed; failed to read node information: " + filename + ".");
				importCompleteAction(null, string.Empty);
				return;
			}
			Dictionary<string, object> dictionary3 = levelData["extraData"] as Dictionary<string, object>;
			GameObject gameObject = new GameObject(levelTitle);
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.rotation = Quaternion.identity;
			Dictionary<int, Transform> dictionary4 = new Dictionary<int, Transform>();
			for (int i = 0; i < list.Count; i++)
			{
				NodeInfo nodeInfo = new NodeInfo();
				Dictionary<string, object> dict = list[i] as Dictionary<string, object>;
				nodeInfo.FromDictionary(dict);
				GameObject gameObject2 = null;
				bool flag = false;
				string key = string.Empty + i + "_0";
				if (nodeInfo.extraDataCount > 0)
				{
					Dictionary<string, object> dictionary5 = dictionary3[key] as Dictionary<string, object>;
					if (string.Compare(dictionary5["type"] as string, "TDResource") == 0)
					{
						string key2 = dictionary5["id"] as string;
						if (tdResourceDict.ContainsKey(key2))
						{
							gameObject2 = UnityEngine.Object.Instantiate(tdResourceDict[key2]) as GameObject;
							gameObject2.name = nodeInfo.name;
						}
						else
						{
							gameObject2 = new GameObject(nodeInfo.name + " (unknown resource)");
						}
						if (!gameObject2.isStatic)
						{
							flag = true;
						}
					}
					else if (string.Compare(dictionary5["type"] as string, "Camera") == 0)
					{
						gameObject2 = new GameObject();
						Camera camera = gameObject2.AddComponent<Camera>();
						camera.fieldOfView = Convert.ToSingle(dictionary5["fov"]);
						camera.nearClipPlane = Convert.ToSingle(dictionary5["nearclip"]);
						camera.farClipPlane = Convert.ToSingle(dictionary5["farclip"]);
						gameObject2.name = nodeInfo.name;
						if (gameObject2.name.CompareTo("ObstacleCamera") == 0)
						{
							RemoveManagedCamera("Setup");
							ManagedCamera managedCamera = camera.gameObject.AddComponent<ManagedCamera>();
							managedCamera.groupId = "Setup";
							gameObject2.AddComponent<SceneSetupCamera>();
							camera.cullingMask = GetManagedCameraCullingMask("Setup");
						}
						else if (gameObject2.name.CompareTo("PreviewCamera") == 0)
						{
							RemoveManagedCamera("Menu");
							ManagedCamera managedCamera2 = camera.gameObject.AddComponent<ManagedCamera>();
							managedCamera2.groupId = "Menu";
							camera.cullingMask = GetManagedCameraCullingMask("Menu");
							gameObject2.AddComponent<PreviewCameraAnimation>();
						}
					}
					else if (string.Compare(dictionary5["type"] as string, "PathNode") == 0)
					{
						string text = dictionary5["pathname"] as string;
						if (!dictionary.ContainsKey(text))
						{
							GameObject gameObject3 = new GameObject();
							gameObject3.name = text;
							dictionary[text] = gameObject3;
							gameObject3.AddComponent<Spline>();
							dictionary2[text] = new List<GameObject>();
						}
						gameObject2 = new GameObject(dictionary5["name"] as string);
						dictionary2[text].Add(gameObject2);
					}
				}
				if (gameObject2 == null)
				{
					gameObject2 = new GameObject(nodeInfo.name);
				}
				if (nodeInfo.parentId >= 0)
				{
					if (dictionary4.ContainsKey(nodeInfo.parentId))
					{
						gameObject2.transform.parent = dictionary4[nodeInfo.parentId];
					}
				}
				else
				{
					gameObject2.transform.parent = gameObject.transform;
				}
				gameObject2.transform.localPosition = nodeInfo.localPosition;
				gameObject2.transform.localRotation = nodeInfo.localRotation;
				gameObject2.transform.localScale = nodeInfo.localScale;
				dictionary4.Add(i, gameObject2.transform);
				if (flag)
				{
					MoveUnderSceneDynamic(gameObject2.transform);
				}
			}
			foreach (KeyValuePair<string, List<GameObject>> item in dictionary2)
			{
				List<GameObject> value = item.Value;
				value.Sort((GameObject p1, GameObject p2) => p1.name.CompareTo(p2.name));
				GameObject gameObject4 = dictionary[item.Key];
				Spline component = gameObject4.GetComponent<Spline>();
				for (int num = 0; num < value.Count; num++)
				{
					GameObject gameObject5 = component.AddSplineNode();
					gameObject5.transform.parent = gameObject4.transform;
				}
				SplineNode[] splineNodes = component.SplineNodes;
				for (int num2 = 0; num2 < splineNodes.Length; num2++)
				{
					splineNodes[num2].transform.position = value[num2].transform.position;
					splineNodes[num2].transform.rotation = value[num2].transform.rotation;
					splineNodes[num2].transform.localScale = value[num2].transform.localScale;
				}
				component.UpdateSpline();
				gameObject4.transform.parent = FindOrCreateSteeringPathsRoot();
				gameObject4.tag = "SteerPath";
			}
			Debug.Log("Level imported; imported " + count + " objects.");
			importCompleteAction(gameObject, levelTitle);
		}

		private void LoadLevelTilesV2AndUp()
		{
			Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
			Dictionary<string, List<GameObject>> dictionary2 = new Dictionary<string, List<GameObject>>();
			List<NPCVehicleSpawner> list = new List<NPCVehicleSpawner>();
			defaultManagedCameras = UnityEngine.Object.FindObjectsOfType<ManagedCamera>();
			List<object> list2 = levelData["nodeList"] as List<object>;
			int count = list2.Count;
			if (count < 1)
			{
				Debug.LogWarning("Level import failed; failed to read node information: " + filename + ".");
				importCompleteAction(null, string.Empty);
				return;
			}
			Dictionary<string, object> dictionary3 = levelData["extraData"] as Dictionary<string, object>;
			GameObject gameObject = new GameObject(levelTitle);
			gameObject.transform.position = Vector3.zero;
			gameObject.transform.rotation = Quaternion.identity;
			Dictionary<int, Transform> dictionary4 = new Dictionary<int, Transform>();
			for (int i = 0; i < list2.Count; i++)
			{
				NodeInfo nodeInfo = new NodeInfo();
				Dictionary<string, object> dict = list2[i] as Dictionary<string, object>;
				nodeInfo.FromDictionary(dict);
				GameObject gameObject2 = null;
				bool flag = false;
				if (nodeInfo.extraDataCount > 0)
				{
					string key = string.Empty + i + "_0";
					Dictionary<string, object> dictionary5 = dictionary3[key] as Dictionary<string, object>;
					if (string.Compare(dictionary5["type"] as string, "TDResource") == 0)
					{
						string key2 = dictionary5["id"] as string;
						if (tdResourceDict.ContainsKey(key2))
						{
							gameObject2 = UnityEngine.Object.Instantiate(tdResourceDict[key2]) as GameObject;
							gameObject2.name = nodeInfo.name;
						}
						else
						{
							gameObject2 = new GameObject(nodeInfo.name + " (unknown TDResource)");
						}
						if (!gameObject2.isStatic)
						{
							flag = true;
						}
					}
				}
				if (gameObject2 == null)
				{
					gameObject2 = new GameObject(nodeInfo.name);
				}
				Transform transform = null;
				if (nodeInfo.parentId >= 0)
				{
					if (dictionary4.ContainsKey(nodeInfo.parentId))
					{
						transform = dictionary4[nodeInfo.parentId];
						gameObject2.transform.parent = transform;
					}
				}
				else
				{
					gameObject2.transform.parent = gameObject.transform;
				}
				gameObject2.transform.localPosition = nodeInfo.localPosition;
				gameObject2.transform.localRotation = nodeInfo.localRotation;
				gameObject2.transform.localScale = nodeInfo.localScale;
				dictionary4.Add(i, gameObject2.transform);
				if (flag)
				{
					MoveUnderSceneDynamic(gameObject2.transform);
				}
				if (nodeInfo.extraDataCount <= 0)
				{
					continue;
				}
				for (int j = 0; j < nodeInfo.extraDataCount; j++)
				{
					string key3 = string.Empty + i + "_" + j;
					Dictionary<string, object> dictionary6 = dictionary3[key3] as Dictionary<string, object>;
					if (string.Compare(dictionary6["type"] as string, "TDResource") == 0)
					{
						continue;
					}
					if (string.Compare(dictionary6["type"] as string, "Camera") == 0)
					{
						Camera camera = gameObject2.AddComponent<Camera>();
						camera.fieldOfView = Convert.ToSingle(dictionary6["fov"]);
						camera.nearClipPlane = Convert.ToSingle(dictionary6["nearclip"]);
						camera.farClipPlane = Convert.ToSingle(dictionary6["farclip"]);
						if (gameObject2.name.CompareTo("ObstacleCamera") == 0)
						{
							RemoveManagedCamera("Setup");
							ManagedCamera managedCamera = camera.gameObject.AddComponent<ManagedCamera>();
							managedCamera.groupId = "Setup";
							gameObject2.AddComponent<SceneSetupCamera>();
							camera.cullingMask = GetManagedCameraCullingMask("Setup");
						}
						else if (gameObject2.name.CompareTo("PreviewCamera") == 0)
						{
							RemoveManagedCamera("Menu");
							ManagedCamera managedCamera2 = camera.gameObject.AddComponent<ManagedCamera>();
							managedCamera2.groupId = "Menu";
							camera.cullingMask = GetManagedCameraCullingMask("Menu");
							gameObject2.AddComponent<PreviewCameraAnimation>();
						}
					}
					else if (string.Compare(dictionary6["type"] as string, "PathInfo") == 0)
					{
						string text = dictionary6["pathType"] as string;
						bool flag2 = Convert.ToBoolean(dictionary6["loop"]);
						Spline spline = gameObject2.AddComponent<Spline>();
						spline.interpolationMode = Spline.InterpolationMode.Hermite;
						spline.tangentMode = Spline.TangentMode.UseTangents;
						spline.normalMode = Spline.NormalMode.UseNodeUpVector;
						if (flag2)
						{
							spline.autoClose = true;
						}
						dictionary[gameObject2.name] = gameObject2;
						dictionary2[gameObject2.name] = new List<GameObject>();
						if (text == "Steering")
						{
							gameObject2.tag = "SteerPath";
						}
					}
					else if (string.Compare(dictionary6["type"] as string, "PathNode") == 0)
					{
						if (!transform)
						{
							Debug.LogError("PathNode has no parent, impossible");
							continue;
						}
						GameObject gameObject3 = transform.gameObject;
						string name = transform.name;
						if (!dictionary.ContainsKey(name))
						{
							dictionary[name] = gameObject3;
							Spline spline2 = gameObject3.AddComponent<Spline>();
							spline2.interpolationMode = Spline.InterpolationMode.Hermite;
							spline2.tangentMode = Spline.TangentMode.UseTangents;
							spline2.normalMode = Spline.NormalMode.UseNodeUpVector;
							dictionary2[name] = new List<GameObject>();
							gameObject3.tag = "SteerPath";
						}
						PathNode pathNode = gameObject2.AddComponent<PathNode>();
						pathNode.customValue = Convert.ToSingle(dictionary6["customValue"]);
						dictionary2[name].Add(gameObject2);
					}
					else if (string.Compare(dictionary6["type"] as string, "NPCVehicleSpawner") == 0)
					{
						NPCVehicleSpawner nPCVehicleSpawner = gameObject2.AddComponent<NPCVehicleSpawner>();
						nPCVehicleSpawner.numVehiclesToSpawn = Convert.ToInt32(dictionary6["numVehiclesToSpawn"]);
						nPCVehicleSpawner.spawnInterval = Convert.ToSingle(dictionary6["spawnInterval"]);
						nPCVehicleSpawner.vehicleVelocity = Convert.ToSingle(dictionary6["vehicleVelocity"]);
						nPCVehicleSpawner.vehicleVelocity = Mathf.Clamp(nPCVehicleSpawner.vehicleVelocity, 0f, 100f);
						nPCVehicleSpawner.followPathName = dictionary6["followPathName"] as string;
						List<object> list3 = dictionary6["vehicleIds"] as List<object>;
						nPCVehicleSpawner.vehicleIds = new List<string>();
						foreach (object item in list3)
						{
							nPCVehicleSpawner.vehicleIds.Add(item as string);
						}
						list.Add(nPCVehicleSpawner);
					}
					else if (string.Compare(dictionary6["type"] as string, "NPCCopData") == 0)
					{
						NPCCopData nPCCopData = gameObject2.GetComponent<NPCCopData>();
						if (nPCCopData == null)
						{
							nPCCopData = gameObject2.AddComponent<NPCCopData>();
						}
						nPCCopData.observeSectorAngle = Convert.ToSingle(dictionary6["observeSectorAngle"]);
						nPCCopData.observeSectorRadius = Convert.ToSingle(dictionary6["observeSectorRadius"]);
						nPCCopData.observeOmniRadius = Convert.ToSingle(dictionary6["observeOmniRadius"]);
						nPCCopData.observeSectorAngle = Mathf.Clamp(nPCCopData.observeSectorAngle, 0f, 180f);
						nPCCopData.observeSectorRadius = Mathf.Clamp(nPCCopData.observeSectorRadius, 0f, 1000f);
						nPCCopData.observeOmniRadius = Mathf.Clamp(nPCCopData.observeOmniRadius, 0f, 1000f);
					}
				}
			}
			foreach (KeyValuePair<string, List<GameObject>> item2 in dictionary2)
			{
				List<GameObject> value = item2.Value;
				value.Sort((GameObject p1, GameObject p2) => p1.name.CompareTo(p2.name));
				GameObject gameObject4 = dictionary[item2.Key];
				Spline component = gameObject4.GetComponent<Spline>();
				for (int num = 0; num < value.Count; num++)
				{
					GameObject gameObject5 = component.AddSplineNode();
					gameObject5.transform.parent = gameObject4.transform;
				}
				SplineNode[] splineNodes = component.SplineNodes;
				for (int num2 = 0; num2 < splineNodes.Length; num2++)
				{
					splineNodes[num2].transform.position = value[num2].transform.position;
					splineNodes[num2].transform.rotation = value[num2].transform.rotation;
					splineNodes[num2].transform.localScale = value[num2].transform.localScale;
					splineNodes[num2].customValue = value[num2].GetComponent<PathNode>().customValue;
				}
				component.UpdateSpline();
				if (gameObject4.tag == "SteerPath")
				{
					gameObject4.transform.parent = FindOrCreateSteeringPathsRoot();
				}
			}
			foreach (NPCVehicleSpawner item3 in list)
			{
				GameObject gameObject6 = item3.gameObject;
				ObjectSpawner objectSpawner = gameObject6.AddComponent<ObjectSpawner>();
				objectSpawner.numObjectsToSpawn = item3.numVehiclesToSpawn;
				objectSpawner.spawnInterval = item3.spawnInterval;
				objectSpawner.overrideObjectVelocity = item3.vehicleVelocity;
				if (dictionary.ContainsKey(item3.followPathName))
				{
					objectSpawner.followPath = dictionary[item3.followPathName].GetComponent<Spline>();
				}
				if (item3.vehicleIds != null && item3.vehicleIds.Count > 0)
				{
					List<GameObject> list4 = new List<GameObject>();
					foreach (string vehicleId in item3.vehicleIds)
					{
						if (tdResourceDict.ContainsKey(vehicleId))
						{
							list4.Add(tdResourceDict[vehicleId]);
						}
					}
					objectSpawner.objectPrefabs = list4.ToArray();
				}
				UnityEngine.Object.Destroy(item3);
			}
			Debug.Log("Level imported; imported " + count + " objects.");
			importCompleteAction(gameObject, levelTitle);
		}
	}
}
