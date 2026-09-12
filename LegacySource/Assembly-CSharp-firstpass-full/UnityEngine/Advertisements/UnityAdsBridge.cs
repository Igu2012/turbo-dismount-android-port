using System;

namespace UnityEngine.Advertisements
{
	public class UnityAdsBridge : MonoBehaviour
	{
		private static UnityAdsBridge impl;

		private Action<ShowResult> callback;

		public static UnityAdsBridge GetImpl()
		{
			if (!impl)
			{
				impl = (UnityAdsBridge)Object.FindObjectOfType(typeof(UnityAdsBridge));
			}
			if (!impl)
			{
				GameObject gameObject = new GameObject();
				gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				GameObject gameObject2 = gameObject;
				impl = gameObject2.AddComponent<UnityAdsBridge>();
				gameObject2.name = "UnityAdsPluginBridgeObject";
				Object.DontDestroyOnLoad(gameObject2);
			}
			return impl;
		}

		public static void SetCallback(Action<ShowResult> showCallback)
		{
			GetImpl().callback = showCallback;
		}

		public void Awake()
		{
			if (base.gameObject == GetImpl().gameObject)
			{
				Object.DontDestroyOnLoad(base.gameObject);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}

		public void onUnityAdsReady(string placementId)
		{
			Debug.Log("onUnityAdsReady " + placementId);
		}

		public void onUnityAdsStart(string placementId)
		{
			Debug.Log("onUnityAdsStart " + placementId);
		}

		public void onUnityAdsCompleted(string placementId)
		{
			Debug.Log("onUnityAdsCompleted" + placementId);
			if (callback != null)
			{
				callback(ShowResult.Finished);
			}
		}

		public void onUnityAdsSkipped(string placementId)
		{
			Debug.Log("onUnityAdsSkipped" + placementId);
			if (callback != null)
			{
				callback(ShowResult.Skipped);
			}
		}

		public void onUnityAdsFailed(string placementId)
		{
			Debug.Log("onUnityAdsFailed" + placementId);
			if (callback != null)
			{
				callback(ShowResult.Failed);
			}
		}
	}
}
