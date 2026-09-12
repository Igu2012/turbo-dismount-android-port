using System;
using System.Collections.Generic;

namespace UnityEngine.Advertisements
{
	internal sealed class AndroidPlatform : IPlatform
	{
		private static AndroidJavaObject wrapper;

		private static bool wrapperInitialized;

		public bool isInitialized
		{
			get
			{
				return getAndroidWrapper().Call<bool>("isInitialized", new object[0]);
			}
		}

		public bool isSupported
		{
			get
			{
				return getAndroidWrapper().Call<bool>("isSupported", new object[0]);
			}
		}

		public string version
		{
			get
			{
				return getAndroidWrapper().Call<string>("getVersion", new object[0]);
			}
		}

		public bool debugMode
		{
			get
			{
				return getAndroidWrapper().Call<bool>("getDebugMode", new object[0]);
			}
			set
			{
				getAndroidWrapper().Call("setDebugMode", value);
			}
		}

		private AndroidJavaObject getAndroidWrapper()
		{
			if (!wrapperInitialized)
			{
				wrapperInitialized = true;
				wrapper = new AndroidJavaObject("com.unity3d.ads.unity4.unity4wrapper.UnityAdsUnity4Wrapper");
			}
			return wrapper;
		}

		private AndroidJavaObject getCurrentActivity()
		{
			return new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
		}

		public void Initialize(string gameId, bool testMode)
		{
			getAndroidWrapper().Call("initialize", getCurrentActivity(), gameId, testMode, UnityAdsBridge.GetImpl().gameObject.name);
		}

		public bool IsReady(string placementId)
		{
			if (placementId == null)
			{
				return getAndroidWrapper().Call<bool>("isReady", new object[0]);
			}
			return getAndroidWrapper().Call<bool>("isReady", new object[1] { placementId });
		}

		public PlacementState GetPlacementState(string placementId)
		{
			AndroidJavaObject androidJavaObject = ((placementId != null) ? getAndroidWrapper().Call<AndroidJavaObject>("getPlacementState", new object[1] { placementId }) : getAndroidWrapper().Call<AndroidJavaObject>("getPlacementState", new object[0]));
			return (PlacementState)androidJavaObject.Call<int>("ordinal", new object[0]);
		}

		public void Show(string placementId, Action<ShowResult> callback)
		{
			UnityAdsBridge.SetCallback(callback);
			if (placementId != null)
			{
				getAndroidWrapper().Call("show", getCurrentActivity(), placementId);
			}
			else
			{
				getAndroidWrapper().Call("show", getCurrentActivity());
			}
		}

		public void SetMetaData(MetaData metaData)
		{
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("com.unity3d.ads.metadata.MetaData", getCurrentActivity());
			androidJavaObject.Call("setCategory", metaData.category);
			foreach (KeyValuePair<string, object> item in metaData.Values())
			{
				androidJavaObject.Call<bool>("set", new object[2] { item.Key, item.Value });
			}
			androidJavaObject.Call("commit");
		}
	}
}
