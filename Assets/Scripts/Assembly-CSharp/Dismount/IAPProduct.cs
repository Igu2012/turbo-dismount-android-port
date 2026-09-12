#pragma warning disable 0618,0619
using System.Collections.Generic;

namespace Dismount
{
	public class IAPProduct
	{
		public IAPStoreInfo storeInfo;

		public string productId = string.Empty;

		public List<string> itemIds = new List<string>();

		public bool isConsumable;

		public int consumableValue;

		public string title => storeInfo != null ? storeInfo.name : string.Empty;

		public string description => storeInfo != null ? storeInfo.description : string.Empty;

		public string price => storeInfo != null ? storeInfo.price : string.Empty;

		public string currencyCode => storeInfo != null ? storeInfo.currency : string.Empty;

		public IAPProduct(string productId, List<string> itemIds, bool isConsumable = false, int consumableValue = 0)
		{
			this.productId = productId;
			this.itemIds.AddRange(itemIds);
			this.isConsumable = isConsumable;
			this.consumableValue = consumableValue;
		}

		public void AddItems(List<string> itemIds)
		{
			this.itemIds.AddRange(itemIds);
		}
	}
}
