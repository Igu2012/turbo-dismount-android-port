using UnityEngine;

public class EveryplaySettings : ScriptableObject
{
	public bool iosSupportEnabled;

	public bool tvosSupportEnabled;

	public bool androidSupportEnabled;

	public bool standaloneSupportEnabled;

	public bool testButtonsEnabled;

	public bool earlyInitializerEnabled = true;

	public bool IsEnabled
	{
		get
		{
			return androidSupportEnabled;
		}
	}

	public bool IsValid
	{
		get
		{
			return true;
		}
	}
}
