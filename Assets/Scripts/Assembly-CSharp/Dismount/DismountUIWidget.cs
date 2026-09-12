#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class DismountUIWidget : MonoBehaviour
	{
		public UITweener showTweener;

		public UITweener hideTweener;

		private bool shown;

		private int activeUILayer = -1;

		private int inactiveUILayer = -1;

		public bool Shown
		{
			get
			{
				return shown;
			}
		}

		private void Awake()
		{
			activeUILayer = LayerMask.NameToLayer("TDUI");
			inactiveUILayer = LayerMask.NameToLayer("TDUIInactive");
			UITweener[] components = GetComponents<UITweener>();
			UITweener[] array = components;
			foreach (UITweener uITweener in array)
			{
				uITweener.enabled = false;
			}
			shown = false;
			hideTweener.onFinished = OnFinished;
			hideTweener.eventReceiver = base.gameObject;
		}

		private void OnFinished(UITweener tween)
		{
			if (tween == hideTweener)
			{
				base.gameObject.SetActive(false);
			}
		}

		public void Reset()
		{
			UITweener[] components = GetComponents<UITweener>();
			UITweener[] array = components;
			foreach (UITweener uITweener in array)
			{
				uITweener.Reset();
			}
			shown = false;
		}

		public void Show()
		{
			if (shown)
			{
				return;
			}
			if ((bool)showTweener)
			{
				if ((bool)hideTweener)
				{
					hideTweener.enabled = false;
				}
				showTweener.Reset();
				showTweener.Play(true);
				showTweener.enabled = true;
			}
			Collider[] componentsInChildren = base.transform.GetComponentsInChildren<Collider>();
			if (componentsInChildren != null)
			{
				Collider[] array = componentsInChildren;
				foreach (Collider collider in array)
				{
					collider.gameObject.layer = activeUILayer;
				}
			}
			shown = true;
		}

		public void Hide()
		{
			if (!shown)
			{
				return;
			}
			if ((bool)hideTweener)
			{
				if ((bool)showTweener)
				{
					showTweener.enabled = false;
				}
				hideTweener.enabled = true;
				hideTweener.Reset();
				hideTweener.Play(true);
			}
			Collider[] componentsInChildren = base.transform.GetComponentsInChildren<Collider>();
			if (componentsInChildren != null)
			{
				Collider[] array = componentsInChildren;
				foreach (Collider collider in array)
				{
					collider.gameObject.layer = inactiveUILayer;
				}
			}
			shown = false;
		}
	}
}
