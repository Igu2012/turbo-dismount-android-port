#pragma warning disable 0618,0619
public class Platform
{
	public static string[] GetAllIds()
	{
		return new string[2] { "tablet", "controller" };
	}

	public static string GetId()
	{
		return (!SXInputManager.IsControllerConnected()) ? "tablet" : "controller";
	}
}
