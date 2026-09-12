#pragma warning disable 0618,0619
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dismount;
using TurboDismountMiniJSON;
using UnityEngine;

public class OnlineConfig : MonoBehaviour
{
	private string url = "http://secretexit.com/TDClientConfig_android.json";

	public int vunglePriority;

	public int unityAdPriority = 1;

	public int adColonyPriority = 2;

	public int adColonyPrioritizedVideoCount = 99;

	public bool useAdColony = true;

	public int vunglePrioritizedVideoCount = 2;

	public bool useVungle = true;

	public int unityAdPrioritizedVideoCount = 99;

	public bool useUnityAd = true;

	public int maxVideoAdsPerSession = 5;

	public bool showSales;

	public bool showSalesStateChanged;

	private int posterVersion;

	private string posterUrl = string.Empty;

	private Texture2D posterTexture;

	public string seasonalHeadOverride = string.Empty;

	public bool snowySeasonOverridden;

	public bool snowySeasonOverride;

	private float previousCheckTime = -1f;

	private bool readInProgress;

	private IEnumerator HTTPGet()
	{
		int retries = 4;
		bool success = false;
		do
		{
			string timestampedUrl = url + "?t=" + UnityEngine.Random.Range(1, 1000);
			WWW www = new WWW(timestampedUrl);
			yield return www;
			if (www.error == null)
			{
				success = true;
			}
			else
			{
				Debug.LogError("WWW error: " + www.error);
			}
			retries--;
			if (success)
			{
				UTF8Encoding encoding = new UTF8Encoding();
				string response = encoding.GetString(www.bytes);
				ConfigSuccess(response);
			}
		}
		while (retries > 0 && !success);
		readInProgress = false;
	}

	private void ConfigSuccess(string response)
	{
		bool flag = showSales;
		Dictionary<string, object> dictionary = Json.Deserialize(response) as Dictionary<string, object>;
		if (dictionary != null)
		{
			if (dictionary.ContainsKey("salesActive") && dictionary["salesActive"].ToString().CompareTo("true") == 0)
			{
				showSales = true;
			}
			else
			{
				showSales = false;
			}
			if (dictionary.ContainsKey("seasonalHeadOverride"))
			{
				seasonalHeadOverride = dictionary["seasonalHeadOverride"].ToString();
			}
			if (dictionary.ContainsKey("snowySeasonOverride"))
			{
				snowySeasonOverride = dictionary["snowySeasonOverride"].ToString().CompareTo("true") == 0;
				snowySeasonOverridden = true;
			}
			if (seasonalHeadOverride != string.Empty || snowySeasonOverridden)
			{
				DismountGame.instance.setupSeasonalEffects(DateTime.UtcNow);
			}
			if (dictionary.ContainsKey("adColonyPriority"))
			{
				adColonyPriority = int.Parse(dictionary["adColonyPriority"].ToString());
			}
			if (dictionary.ContainsKey("adColonyPrioritizedVideoCount"))
			{
				adColonyPrioritizedVideoCount = int.Parse(dictionary["adColonyPrioritizedVideoCount"].ToString());
			}
			if (dictionary.ContainsKey("useAdColony"))
			{
				useAdColony = dictionary["useAdColony"].ToString().CompareTo("true") == 0;
			}
			if (dictionary.ContainsKey("vunglePriority"))
			{
				vunglePriority = int.Parse(dictionary["vunglePriority"].ToString());
			}
			if (dictionary.ContainsKey("vunglePrioritizedVideoCount"))
			{
				vunglePrioritizedVideoCount = int.Parse(dictionary["vunglePrioritizedVideoCount"].ToString());
			}
			if (dictionary.ContainsKey("useVungle"))
			{
				useVungle = dictionary["useVungle"].ToString().CompareTo("true") == 0;
			}
			if (dictionary.ContainsKey("unityAdPriority"))
			{
				unityAdPriority = int.Parse(dictionary["unityAdPriority"].ToString());
			}
			if (dictionary.ContainsKey("unityAdPrioritizedVideoCount"))
			{
				unityAdPrioritizedVideoCount = int.Parse(dictionary["unityAdPrioritizedVideoCount"].ToString());
			}
			if (dictionary.ContainsKey("useUnityAd"))
			{
				useUnityAd = dictionary["useUnityAd"].ToString().CompareTo("true") == 0;
			}
			if (dictionary.ContainsKey("maxVideoAdsPerSession"))
			{
				maxVideoAdsPerSession = int.Parse(dictionary["maxVideoAdsPerSession"].ToString());
			}
			if (dictionary.ContainsKey("posterVersion") && dictionary.ContainsKey("posterUrl"))
			{
				posterVersion = int.Parse(dictionary["posterVersion"].ToString());
				posterUrl = dictionary["posterUrl"].ToString();
				if (posterUrl == string.Empty)
				{
					string path = Application.persistentDataPath + "/poster.jpg";
					if (File.Exists(path))
					{
						File.Delete(path);
					}
				}
				else if (Prefs.GetInt("posterVersion", 0) < posterVersion)
				{
					StartCoroutine(DownloadPosterTexture());
				}
			}
		}
		if (showSales != flag)
		{
			showSalesStateChanged = true;
		}
	}

	private IEnumerator DownloadPosterTexture()
	{
		using (WWW www = new WWW(posterUrl))
		{
			yield return www;
			if (string.IsNullOrEmpty(www.error))
			{
				string downloadPath = Application.persistentDataPath + "/";
				try
				{
					if (!Directory.Exists(downloadPath))
					{
						Directory.CreateDirectory(downloadPath);
					}
					string path = downloadPath + "poster.jpg";
					if (File.Exists(path))
					{
						File.Delete(path);
					}
					File.WriteAllBytes(path, www.bytes);
					Prefs.SetInt("posterVersion", posterVersion);
					yield break;
				}
				catch (Exception ex)
				{
					Exception e = ex;
					Debug.LogError(e);
					yield break;
				}
			}
			Debug.LogError(www.error);
		}
	}

	public void TryReadConfig()
	{
		bool flag = false;
		if (previousCheckTime != -1f)
		{
			float num = Time.time - previousCheckTime;
			if (num < 600f)
			{
				flag = true;
			}
		}
		if (!readInProgress && !flag)
		{
			previousCheckTime = Time.time;
			readInProgress = true;
			StartCoroutine(HTTPGet());
		}
	}

	public void ReadConfig()
	{
		TryReadConfig();
	}
}
