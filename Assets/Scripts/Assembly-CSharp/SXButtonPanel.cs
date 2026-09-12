#pragma warning disable 0618,0619
using UnityEngine;

[AddComponentMenu("SX Button Panel")]
public class SXButtonPanel : MonoBehaviour
{
	public SXButtonKeys[] selectionList = new SXButtonKeys[0];

	public SXButtonKeys selected;

	public static SXButtonKeys currentlyActivated;

	public void Reset()
	{
		selectionList = new SXButtonKeys[0];
		selected = null;
	}

	private SXButtonKeys FirstEnabled()
	{
		for (int i = 0; i < selectionList.Length; i++)
		{
			SXButtonKeys sXButtonKeys = selectionList[i];
			if (sXButtonKeys != null && sXButtonKeys.gameObject.activeInHierarchy)
			{
				return sXButtonKeys;
			}
		}
		return selected;
	}

	private void Update()
	{
		if (!selected || !selected.gameObject.activeInHierarchy)
		{
			selected = FirstEnabled();
		}
		if (selected != currentlyActivated)
		{
			Activate(selected, false);
		}
	}

	public void Activate(SXButtonKeys bk, bool save)
	{
		if (!bk || !bk.gameObject.activeInHierarchy)
		{
			bk = FirstEnabled();
		}
		selected = bk;
		if (bk != currentlyActivated && (bool)bk && bk.gameObject.activeInHierarchy && SXInputManager.IsControllerConnected())
		{
			UICamera.selectedObject = bk.gameObject;
			currentlyActivated = bk;
		}
	}
}
