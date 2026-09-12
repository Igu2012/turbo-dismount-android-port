#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class ItemSelectDialogItem : MonoBehaviour
	{
		public string eventReceiver;

		public string eventName;

		public float iconScale = 1f;

		private GameItem item;

		public GameItem Item
		{
			get
			{
				return item;
			}
			set
			{
				item = value;
				FixItemSprite();
			}
		}

		private void FixItemSprite()
		{
			if (!item)
			{
				return;
			}
			if ((bool)item.menuPrefab)
			{
				UISprite component = item.menuPrefab.GetComponent<UISprite>();
				if ((bool)component)
				{
					GameObject gameObject = base.transform.Find("Image Button/ItemSprite").gameObject;
					UISprite component2 = gameObject.GetComponent<UISprite>();
					component2.atlas = component.atlas;
					component2.spriteName = component.spriteName;
					component2.MakePixelPerfect();
					component2.transform.localScale /= 0.75f;
					gameObject.transform.localScale *= iconScale;
				}
			}
			Transform transform = base.transform.Find("ItemName");
			if ((bool)transform)
			{
				UILabel component3 = transform.gameObject.GetComponent<UILabel>();
				if (item.itemName != string.Empty)
				{
					component3.text = item.itemName;
				}
			}
		}

		public void OnClick()
		{
			Utils.SendMessage(eventReceiver, eventName, item);
		}
	}
}
