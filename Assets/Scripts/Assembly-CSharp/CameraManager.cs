#pragma warning disable 0618,0619
using System.Collections.Generic;
using Dismount;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	private Dictionary<string, GameObject> cameras = new Dictionary<string, GameObject>();

	private Dictionary<string, Transform> cameraTargets = new Dictionary<string, Transform>();

	private Dictionary<string, float> chaseCameraDistances = new Dictionary<string, float>();

	private string currentCameraId = string.Empty;

	public GameObject currentCamera;

	private bool replayMode;

	public bool hasValidFreeFlyPosition;

	private Material overrideSkyboxMaterial;

	private Vector3 cameraTransitionStartPosition;

	private Quaternion cameraTransitionStartRotation;

	private float cameraTransitionStartFov;

	private float cameraTransitionStartNearClip;

	private float cameraTransitionStartFarClip;

	private float cameraTransitionStartTime = -1f;

	private float cameraTransitionTime = 0.5f;

	public GameObject globalCamera;

	public GameObject globalListener;

	public DamageFlash damageFlash;

	private bool hasSceneCamera;

	private BlurEffect blurEffect;

	private float chasemodeEnableTime = -1f;

	public Material OverrideSkyboxMaterial
	{
		get
		{
			return overrideSkyboxMaterial;
		}
		set
		{
			overrideSkyboxMaterial = value;
		}
	}

	private void Start()
	{
		globalListener.GetComponent<AudioListener>().velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
		blurEffect = globalCamera.GetComponent<BlurEffect>();
		chaseCameraDistances.Add("vehicle.shoppingcart", 5f);
		chaseCameraDistances.Add("vehicle.skateboard", 5f);
		chaseCameraDistances.Add("vehicle.officechair", 5f);
		chaseCameraDistances.Add("vehicle.tricycle", 7f);
		chaseCameraDistances.Add("vehicle.motorcycle", 7f);
		chaseCameraDistances.Add("vehicle.scooter", 7f);
		chaseCameraDistances.Add("vehicle.bird", 7f);
		chaseCameraDistances.Add("vehicle.superbike", 7f);
		chaseCameraDistances.Add("vehicle.monstervan", 11f);
		chaseCameraDistances.Add("vehicle.tractor", 11f);
		chaseCameraDistances.Add("vehicle.bulldozer", 11f);
		chaseCameraDistances.Add("vehicle.splittruck", 15f);
		chaseCameraDistances.Add("vehicle.firetruck", 15f);
		chaseCameraDistances.Add("vehicle.crane", 15f);
		chaseCameraDistances.Add("vehicle.londonbus", 15f);
		chaseCameraDistances.Add("vehicle.quad", 7f);
	}

	public void EnableBlur(bool enable)
	{
	}

	private void FixedUpdate()
	{
		if (chasemodeEnableTime > 0f && Time.fixedTime > chasemodeEnableTime)
		{
			DoSetChaseMode(DismountGame.playerState.manualSteering);
		}
		if (!replayMode)
		{
			float t = 1f;
			if (cameraTransitionStartTime != -1f)
			{
				t = Mathf.Min(1f, (Time.fixedTime - cameraTransitionStartTime) / cameraTransitionTime);
			}
			if ((bool)currentCamera)
			{
				globalListener.transform.position = Vector3.Lerp(cameraTransitionStartPosition, currentCamera.transform.position, t);
				globalListener.transform.rotation = Quaternion.Lerp(cameraTransitionStartRotation, currentCamera.transform.rotation, t);
			}
		}
	}

	public void SetSnowFX(bool active)
	{
		bool active2 = active && DismountGame.instance.snowSeason;
		if (DismountGame.IsLowPerformanceDevice())
		{
			active2 = false;
		}
		globalCamera.transform.Find("SnowFx").gameObject.SetActive(active2);
	}

	private void LateUpdate()
	{
		float t = 1f;
		if (cameraTransitionStartTime != -1f && cameraTransitionTime > 0.01f)
		{
			t = Mathf.Min(1f, (Time.fixedTime - cameraTransitionStartTime) / cameraTransitionTime);
		}
		if ((bool)currentCamera)
		{
			globalCamera.transform.position = Vector3.Lerp(cameraTransitionStartPosition, currentCamera.transform.position, t);
			globalCamera.transform.rotation = Quaternion.Lerp(cameraTransitionStartRotation, currentCamera.transform.rotation, t);
			Camera component = currentCamera.GetComponent<Camera>();
			Camera component2 = globalCamera.GetComponent<Camera>();
			component2.fieldOfView = Mathf.Lerp(cameraTransitionStartFov, component.fieldOfView, t);
			component2.nearClipPlane = Mathf.Lerp(cameraTransitionStartNearClip, component.nearClipPlane, t);
			component2.farClipPlane = Mathf.Lerp(cameraTransitionStartFarClip, component.farClipPlane, t);
			if (replayMode)
			{
				globalListener.transform.position = Vector3.Lerp(cameraTransitionStartPosition, currentCamera.transform.position, t);
				globalListener.transform.rotation = Quaternion.Lerp(cameraTransitionStartRotation, currentCamera.transform.rotation, t);
			}
		}
	}

	public void FindSceneCameras()
	{
		currentCameraId = string.Empty;
		cameras.Clear();
		hasSceneCamera = false;
		ManagedCamera[] array = Object.FindObjectsOfType(typeof(ManagedCamera)) as ManagedCamera[];
		ManagedCamera[] array2 = array;
		foreach (ManagedCamera managedCamera in array2)
		{
			managedCamera.GetComponent<Camera>().enabled = true;
			int cullingMask = managedCamera.GetComponent<Camera>().cullingMask;
			cullingMask &= ~(1 << LayerMask.NameToLayer("EditorOnly"));
			if (managedCamera.groupId != "1stPerson")
			{
				cullingMask |= 1 << (LayerMask.NameToLayer("Face") & 0x1F);
			}
			cullingMask &= ~(1 << LayerMask.NameToLayer("UIFix"));
			cullingMask &= ~(1 << LayerMask.NameToLayer("VideoDecoration"));
			cullingMask = ((!(managedCamera.groupId != "Setup")) ? (cullingMask | (1 << LayerMask.NameToLayer("Gizmo"))) : (cullingMask & ~(1 << LayerMask.NameToLayer("Gizmo"))));
			managedCamera.GetComponent<Camera>().cullingMask = cullingMask;
			if (managedCamera.gameObject != currentCamera)
			{
				managedCamera.gameObject.SetActive(false);
			}
			cameras[managedCamera.groupId] = managedCamera.gameObject;
			if (managedCamera.groupId.CompareTo("Scene") == 0)
			{
				hasSceneCamera = true;
			}
		}
	}

	public string GetActiveCameraId()
	{
		return currentCameraId;
	}

	public void SetActiveCamera(string id, float transitionTime = 0.5f)
	{
		if (!(currentCameraId == id))
		{
			if (id.CompareTo("Scene") == 0 && !hasSceneCamera)
			{
				id = "Character";
			}
			currentCameraId = id;
			if (id == "FreeFly" && !hasValidFreeFlyPosition)
			{
				Transform transform = DismountGame.playerState.currentVehicleInstance.transform;
				GameObject gameObject = cameras["FreeFly"];
				FreeFlyCamera component = gameObject.GetComponent<FreeFlyCamera>();
				Transform transform2 = component.transform;
				transform2.position = transform.position + transform.forward * 30f + transform.right * 10f + Vector3.up * 10f;
				transform2.LookAt(transform.position, Vector3.up);
				component.euler = transform2.eulerAngles;
				component.position = transform2.position;
				component.fov = 50f;
			}
			if (currentCameraId == "1stPerson")
			{
				damageFlash.gameObject.SetActive(true);
			}
			else
			{
				damageFlash.gameObject.SetActive(false);
			}
			ChangeCamera(cameras[id], transitionTime);
			if (currentCameraId == "Character" || currentCameraId == "1stPerson")
			{
				DismountGame.hudManager.SetStatsMode(HUDManager.StatsMode.Character);
			}
			else if (currentCameraId == "Vehicle")
			{
				DismountGame.hudManager.SetStatsMode(HUDManager.StatsMode.Vehicle);
				DoSetChaseMode(DismountGame.playerState.manualSteering);
			}
			else
			{
				DismountGame.hudManager.SetStatsMode(HUDManager.StatsMode.None);
			}
		}
	}

	public void ActivateFreeFlyCamera()
	{
		Vector3 position = currentCamera.transform.position;
		Vector3 eulerAngles = currentCamera.transform.eulerAngles;
		eulerAngles.z = 0f;
		float fieldOfView = currentCamera.GetComponent<Camera>().fieldOfView;
		GameObject gameObject = cameras["FreeFly"];
		FreeFlyCamera component = gameObject.GetComponent<FreeFlyCamera>();
		if ((bool)component)
		{
			component.position = position;
			component.euler = eulerAngles;
			component.fov = fieldOfView;
			gameObject.transform.position = position;
			gameObject.transform.eulerAngles = eulerAngles;
			gameObject.GetComponent<Camera>().fieldOfView = fieldOfView;
		}
		hasValidFreeFlyPosition = true;
		SetActiveCamera("FreeFly", 0.25f);
		DismountGame.hudManager.ShowCameraName("Free Fly Camera");
	}

	public void CycleActiveCamera(float transitionTime = 0f)
	{
		bool flag = SXInputManager.IsControllerConnected() && SXInputManager.controllerEnabled;
		string[] array;
		string[] array2;
		if (hasSceneCamera)
		{
			if (flag)
			{
				array = new string[5] { "Character", "Vehicle", "1stPerson", "Scene", "FreeFly" };
				array2 = new string[5] { "Character Camera", "Vehicle Camera", "First Person Camera", "Cinematic Camera", "Free Fly Camera" };
			}
			else
			{
				array = new string[4] { "Character", "Vehicle", "1stPerson", "Scene" };
				array2 = new string[4] { "Character Camera", "Vehicle Camera", "First Person Camera", "Cinematic Camera" };
			}
		}
		else if (flag)
		{
			array = new string[4] { "Character", "Vehicle", "1stPerson", "FreeFly" };
			array2 = new string[4] { "Character Camera", "Vehicle Camera", "First Person Camera", "Free Fly Camera" };
		}
		else
		{
			array = new string[3] { "Character", "Vehicle", "1stPerson" };
			array2 = new string[3] { "Character Camera", "Vehicle Camera", "First Person Camera" };
		}
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (currentCameraId == array[i])
			{
				num = i;
				break;
			}
		}
		num++;
		num %= array.Length;
		SetActiveCamera(array[num], transitionTime);
		DismountGame.hudManager.ShowCameraName(array2[num]);
	}

	public GameObject GetSetupCamera()
	{
		return cameras["Setup"];
	}

	private void ChangeCamera(GameObject cameraObject, float transitionTime = 0.5f)
	{
		chasemodeEnableTime = -1f;
		if ((bool)currentCamera)
		{
			cameraTransitionStartPosition = currentCamera.transform.position;
			cameraTransitionStartRotation = currentCamera.transform.rotation;
			Camera component = currentCamera.GetComponent<Camera>();
			cameraTransitionStartFov = component.fieldOfView;
			cameraTransitionStartNearClip = component.nearClipPlane;
			cameraTransitionStartFarClip = component.farClipPlane;
			cameraTransitionStartTime = Time.fixedTime;
			cameraTransitionTime = transitionTime;
			currentCamera.gameObject.SetActive(false);
		}
		currentCamera = cameraObject;
		if (!currentCamera)
		{
			return;
		}
		currentCamera.SetActive(true);
		currentCamera.GetComponent<Camera>().enabled = false;
		if (replayMode)
		{
			cameraObject.SendMessage("EnterReplayMode", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			cameraObject.SendMessage("ExitReplayMode", SendMessageOptions.DontRequireReceiver);
		}
		if ((bool)overrideSkyboxMaterial)
		{
			Skybox component2 = globalCamera.GetComponent<Skybox>();
			if ((bool)component2)
			{
				component2.material = overrideSkyboxMaterial;
			}
		}
		globalCamera.GetComponent<Camera>().cullingMask = currentCamera.GetComponent<Camera>().cullingMask;
		globalCamera.SendMessage("Reset", SendMessageOptions.DontRequireReceiver);
	}

	public void SetCameraTarget(string cameraId, Transform target, Transform secondaryTarget = null)
	{
		if (cameras.ContainsKey(cameraId))
		{
			GameObject gameObject = cameras[cameraId];
			cameraTargets[cameraId] = target;
			TVCamera component = gameObject.GetComponent<TVCamera>();
			if ((bool)component)
			{
				component.SetTarget(target);
			}
			OrbitCamera component2 = gameObject.GetComponent<OrbitCamera>();
			if ((bool)component2)
			{
				component2.target = target;
			}
			FirstPersonCamera component3 = gameObject.GetComponent<FirstPersonCamera>();
			if ((bool)component3)
			{
				component3.SetTarget(target);
			}
			SplinePathCamera component4 = gameObject.GetComponent<SplinePathCamera>();
			if ((bool)component4)
			{
				component4.ControlTarget = target;
				component4.LookAtTarget = target;
			}
			TrackCamera component5 = gameObject.GetComponent<TrackCamera>();
			if ((bool)component5)
			{
				component5.primaryTarget = target;
				component5.secondaryTarget = secondaryTarget;
			}
		}
	}

	public void SetChaseMode(bool enabled, bool delayed)
	{
		if (enabled && delayed)
		{
			chasemodeEnableTime = Time.fixedTime + 1.5f;
			return;
		}
		chasemodeEnableTime = -1f;
		DoSetChaseMode(enabled);
	}

	private void DoSetChaseMode(bool enabled)
	{
		chasemodeEnableTime = -1f;
		if (!cameras.ContainsKey("Vehicle"))
		{
			return;
		}
		GameObject gameObject = cameras["Vehicle"];
		if (gameObject == null)
		{
			return;
		}
		string currentVehicleItemId = DismountGame.playerState.currentVehicleItemId;
		OrbitCamera component = gameObject.GetComponent<OrbitCamera>();
		if ((bool)component)
		{
			component.chaseMode = enabled;
			if (enabled)
			{
				float chaseDistance = 9f;
				float chaseAngle = -17.5f;
				if (chaseCameraDistances.ContainsKey(currentVehicleItemId))
				{
					chaseDistance = chaseCameraDistances[currentVehicleItemId];
				}
				component.SetChaseCameraDistanceAndAngle(chaseDistance, chaseAngle);
			}
		}
		if (enabled && currentCameraId.Length != 0 && currentCameraId.CompareTo("Vehicle") != 0)
		{
			SetActiveCamera("Vehicle");
			DismountGame.hudManager.ShowCameraName("Vehicle Camera");
		}
	}

	public Transform GetCameraTarget(string cameraId)
	{
		if (cameraTargets.ContainsKey(cameraId))
		{
			return cameraTargets[cameraId];
		}
		return null;
	}

	public void EnterReplayMode()
	{
		replayMode = true;
		globalListener.GetComponent<AudioListener>().velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
		foreach (GameObject value in cameras.Values)
		{
			if (value != null)
			{
				value.SendMessage("EnterReplayMode", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void ExitReplayMode()
	{
		replayMode = false;
		globalListener.GetComponent<AudioListener>().velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
		foreach (GameObject value in cameras.Values)
		{
			if (value != null)
			{
				value.SendMessage("ExitReplayMode", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void EnableRewindEffects(bool doEnable)
	{
	}
}
