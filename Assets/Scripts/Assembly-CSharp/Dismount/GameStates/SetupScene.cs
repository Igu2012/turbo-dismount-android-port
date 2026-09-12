#pragma warning disable 0618,0619
using Dismount.Vehicular;
using UnityEngine;

namespace Dismount.GameStates
{
	public class SetupScene : MonoBehaviour
	{
		private SelectionHotspot currentHotspot;

		private Vehicle vehicle;

		private UIManager.State previousUIState;

		private string previousLevel = string.Empty;

		private static Vector3 savedCameraPosition;

		private static Quaternion savedCameraRotation;

		private static float savedCameraFov;

		private Vector3 originalForward = Vector3.forward;

		private Vector3 originalRight = Vector3.right;

		public SelectionHotspot CurrentHotSpot
		{
			get
			{
				return currentHotspot;
			}
		}

		private void EnterState()
		{
			vehicle = DismountGame.playerState.currentVehicleInstance.GetComponent<Vehicle>();
			DismountGame.hudManager.Disable();
			DismountGame.cameraManager.SetActiveCamera("Setup");
			if (previousLevel == DismountGame.playerState.currentLevelName)
			{
				FreeFlyCamera component = DismountGame.cameraManager.currentCamera.GetComponent<FreeFlyCamera>();
				if ((bool)component)
				{
					component.transform.position = savedCameraPosition;
					component.transform.rotation = savedCameraRotation;
					component.fov = savedCameraFov;
					component.euler = savedCameraRotation.eulerAngles;
					component.position = savedCameraPosition;
				}
			}
			DismountGame.uiManager.ChangeState(UIManager.State.SetupScene);
			DismountGame.level.ShowSteerPath();
			DismountGame.uiManager.OpenDoor();
		}

		private void ExitState()
		{
			previousLevel = DismountGame.playerState.currentLevelName;
			savedCameraPosition = DismountGame.cameraManager.currentCamera.transform.position;
			savedCameraRotation = DismountGame.cameraManager.currentCamera.transform.rotation;
			savedCameraFov = DismountGame.cameraManager.currentCamera.GetComponent<Camera>().fieldOfView;
			DismountGame.level.HideSteerPath();
			base.gameObject.SetActive(false);
			DismountGame.level.SaveObstacles("user");
		}

		private void Update()
		{
			float inputThrottle = Utils.DismountBarFillFunction(Time.time - DismountGame.playerState.revStartTime, 0.3f);
			vehicle.inputThrottle = inputThrottle;
		}

		private void UpdateObstacleTransforms()
		{
			DismountGame.level.UpdateObstacleTransforms();
		}

		private void OnOpenSelection(SelectionHotspot hotspot)
		{
			if (hotspot.category == GameItem.ItemCategory.Obstacle)
			{
				currentHotspot = hotspot;
				previousUIState = DismountGame.uiManager.CurrentState;
				DismountGame.uiManager.ChangeState(UIManager.State.SetupObstacle);
			}
		}

		private void OnCloseSelection()
		{
			DismountGame.uiManager.ChangeState(UIManager.State.SetupScene);
		}

		private void OnSelectObstacle(GameItem obstacleItem)
		{
			if (!obstacleItem.isLocked)
			{
				currentHotspot.item = obstacleItem;
				DismountGame.level.ChangeObstacle(currentHotspot);
				OnCloseSelection();
			}
			else
			{
				DismountGame.uiManager.ChangeState(UIManager.State.WebNag);
			}
		}

		private void CloseDialog()
		{
			OnGotoPreviousUIState();
		}

		private void OnGotoPreviousUIState()
		{
			UIManager.State currentState = DismountGame.uiManager.CurrentState;
			if (currentState == UIManager.State.SetupObstacle)
			{
				DismountGame.uiManager.ChangeState(UIManager.State.SetupScene);
			}
			if (currentState == UIManager.State.SetupScene)
			{
				DismountGame.instance.OnReady();
			}
			if (currentState == UIManager.State.WebNag)
			{
				DismountGame.uiManager.ChangeState(UIManager.State.SetupObstacle);
			}
		}
	}
}
