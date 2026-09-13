#pragma warning disable 0618,0619
using System.Collections;
using UnityEngine;

namespace Dismount
{
	public class Loader : MonoBehaviour
	{
		public UISlider slider;

		public GameObject[] destroyAfterLoad;

		public GameObject[] destroyAfterFadeIn;

		public UISprite white;

		public UITweener alphaTweenIn;

		public UITweener alphaTweenOut;

		public Camera loaderCamera;

		private string expPath;

		private bool loadOBB;

		private AsyncOperation operation;

		private void Start()
		{
			Object.DontDestroyOnLoad(base.gameObject);
			alphaTweenIn.callWhenFinished = "FadeInComplete";
			alphaTweenOut.callWhenFinished = "FadeOutComplete";
			alphaTweenIn.enabled = false;
			alphaTweenOut.enabled = false;
			Invoke("StartLoading", 1f);
			white.alpha = 1f;
			white.transform.position = new Vector3(white.transform.position.x, white.transform.position.y, 10f);
		}

		private void StartLoading()
		{
			StartCoroutine("LoadLevel");
		}

		private IEnumerator LoadLevel()
		{
			Steamworks.BringApplicationToFront();
			Inventory inventory = base.transform.Find("Inventory").GetComponent<Inventory>();
			string levelName = Prefs.GetString("currentLevel", "LevelDevelopment");
			if (inventory.GetItemByReferenceName(levelName) == null)
			{
				levelName = "LevelDevelopment";
			}
			if (Application.CanStreamedLevelBeLoaded(levelName + "_ios"))
			{
				levelName += "_ios";
			}
			operation = Application.LoadLevelAsync(levelName);
			while (!operation.isDone)
			{
				slider.sliderValue = operation.progress;
				yield return 0;
			}
			LoadingComplete();
			GameObject[] array = destroyAfterLoad;
			foreach (GameObject go in array)
			{
				Object.Destroy(go);
			}
		}

		private void LoadingComplete()
		{
			white.transform.position = new Vector3(white.transform.position.x, white.transform.position.y, -1f);
			alphaTweenIn.enabled = true;
			alphaTweenIn.Reset();
			alphaTweenIn.Play(true);
		}

		private void FadeOutComplete()
		{
			Object.Destroy(base.gameObject);
		}

		private void FadeInComplete()
		{
			GameObject[] array = destroyAfterFadeIn;
			foreach (GameObject obj in array)
			{
				Object.Destroy(obj);
			}
			loaderCamera.clearFlags = CameraClearFlags.Depth;
			alphaTweenOut.enabled = true;
			alphaTweenOut.Reset();
			alphaTweenOut.Play(true);
		}
	}
}
