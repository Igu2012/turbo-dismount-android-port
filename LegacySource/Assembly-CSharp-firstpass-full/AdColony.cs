using System;
using UnityEngine;

public class AdColony : MonoBehaviour
{
	public delegate void VideoStartedDelegate();

	public delegate void VideoFinishedDelegate(bool ad_shown);

	public delegate void VideoFinishedWithInfoDelegate(AdColonyAd ad_shown);

	public delegate void V4VCResultDelegate(bool success, string name, int amount);

	public delegate void AdAvailabilityChangeDelegate(bool available, string zone_id);

	private static AdColony instance;

	public static string version = "2.1.4.1";

	public static VideoStartedDelegate OnVideoStarted;

	public static VideoFinishedDelegate OnVideoFinished;

	public static VideoFinishedWithInfoDelegate OnVideoFinishedWithInfo;

	public static V4VCResultDelegate OnV4VCResult;

	public static AdAvailabilityChangeDelegate OnAdAvailabilityChange;

	private static bool configured;

	private static bool adr_initialized = false;

	private static AndroidJavaClass class_UnityPlayer;

	private static IntPtr class_UnityADC = IntPtr.Zero;

	private static IntPtr method_configure = IntPtr.Zero;

	private static IntPtr method_setCustomID = IntPtr.Zero;

	private static IntPtr method_getCustomID = IntPtr.Zero;

	private static IntPtr method_isVideoAvailable = IntPtr.Zero;

	private static IntPtr method_isV4VCAvailable = IntPtr.Zero;

	private static IntPtr method_getDeviceID = IntPtr.Zero;

	private static IntPtr method_getV4VCAmount = IntPtr.Zero;

	private static IntPtr method_getV4VCName = IntPtr.Zero;

	private static IntPtr method_showVideo = IntPtr.Zero;

	private static IntPtr method_showV4VC = IntPtr.Zero;

	private static IntPtr method_offerV4VC = IntPtr.Zero;

	private static IntPtr method_statusForZone = IntPtr.Zero;

	private static IntPtr method_getAvailableViews = IntPtr.Zero;

	private static IntPtr method_notifyIAPComplete = IntPtr.Zero;

	private static void ensureInstance()
	{
		if (instance == null)
		{
			instance = UnityEngine.Object.FindObjectOfType(typeof(AdColony)) as AdColony;
			if (instance == null)
			{
				instance = new GameObject("AdColony").AddComponent<AdColony>();
			}
		}
	}

	private void Awake()
	{
		base.name = "AdColony";
		UnityEngine.Object.DontDestroyOnLoad(base.transform.gameObject);
	}

	public void OnAdColonyVideoStarted(string args)
	{
		if (OnVideoStarted != null)
		{
			OnVideoStarted();
		}
	}

	public void OnAdColonyVideoFinished(string args)
	{
		string[] array = args.Split('|');
		if (OnVideoFinished != null)
		{
			OnVideoFinished(array[0].Equals("true"));
		}
		if (OnVideoFinishedWithInfo != null)
		{
			OnVideoFinishedWithInfo(new AdColonyAd(array));
		}
	}

	public void OnAdColonyV4VCResult(string args)
	{
		if (OnV4VCResult != null)
		{
			string[] array = args.Split('|');
			bool success = array[0].Equals("true");
			int amount = int.Parse(array[1]);
			string text = array[2];
			OnV4VCResult(success, text, amount);
		}
	}

	public void OnAdColonyAdAvailabilityChange(string args)
	{
		if (OnAdAvailabilityChange != null)
		{
			string[] array = args.Split('|');
			OnAdAvailabilityChange(array[0].Equals("true"), array[1]);
		}
	}

	public static void Configure(string app_version, string app_id, params string[] zone_ids)
	{
		if (!configured)
		{
			ensureInstance();
			AndroidConfigure(app_version, app_id, zone_ids);
		}
	}

