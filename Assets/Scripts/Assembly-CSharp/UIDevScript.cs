#pragma warning disable 0618,0619
using UnityEngine;

public class UIDevScript : MonoBehaviour
{
	private void Awake()
	{
		if (!GameObject.Find("/DismountGame"))
		{
			Application.LoadLevel("LevelDevelopment");
		}
	}
}
