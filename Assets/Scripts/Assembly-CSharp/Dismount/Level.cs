#pragma warning disable 0618,0619
using System;
using System.Collections;
using System.Collections.Generic;
using Dismount.LevelEditor;
using Dismount.Vehicular;
using UnityEngine;
using reLive;

namespace Dismount
{
	public class Level : MonoBehaviour
	{
		private List<GameObject> selectionHotspots = new List<GameObject>();

		private List<GameObject> instantiatedObstacles = new List<GameObject>();

		private List<Spline> steerPaths = new List<Spline>();

		private GameObject currentSteerPathMeshObject;

		private SelectionHotspot[] obstacleHotspots;

		private int currentObstacleHotspot;

		private Dictionary<string, Transform> tPoses = new Dictionary<string, Transform>();

		private Dictionary<string, Transform> vehicles = new Dictionary<string, Transform>();

		private Dictionary<string, Transform> startPoses = new Dictionary<string, Transform>();

		public float groundLevel;

		public float dismountTimeLimit = 40f;

		public float vehicleFuelAmount = 30f;

		public bool allowManualSteering = true;

		private string nextLevel = string.Empty;

		private bool nextLevelIsCustom;

		private bool nextLevelIsUnpublished;

		public string loadedLevel = string.Empty;

		public bool loadedLevelIsCustom;

		public bool loadedLevelIsUnpublished;

		public string customLevelName = string.Empty;

		private SelectionHotspot vehicleHotSpot;

		private Vector3 vehicleHotSpotPosition = Vector3.zero;

		private Dictionary<string, string> characterNameToPoseName2 = new Dictionary<string, string>();

		private bool vehicleSceneLoaded;

		private GameObject[] characterInstances;

		private string previouslyLoadedVehicle = string.Empty;

		private bool _isDismountActive;

		public SelectionHotspot VehicleHotSpot
		{
			get
			{
				return vehicleHotSpot;
			}
		}

		public Vector3 VehicleHotSpotPosition
		{
			get
			{
				return vehicleHotSpotPosition;
			}
		}

		public SelectionHotspot activeObstacleHotspot
		{
			get
			{
				if (currentObstacleHotspot < obstacleHotspots.Length)
				{
					return obstacleHotspots[currentObstacleHotspot];
				}
				return null;
			}
			set
			{
				for (int i = 0; i < obstacleHotspots.Length; i++)
				{
					if (obstacleHotspots[i] == value)
					{
						currentObstacleHotspot = i;
						break;
					}
				}
			}
		}

		public bool isDismountActive
		{
			get
			{
				return _isDismountActive;
			}
		}

		public void NextLevel(string levelName, bool isCustom, bool isUnpublished)
		{
			nextLevel = levelName;
			nextLevelIsCustom = isCustom;
			nextLevelIsUnpublished = isUnpublished;
		}

		private void DestroyLevel()
		{
			GizmoManager.Clear();
			if ((bool)DismountGame.playerState.currentCharacterInstance)
			{
				UnityEngine.Object.Destroy(DismountGame.playerState.currentCharacterInstance);
				DismountGame.playerState.currentCharacterInstance = null;
			}
			if ((bool)DismountGame.playerState.currentVehicleInstance)
			{
				UnityEngine.Object.Destroy(DismountGame.playerState.currentVehicleInstance);
				DismountGame.playerState.currentVehicleInstance = null;
			}
			foreach (GameObject instantiatedObstacle in instantiatedObstacles)
			{
				UnityEngine.Object.Destroy(instantiatedObstacle);
			}
			instantiatedObstacles.Clear();
			selectionHotspots.Clear();
			steerPaths.Clear();
			if ((bool)currentSteerPathMeshObject)
			{
				UnityEngine.Object.Destroy(currentSteerPathMeshObject);
				currentSteerPathMeshObject = null;
			}
		}

		private IEnumerator CustomLevelLoader()
		{
			Application.LoadLevel("CustomLevelBase");
			yield return 0;
			FinishLoadCustomLevelBase();
		}

		private IEnumerator LevelLoader()
		{
			string level = nextLevel;
			if (DismountGame.inventory.GetItemByReferenceName(level) == null)
			{
				level = "LevelDevelopment";
			}
			if (Application.CanStreamedLevelBeLoaded(level + "_ios"))
			{
				level += "_ios";
			}
			Application.LoadLevel(level);
			yield return 0;
			FinishLoadLevel(false);
		}

		public void LoadLevel(bool doLoad)
		{
			_isDismountActive = false;
			loadedLevel = string.Empty;
			DismountGame.audioManager.Reset();
			DestroyLevel();
			if (nextLevelIsCustom)
			{
				StartCoroutine(CustomLevelLoader());
			}
			else if (doLoad)
			{
				StartCoroutine(LevelLoader());
			}
			else
			{
				FinishLoadLevel(false);
			}
		}

