#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class FaceSelectDialogItem : MonoBehaviour
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
			if (item != null)
			{
				UISprite component = item.menuPrefab.GetComponent<UISprite>();
				if ((bool)component)
				{
					GameObject gameObject = base.transform.Find("Image Button/ItemSprite").gameObject;
					UISprite component2 = gameObject.GetComponent<UISprite>();
					component2.atlas = component.atlas;
					component2.spriteName = component.spriteName;
					component2.MakePixelPerfect();
					gameObject.transform.localScale *= iconScale;
				}
				UILabel component3 = base.transform.Find("ItemName").gameObject.GetComponent<UILabel>();
				component3.text = item.itemName;
			}
		}

		private void OnClick()
		{
			Utils.SendMessage(eventReceiver, eventName, item);
		}
	}
}
