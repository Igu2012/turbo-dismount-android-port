#pragma warning disable 0618,0619
using System.Collections;
using UnityEngine;

namespace Dismount.GameStates
{
	public class Splash : MonoBehaviour
	{
		private void EnterState()
		{
			SXInputManager.controllerEnabled = Prefs.GetInt("controllerEnabled", 1) != 0;
			StartCoroutine("UILoader");
		}

		private IEnumerator UILoader()
		{
			Application.LoadLevelAdditive("UIBuilder");
			yield return 0;
			UILoaded();
		}

		private void UILoaded()
		{
			StartCoroutine("HUDLoader");
		}

		private IEnumerator HUDLoader()
		{
			Application.LoadLevelAdditive("HUD");
			yield return 0;
			HUDLoaded();
		}

		private void HUDLoaded()
		{
			Transform transform = base.transform.Find("/GameUI");
			Transform transform2 = base.transform.Find("/HUD");
			transform.parent = DismountGame.instance.transform;
			transform2.parent = DismountGame.instance.transform;
			DismountGame.instance.OnUILoaded();
			DismountGame.uiManager.SetItems(UIManager.ItemDialogType.Obstacles, DismountGame.inventory.GetItemsOfCategory(GameItem.ItemCategory.Obstacle));
			DismountGame.uiManager.SetItems(UIManager.ItemDialogType.Vehicles, DismountGame.inventory.GetItemsOfCategory(GameItem.ItemCategory.Vehicle));
			DismountGame.uiManager.SetItems(UIManager.ItemDialogType.Characters, DismountGame.inventory.GetItemsOfCategory(GameItem.ItemCategory.Character));
			DismountGame.uiManager.SetItems(UIManager.ItemDialogType.Heads, DismountGame.inventory.GetItemsOfCategory(GameItem.ItemCategory.Head));
			if (!DismountGame.playerState.browsingCustomLevels)
			{
				DismountGame.uiManager.SetItems(UIManager.ItemDialogType.Levels, DismountGame.inventory.GetItemsOfCategory(GameItem.ItemCategory.Level));
			}
			string itemId = Prefs.GetString("vehicle");
			GameItem item = DismountGame.inventory.GetItem(itemId);
			if (item != null && item.isLocked)
			{
				Prefs.SetString("vehicle", "MilkVan");
			}
			string text = Application.loadedLevelName;
			if (text.EndsWith("_ios"))
			{
				text = text.Substring(0, text.IndexOf("_ios"));
			}
			DismountGame.level.NextLevel(text, false, false);
			DismountGame.level.LoadLevel(false);
		}

		private void ExitState()
		{
			base.gameObject.SetActive(false);
		}
	}
}