		private void AddLevelBorders(GameObject importRoot)
		{
			Bounds bounds = new Bounds(importRoot.transform.position, Vector3.zero);
			Collider[] componentsInChildren = importRoot.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				bounds.Encapsulate(componentsInChildren[i].bounds);
			}
			GameObject gameObject = new GameObject("LevelBorders");
			gameObject.transform.parent = importRoot.transform;
			bounds.Encapsulate(new Vector3(bounds.center.x, bounds.max.y + 500f, bounds.center.y));
			bounds.Expand(new Vector3(20f, 0f, 20f));
			bounds.Expand(2.5f * Vector3.one);
			GameObject levelBorderPrefab = DismountGame.instance.levelBorderPrefab;
			GameObject gameObject2 = UnityEngine.Object.Instantiate(levelBorderPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			gameObject2.name = "Bottom";
			Vector3 center = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
			Vector3 size = new Vector3(bounds.size.x + 5f, 5f, bounds.size.z + 5f);
			BoxCollider component = gameObject2.GetComponent<BoxCollider>();
			component.center = center;
			component.size = size;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2 = UnityEngine.Object.Instantiate(levelBorderPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			gameObject2.name = "Top";
			center = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
			size = new Vector3(bounds.size.x + 5f, 5f, bounds.size.z + 5f);
			component = gameObject2.GetComponent<BoxCollider>();
			component.center = center;
			component.size = size;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2 = UnityEngine.Object.Instantiate(levelBorderPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			gameObject2.name = "Left";
			center = new Vector3(bounds.min.x, bounds.center.y, bounds.center.z);
			size = new Vector3(5f, bounds.size.y + 5f, bounds.size.z + 5f);
			component = gameObject2.GetComponent<BoxCollider>();
			component.center = center;
			component.size = size;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2 = UnityEngine.Object.Instantiate(levelBorderPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			gameObject2.name = "Right";
			center = new Vector3(bounds.max.x, bounds.center.y, bounds.center.z);
			size = new Vector3(5f, bounds.size.y + 5f, bounds.size.z + 5f);
			component = gameObject2.GetComponent<BoxCollider>();
			component.center = center;
			component.size = size;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2 = UnityEngine.Object.Instantiate(levelBorderPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			gameObject2.name = "Back";
			center = new Vector3(bounds.center.x, bounds.center.y, bounds.min.z);
			size = new Vector3(bounds.size.x + 5f, bounds.size.y + 5f, 5f);
			component = gameObject2.GetComponent<BoxCollider>();
			component.center = center;
			component.size = size;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2 = UnityEngine.Object.Instantiate(levelBorderPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			gameObject2.name = "Front";
			center = new Vector3(bounds.center.x, bounds.center.y, bounds.max.z);
			size = new Vector3(bounds.size.x + 5f, bounds.size.y + 5f, 5f);
			component = gameObject2.GetComponent<BoxCollider>();
			component.center = center;
			component.size = size;
			gameObject2.transform.parent = gameObject.transform;
		}

		private void FinishLoadCustomLevelBase()
		{
			string filename = nextLevel;
			CustomLevelImporter instance = CustomLevelImporter.instance;
			instance.ImportLevel(filename, (GameObject importRoot, string title) =>
			{
				if (importRoot == null)
				{
					nextLevel = "LevelDevelopment";
					nextLevelIsCustom = false;
					nextLevelIsUnpublished = false;
					LoadLevel(true);
				}
				else
				{
					importRoot.isStatic = true;
					StaticBatchingUtility.Combine(importRoot);
					customLevelName = title;
					FinishLoadLevel(true);
				}
			});
		}

		private void FinishLoadLevel(bool customLevel)
		{
			if (customLevel)
			{
				DismountGame.playerState.currentLevel = Utils.GetCustomLevelKey(nextLevel);
				DismountGame.playerState.customLevelFilename = nextLevel;
				DismountGame.playerState.isUnpublishedLevel = nextLevelIsUnpublished;
			}
			else
			{
				DismountGame.playerState.currentLevel = nextLevel;
				DismountGame.playerState.customLevelFilename = string.Empty;
				DismountGame.playerState.isUnpublishedLevel = false;
			}
			LevelSettings levelSettings = UnityEngine.Object.FindObjectOfType<LevelSettings>();
			if ((bool)levelSettings)
			{
				groundLevel = levelSettings.groundLevel;
				dismountTimeLimit = levelSettings.dismountTimeLimit;
				vehicleFuelAmount = levelSettings.vehicleFuelAmount;
			}
			else
			{
				groundLevel = 0f;
				dismountTimeLimit = 40f;
				vehicleFuelAmount = 30f;
			}
			InitSteerPaths();
			InitEndZones();
			GameObject[] array = GameObject.FindGameObjectsWithTag("SetupHelper");
			List<SelectionHotspot> list = new List<SelectionHotspot>();
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				SelectionHotspot component = gameObject.GetComponent<SelectionHotspot>();
				if (component != null)
				{
					selectionHotspots.Add(gameObject);
					if (component.category == GameItem.ItemCategory.Obstacle)
					{
						list.Add(component);
					}
				}
			}
			obstacleHotspots = list.ToArray();
			Array.Sort(obstacleHotspots, (SelectionHotspot s1, SelectionHotspot s2) => s1.name.CompareTo(s2.name));
			currentObstacleHotspot = 0;
			SaveObstacleTransforms("default");
			bool flag = CheckLevelSave("user");
			if (flag)
			{
				LoadObstacles("user");
			}
			if (DismountGame.playerState.CheckVehicleSave())
			{
				DismountGame.playerState.LoadVehicleAndCharacter();
			}
			foreach (GameObject selectionHotspot in selectionHotspots)
			{
				SelectionHotspot component2 = selectionHotspot.GetComponent<SelectionHotspot>();
				CastToGround component3 = selectionHotspot.GetComponent<CastToGround>();
				if ((bool)component3)
				{
					component3.Cast();
				}
				if (component2.category == GameItem.ItemCategory.Obstacle)
				{
					InstantiateInitialObstacle(component2);
				}
				if (component2.category == GameItem.ItemCategory.Vehicle)
				{
					InstantiateInitialVehicle(component2);
					vehicleHotSpotPosition = component2.transform.position;
				}
			}
			if (!flag)
			{
				SaveObstacles("default");
			}
			loadedLevel = nextLevel;
			loadedLevelIsCustom = nextLevelIsCustom;
			loadedLevelIsUnpublished = nextLevelIsUnpublished;
		}

		private void InitEndZones()
		{
			EndZone[] array = UnityEngine.Object.FindObjectsOfType<EndZone>();
			EndZone[] array2 = array;
			foreach (EndZone endZone in array2)
			{
				endZone.gameObject.AddComponent<EndZoneChecker>();
				BoxCollider boxCollider = endZone.gameObject.AddComponent<BoxCollider>();
				boxCollider.isTrigger = true;
				boxCollider.center = Vector3.zero;
				boxCollider.size = Vector3.one;
			}
		}

		private void InitSteerPaths()
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("SteerPath");
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				if ((bool)gameObject.GetComponent<Spline>())
				{
					Spline component = gameObject.GetComponent<Spline>();
					component.updateMode = Spline.UpdateMode.DontUpdate;
					steerPaths.Add(component);
				}
			}
			if (steerPaths.Count > 0)
			{
				steerPaths.Sort((Spline pathA, Spline pathB) => pathA.name.CompareTo(pathB.name));
				DismountGame.playerState.LoadLevelSteerPath();
				if (DismountGame.playerState.currentLevelSteerPath == -1)
				{
					SetManualSteering(true, false);
				}
				else
				{
					if (DismountGame.playerState.currentLevelSteerPath < 0 || DismountGame.playerState.currentLevelSteerPath >= steerPaths.Count)
					{
						DismountGame.playerState.currentLevelSteerPath = 0;
					}
					DismountGame.playerState.SaveLevelSteerPath();
					SetManualSteering(false, false);
				}
				InstantiateSteerPathMesh();
				HideSteerPath();
			}
			else
			{
				DismountGame.playerState.currentLevelSteerPath = -1;
				DismountGame.playerState.SaveLevelSteerPath();
				SetManualSteering(true, false);
			}
		}

		private void InstantiateSteerPathMesh()
		{
			if ((bool)currentSteerPathMeshObject)
			{
				UnityEngine.Object.Destroy(currentSteerPathMeshObject);
				currentSteerPathMeshObject = null;
			}
			if (DismountGame.playerState.currentLevelSteerPath >= 0)
			{
				Spline spline = steerPaths[DismountGame.playerState.currentLevelSteerPath];
				spline.updateMode = Spline.UpdateMode.DontUpdate;
				GameObject gameObject = UnityEngine.Object.Instantiate(DismountGame.instance.steerPathMeshPrefab) as GameObject;
				gameObject.name = "SteerPathMesh";
				gameObject.transform.parent = spline.transform.parent;
				gameObject.transform.localPosition = spline.transform.localPosition;
				gameObject.transform.localRotation = spline.transform.localRotation;
				SplineMesh component = gameObject.GetComponent<SplineMesh>();
				component.spline = spline;
				component.segmentCount = (int)(spline.Length * 0.5f);
				component.uvScale.y = spline.Length * 0.025f;
				component.UpdateMesh();
				component.updateMode = SplineMesh.UpdateMode.DontUpdate;
				currentSteerPathMeshObject = gameObject;
			}
		}

		public void CycleSteerPath()
		{
			int currentLevelSteerPath = DismountGame.playerState.currentLevelSteerPath;
			Spline spline = null;
			currentLevelSteerPath++;
			if (currentLevelSteerPath >= steerPaths.Count)
			{
				currentLevelSteerPath = -1;
			}
			DismountGame.playerState.currentLevelSteerPath = currentLevelSteerPath;
			DismountGame.playerState.SaveLevelSteerPath();
			if (currentLevelSteerPath == -1)
			{
				spline = null;
				SetManualSteering(true, true);
			}
			else
			{
				spline = steerPaths[currentLevelSteerPath];
				SetManualSteering(false, true);
			}
			if ((bool)DismountGame.playerState.currentVehicleInstance)
			{
				DismountGame.playerState.currentVehicleInstance.GetComponent<Vehicle>().steerSpline = spline;
			}
			InstantiateSteerPathMesh();
		}

		private void SetManualSteering(bool enabled, bool setChaseCamera)
		{
			DismountGame.playerState.manualSteering = enabled;
			DismountGame.uiManager.SetManualSteering(enabled);
			if (enabled && setChaseCamera)
			{
				DismountGame.cameraManager.SetChaseMode(enabled, true);
			}
			else if (!enabled && setChaseCamera)
			{
				DismountGame.cameraManager.SetChaseMode(enabled, false);
			}
		}

		public void ShowSteerPath()
		{
			if ((bool)currentSteerPathMeshObject)
			{
				currentSteerPathMeshObject.SetActive(true);
			}
		}

		public void HideSteerPath()
		{
			if ((bool)currentSteerPathMeshObject)
			{
				currentSteerPathMeshObject.SetActive(false);
			}
		}

		public void SelectNextObstacleHotspot(bool dec)
		{
			int num = obstacleHotspots.Length;
			if (num != 0)
			{
				if (dec)
				{
					currentObstacleHotspot--;
				}
				else
				{
					currentObstacleHotspot++;
				}
				currentObstacleHotspot = (currentObstacleHotspot + num) % num;
			}
		}

		public void HighlightSelectedObstacleHotspot()
		{
			if (obstacleHotspots.Length > 0)
			{
				obstacleHotspots[currentObstacleHotspot].Hover = true;
			}
		}

		public void OpenSelectedObstacleHotspot()
		{
			if (obstacleHotspots.Length > 0)
			{
				Utils.SendMessage("SetupScene", "OnOpenSelection", obstacleHotspots[currentObstacleHotspot]);
			}
		}

		private bool CheckLevelSave(string configuration)
		{
			foreach (GameObject selectionHotspot in selectionHotspots)
			{
				SelectionHotspot component = selectionHotspot.GetComponent<SelectionHotspot>();
				if (component.category != GameItem.ItemCategory.Obstacle)
				{
					continue;
				}
				string key = DismountGame.playerState.currentLevel + "/" + configuration + "/" + GetObstacleName(component);
				return Prefs.HasKey(key);
			}
			return false;
		}

		private void LoadObstacles(string configuration)
		{
			foreach (GameObject selectionHotspot in selectionHotspots)
			{
				SelectionHotspot component = selectionHotspot.GetComponent<SelectionHotspot>();
				if (component.category != GameItem.ItemCategory.Obstacle)
				{
					continue;
				}
				string text = DismountGame.playerState.currentLevel + "/" + configuration + "/";
				string text2 = text + GetObstacleName(component);
				string text3 = Prefs.GetString(text2, "Empty");
				component.item = null;
				if (text3 != "Empty")
				{
					GameItem item = DismountGame.inventory.GetItem(text3);
					if ((bool)item && item.itemCategory == GameItem.ItemCategory.Obstacle && !item.isLocked)
					{
						component.item = item;
					}
				}
				if (Prefs.HasKey(text2 + "position_x"))
				{
					Vector3 position = new Vector3(Prefs.GetFloat(text2 + "position_x", selectionHotspot.transform.position.x), Prefs.GetFloat(text2 + "position_y", selectionHotspot.transform.position.y), Prefs.GetFloat(text2 + "position_z", selectionHotspot.transform.position.z));
					Quaternion rotation = new Quaternion(Prefs.GetFloat(text2 + "rotation_x", selectionHotspot.transform.rotation.x), Prefs.GetFloat(text2 + "rotation_y", selectionHotspot.transform.rotation.y), Prefs.GetFloat(text2 + "rotation_z", selectionHotspot.transform.rotation.z), Prefs.GetFloat(text2 + "rotation_w", selectionHotspot.transform.rotation.w));
					selectionHotspot.transform.position = position;
					selectionHotspot.transform.rotation = rotation;
				}
			}
		}

		private void SaveObstacleTransforms(string configuration)
		{
			foreach (GameObject selectionHotspot in selectionHotspots)
			{
				SelectionHotspot component = selectionHotspot.GetComponent<SelectionHotspot>();
				if (component.category == GameItem.ItemCategory.Obstacle)
				{
					string text = DismountGame.playerState.currentLevel + "/" + configuration + "/";
					string text2 = text + GetObstacleName(component);
					Prefs.SetFloat(text2 + "position_x", selectionHotspot.transform.position.x);
					Prefs.SetFloat(text2 + "position_y", selectionHotspot.transform.position.y);
					Prefs.SetFloat(text2 + "position_z", selectionHotspot.transform.position.z);
					Prefs.SetFloat(text2 + "rotation_x", selectionHotspot.transform.rotation.x);
					Prefs.SetFloat(text2 + "rotation_y", selectionHotspot.transform.rotation.y);
					Prefs.SetFloat(text2 + "rotation_z", selectionHotspot.transform.rotation.z);
					Prefs.SetFloat(text2 + "rotation_w", selectionHotspot.transform.rotation.w);
				}
			}
			Prefs.Save();
		}

		public void SaveObstacles(string configuration)
		{
			foreach (GameObject selectionHotspot in selectionHotspots)
			{
				SelectionHotspot component = selectionHotspot.GetComponent<SelectionHotspot>();
				if (component.category == GameItem.ItemCategory.Obstacle)
				{
					string text = DismountGame.playerState.currentLevel + "/" + configuration + "/";
					string text2 = text + GetObstacleName(component);
					GameItem item = component.item;
					if (item != null)
					{
						Prefs.SetString(text2, item.itemId);
					}
					else
					{
						Prefs.SetString(text2, "Empty");
					}
					Prefs.SetFloat(text2 + "position_x", selectionHotspot.transform.position.x);
					Prefs.SetFloat(text2 + "position_y", selectionHotspot.transform.position.y);
					Prefs.SetFloat(text2 + "position_z", selectionHotspot.transform.position.z);
					Prefs.SetFloat(text2 + "rotation_x", selectionHotspot.transform.rotation.x);
					Prefs.SetFloat(text2 + "rotation_y", selectionHotspot.transform.rotation.y);
					Prefs.SetFloat(text2 + "rotation_z", selectionHotspot.transform.rotation.z);
					Prefs.SetFloat(text2 + "rotation_w", selectionHotspot.transform.rotation.w);
				}
			}
			Prefs.Save();
		}

		private void InstantiateInitialObstacle(SelectionHotspot hotspot)
		{
			InstantiateObstacle(hotspot);
		}

		private void InstantiateInitialVehicle(SelectionHotspot hotspot)
		{
			if (vehicles.Count > 0)
			{
				InstantiateVehicle(hotspot);
			}
			else
			{
				StartCoroutine(LoadVehicles(hotspot));
			}
		}

		private Transform DuplicateAndCleanCharacterHierarchy(Transform parent)
		{
			GameObject gameObject = new GameObject(parent.name);
			BodyPart component = parent.gameObject.GetComponent<BodyPart>();
			if ((bool)component && (component.attachForce != 0f || component.attachRigidbodyName.CompareTo(string.Empty) != 0))
			{
				BodyPartAttachData bodyPartAttachData = gameObject.AddComponent<BodyPartAttachData>();
				bodyPartAttachData.attachForce = component.attachForce;
				bodyPartAttachData.attachRigidbodyName = component.attachRigidbodyName;
			}
			MrDismount component2 = parent.gameObject.GetComponent<MrDismount>();
			if ((bool)component2)
			{
				gameObject.AddComponent<MrDismountSetupData>().kinematicDismountSetup = component2.kinematicDismountSetup;
			}
			gameObject.transform.position = parent.position;
			gameObject.transform.rotation = parent.rotation;
			gameObject.transform.localScale = parent.localScale;
			foreach (Transform item in parent)
			{
				Transform transform = DuplicateAndCleanCharacterHierarchy(item);
				transform.parent = gameObject.transform;
			}
			if (!parent.gameObject.activeSelf)
			{
				gameObject.SetActive(false);
			}
			return gameObject.transform;
		}

		private IEnumerator LoadVehicles(SelectionHotspot hotspot)
		{
			if (characterNameToPoseName2.Count == 0)
			{
				characterNameToPoseName2["MsDismount"] = "MrDismount";
			}
			if (!vehicleSceneLoaded)
			{
				Application.LoadLevelAdditive("Vehicles");
				yield return 0;
				vehicleSceneLoaded = true;
				UnityEngine.Object.DestroyImmediate(GameObject.Find("/NonGameObjects"));
				Transform tPoseRoot = GameObject.Find("/TPoses").transform;
				for (int i = 0; i < tPoseRoot.childCount; i++)
				{
					Transform tPose = tPoseRoot.GetChild(i);
					tPoses.Add(tPose.name, tPose);
					tPose.gameObject.SetActive(false);
				}
				Transform vehicleRoot = base.transform.Find("/Vehicles");
				GameObject startPosesObject = new GameObject("StartPoses");
				Transform startPoseRoot = startPosesObject.transform;
				startPoseRoot.position = Vector3.zero;
				startPoseRoot.rotation = Quaternion.identity;
				tPoseRoot.parent = base.transform;
				vehicleRoot.parent = base.transform;
				startPoseRoot.parent = base.transform;
			}
			InstantiateVehicle(hotspot);
		}

		private void ProcessLoadedVehicle(GameObject vehicle)
		{
			if (characterNameToPoseName2.Count == 0)
			{
				characterNameToPoseName2["MsDismount"] = "MrDismount";
			}
			Transform transform = base.transform.Find("StartPoses");
			if ((bool)transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			Transform transform2 = base.transform.Find("Vehicles");
			if ((bool)transform2)
			{
				UnityEngine.Object.Destroy(transform2.gameObject);
			}
			transform2 = new GameObject("Vehicles").transform;
			startPoses.Clear();
			GameObject gameObject = new GameObject("StartPoses");
			transform = gameObject.transform;
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
			List<GameItem> itemsOfCategory = DismountGame.inventory.GetItemsOfCategory(GameItem.ItemCategory.Character);
			foreach (KeyValuePair<string, string> item in characterNameToPoseName2)
			{
				if (!vehicle.transform.Find(item.Key))
				{
					Transform transform3 = vehicle.transform.Find(characterNameToPoseName2[item.Key]);
					Transform transform4 = UnityEngine.Object.Instantiate(transform3) as Transform;
					transform4.name = item.Key;
					transform4.parent = transform3.parent;
					transform4.localPosition = transform3.localPosition;
					transform4.localRotation = transform3.localRotation;
					transform4.localScale = transform3.localScale;
				}
			}
			vehicle.gameObject.SetActive(false);
			GameObject gameObject2 = new GameObject(vehicle.name);
			Transform transform5 = gameObject2.transform;
			transform5.position = vehicle.transform.position;
			transform5.rotation = vehicle.transform.rotation;
			transform5.parent = transform;
			foreach (GameItem item2 in itemsOfCategory)
			{
				string referenceName = item2.referenceName;
				Transform transform6 = vehicle.transform.Find(item2.referenceName);
				Transform transform7 = DuplicateAndCleanCharacterHierarchy(transform6);
				transform7.parent = transform6.parent;
				transform6.parent = null;
				UnityEngine.Object.Destroy(transform6.gameObject);
				transform6 = transform7;
				if ((bool)transform6)
				{
					transform6.parent = transform5;
					transform6.gameObject.SetActive(true);
					for (int i = 0; i < transform6.childCount; i++)
					{
						Transform child = transform6.GetChild(i);
						startPoses.Add(vehicle.name + "/" + referenceName + "/" + child.name, child);
						child.gameObject.SetActive(false);
					}
				}
				else
				{
					Debug.LogError("Configured character item " + item2.itemId + " had a sceneNodeName that could not be found in vehicle scene for " + vehicle.name + ": " + item2.referenceName);
				}
			}
			vehicle.transform.parent = transform2;
			transform2.parent = base.transform;
			transform.parent = base.transform;
		}

		private GameObject InstantiatePose(string vehicleName, GameObject vehicleInstance, string characterName, int startPosition, Transform tPose)
		{
			GameObject gameObject = null;
			string key = vehicleName + "/" + characterName + "/" + (startPosition + 1);
			if (startPoses.ContainsKey(key))
			{
				Transform transform = startPoses[key];
				gameObject = InstantiateCharacter(tPose.gameObject, transform.gameObject);
				DismountGame.playerState.currentCharacterInstance = gameObject;
				gameObject.transform.parent = vehicleInstance.transform;
				gameObject.SetActive(true);
			}
			else
			{
				Debug.LogError("Could not find character start pose: " + vehicleName + "/" + characterName + "/" + (startPosition + 1));
			}
			return gameObject;
		}

		private bool CheckForOverlap(List<Renderer> renderers, Renderer renderer)
		{
			Bounds bounds = new Bounds(renderer.bounds.center, renderer.bounds.size * 1.1f);
			foreach (Renderer renderer2 in renderers)
			{
				if (renderer2.bounds.Intersects(bounds))
				{
					return true;
				}
			}
			return false;
		}

		public void InstantiateVehicle(SelectionHotspot hotspot)
		{
			string text = DismountGame.playerState.currentVehicleName;
			GameObject gameObject = null;
			if (previouslyLoadedVehicle.CompareTo(text) != 0)
			{
				if (DismountGame.inventory.GetItemByReferenceName(text) == null)
				{
					text = "MilkVan_split";
				}
				if (gameObject == null)
				{
					gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("PlayerVehicles/" + text)) as GameObject;
				}
				gameObject.name = text;
				ProcessLoadedVehicle(gameObject);
				previouslyLoadedVehicle = text;
			}
			else
			{
				gameObject = base.transform.Find("Vehicles/" + text).gameObject;
			}
			vehicleHotSpot = hotspot;
			if ((bool)DismountGame.playerState.currentCharacterInstance)
			{
				UnityEngine.Object.Destroy(DismountGame.playerState.currentCharacterInstance);
				DismountGame.playerState.currentCharacterInstance = null;
				if (characterInstances != null)
				{
					for (int i = 0; i < characterInstances.Length; i++)
					{
						if (characterInstances[i] != null)
						{
							UnityEngine.Object.Destroy(characterInstances[i]);
						}
					}
				}
			}
			if ((bool)DismountGame.playerState.currentVehicleInstance)
			{
				DismountGame.playerState.currentVehicleInstance.GetComponent<Vehicle>().RecycleSkidmarks();
				DismountGame.playerState.currentVehicleInstance.SetActive(false);
				UnityEngine.Object.Destroy(DismountGame.playerState.currentVehicleInstance);
				DismountGame.playerState.currentVehicleInstance = null;
				Transform transform = GameObject.Find("SceneDynamic").transform;
				DynamicPhysicsPart[] componentsInChildren = transform.GetComponentsInChildren<DynamicPhysicsPart>();
				foreach (DynamicPhysicsPart dynamicPhysicsPart in componentsInChildren)
				{
					UnityEngine.Object.Destroy(dynamicPhysicsPart.gameObject);
				}
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject) as GameObject;
			DismountGame.playerState.currentVehicleInstance = gameObject2;
			Vehicle component = gameObject2.GetComponent<Vehicle>();
			string currentCharacterName = DismountGame.playerState.currentCharacterName;
			Transform tPose = tPoses[currentCharacterName];
			int num = 1;
			if (DismountGame.playerState.multipleRagdollsActive)
			{
				num = 10;
			}
			characterInstances = new GameObject[num];
			List<Renderer> list = new List<Renderer>();
			if (DismountGame.playerState.multipleRagdollsActive)
			{
				string[] array = new string[8] { "MrDismount", "MsDismount", "MrHeft", "MsBumblebee", "MrReach", "MrStalwart", "MsDiva", "MrEgo" };
				int num2 = 0;
				int num3 = 0;
				while (num3 < num && num2 < num * 5)
				{
					string text2 = array[UnityEngine.Random.Range(0, array.Length)];
					tPose = tPoses[text2];
					GameObject gameObject3 = InstantiatePose(text, gameObject2, text2, UnityEngine.Random.Range(0, GetCurrentStartingPositionCount(text, text2)), tPose);
					Renderer[] componentsInChildren2 = gameObject3.GetComponentsInChildren<Renderer>(true);
					bool flag = false;
					Renderer[] array2 = componentsInChildren2;
					foreach (Renderer renderer in array2)
					{
						flag = CheckForOverlap(list, renderer);
						if (flag)
						{
							break;
						}
					}
					if (flag)
					{
						UnityEngine.Object.Destroy(gameObject3);
					}
					else
					{
						characterInstances[num3] = gameObject3;
						num3++;
						Renderer[] array3 = componentsInChildren2;
						foreach (Renderer item in array3)
						{
							list.Add(item);
						}
					}
					num2++;
				}
				if (num3 != num)
				{
					GameObject[] array4 = new GameObject[num3];
					for (int m = 0; m < num3; m++)
					{
						array4[m] = characterInstances[m];
					}
					characterInstances = array4;
					num = num3;
				}
			}
			else
			{
				int num4 = DismountGame.playerState.currentCharacterStartPosition;
				if (num4 >= DismountGame.level.GetCurrentStartingPositionCount() || num4 < 0)
				{
					num4 = 0;
				}
				characterInstances[0] = InstantiatePose(text, gameObject2, currentCharacterName, num4, tPose);
			}
			DismountGame.playerState.currentCharacterInstance = characterInstances[0];
			gameObject2.transform.position = hotspot.transform.TransformPoint(gameObject.transform.localPosition);
			gameObject2.transform.rotation = hotspot.transform.rotation;
			gameObject2.SetActive(true);
			if (steerPaths.Count > 0 && DismountGame.playerState.currentLevelSteerPath >= 0)
			{
				Spline steerSpline = steerPaths[DismountGame.playerState.currentLevelSteerPath];
				component.steerSpline = steerSpline;
			}
			for (int n = 0; n < characterInstances.Length; n++)
			{
				characterInstances[n].GetComponent<MrDismount>().AttachToVehicle(gameObject2);
			}
			MrDismount component2 = characterInstances[0].GetComponent<MrDismount>();
			component.SetCharacter(component2);
			gameObject2.SendMessage("WakeUp");
			for (int num5 = 0; num5 < characterInstances.Length; num5++)
			{
				MrDismount component3 = characterInstances[num5].GetComponent<MrDismount>();
				if (component3.kinematicDismountSetup)
				{
					component3.SetKinematic(true);
				}
				if (!component3.kinematicDismountSetup)
				{
					component3.SetKinematic(false);
					characterInstances[num5].transform.parent = null;
				}
			}
			DismountGame.instance.OnLevelLoaded();
			GameObject gameObject4 = GameObject.Find("LevelLogic");
			if ((bool)gameObject4)
			{
				gameObject4.SendMessage("OnLevelLoaded", SendMessageOptions.DontRequireReceiver);
			}
		}

		public int GetCurrentStartingPositionCount()
		{
			string currentVehicleName = DismountGame.playerState.currentVehicleName;
			string currentCharacterName = DismountGame.playerState.currentCharacterName;
			return GetCurrentStartingPositionCount(currentVehicleName, currentCharacterName);
		}

		public int GetCurrentStartingPositionCount(string vehicleName, string characterName)
		{
			for (int num = 9; num >= 0; num--)
			{
				string key = vehicleName + "/" + characterName + "/" + (num + 1);
				if (startPoses.ContainsKey(key))
				{
					return num + 1;
				}
			}
			return 0;
		}

		public void ChangeVehicle(SelectionHotspot hotspot)
		{
			DismountGame.audioManager.SFXFadeIn();
			InstantiateVehicle(hotspot);
			DismountGame.playerState.SaveVehicleAndCharacter();
			if (DismountGame.cameraManager.GetActiveCameraId().CompareTo("Vehicle") == 0)
			{
				SetManualSteering(DismountGame.playerState.manualSteering, true);
			}
		}

		private Transform FindTargetNode(string name, Transform[] nodes)
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				if (nodes[i].name.CompareTo(name) == 0)
				{
					return nodes[i];
				}
			}
			return null;
		}

