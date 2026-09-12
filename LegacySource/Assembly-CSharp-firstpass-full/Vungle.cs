using System;
using System.Collections.Generic;
using System.Text;

public class Vungle
{
	public enum Consent
	{
		Undefined = 0,
		Accepted = 1,
		Denied = 2
	}

	private const string PLUGIN_VERSION = "6.2.0";

	private const string IOS_SDK_VERSION = "6.2.0";

	private const string WIN_SDK_VERSION = "6.2.0";

	private const string ANDROID_SDK_VERSION = "6.2.5";

	public static string VersionInfo
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder("unity-");
			return stringBuilder.Append("6.2.0").Append("/android-").Append("6.2.5")
				.ToString();
		}
	}

	public static event Action<string> onAdStartedEvent;

	public static event Action<string, AdFinishedEventArgs> onAdFinishedEvent;

	public static event Action<string, bool> adPlayableEvent;

	public static event Action onInitializeEvent;

	public static event Action<string> onLogEvent;

	public static event Action<string, string> onPlacementPreparedEvent;

	public static event Action<string, string> onVungleCreativeEvent;

	static Vungle()
	{
		VungleManager.OnAdStartEvent += adStarted;
		VungleManager.OnAdFinishedEvent += adFinished;
		VungleManager.OnAdPlayableEvent += adPlayable;
		VungleManager.OnSDKLogEvent += onLog;
		VungleManager.OnSDKInitializeEvent += onInitialize;
		VungleManager.OnPlacementPreparedEvent += onPlacementPrepared;
		VungleManager.OnVungleCreativeEvent += onVungleCreative;
	}

	private static void adStarted(string placementID)
	{
		if (onAdStartedEvent != null)
		{
			onAdStartedEvent(placementID);
		}
	}

	private static void adPlayable(string placementID, bool playable)
	{
		if (adPlayableEvent != null)
		{
			adPlayableEvent(placementID, playable);
		}
	}

	private static void onLog(string log)
	{
		if (onLogEvent != null)
		{
			onLogEvent(log);
		}
	}

	private static void onPlacementPrepared(string placementID, string bidToken)
	{
		if (onPlacementPreparedEvent != null)
		{
			onPlacementPreparedEvent(placementID, bidToken);
		}
	}

	private static void onVungleCreative(string placementID, string creativeID)
	{
		if (onVungleCreativeEvent != null)
		{
			onVungleCreativeEvent(placementID, creativeID);
		}
	}

	private static void adFinished(string placementID, AdFinishedEventArgs args)
	{
		if (onAdFinishedEvent != null)
		{
			onAdFinishedEvent(placementID, args);
		}
	}

	private static void onInitialize()
	{
		if (onInitializeEvent != null)
		{
			onInitializeEvent();
		}
	}

	public static void init(string appId, string[] placements)
	{
		VungleAndroid.init(appId, placements, "6.2.0");
	}

	public static void init(string appId, string[] placements, bool initHeaderBiddingDelegate)
	{
		VungleAndroid.init(appId, placements, "6.2.0");
	}

	public static void setSoundEnabled(bool isEnabled)
	{
		VungleAndroid.setSoundEnabled(isEnabled);
	}

	public static bool isAdvertAvailable(string placementID)
	{
		return VungleAndroid.isVideoAvailable(placementID);
	}

	public static void loadAd(string placementID)
	{
		VungleAndroid.loadAd(placementID);
	}

	public static bool closeAd(string placementID)
	{
		return VungleAndroid.closeAd(placementID);
	}

	public static void playAd(string placementID)
	{
		VungleAndroid.playAd(placementID);
	}

	public static void playAd(Dictionary<string, object> options, string placementID)
	{
		if (options == null)
		{
			options = new Dictionary<string, object>();
		}
		VungleAndroid.playAd(options, placementID);
	}

	public static void updateConsentStatus(Consent consent)
	{
		VungleAndroid.updateConsentStatus(consent);
	}

	public static Consent getConsentStatus()
	{
		return VungleAndroid.getConsentStatus();
	}

	public static void clearSleep()
	{
	}

	public static void setEndPoint(string endPoint)
	{
	}

	public static void setLogEnable(bool enable)
	{
	}

	public static string getEndPoint()
	{
		return string.Empty;
	}

	public static void onResume()
	{
		VungleAndroid.onResume();
	}

	public static void onPause()
	{
		VungleAndroid.onPause();
	}
}
