#pragma warning disable 0618,0619
using Dismount;
using UnityEngine;

[AddComponentMenu("SX Button Keys")]
public class SXButtonKeys : MonoBehaviour
{
	public SXButtonPanel panel;

	public MonoBehaviour targetUp;

	public MonoBehaviour targetDown;

	public MonoBehaviour targetLeft;

	public MonoBehaviour targetRight;

	public SXInputManager.Button button = SXInputManager.Button.A;

	public string msgUp = string.Empty;

	public string msgDown = string.Empty;

	public string msgLeft = string.Empty;

	public string msgRight = string.Empty;

	public bool propagateUp = true;

	public bool propagateDown = true;

	public bool propagateLeft = true;

	public bool propagateRight = true;

	public bool selectWithButtonUp;

	public void OnDisable()
	{
		if (SXButtonPanel.currentlyActivated == this)
		{
			SXButtonPanel.currentlyActivated = null;
		}
	}

	public void OnSelect()
	{
		panel.Activate(this, true);
	}

	public void HandleButtons()
	{
		if (!(panel != null) || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		if ((!selectWithButtonUp) ? SXInputManager.GetButtonDown(button) : SXInputManager.GetButtonUp(button))
		{
			UIButtonMessage component = GetComponent<UIButtonMessage>();
			ButtonClick component2 = GetComponent<ButtonClick>();
			panel.Activate(this, true);
			if (component != null)
			{
				component.SendMessage("OnClick");
			}
			if (component2 != null)
			{
				component2.OnPress(true);
			}
			return;
		}
		bool flag = SXInputManager.GetButtonDown(SXInputManager.Button.DPADL) || SXInputManager.GetButtonDown(SXInputManager.Button.LSTICKL);
		bool flag2 = SXInputManager.GetButtonDown(SXInputManager.Button.DPADU) || SXInputManager.GetButtonDown(SXInputManager.Button.LSTICKU);
		bool flag3 = SXInputManager.GetButtonDown(SXInputManager.Button.DPADR) || SXInputManager.GetButtonDown(SXInputManager.Button.LSTICKR);
		bool flag4 = SXInputManager.GetButtonDown(SXInputManager.Button.DPADD) || SXInputManager.GetButtonDown(SXInputManager.Button.LSTICKD);
		if (flag && flag3)
		{
			flag = (flag3 = false);
		}
		if (flag2 && flag4)
		{
			flag2 = (flag4 = false);
		}
		SXButtonKeys sXButtonKeys = this;
		if (flag2)
		{
			MonoBehaviour monoBehaviour;
			while ((monoBehaviour = sXButtonKeys.targetUp) != null)
			{
				sXButtonKeys = monoBehaviour.GetComponent<SXButtonKeys>();
				if (monoBehaviour.gameObject.activeInHierarchy)
				{
					if (msgUp.Length > 0)
					{
						monoBehaviour.gameObject.SendMessage(msgUp);
					}
					else if ((bool)sXButtonKeys)
					{
						panel.Activate(sXButtonKeys, true);
					}
					break;
				}
				if (!sXButtonKeys || !sXButtonKeys.propagateUp)
				{
					break;
				}
			}
		}
		else if (flag4)
		{
			MonoBehaviour monoBehaviour;
			while ((monoBehaviour = sXButtonKeys.targetDown) != null)
			{
				sXButtonKeys = monoBehaviour.GetComponent<SXButtonKeys>();
				if (monoBehaviour.gameObject.activeInHierarchy)
				{
					if (msgDown.Length > 0)
					{
						monoBehaviour.gameObject.SendMessage(msgDown);
					}
					else if ((bool)sXButtonKeys)
					{
						panel.Activate(sXButtonKeys, true);
					}
					break;
				}
				if (!sXButtonKeys || !sXButtonKeys.propagateDown)
				{
					break;
				}
			}
		}
		else if (flag)
		{
			MonoBehaviour monoBehaviour;
			while ((monoBehaviour = sXButtonKeys.targetLeft) != null)
			{
				sXButtonKeys = monoBehaviour.GetComponent<SXButtonKeys>();
				if (monoBehaviour.gameObject.activeInHierarchy)
				{
					if (msgLeft.Length > 0)
					{
						monoBehaviour.gameObject.SendMessage(msgLeft);
					}
					else if ((bool)sXButtonKeys)
					{
						panel.Activate(sXButtonKeys, true);
					}
					break;
				}
				if (!sXButtonKeys || !sXButtonKeys.propagateLeft)
				{
					break;
				}
			}
		}
		else
		{
			if (!flag3)
			{
				return;
			}
			MonoBehaviour monoBehaviour;
			while ((monoBehaviour = sXButtonKeys.targetRight) != null)
			{
				sXButtonKeys = monoBehaviour.GetComponent<SXButtonKeys>();
				if (monoBehaviour.gameObject.activeInHierarchy)
				{
					if (msgRight.Length > 0)
					{
						monoBehaviour.gameObject.SendMessage(msgRight);
					}
					else if ((bool)sXButtonKeys)
					{
						panel.Activate(sXButtonKeys, true);
					}
					break;
				}
				if (!sXButtonKeys || !sXButtonKeys.propagateRight)
				{
					break;
				}
			}
		}
	}
}