		private GameObject InstantiateCharacter(GameObject tPose, GameObject startPose)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(tPose, startPose.transform.position, startPose.transform.rotation) as GameObject;
			startPose.SetActive(true);
			gameObject.SetActive(true);
			gameObject.transform.parent = startPose.transform.parent;
			Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
			Transform[] componentsInChildren2 = startPose.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Transform transform = FindTargetNode(componentsInChildren[i].name, componentsInChildren2);
				if (!(transform != null))
				{
					continue;
				}
				componentsInChildren[i].localPosition = transform.localPosition;
				componentsInChildren[i].localRotation = transform.localRotation;
				componentsInChildren[i].localScale = transform.localScale;
				if ((bool)componentsInChildren[i].GetComponent<BodyPart>())
				{
					BodyPartAttachData component = transform.GetComponent<BodyPartAttachData>();
					if (component != null)
					{
						componentsInChildren[i].GetComponent<BodyPart>().attachForce = component.attachForce;
						componentsInChildren[i].GetComponent<BodyPart>().attachRigidbodyName = component.attachRigidbodyName;
					}
				}
			}
			gameObject.GetComponent<MrDismount>().kinematicDismountSetup = startPose.GetComponent<MrDismountSetupData>().kinematicDismountSetup;
			startPose.SetActive(false);
			if (DismountGame.playerState.currentHeadItem != null && DismountGame.playerState.currentHeadItem.referenceName.CompareTo("empty") != 0 && !DismountGame.playerState.IsBubbleHeadMode)
			{
				DismountGame.instance.customHead = Resources.Load<GameObject>("CustomHeads/" + DismountGame.playerState.currentHeadName);
				gameObject.GetComponent<MrDismount>().SetCustomHead(DismountGame.instance.customHead);
			}
			return gameObject;
		}

		private string GetObstacleName(SelectionHotspot hotspot)
		{
			return hotspot.name + "_" + hotspot.category;
		}

		public void ChangeObstacle(SelectionHotspot hotspot)
		{
			string obstacleName = GetObstacleName(hotspot);
			DeleteOldObstacle(obstacleName);
			InstantiateObstacle(hotspot);
			SaveObstacles("user");
		}

		public void UpdateObstacleTransforms()
		{
			for (int i = 0; i < selectionHotspots.Count; i++)
			{
				SelectionHotspot component = selectionHotspots[i].GetComponent<SelectionHotspot>();
				string obstacleName = GetObstacleName(component);
				GameObject gameObject = GameObject.Find(obstacleName);
				if ((bool)gameObject)
				{
					gameObject.transform.position = component.gameObject.transform.position;
					gameObject.transform.rotation = component.gameObject.transform.rotation;
				}
			}
		}

		private void DeleteOldObstacle(string obstacleName)
		{
			GameObject gameObject = GameObject.Find(obstacleName);
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
				instantiatedObstacles.Remove(gameObject);
			}
		}

		private void InstantiateObstacle(SelectionHotspot hotspot)
		{
			if (hotspot.category != GameItem.ItemCategory.Obstacle || hotspot.item == null || hotspot.item.ingamePrefab == null)
			{
				return;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(hotspot.item.ingamePrefab, hotspot.transform.position, hotspot.transform.rotation) as GameObject;
			gameObject.name = GetObstacleName(hotspot);
			BrickWallGenerator component = gameObject.GetComponent<BrickWallGenerator>();
			if ((bool)component)
			{
				component.Generate();
			}
			Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>(true);
			Collider[] array = componentsInChildren;
			foreach (Collider collider in array)
			{
				collider.enabled = false;
			}
			Rigidbody[] componentsInChildren2 = gameObject.GetComponentsInChildren<Rigidbody>(true);
			Rigidbody[] array2 = componentsInChildren2;
			foreach (Rigidbody rigidbody in array2)
			{
				if (rigidbody.isKinematic)
				{
					rigidbody.useGravity = false;
				}
				else
				{
					rigidbody.useGravity = true;
				}
				rigidbody.isKinematic = true;
			}
			Utils.SetSubtreeLayer(gameObject.transform, LayerMask.NameToLayer("Default"));
			instantiatedObstacles.Add(gameObject);
		}

		public void ClearObstacles()
		{
			LoadObstacles("default");
			foreach (GameObject instantiatedObstacle in instantiatedObstacles)
			{
				UnityEngine.Object.Destroy(instantiatedObstacle);
			}
			instantiatedObstacles.Clear();
			SelectionHotspot[] array = obstacleHotspots;
			foreach (SelectionHotspot selectionHotspot in array)
			{
				selectionHotspot.item = null;
			}
			SaveObstacles("user");
		}

		public void ReActivateRecordedObjects()
		{
			GameObject particleSystemRoot = DismountGame.particleManager.GetParticleSystemRoot();
			particleSystemRoot.SetActive(true);
			for (int i = 0; i < particleSystemRoot.transform.childCount; i++)
			{
				particleSystemRoot.transform.GetChild(i).gameObject.SetActive(true);
			}
		}

		public void AddObjectsForReplay()
		{
			Replay instance = Replay.Instance;
			List<GameObject> gameObjectsToBeRecorded = instance.GameObjectsToBeRecorded;
			GameObject item = DismountGame.instance.transform.Find("DismountGameProxy").gameObject;
			gameObjectsToBeRecorded.Add(item);
			HUDManager.DamageIcon[] damageIcons = DismountGame.hudManager.GetDamageIcons();
			HUDManager.DamageIcon[] array = damageIcons;
			foreach (HUDManager.DamageIcon damageIcon in array)
			{
				gameObjectsToBeRecorded.Add(damageIcon.go);
			}
			gameObjectsToBeRecorded.Add(DismountGame.particleManager.GetParticleSystemRoot());
			Transform transform = GameObject.Find("SceneDynamic").transform;
			if ((bool)transform)
			{
				gameObjectsToBeRecorded.Add(transform.gameObject);
			}
			foreach (GameObject instantiatedObstacle in instantiatedObstacles)
			{
				if ((bool)instantiatedObstacle)
				{
					gameObjectsToBeRecorded.Add(instantiatedObstacle);
				}
			}
			gameObjectsToBeRecorded.Add(DismountGame.playerState.currentVehicleInstance);
			if (characterInstances == null)
			{
				return;
			}
			for (int j = 0; j < characterInstances.Length; j++)
			{
				if (characterInstances[j].transform.root != DismountGame.playerState.currentVehicleInstance.transform)
				{
					gameObjectsToBeRecorded.Add(characterInstances[j]);
				}
			}
		}

		public void OnDismountStarted()
		{
			_isDismountActive = true;
			GameObject gameObject = GameObject.Find("LevelLogic");
			if ((bool)gameObject)
			{
				gameObject.SendMessage("OnDismountStarted", SendMessageOptions.DontRequireReceiver);
			}
			Cop[] array = UnityEngine.Object.FindObjectsOfType<Cop>();
			if (array != null && array.Length > 0)
			{
				Rigidbody[] targets = new Rigidbody[2]
				{
					DismountGame.playerState.currentVehicleInstance.GetComponent<Rigidbody>(),
					DismountGame.playerState.currentCharacterInstance.GetComponent<MrDismount>().cameraTarget.GetComponent<Rigidbody>()
				};
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Chase(targets);
					if (!array[i].isTrafficCop)
					{
						array[i].GetComponent<Rigidbody>().isKinematic = false;
						array[i].GetComponent<Rigidbody>().useGravity = true;
					}
				}
			}
			PinballBumper[] array2 = UnityEngine.Object.FindObjectsOfType<PinballBumper>();
			if (array2 != null)
			{
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j].OnDismountStarted();
				}
			}
			foreach (GameObject instantiatedObstacle in instantiatedObstacles)
			{
				if (!instantiatedObstacle)
				{
					continue;
				}
				Collider[] componentsInChildren = instantiatedObstacle.GetComponentsInChildren<Collider>(true);
				Collider[] array3 = componentsInChildren;
				foreach (Collider collider in array3)
				{
					collider.enabled = true;
				}
				Rigidbody[] componentsInChildren2 = instantiatedObstacle.GetComponentsInChildren<Rigidbody>(true);
				Rigidbody[] array4 = componentsInChildren2;
				foreach (Rigidbody rigidbody in array4)
				{
					if (rigidbody.useGravity)
					{
						rigidbody.isKinematic = false;
						rigidbody.WakeUp();
					}
				}
				instantiatedObstacle.BroadcastMessage("OnDismountStarted", SendMessageOptions.DontRequireReceiver);
			}
		}

		public void OnDismountComplete()
		{
			_isDismountActive = false;
			GameObject gameObject = GameObject.Find("LevelLogic");
			if ((bool)gameObject)
			{
				gameObject.SendMessage("OnDismountComplete", SendMessageOptions.DontRequireReceiver);
			}
		}

		public void OnDismountReset()
		{
			_isDismountActive = false;
			GameObject gameObject = GameObject.Find("LevelLogic");
			if ((bool)gameObject)
			{
				gameObject.SendMessage("OnDismountReset", SendMessageOptions.DontRequireReceiver);
			}
		}

		public void OnDismountExit()
		{
			_isDismountActive = false;
		}
	}
}
