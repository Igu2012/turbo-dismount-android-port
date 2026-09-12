#pragma warning disable 0618,0619
using UnityEngine;

public class AnalyticsExampleMainScreen : MonoBehaviour
{
	public Analytics analytics;

	private void OnGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("v");
		GUILayout.Label(analytics.appVersion);
		GUILayout.BeginVertical();
		GUILayout.Label("- Google Universal Analytics for Unity");
		GUILayout.Label(" Current scene: " + Application.loadedLevelName);
		if (GUILayout.Button("Go to Secondary Scene"))
		{
			Application.LoadLevel("AnalyticsExampleSecondaryScene");
		}
		GUILayout.Label("Imaginary hits to switch menu screens:");
		if (GUILayout.Button("Send \"Menuscreen A\" Hit"))
		{
			Analytics.changeScreen("AnalyticsExample - Menuscreen A");
		}
		if (GUILayout.Button("Send \"Menuscreen B\" Hit"))
		{
			Analytics.changeScreen("AnalyticsExample - Menuscreen B");
		}
		GUILayout.Label("Web Links:");
		if (GUILayout.Button("Strobotnik in Google+"))
		{
			Analytics.gua.sendSocialHit("GooglePlus", "plus", "StrobotnikGooglePlus");
			Application.OpenURL("http://plus.google.com/101873213646861422131");
		}
		if (GUILayout.Button("Strobotnik in Facebook"))
		{
			Analytics.gua.sendSocialHit("Facebook", "like", "StrobotnikFacebook");
			Application.OpenURL("http://facebook.com/strobotnik");
		}
		if (GUILayout.Button("Strobotnik in Twitter"))
		{
			Analytics.gua.sendSocialHit("Twitter", "follow", "StrobotnikTwitter");
			Application.OpenURL("http://twitter.com/strobotnik");
		}
		if (GUILayout.Button("Strobotnik Web Site"))
		{
			Analytics.gua.sendEventHit("OpenWebsite", "Strobotnik.com");
			Application.OpenURL("http://strobotnik.com");
		}
		GUILayout.Label("---");
		if (GUILayout.Button("Quit"))
		{
			Analytics.gua.beginHit(GoogleUniversalAnalytics.HitType.Appview);
			Analytics.gua.addContentDescription("AnalyticsExample - Quit");
			Analytics.gua.addSessionControl(false);
			Analytics.gua.sendHit();
			base.gameObject.SetActive(false);
			Application.Quit();
		}
		string text = "Network Reachability: none";
		if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
		{
			text = "Network Reachability: via carrier data network";
		}
		else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
		{
			text = "Network Reachability: via local area network";
		}
		GUILayout.Label(text);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}
}
