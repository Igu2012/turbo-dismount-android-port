#pragma warning disable 0618,0619
using System;
using UnityEngine;

public class SXJNI
{
	private class RequestPermissionsResultCallback : AndroidJavaProxy
	{
		private readonly Action<string[], bool[]> mCallback;

		public RequestPermissionsResultCallback(Action<string[], bool[]> callback)
			: base("com.secretexit.SXPermissionQuery$RequestPermissionsResultCallback$Interface")
		{
			mCallback = callback;
		}

		public void onRequestPermissionsResultCallback(AndroidJavaObject jo)
		{
			AndroidJavaObject androidJavaObject = jo.Get<AndroidJavaObject>("permissions");
			AndroidJavaObject androidJavaObject2 = jo.Get<AndroidJavaObject>("grantResults");
			string[] permissions = AndroidJNIHelper.ConvertFromJNIArray<string[]>(androidJavaObject.GetRawObject());
			bool[] grantResults = AndroidJNIHelper.ConvertFromJNIArray<bool[]>(androidJavaObject2.GetRawObject());
			UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				mCallback(permissions, grantResults);
			});
		}
	}

	private class IsSignedInCallback : AndroidJavaProxy
	{
		private readonly Action<bool, string> mCallback;

		public IsSignedInCallback(Action<bool, string> callback)
			: base("com.secretexit.SXGooglePlayGames$IsSignedInCallback$Interface")
		{
			mCallback = callback;
		}

		public void result(bool signedIn, string displayName)
		{
			UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				mCallback(signedIn, displayName);
			});
		}
	}

	private class SignInCallback : AndroidJavaProxy
	{
		private readonly Action<bool, string, string> mCallback;

		public SignInCallback(Action<bool, string, string> callback)
			: base("com.secretexit.SXGooglePlayGames$SignInCallback$Interface")
		{
			mCallback = callback;
		}

		public void result(bool signedIn, string id, string displayName)
		{
			UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				mCallback(signedIn, id, displayName);
			});
		}
	}

	private class UnlockAchievementCallback : AndroidJavaProxy
	{
		private readonly Action<bool> mCallback;

		public UnlockAchievementCallback(Action<bool> callback)
			: base("com.secretexit.SXGooglePlayGames$UnlockAchievementCallback$Interface")
		{
			mCallback = callback;
		}

		public void result(bool unlocked)
		{
			UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				mCallback(unlocked);
			});
		}
	}

	private class GetScoreCallback : AndroidJavaProxy
	{
		private readonly Action<long> mCallback;

		public GetScoreCallback(Action<long> callback)
			: base("com.secretexit.SXGooglePlayGames$GetScoreCallback$Interface")
		{
			mCallback = callback;
		}

		public void result(AndroidJavaObject jo)
		{
			long score = ((jo == null) ? (-1) : jo.Get<long>("score"));
			UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				mCallback(score);
			});
		}
	}

	private class GetLeaderboardDataCallback : AndroidJavaProxy
	{
		private readonly Action<long[], string[], long[]> mCallback;

		public GetLeaderboardDataCallback(Action<long[], string[], long[]> callback)
			: base("com.secretexit.SXGooglePlayGames$GetLeaderboardDataCallback$Interface")
		{
			mCallback = callback;
		}

		public void result(AndroidJavaObject jo)
		{
			AndroidJavaObject androidJavaObject = jo.Get<AndroidJavaObject>("ranks");
			AndroidJavaObject androidJavaObject2 = jo.Get<AndroidJavaObject>("names");
			AndroidJavaObject androidJavaObject3 = jo.Get<AndroidJavaObject>("scores");
			long[] ranks = AndroidJNIHelper.ConvertFromJNIArray<long[]>(androidJavaObject.GetRawObject());
			string[] names = AndroidJNIHelper.ConvertFromJNIArray<string[]>(androidJavaObject2.GetRawObject());
			long[] scores = AndroidJNIHelper.ConvertFromJNIArray<long[]>(androidJavaObject3.GetRawObject());
			UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				mCallback(ranks, names, scores);
			});
		}
	}

	private class SubmitScoreCallback : AndroidJavaProxy
	{
		private readonly Action<bool> mCallback;

		public SubmitScoreCallback(Action<bool> callback)
			: base("com.secretexit.SXGooglePlayGames$SubmitScoreCallback$Interface")
		{
			mCallback = callback;
		}

		public void result(bool success)
		{
			UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				mCallback(success);
			});
		}
	}

	private class ShowAchievementsCallback : AndroidJavaProxy
	{
		private readonly Action<bool> mCallback;

		public ShowAchievementsCallback(Action<bool> callback)
			: base("com.secretexit.SXGooglePlayGames$ShowAchievementsCallback$Interface")
		{
			mCallback = callback;
		}

		public void result(bool success)
		{
			UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				mCallback(success);
			});
		}
	}

	private class ShowLeaderboardCallback : AndroidJavaProxy
	{
		private readonly Action<bool> mCallback;

		public ShowLeaderboardCallback(Action<bool> callback)
			: base("com.secretexit.SXGooglePlayGames$ShowLeaderboardCallback$Interface")
		{
			mCallback = callback;
		}

		public void result(bool success)
		{
			UnityThreadHelper.Dispatcher.Dispatch(() =>
			{
				mCallback(success);
			});
		}
	}

	public enum TimeSpan
	{
		DAILY = 0,
		WEEKLY = 1,
		ALL = 2
	}

	public const string READ_EXTERNAL_STORAGE = "android.permission.READ_EXTERNAL_STORAGE";

	public const string WRITE_EXTERNAL_STORAGE = "android.permission.WRITE_EXTERNAL_STORAGE";

	private static SXJNI _instance;

	private AndroidJavaClass ActivityClass;

	private AndroidJavaClass PermissionQueryClass;

	private AndroidJavaClass GooglePlayGames;

	public static SXJNI Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new SXJNI();
			}
			return _instance;
		}
	}

	private SXJNI()
	{
		AndroidJNI.AttachCurrentThread();
		ActivityClass = new AndroidJavaClass("com.secretexit.SXActivity");
		PermissionQueryClass = new AndroidJavaClass("com.secretexit.SXPermissionQuery");
		GooglePlayGames = new AndroidJavaClass("com.secretexit.SXGooglePlayGames");
		UnityThreadHelper.EnsureHelper();
	}

	public string GetLanguage()
	{
		return ActivityClass.CallStatic<string>("getLanguage", new object[0]);
	}

	public void RequestPermissions(string[] permissions, Action<string[], bool[]> callback)
	{
		PermissionQueryClass.CallStatic("requestPermissions", permissions, new RequestPermissionsResultCallback(callback));
	}

	public bool CheckPermission(string permission)
	{
		return PermissionQueryClass.CallStatic<bool>("checkPermission", new object[1] { permission });
	}

	public bool IsAutoLoginEnabled()
	{
		return GooglePlayGames.CallStatic<bool>("isAutoLoginEnabled", new object[0]);
	}

	public bool IsSignedIn()
	{
		return GooglePlayGames.CallStatic<bool>("isSignedIn", new object[0]);
	}

	public void IsSignedInAsync(Action<bool, string> callback)
	{
		GooglePlayGames.CallStatic("isSignedInAsync", new IsSignedInCallback(callback));
	}

	public void SetPersistentSignInCallback(Action<bool, string, string> callback)
	{
		GooglePlayGames.CallStatic("setPersistentSignInCallback", new SignInCallback(callback));
	}

	public void SignIn(bool attemptAutoLogin, bool implicitLogin, Action<bool, string, string> callback)
	{
		GooglePlayGames.CallStatic("signIn", attemptAutoLogin, implicitLogin, new SignInCallback(callback));
	}

	public void SignOut(bool setAutoLoginStatus, bool autoLoginStatus)
	{
		GooglePlayGames.CallStatic("signOut", setAutoLoginStatus, autoLoginStatus);
	}

	public void Unlock(string id, Action<bool> callback)
	{
		GooglePlayGames.CallStatic("unlock", id, new UnlockAchievementCallback(callback));
	}

	public void Increment(string id, int n, Action<bool> callback)
	{
		GooglePlayGames.CallStatic("increment", id, n, new UnlockAchievementCallback(callback));
	}

	public void ShowAchievements(Action<bool> callback)
	{
		GooglePlayGames.CallStatic("showAchievements", new ShowAchievementsCallback(callback));
	}

	public void ShowLeaderboard(string id, TimeSpan timeSpan, Action<bool> callback)
	{
		GooglePlayGames.CallStatic("showLeaderboard", id, (int)timeSpan, new ShowLeaderboardCallback(callback));
	}

	public void GetScore(string id, TimeSpan timeSpan, Action<long> callback)
	{
		GooglePlayGames.CallStatic("getScore", id, (int)timeSpan, new GetScoreCallback(callback));
	}

	public void SubmitScore(string id, long score, Action<bool> callback)
	{
		GooglePlayGames.CallStatic("submitScore", id, score, new SubmitScoreCallback(callback));
	}

	public void GetLeaderboardData(string id, int n, TimeSpan timeSpan, Action<long[], string[], long[]> callback)
	{
		GooglePlayGames.CallStatic("getLeaderboardData", id, n, (int)timeSpan, new GetLeaderboardDataCallback(callback));
	}

	public void LaunchPicker(string path, int w, int h)
	{
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.secretexit.SXActivity"))
		{
			using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
			{
				androidJavaObject.Call("requestImagePicker", path, w, h);
			}
		}
	}
}