	private static void AndroidInitializePlugin()
	{
		bool flag = true;
		IntPtr intPtr = AndroidJNI.FindClass("com/jirbo/unityadc/UnityADC");
		if (intPtr != IntPtr.Zero)
		{
			class_UnityADC = AndroidJNI.NewGlobalRef(intPtr);
			AndroidJNI.DeleteLocalRef(intPtr);
			IntPtr intPtr2 = AndroidJNI.FindClass("com/jirbo/adcolony/AdColony");
			if (intPtr2 != IntPtr.Zero)
			{
				AndroidJNI.DeleteLocalRef(intPtr2);
			}
			else
			{
				flag = false;
			}
		}
		else
		{
			flag = false;
		}
		if (flag)
		{
			class_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			method_configure = AndroidJNI.GetStaticMethodID(class_UnityADC, "configure", "(Landroid/app/Activity;Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;)V");
			method_setCustomID = AndroidJNI.GetStaticMethodID(class_UnityADC, "setCustomID", "(Ljava/lang/String;)V");
			method_getCustomID = AndroidJNI.GetStaticMethodID(class_UnityADC, "getCustomID", "()Ljava/lang/String;");
			method_isVideoAvailable = AndroidJNI.GetStaticMethodID(class_UnityADC, "isVideoAvailable", "(Ljava/lang/String;)Z");
			method_isV4VCAvailable = AndroidJNI.GetStaticMethodID(class_UnityADC, "isV4VCAvailable", "(Ljava/lang/String;)Z");
			method_getDeviceID = AndroidJNI.GetStaticMethodID(class_UnityADC, "getDeviceID", "()Ljava/lang/String;");
			method_getV4VCAmount = AndroidJNI.GetStaticMethodID(class_UnityADC, "getV4VCAmount", "(Ljava/lang/String;)I");
			method_getV4VCName = AndroidJNI.GetStaticMethodID(class_UnityADC, "getV4VCName", "(Ljava/lang/String;)Ljava/lang/String;");
			method_showVideo = AndroidJNI.GetStaticMethodID(class_UnityADC, "showVideo", "(Ljava/lang/String;)Z");
			method_showV4VC = AndroidJNI.GetStaticMethodID(class_UnityADC, "showV4VC", "(ZLjava/lang/String;)Z");
			method_offerV4VC = AndroidJNI.GetStaticMethodID(class_UnityADC, "offerV4VC", "(ZLjava/lang/String;)V");
			method_statusForZone = AndroidJNI.GetStaticMethodID(class_UnityADC, "statusForZone", "(Ljava/lang/String;)Ljava/lang/String;");
			method_getAvailableViews = AndroidJNI.GetStaticMethodID(class_UnityADC, "getAvailableViews", "(Ljava/lang/String;)I");
			method_notifyIAPComplete = AndroidJNI.GetStaticMethodID(class_UnityADC, "notifyIAPComplete", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;D)V");
			adr_initialized = true;
		}
		else
		{
			Debug.LogError("AdColony configuration error - make sure adcolony.jar and unityadc.jar libraries are in your Unity project's Assets/Plugins/Android folder.");
		}
	}

	private static void AndroidConfigure(string app_version, string app_id, string[] zone_ids)
	{
		if (!adr_initialized)
		{
			AndroidInitializePlugin();
		}
		class_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject androidJavaObject = class_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		IntPtr l = AndroidJNI.NewStringUTF(app_version);
		IntPtr l2 = AndroidJNI.NewStringUTF(app_id);
		IntPtr l3 = AndroidJNIHelper.ConvertToJNIArray(zone_ids);
		jvalue[] array = new jvalue[4];
		array[0].l = androidJavaObject.GetRawObject();
		array[1].l = l;
		array[2].l = l2;
		array[3].l = l3;
		AndroidJNI.CallStaticVoidMethod(class_UnityADC, method_configure, array);
		configured = true;
	}

