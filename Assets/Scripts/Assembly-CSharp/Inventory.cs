#pragma warning disable 0618,0619
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dismount;
using Prime31;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	private enum BillingState
	{
		FAILED = 0,
		INIT = 1,
		SUPPORTED = 2
	}

	private enum RequestState
	{
		FAILED = 0,
		INIT = 1,
		OK = 2
	}

	public Dictionary<string, Dismount.IAPProduct> IAPProducts = new Dictionary<string, Dismount.IAPProduct>();

	public List<GameItem> vehicles = new List<GameItem>();

	public List<GameItem> characters = new List<GameItem>();

	public List<GameItem> obstacles = new List<GameItem>();

	public List<GameItem> levels = new List<GameItem>();

	public List<GameItem> heads = new List<GameItem>();

	public List<GameItem> customLevels = new List<GameItem>();

	private List<GameItem> items = new List<GameItem>();

	[HideInInspector]
	public bool freeWithAnyPurchasesUnlocked;

	private BillingState billingSupported;

	private RequestState requestState;

	private static bool alertShown;

	private UIManager.State stateBeforePurchase;

	private string pendingProductId;

	private void PopulateInventory()
	{
		string[] array = new string[32]
		{
			"level.classic", "level.headon", "level.overpass", "level.chickentwist", "level.spaceprogram", "level.sideon", "level.dragstrip", "level.freeway", "level.underbridge", "level.thelma",
			"level.scaramanga", "level.roundabout", "level.loopdeloop", "level.parking", "level.lanemerger", "level.dmz", "level.froggerer", "level.roundandround", "level.bigaircompo", "level.missingdelivery",
			"level.mallchase", "level.fastlane", "level.monorailincident", "level.knievelsplit", "level.swissbanks", "level.windfarm", "level.figurescraping", "level.chasegetaway", "level.chasecityheights", "level.chasehighwaypatrol",
			"level.chasehornetsnest", "level.chaseroadblockbridge"
		};
		string[] array2 = new string[39]
		{
			"vehicle.splitvan", "vehicle.splitsportscar", "vehicle.splittruck", "vehicle.tricycle", "vehicle.firetruck", "vehicle.shoppingcart", "vehicle.monstervan", "vehicle.limousine", "vehicle.motorcycle", "vehicle.ranger",
			"vehicle.skateboard", "vehicle.tractor", "vehicle.scooter", "vehicle.officechair", "vehicle.minibus", "vehicle.bulldozer", "vehicle.crane", "vehicle.bird", "vehicle.superbike", "vehicle.hearse",
			"vehicle.formula", "vehicle.dunebuggy", "vehicle.tford", "vehicle.ledorean", "vehicle.quad", "vehicle.londonbus", "vehicle.quadfighter", "vehicle.chaser", "vehicle.copcar", "vehicle.agentcar",
			"vehicle.couch", "vehicle.bentley", "vehicle.tank", "vehicle.train", "vehicle.minecart_split", "vehicle.zither", "vehicle.bumper", "vehicle.beachbuggy", "vehicle.ambulance"
		};
		string[] array3 = new string[28]
		{
			"head.emptysquare", "head.snowmanhead", "head.spacehead", "head.valentinehead", "head.bobbyhead", "head.leprechaunhead", "head.bandithead", "head.skullhead", "head.formulahead", "head.wastelanderhead",
			"head.summerhead", "head.tophat", "head.cophat", "head.quadhelmet", "head.futurehead", "head.agenthat", "head.HalloweenMask", "head.turkeyhead", "head.santahead", "head.quencher",
			"head.pilothelmet", "head.tankhead", "head.trainhead", "head.minecarthelmet", "head.zitherhead", "head.propelhat", "head.beachhead", "head.neckhead"
		};
		string[] array4 = new string[8] { "character.mrdismount", "character.msdismount", "character.mrheft", "character.msbumblebee", "character.mrreach", "character.mrstalwart", "character.msdiva", "character.mrego" };
		string[] array5 = new string[23]
		{
			"obstacle.emptysquare", "obstacle.miniramp", "obstacle.quarterpipe", "obstacle.mediumramp", "obstacle.semibigramp", "obstacle.twistleft", "obstacle.twistright", "obstacle.bigramp", "obstacle.turbopad", "obstacle.brakepad",
			"obstacle.roadbump", "obstacle.oilslick", "obstacle.brickwall_lite", "obstacle.megawall", "obstacle.overpass", "obstacle.alternatingramps", "obstacle.buslaneblocker", "obstacle.trafficcones", "obstacle.roadblock", "obstacle.lowblow",
			"obstacle.bowlingpins", "obstacle.minefield", "obstacle.lootbox"
		};
		for (int i = 0; i < array.Length; i++)
		{
			items.Add(FindItem(levels, array[i]));
		}
		for (int j = 0; j < array2.Length; j++)
		{
			items.Add(FindItem(vehicles, array2[j]));
		}
		for (int k = 0; k < array4.Length; k++)
		{
			items.Add(FindItem(characters, array4[k]));
		}
		for (int l = 0; l < array3.Length; l++)
		{
			items.Add(FindItem(heads, array3[l]));
		}
		for (int m = 0; m < array5.Length; m++)
		{
			items.Add(FindItem(obstacles, array5[m]));
		}
	}

	private GameItem FindItem(List<GameItem> list, string id)
	{
		foreach (GameItem item in list)
		{
			if (item.itemId == id)
			{
				return item;
			}
		}
		Debug.LogError("Could not find item when populating inventory: " + id);
		return null;
	}

	private void Awake()
	{
		PopulateInventory();
		foreach (GameItem item in items)
		{
			item.isLocked = false;
		}
	}

	public void Initialize()
	{
		InitIAPProducts();
		LoadPurchases();
	}

	public void SetCustomLevels(List<GameItem> levels)
	{
		customLevels = levels;
	}

	public List<GameItem> GetLockedItemsOfCategory(GameItem.ItemCategory category)
	{
		List<GameItem> itemsOfCategory = GetItemsOfCategory(category);
		List<GameItem> list = new List<GameItem>();
		foreach (GameItem item in itemsOfCategory)
		{
			if (item.isLocked)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<GameItem> GetItemsOfCategory(GameItem.ItemCategory category)
	{
		List<GameItem> list = new List<GameItem>();
		if (category == GameItem.ItemCategory.Level && DismountGame.playerState.browsingCustomLevels)
		{
			foreach (GameItem customLevel in customLevels)
			{
				list.Add(customLevel);
			}
		}
		else
		{
			foreach (GameItem item in items)
			{
				if (item.itemCategory == category)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public GameItem GetItem(string itemId)
	{
		foreach (GameItem item in items)
		{
			if (item.itemId == itemId)
			{
				return item;
			}
		}
		return null;
	}

	public GameItem GetItemByReferenceName(string referenceName)
	{
		foreach (GameItem item in items)
		{
			if (item.referenceName == referenceName)
			{
				return item;
			}
		}
		return null;
	}

	private void VerifyProductItems()
	{
		foreach (Dismount.IAPProduct value in IAPProducts.Values)
		{
			foreach (string itemId in value.itemIds)
			{
				bool flag = false;
				foreach (GameItem item in items)
				{
					if (item.itemId == itemId)
					{
						flag = true;
						item.IAPProductId = value.productId;
						break;
					}
				}
				if (!flag)
				{
					Debug.LogError("Inventory: Item configured for IAP, but not found in inventory items: " + value.productId + " - " + itemId);
				}
			}
		}
	}

	public void AddIAPProduct(string id, params string[] items)
	{
		List<string> list = new List<string>();
		foreach (string text in items)
		{
			list.Add(text);
			GameItem item = GetItem(text);
			if ((bool)item)
			{
				item.isLocked = true;
			}
			else
			{
				Debug.LogError("Could not find IAP product: " + text);
			}
		}
		Dismount.IAPProduct iAPProduct = new Dismount.IAPProduct(id, list);
		IAPProducts.Add(iAPProduct.productId, iAPProduct);
	}

	public void AddPremiumIAPProduct(string id)
	{
		List<string> list = new List<string>();
		foreach (GameItem item in items)
		{
			list.Add(item.itemId);
		}
		Dismount.IAPProduct iAPProduct = new Dismount.IAPProduct(id, list);
		IAPProducts.Add(iAPProduct.productId, iAPProduct);
	}

	private void RequestProductData()
	{
		if (requestState != RequestState.INIT)
		{
			string[] array = new string[IAPProducts.Count];
			IAPProducts.Keys.CopyTo(array, 0);
			requestState = RequestState.INIT;
			IAP.requestProductData(array, array, UpdateStoreCompletionHandler);
		}
	}

	private void billingSupportedEvent()
	{
		Debug.Log("Billing supported");
		billingSupported = BillingState.SUPPORTED;
		StartCoroutine(PollIAPProductData());
	}

	private void billingNotSupportedEvent(string error)
	{
		string text = ((error == null) ? "<null>" : error);
		Debug.Log("Billing not supported: " + text);
		billingSupported = BillingState.FAILED;
		if (!alertShown)
		{
			DismountGame.uiManager.ShowDialog("In-app purchases not available.\nPlease check your primary account settings.", "OK", null, (bool s) =>
			{
			}, 0f, false);
			alertShown = true;
		}
	}

	private void InitializeIAP()
	{
		billingSupported = BillingState.INIT;
		IAP.init("MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA5foq6omDDKfoA7K3ozKMt2hh39IPZMuQWzb7qJUYgywJTL/4blfIs1BXq4hgStQfdCW0pARdSDvwWkZmnlm0V4bY9ptDWHSxTuCqJxIn8eMdqsc7cT5LVvFe0xiJqS+4P8x6TeFlxD6bRTL+vDaIxtjpolV/SCx8uqZU6MKFqukXX75eXzsWKv0Jr3hCAJoQHk+HpIGhutfmbr5NK6cHY0uU+ZXLNcZ4o+vy+8oA8/y4oX+9tNqgCMDJKBkTrrDaNDFBd1z4yMAjxGtrGd2QrMzUOE3UCJoXwmik7iCjJdenj1FRauSucF0cn8XczsSYm5UWcCWWMiztxymF2PyVjwIDAQAB");
	}

	private void InitIAPProducts()
	{
		AddIAPProduct("com.secretexit.turbodismount.vehiclebundle.free", "vehicle.formula", "vehicle.dunebuggy", "vehicle.tford", "vehicle.ledorean", "vehicle.quad");
		AddIAPProduct("com.secretexit.turbodismount.vehiclebundle.vehiclebundle1", "vehicle.tricycle", "vehicle.skateboard", "vehicle.shoppingcart");
		AddIAPProduct("com.secretexit.turbodismount.vehiclebundle.vehiclebundle2", "vehicle.motorcycle", "vehicle.scooter", "vehicle.splitsportscar");
		AddIAPProduct("com.secretexit.turbodismount.vehiclebundle.vehiclebundle3", "vehicle.limousine", "vehicle.minibus", "vehicle.officechair");
		AddIAPProduct("com.secretexit.turbodismount.vehiclebundle.vehiclebundle4", "vehicle.monstervan", "vehicle.tractor", "vehicle.ranger");
		AddIAPProduct("com.secretexit.turbodismount.vehiclebundle.vehiclebundle5", "vehicle.firetruck", "vehicle.bulldozer", "vehicle.crane");
		AddIAPProduct("com.secretexit.turbodismount.levelbundle.levelbundle1", "level.chickentwist", "level.lanemerger", "level.loopdeloop", "level.roundabout");
		AddIAPProduct("com.secretexit.turbodismount.levelbundle.levelbundle2", "level.freeway", "level.spaceprogram", "level.dragstrip", "level.roundandround");
		AddIAPProduct("com.secretexit.turbodismount.levelbundle.levelbundle3", "level.overpass", "level.froggerer", "level.dmz", "level.parking");
		AddIAPProduct("com.secretexit.turbodismount.levelbundle.levelbundle4", "level.fastlane", "level.knievelsplit", "level.monorailincident", "level.swissbanks");
		AddIAPProduct("com.secretexit.turbodismount.levelbundle.levelbundle5", "level.chasehighwaypatrol", "level.chaseroadblockbridge", "level.chasecityheights", "level.chasehornetsnest");
		AddIAPProduct("com.secretexit.turbodismount.characterbundle.characterbundle1", "character.msbumblebee", "character.mrreach", "character.mrstalwart", "character.msdiva");
		AddPremiumIAPProduct("com.secretexit.turbodismount.premium1");
		AddPremiumIAPProduct("com.secretexit.turbodismount.premium2");
		AddPremiumIAPProduct("com.secretexit.turbodismount.premium3");
		AddPremiumIAPProduct("com.secretexit.turbodismount.premium4");
		AddPremiumIAPProduct("com.secretexit.turbodismount.premium5");
		AddPremiumIAPProduct("com.secretexit.turbodismount.premium6");
		GoogleIABManager.billingSupportedEvent += billingSupportedEvent;
		GoogleIABManager.billingNotSupportedEvent += billingNotSupportedEvent;
		GoogleIABManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
		GoogleIABManager.queryInventoryFailedEvent += queryInventoryFailedEvent;
		GoogleIABManager.purchaseCompleteAwaitingVerificationEvent += (string s, string t) =>
		{
			Debug.Log("purchaseCompleteAwaitingVerificationEvent: " + s + "/" + t);
		};
		GoogleIABManager.purchaseSucceededEvent += (GooglePurchase a) =>
		{
		};
		GoogleIABManager.purchaseFailedEvent += (string s, int r) =>
		{
		};
		GoogleIABManager.consumePurchaseSucceededEvent += (GooglePurchase a) =>
		{
		};
		GoogleIABManager.consumePurchaseFailedEvent += (string s) =>
		{
			Debug.Log("consumePurchaseFailedEvent: " + s);
		};
		VerifyProductItems();
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			InitializeIAP();
			StartCoroutine(PollIAPInit());
		}
	}

	public Dismount.IAPProduct GetIAPProduct(string id)
	{
		if (IAPProducts.ContainsKey(id))
		{
			Dismount.IAPProduct iAPProduct = IAPProducts[id];
			return IAPProducts[id];
		}
		return null;
	}

	public List<Dismount.IAPProduct> GetIAPItemsOfCategory(GameItem.ItemCategory category)
	{
		List<Dismount.IAPProduct> list = new List<Dismount.IAPProduct>();
		string text = "com.secretexit.turbodismount.";
		switch (category)
		{
		case GameItem.ItemCategory.Character:
			text += "characterbundle";
			break;
		case GameItem.ItemCategory.Level:
			text += "levelbundle";
			break;
		case GameItem.ItemCategory.Vehicle:
			text += "vehiclebundle";
			break;
		default:
			Debug.LogError("Unknown IAP type!");
			return null;
		}
		foreach (KeyValuePair<string, Dismount.IAPProduct> iAPProduct in IAPProducts)
		{
			Dismount.IAPProduct value = iAPProduct.Value;
			if (value.productId.StartsWith(text) && !CheckItemsUnlocked(value))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public int GetNumberOfLockedIAPs()
	{
		int num = 0;
		foreach (KeyValuePair<string, Dismount.IAPProduct> iAPProduct in IAPProducts)
		{
			if (!iAPProduct.Value.productId.StartsWith("com.secretexit.turbodismount.premium") && !iAPProduct.Value.productId.StartsWith("com.secretexit.turbodismount.character") && !CheckItemsUnlocked(iAPProduct.Value))
			{
				num++;
			}
		}
		return num;
	}

	private void SavePurchases()
	{
		foreach (KeyValuePair<string, Dismount.IAPProduct> iAPProduct in IAPProducts)
		{
			if (CheckItemsUnlocked(iAPProduct.Value))
			{
				Prefs.SetString(iAPProduct.Value.productId, "true");
			}
		}
	}

	private void SavePurchase(string productId, string status)
	{
		Prefs.SetString(productId, status);
	}

	private void LoadPurchases()
	{
		foreach (KeyValuePair<string, Dismount.IAPProduct> iAPProduct in IAPProducts)
		{
			if (Prefs.GetString(iAPProduct.Value.productId, "false") == "true")
			{
				UnlockProductItems(iAPProduct.Value.productId);
			}
		}
	}

	private void PurchaseProduct(Dismount.IAPProduct product)
	{
		pendingProductId = product.productId;
		if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
		{
			if (product.isConsumable)
			{
				IAP.purchaseConsumableProduct(product.productId, PurchaseCompleted);
			}
			else
			{
				IAP.purchaseNonconsumableProduct(product.productId, PurchaseCompleted);
			}
		}
		else
		{
			PurchaseCompleted(true, null);
		}
	}

	public void PurchaseProduct(string productId)
	{
		if (billingSupported == BillingState.SUPPORTED && requestState == RequestState.OK && IAPProducts.ContainsKey(productId))
		{
			stateBeforePurchase = DismountGame.uiManager.CurrentState;
			DismountGame.uiManager.SetBusyLabel("Completing Purchase...");
			DismountGame.uiManager.ChangeState(UIManager.State.Busy);
			PurchaseProduct(IAPProducts[productId]);
		}
	}

	public void RestorePurchases()
	{
		RequestProductData();
	}

	private void queryInventorySucceededEvent(List<GooglePurchase> a, List<GoogleSkuInfo> b)
	{
		requestState = RequestState.OK;
	}

	private void queryInventoryFailedEvent(string error)
	{
		string text = ((error == null) ? "<null>" : error);
		Debug.Log("queryInventoryFailed: " + text);
		requestState = RequestState.FAILED;
		if (!alertShown)
		{
			DismountGame.uiManager.ShowDialog("In-app purchase store listing not available.\n\nPlease connect to the internet.", "OK", null, (bool s) =>
			{
			}, 0f, false);
			alertShown = true;
		}
	}

	private IEnumerator PollIAPInit()
	{
		while (true)
		{
			Debug.Log("Poll IAP Init");
			if (billingSupported == BillingState.FAILED)
			{
				InitializeIAP();
			}
			else if (billingSupported == BillingState.SUPPORTED)
			{
				break;
			}
			yield return new WaitForSeconds(3f);
		}
	}

	private IEnumerator PollIAPProductData()
	{
		while (true)
		{
			Debug.Log("Poll IAP Product Data");
			if (requestState == RequestState.FAILED)
			{
				RequestProductData();
			}
			else if (requestState == RequestState.OK)
			{
				break;
			}
			yield return new WaitForSeconds(3f);
		}
	}

	private void RestorePurchasesCompletionHandler(string productId)
	{
		DismountGame.uiManager.ChangeState(stateBeforePurchase);
		if (productId != null)
		{
			UnlockProductItems(productId);
		}
	}

	private bool CheckItemsUnlocked(Dismount.IAPProduct product)
	{
		if (product.isConsumable)
		{
			return false;
		}
		foreach (string itemId in product.itemIds)
		{
			foreach (GameItem item in items)
			{
				if (item.itemId == itemId && item.isLocked)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void UnlockFreeWithAnyPurchaseItems()
	{
		UnlockProductItems("com.secretexit.turbodismount.characterbundle.characterbundle1");
		UnlockProductItems("com.secretexit.turbodismount.vehiclebundle.free");
	}

	private void UnlockProductItems(string productId, int quantity = 1)
	{
		if (!IAPProducts.ContainsKey(productId))
		{
			return;
		}
		Dismount.IAPProduct iAPProduct = IAPProducts[productId];
		if (iAPProduct == null)
		{
			return;
		}
		if (!freeWithAnyPurchasesUnlocked)
		{
			freeWithAnyPurchasesUnlocked = true;
			UnlockFreeWithAnyPurchaseItems();
			DismountGame.instance.ShowFreeWithAnyPurchasesUnlocked();
		}
		foreach (string itemId in iAPProduct.itemIds)
		{
			foreach (GameItem item in items)
			{
				if (item.itemId == itemId)
				{
					item.isLocked = false;
				}
			}
		}
		SavePurchase(productId, "true");
		DismountGame.instance.PopulateItemDialogs();
	}

	private void LockProduct(string productId)
	{
		if (IAPProducts.ContainsKey(productId))
		{
			SavePurchase(productId, "false");
		}
	}

	public void UpdateStoreCompletionHandler(List<IAPProduct> iapItems)
	{
		bool flag = false;
		if (iapItems == null)
		{
			return;
		}
		foreach (IAPProduct iapItem in iapItems)
		{
			IAPStoreInfo iAPStoreInfo = new IAPStoreInfo();
			iAPStoreInfo.name = iapItem.title;
			iAPStoreInfo.description = iapItem.description;
			iAPStoreInfo.price = Regex.Replace(iapItem.price, "[^0-9., ]", string.Empty).Trim();
			iAPStoreInfo.currency = iapItem.currencyCode;
			Dismount.IAPProduct iAPProduct = IAPProducts[iapItem.productId];
			if (iAPProduct != null)
			{
				flag = true;
				iAPProduct.storeInfo = iAPStoreInfo;
			}
			LockProduct(iapItem.productId);
		}
		if (flag)
		{
			DismountGame.instance.PopulateItemDialogs();
		}
		foreach (GooglePurchase androidPurchasedItem in IAP.androidPurchasedItems)
		{
			if (androidPurchasedItem.productId != null && androidPurchasedItem.purchaseState == GooglePurchase.GooglePurchaseState.Purchased)
			{
				UnlockProductItems(androidPurchasedItem.productId);
			}
		}
	}

	private void PurchaseCompleted(bool success, string error)
	{
		if (success && pendingProductId.StartsWith("com.secretexit.turbodismount.premium"))
		{
			Prefs.SetInt("NewContentPosterShown", 1);
			Prefs.Save();
		}
		DismountGame.uiManager.ChangeState(stateBeforePurchase);
		if (success && pendingProductId != null)
		{
			UnlockProductItems(pendingProductId);
		}
		else if (!success)
		{
			if (error.ToLower().Contains("already own"))
			{
				UnlockProductItems(pendingProductId);
				RequestProductData();
			}
			Debug.Log("PurchaseCompleted: " + error);
		}
		pendingProductId = null;
	}

	public bool HasPremiumBeenPurchased()
	{
		for (int i = 1; i <= 30; i++)
		{
			if (Prefs.GetString("com.secretexit.turbodismount.premium" + i, "false") == "true")
			{
				return true;
			}
		}
		return false;
	}
}
