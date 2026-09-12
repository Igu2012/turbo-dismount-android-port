#pragma warning disable 0618,0619
using System.Text;
using UnityEngine;

public class DebugScreen : MonoBehaviour
{
	private StringBuilder log = new StringBuilder();

	private Vector2 scrollPos = Vector2.zero;

	public void Print(string text)
	{
		log.AppendLine(text);
	}

	public void Clear()
	{
		log = new StringBuilder();
	}

	private void OnGUI()
	{
		if (log.Length > 0)
		{
			Rect position = new Rect(20f, 20f, Screen.width - 40, Screen.height - 40);
			Rect rect = new Rect(0f, 0f, Screen.width - 40, Screen.height * 100);
			scrollPos = GUI.BeginScrollView(position, scrollPos, rect);
			GUI.TextArea(rect, log.ToString());
			GUI.EndScrollView(true);
		}
	}
}
