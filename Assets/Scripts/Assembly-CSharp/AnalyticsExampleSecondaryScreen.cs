#pragma warning disable 0618,0619
using UnityEngine;

public class AnalyticsExampleSecondaryScreen : MonoBehaviour
{
	private void OnGUI()
	{
		GUILayout.BeginVertical();
		GUILayout.Label(" This scene is just to demonstrate automatic screen");
		GUILayout.Label(" switch events sent by the analytics example.");
		GUILayout.Label(" Current scene: " + Application.loadedLevelName);
		if (GUILayout.Button("Back to Main"))
		{
			Application.LoadLevel("AnalyticsExample");
		}
		GUILayout.EndVertical();
	}
}
