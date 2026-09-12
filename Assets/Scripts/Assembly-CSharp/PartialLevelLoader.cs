#pragma warning disable 0618,0619
using System;
using System.Collections;
using UnityEngine;

public class PartialLevelLoader : MonoBehaviour
{
	private Action<bool> completeAction;

	public void LoadLevel(string levelName, Action<bool> loadingCompleteAction)
	{
		completeAction = loadingCompleteAction;
		StartCoroutine("DoLoadLevel", levelName);
	}

	private IEnumerator DoLoadLevel(string levelName)
	{
		Application.LoadLevel(levelName);
		yield return 0;
		completeAction(true);
	}

	public void LoadLevelAdditive(string levelName, Action<bool> loadingCompleteAction)
	{
		completeAction = loadingCompleteAction;
		StartCoroutine("DoLoadLevelAdditive", levelName);
	}

	private IEnumerator DoLoadLevelAdditive(string levelName)
	{
		Application.LoadLevelAdditive(levelName);
		yield return 0;
		completeAction(true);
		yield return 0;
	}
}
