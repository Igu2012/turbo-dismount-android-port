#pragma warning disable 0618,0619
using UnityEngine;

public class Overlay : MonoBehaviour
{
	public GameObject OverlayTweener;

	public GameObject Receiver;

	public bool ControlActiveFlag = true;

	public bool HideAtAwake = true;

	private bool shown = true;

	private bool transitionActive;

	public bool isTransitionActive
	{
		get
		{
			return transitionActive;
		}
	}

	protected void Awake()
	{
		if ((bool)OverlayTweener && OverlayTweener.GetComponents<UITweener>().Length < 2)
		{
			Debug.LogError("OverlayTweener needs two tween components for Showing and hiding");
		}
		if (!OverlayTweener && GetComponents<UITweener>().Length >= 2)
		{
			OverlayTweener = base.gameObject;
		}
	}

	private void Start()
	{
		if (HideAtAwake)
		{
			shown = false;
			if ((bool)OverlayTweener)
			{
				OverlayTweener.GetComponents<UITweener>()[0].enabled = false;
				OverlayTweener.GetComponents<UITweener>()[1].enabled = false;
			}
			if (ControlActiveFlag)
			{
				base.gameObject.SetActive(false);
			}
		}
	}

	private void NotifyShown()
	{
		if ((bool)Receiver)
		{
			Receiver.SendMessage("OnOverlayShown", SendMessageOptions.RequireReceiver);
		}
		else
		{
			SendMessage("OnOverlayShown", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void NotifyHidden()
	{
		if ((bool)Receiver)
		{
			Receiver.SendMessage("OnOverlayHidden", SendMessageOptions.RequireReceiver);
		}
		else
		{
			SendMessage("OnOverlayHidden", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Show()
	{
		if (!shown)
		{
			shown = true;
			if (ControlActiveFlag)
			{
				base.gameObject.SetActive(true);
			}
			if ((bool)OverlayTweener)
			{
				OverlayTweener.GetComponents<UITweener>()[0].enabled = true;
				transitionActive = true;
			}
			else
			{
				OverlayShown();
			}
		}
	}

	public void Hide()
	{
		if (shown)
		{
			shown = false;
			if ((bool)OverlayTweener)
			{
				OverlayTweener.GetComponents<UITweener>()[1].enabled = true;
				transitionActive = false;
			}
			else
			{
				OverlayHidden();
			}
		}
	}

	public void OverlayShown()
	{
		if ((bool)OverlayTweener)
		{
			OverlayTweener.GetComponents<UITweener>()[0].enabled = false;
		}
		transitionActive = false;
		shown = true;
		NotifyShown();
	}

	public void OverlayHidden()
	{
		if ((bool)OverlayTweener)
		{
			OverlayTweener.GetComponents<UITweener>()[1].enabled = false;
		}
		transitionActive = false;
		if (ControlActiveFlag)
		{
			base.gameObject.SetActive(false);
		}
		NotifyHidden();
	}
}
