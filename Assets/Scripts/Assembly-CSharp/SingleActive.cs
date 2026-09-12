#pragma warning disable 0618,0619
using UnityEngine;

public class SingleActive : MonoBehaviour
{
	public GameObject[] objects;

	private int activeIndex;

	private GameObject currentActive;

	private void OnChildActivated(GameObject o)
	{
		if (currentActive != null)
		{
			currentActive.SetActive(false);
		}
		currentActive = o;
	}
}
