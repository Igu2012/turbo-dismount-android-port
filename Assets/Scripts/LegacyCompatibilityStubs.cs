using System;
using System.Collections.Generic;
using UnityEngine;

public class BlurEffect : MonoBehaviour { }

public class InteractiveCloth : MonoBehaviour
{
    public Vector3[] vertices = Array.Empty<Vector3>();
    public Vector3 externalAcceleration;
}

public static class Everyplay
{
    public static event Action RecordingStarted;
    public static event Action RecordingStopped;
    public static bool IsRecordingSupported() => false;
    public static bool IsRecording() => false;
    public static bool IsPaused() => false;
    public static void StartRecording() { }
    public static void StopRecording() { }
    public static void PauseRecording() { }
    public static void ResumeRecording() { }
    public static void PlayLastRecording() { }
    public static void ShowSharingModal() { }
}

public static class EtceteraAndroid
{
    public static void openReviewPageInPlayStore() { }
    public static void openReviewPageInPlayStore(string packageName) { }
}

namespace Prime31
{
    public class GooglePurchase
    {
        public enum GooglePurchaseState { Purchased = 0, Canceled = 1, Refunded = 2 }
        public string productId;
        public string developerPayload;
        public GooglePurchaseState purchaseState;
    }

    public class GoogleSkuInfo
    {
        public string productId;
        public string price;
        public string title;
        public string description;
    }

    public static class GoogleIABManager
    {
        public static event Action billingSupportedEvent;
        public static event Action<string> billingNotSupportedEvent;
        public static event Action<List<GooglePurchase>, List<GoogleSkuInfo>> queryInventorySucceededEvent;
        public static event Action<string> queryInventoryFailedEvent;
        public static event Action<string, string> purchaseCompleteAwaitingVerificationEvent;
        public static event Action<GooglePurchase> purchaseSucceededEvent;
        public static event Action<string, int> purchaseFailedEvent;
        public static event Action<GooglePurchase> consumePurchaseSucceededEvent;
        public static event Action<string> consumePurchaseFailedEvent;
    }

    public static class GoogleIAB
    {
        public static void init(string publicKey) { }
        public static void queryInventory(string[] skus) { }
        public static void purchaseProduct(string productId, string payload) { }
        public static void consumeProduct(string productId) { }
    }
}

public static class IAP
{
    public static List<Prime31.GooglePurchase> androidPurchasedItems = new List<Prime31.GooglePurchase>();
    public static void init(string androidPublicKey) { }
    public static void requestProductData(string[] ios, string[] android, Action<List<Dismount.IAPProduct>> callback) => callback?.Invoke(new List<Dismount.IAPProduct>());
    public static void purchaseConsumableProduct(string id, Action<bool, string> callback) => callback?.Invoke(false, "In-app purchases unavailable in this build");
    public static void purchaseNonconsumableProduct(string id, Action<bool, string> callback) => callback?.Invoke(false, "In-app purchases unavailable in this build");
    public static void restoreCompletedTransactions(Action<string> callback) => callback?.Invoke(null);
}
