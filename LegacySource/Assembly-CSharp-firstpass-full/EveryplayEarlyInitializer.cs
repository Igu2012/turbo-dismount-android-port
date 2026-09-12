using System.Collections;
using UnityEngine;

public class EveryplayEarlyInitializer : MonoBehaviour
{
	private void Start()
	{
		EveryplaySettings everyplaySettings = (EveryplaySettings)Resources.Load("EveryplaySettings");
		if (everyplaySettings != null && everyplaySettings.earlyInitializerEnabled && everyplaySettings.IsEnabled && everyplaySettings.IsValid)
		{
			StartCoroutine(InitializeEveryplay());
		}
	}

	private IEnumerator InitializeEveryplay()
	{
		yield return 0;
		Everyplay.Initialize();
		Object.Destroy(base.gameObject);
	}
}