	public static void SetCustomID(string custom_id)
	{
		if (!adr_initialized)
		{
			AndroidInitializePlugin();
		}
		jvalue[] array = new jvalue[1];
		array[0].l = AndroidJNI.NewStringUTF(custom_id);
		AndroidJNI.CallStaticVoidMethod(class_UnityADC, method_setCustomID, array);
	}

	public static string GetCustomID()
	{
		jvalue[] args = new jvalue[0];
		return AndroidJNI.CallStaticStringMethod(class_UnityADC, method_getCustomID, args);
	}

	public static bool IsVideoAvailable(string zone_id)
	{
		jvalue[] array = new jvalue[1];
		array[0].l = AndroidJNI.NewStringUTF(zone_id);
		return AndroidJNI.CallStaticBooleanMethod(class_UnityADC, method_isVideoAvailable, array);
	}

	public static bool IsV4VCAvailable(string zone_id)
	{
		jvalue[] array = new jvalue[1];
		array[0].l = AndroidJNI.NewStringUTF(zone_id);
		return AndroidJNI.CallStaticBooleanMethod(class_UnityADC, method_isV4VCAvailable, array);
	}

	public static string GetDeviceID()
	{
		jvalue[] args = new jvalue[0];
		return AndroidJNI.CallStaticStringMethod(class_UnityADC, method_getDeviceID, args);
	}

	public static string AndroidGetOpenUDID()
	{
		return "undefined";
	}

	public static int GetV4VCAmount(string zone_id)
	{
		jvalue[] array = new jvalue[1];
		array[0].l = AndroidJNI.NewStringUTF(zone_id);
		return AndroidJNI.CallStaticIntMethod(class_UnityADC, method_getV4VCAmount, array);
	}

	public static string GetV4VCName(string zone_id)
	{
		jvalue[] array = new jvalue[1];
		array[0].l = AndroidJNI.NewStringUTF(zone_id);
		return AndroidJNI.CallStaticStringMethod(class_UnityADC, method_getV4VCName, array);
	}

	public static bool ShowVideoAd(string zone_id)
	{
		jvalue[] array = new jvalue[1];
		array[0].l = AndroidJNI.NewStringUTF(zone_id);
		AndroidJNI.CallStaticBooleanMethod(class_UnityADC, method_showVideo, array);
		return true;
	}

	public static bool ShowV4VC(bool popup_result, string zone_id)
	{
		jvalue[] array = new jvalue[2];
		array[0].z = popup_result;
		array[1].l = AndroidJNI.NewStringUTF(zone_id);
		AndroidJNI.CallStaticBooleanMethod(class_UnityADC, method_showV4VC, array);
		return true;
	}

	public static void OfferV4VC(bool popup_result, string zone_id)
	{
		jvalue[] array = new jvalue[2];
		array[0].z = popup_result;
		array[1].l = AndroidJNI.NewStringUTF(zone_id);
		AndroidJNI.CallStaticVoidMethod(class_UnityADC, method_offerV4VC, array);
	}

	public static string StatusForZone(string zone_id)
	{
		jvalue[] array = new jvalue[1];
		array[0].l = AndroidJNI.NewStringUTF(zone_id);
		return AndroidJNI.CallStaticStringMethod(class_UnityADC, method_statusForZone, array);
	}

	public static int GetAvailableViews(string zone_id)
	{
		jvalue[] array = new jvalue[1];
		array[0].l = AndroidJNI.NewStringUTF(zone_id);
		return AndroidJNI.CallStaticIntMethod(class_UnityADC, method_getAvailableViews, array);
	}

	public static void NotifyIAPComplete(string zone_id, string trans_id, string currency_code, double price, int quantity)
	{
		jvalue[] array = new jvalue[4];
		array[0].l = AndroidJNI.NewStringUTF(zone_id);
		array[1].l = AndroidJNI.NewStringUTF(trans_id);
		array[2].l = AndroidJNI.NewStringUTF(currency_code);
		array[3].d = price;
		AndroidJNI.CallStaticVoidMethod(class_UnityADC, method_notifyIAPComplete, array);
	}
}
