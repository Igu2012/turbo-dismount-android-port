#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(Camera))]
	[RequireComponent(typeof(UIDraggableCamera))]
	public class DragCameraCenterOnObject : MonoBehaviour
	{
		public Transform itemsPanel;

		public GameObject selectableItemPrefab;

		public GameItem.ItemCategory itemCategory;

		private Dictionary<int, GameObject> items = new Dictionary<int, GameObject>();

		private bool dragStarted;

		private float startPosX;

		private int currentVisibleItem;

		private void Start()
		{
			PopulateItems();
			if (items.Count == 0)
			{
				Debug.LogError("No items set in DragCameraCenterOnObject");
			}
		}

		private void PopulateItems()
		{
			Inventory inventory = DismountGame.inventory;
			List<GameItem> itemsOfCategory = inventory.GetItemsOfCategory(itemCategory);
			int num = 0;
			foreach (GameItem item in itemsOfCategory)
			{
				GameObject gameObject = Object.Instantiate(selectableItemPrefab) as GameObject;
				gameObject.transform.parent = itemsPanel;
				gameObject.transform.localPosition = new Vector3(164f, -116.5981f, 0f) + num * new Vector3(328f, 0f, 0f);
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
				gameObject.name = "SelectableItem " + num;
				gameObject.GetComponent<SelectableItem>().gameItem = item;
				UILabel component = gameObject.transform.Find("UILabel").GetComponent<UILabel>();
				component.text = item.itemName;
				if (item.isLocked)
				{
					component.text = "LOCKED " + item.itemName;
				}
				GameObject gameObject2 = Object.Instantiate(item.menuPrefab, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.localScale = Vector3.one;
				gameObject2.layer = LayerMask.NameToLayer("Setup");
				items.Add(num, gameObject);
				num++;
			}
		}

		public bool ScrollToNextItemRight()
		{
			int key = currentVisibleItem + 1;
			if (items.ContainsKey(key))
			{
				GameObject gameObject = items[key];
				iTween.MoveTo(base.gameObject, iTween.Hash("x", gameObject.transform.position.x, "speed", 10, "easetype", iTween.EaseType.linear));
				currentVisibleItem++;
				return true;
			}
			return false;
		}

		public bool ScrollToNextItemLeft()
		{
			int key = currentVisibleItem - 1;
			if (items.ContainsKey(key))
			{
				GameObject gameObject = items[key];
				iTween.MoveTo(base.gameObject, iTween.Hash("x", gameObject.transform.position.x, "speed", 10, "easetype", iTween.EaseType.linear));
				currentVisibleItem--;
				return true;
			}
			return false;
		}

		public GameObject GetSelectedItemContents()
		{
			return items[currentVisibleItem].GetComponent<SelectableItem>().gameItem.ingamePrefab;
		}

		private void Update()
		{
			if (!ShouldCheckDrag())
			{
				return;
			}
			if (SXInputManager.GetMouseButtonDown(0))
			{
				foreach (KeyValuePair<int, GameObject> item in items)
				{
					if (item.Key == currentVisibleItem)
					{
						dragStarted = true;
						startPosX = base.transform.position.x;
						break;
					}
				}
			}
			if (!dragStarted || SXInputManager.GetMouseButton(0))
			{
				return;
			}
			bool flag = false;
			dragStarted = false;
			float num = base.transform.position.x - startPosX;
			bool flag2 = ((num > 0f) ? true : false);
			if (Mathf.Abs(num) > 0.1f)
			{
				if (flag2)
				{
					if (!ScrollToNextItemRight())
					{
						flag = true;
					}
				}
				else if (!ScrollToNextItemLeft())
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				iTween.MoveTo(base.gameObject, iTween.Hash("x", items[currentVisibleItem].transform.position.x, "speed", 10, "easetype", iTween.EaseType.linear));
			}
		}

		private bool ShouldCheckDrag()
		{
			if (dragStarted)
			{
				return true;
			}
			if (UICamera.lastHit.transform == null)
			{
				return false;
			}
			foreach (KeyValuePair<int, GameObject> item in items)
			{
				if (item.Value == UICamera.lastHit.transform.gameObject)
				{
					return true;
				}
			}
			return false;
		}
	}
}
