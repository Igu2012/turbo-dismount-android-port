#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using System.Linq;
using Dismount;
using Rewired;
using UnityEngine;

public class SXInputManager : MonoBehaviour
{
	public enum Button
	{
		BACK = 0,
		START = 1,
		DPADL = 2,
		DPADR = 3,
		DPADU = 4,
		DPADD = 5,
		A = 6,
		B = 7,
		X = 8,
		Y = 9,
		L1 = 10,
		L2 = 11,
		L3 = 12,
		R1 = 13,
		R2 = 14,
		R3 = 15,
		LSTICKL = 16,
		LSTICKR = 17,
		LSTICKU = 18,
		LSTICKD = 19
	}

	public enum Axis
	{
		LSTICKH = 0,
		LSTICKV = 1,
		RSTICKH = 2,
		RSTICKV = 3,
		L2 = 4,
		R2 = 5
	}

	private class ButtonBinding
	{
		protected bool axis;

		protected Button name;

		protected int index;

		protected int invert;

		protected int timeStamp;

		protected bool current;

		protected bool previous;

		protected KeyCode keyCode;

		public ButtonBinding(Button id, int idx, int inv)
		{
			axis = true;
			name = id;
			index = idx;
			invert = inv;
		}

		public ButtonBinding(Button id, KeyCode kc, int inv = 1)
		{
			axis = false;
			name = id;
			keyCode = kc;
			invert = inv;
		}

		private void UpdateAxisState()
		{
			int frameCount = Time.frameCount;
			if (timeStamp != frameCount)
			{
				previous = current;
				float num = Input.GetAxis("Axis" + index) * (float)invert;
				if ((double)num > 0.8)
				{
					current = true;
				}
				else if ((double)num < 0.2)
				{
					current = false;
				}
				timeStamp = frameCount;
			}
		}

		public bool Down()
		{
			if (axis)
			{
				UpdateAxisState();
				bool flag = current && !previous;
				if (flag)
				{
					disableMouse = true;
				}
				return flag;
			}
			return Input.GetKeyDown(keyCode);
		}

		public bool Up()
		{
			if (axis)
			{
				UpdateAxisState();
				return !current && previous;
			}
			return Input.GetKeyUp(keyCode);
		}

		public bool Pressed()
		{
			if (axis)
			{
				UpdateAxisState();
				return current;
			}
			return Input.GetKey(keyCode);
		}

		public Button Name()
		{
			return name;
		}
	}

	private class AxisBinding
	{
		protected bool axis;

		protected Axis name;

		protected int index;

		protected int invert;

		protected int timeStamp;

		protected bool current;

		protected bool previous;

		protected KeyCode keyCode;

		public AxisBinding(Axis id, int idx, int inv)
		{
			axis = true;
			name = id;
			index = idx;
			invert = inv;
		}

		public AxisBinding(Axis id, KeyCode kc, int inv = 1)
		{
			axis = false;
			name = id;
			keyCode = kc;
			invert = inv;
		}

		public float Value(float threshold)
		{
			if (axis)
			{
				float f = Input.GetAxis("Axis" + index) * (float)invert;
				f = Mathf.Clamp01(Mathf.Abs(f) - threshold) / (1f - threshold) * Mathf.Sign(f);
				if (f > 0.5f)
				{
					disableMouse = true;
				}
				return f;
			}
			return (!Input.GetKey(keyCode)) ? 0f : ((float)invert);
		}

		public Axis Name()
		{
			return name;
		}
	}

	private static Player rwPlayer;

	public static bool controllerEnabled = true;

	private static bool disableMouse = false;

	private static int mousePos = 0;

	private static Dictionary<Button, string> buttonToString = new Dictionary<Button, string>();

	private static Dictionary<Axis, string> axisToString = new Dictionary<Axis, string>();

	private static Lookup<Button, ButtonBinding> buttons = null;

	private static Lookup<Axis, AxisBinding> axes = null;

	private static string controllerName = string.Empty;

	private static int lastChecked = 0;

	private static bool connected = false;

	private void Start()
	{
		string[] joystickNames = Input.GetJoystickNames();
		connected = false;
		int num = -1;
		for (int i = 0; i < joystickNames.Length; i++)
		{
			connected = !string.IsNullOrEmpty(joystickNames[i]) && !joystickNames[i].Equals("uinput-fpc");
			if (connected)
			{
				num = i;
				break;
			}
		}
		if (connected)
		{
			Debug.Log("Controller found: " + joystickNames[num]);
			controllerName = joystickNames[num];
			Debug.Log("Controller bindings for: " + controllerName);
		}
		DefaultBinding();
	}

	private static void WindowsBinding(List<ButtonBinding> buttonList, List<AxisBinding> axisList)
	{
		OperatingSystem oSVersion = Environment.OSVersion;
		if (oSVersion.Version.CompareTo(new Version(10, 0)) >= 0 && controllerName.CompareTo("Controller (Xbox One For Windows)") == 0)
		{
			buttonList.Add(new ButtonBinding(Button.A, KeyCode.JoystickButton0));
			buttonList.Add(new ButtonBinding(Button.B, KeyCode.JoystickButton1));
			buttonList.Add(new ButtonBinding(Button.X, KeyCode.JoystickButton2));
			buttonList.Add(new ButtonBinding(Button.Y, KeyCode.JoystickButton3));
			buttonList.Add(new ButtonBinding(Button.L1, KeyCode.JoystickButton4));
			buttonList.Add(new ButtonBinding(Button.L2, 9, 1));
			buttonList.Add(new ButtonBinding(Button.L3, KeyCode.JoystickButton8));
			buttonList.Add(new ButtonBinding(Button.R1, KeyCode.JoystickButton5));
			buttonList.Add(new ButtonBinding(Button.R2, 6, 1));
			buttonList.Add(new ButtonBinding(Button.R3, KeyCode.JoystickButton9));
			buttonList.Add(new ButtonBinding(Button.START, KeyCode.JoystickButton7));
			buttonList.Add(new ButtonBinding(Button.BACK, KeyCode.JoystickButton6));
			buttonList.Add(new ButtonBinding(Button.DPADU, 8, 1));
			buttonList.Add(new ButtonBinding(Button.DPADL, 7, -1));
			buttonList.Add(new ButtonBinding(Button.DPADD, 8, -1));
			buttonList.Add(new ButtonBinding(Button.DPADR, 7, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKL, 1, -1));
			buttonList.Add(new ButtonBinding(Button.LSTICKU, 2, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKR, 1, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKD, 2, -1));
			axisList.Add(new AxisBinding(Axis.LSTICKH, 1, 1));
			axisList.Add(new AxisBinding(Axis.LSTICKV, 2, 1));
			axisList.Add(new AxisBinding(Axis.RSTICKH, 4, -1));
			axisList.Add(new AxisBinding(Axis.RSTICKV, 5, -1));
			axisList.Add(new AxisBinding(Axis.L2, 9, 1));
			axisList.Add(new AxisBinding(Axis.R2, 6, 1));
		}
		else
		{
			buttonList.Add(new ButtonBinding(Button.A, KeyCode.JoystickButton0));
			buttonList.Add(new ButtonBinding(Button.B, KeyCode.JoystickButton1));
			buttonList.Add(new ButtonBinding(Button.X, KeyCode.JoystickButton2));
			buttonList.Add(new ButtonBinding(Button.Y, KeyCode.JoystickButton3));
			buttonList.Add(new ButtonBinding(Button.L1, KeyCode.JoystickButton4));
			buttonList.Add(new ButtonBinding(Button.L2, 9, 1));
			buttonList.Add(new ButtonBinding(Button.L3, KeyCode.JoystickButton8));
			buttonList.Add(new ButtonBinding(Button.R1, KeyCode.JoystickButton5));
			buttonList.Add(new ButtonBinding(Button.R2, 10, 1));
			buttonList.Add(new ButtonBinding(Button.R3, KeyCode.JoystickButton9));
			buttonList.Add(new ButtonBinding(Button.START, KeyCode.JoystickButton7));
			buttonList.Add(new ButtonBinding(Button.BACK, KeyCode.JoystickButton6));
			buttonList.Add(new ButtonBinding(Button.DPADL, 6, -1));
			buttonList.Add(new ButtonBinding(Button.DPADU, 7, 1));
			buttonList.Add(new ButtonBinding(Button.DPADR, 6, 1));
			buttonList.Add(new ButtonBinding(Button.DPADD, 7, -1));
			buttonList.Add(new ButtonBinding(Button.LSTICKL, 1, -1));
			buttonList.Add(new ButtonBinding(Button.LSTICKU, 2, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKR, 1, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKD, 2, -1));
			axisList.Add(new AxisBinding(Axis.LSTICKH, 1, 1));
			axisList.Add(new AxisBinding(Axis.LSTICKV, 2, 1));
			axisList.Add(new AxisBinding(Axis.RSTICKH, 4, -1));
			axisList.Add(new AxisBinding(Axis.RSTICKV, 5, -1));
			axisList.Add(new AxisBinding(Axis.L2, 9, 1));
			axisList.Add(new AxisBinding(Axis.R2, 10, 1));
		}
	}

	private static void MacBinding(List<ButtonBinding> buttonList, List<AxisBinding> axisList)
	{
		if (controllerName.Contains("Microsoft"))
		{
			buttonList.Add(new ButtonBinding(Button.A, KeyCode.JoystickButton16));
			buttonList.Add(new ButtonBinding(Button.B, KeyCode.JoystickButton17));
			buttonList.Add(new ButtonBinding(Button.X, KeyCode.JoystickButton18));
			buttonList.Add(new ButtonBinding(Button.Y, KeyCode.JoystickButton19));
			buttonList.Add(new ButtonBinding(Button.L1, KeyCode.JoystickButton13));
			buttonList.Add(new ButtonBinding(Button.L2, 5, 1));
			buttonList.Add(new ButtonBinding(Button.L3, KeyCode.JoystickButton11));
			buttonList.Add(new ButtonBinding(Button.R1, KeyCode.JoystickButton14));
			buttonList.Add(new ButtonBinding(Button.R2, 6, 1));
			buttonList.Add(new ButtonBinding(Button.R3, KeyCode.JoystickButton12));
			buttonList.Add(new ButtonBinding(Button.START, KeyCode.JoystickButton9));
			buttonList.Add(new ButtonBinding(Button.BACK, KeyCode.JoystickButton10));
			buttonList.Add(new ButtonBinding(Button.DPADL, KeyCode.JoystickButton7));
			buttonList.Add(new ButtonBinding(Button.DPADU, KeyCode.JoystickButton5));
			buttonList.Add(new ButtonBinding(Button.DPADR, KeyCode.JoystickButton8));
			buttonList.Add(new ButtonBinding(Button.DPADD, KeyCode.JoystickButton6));
			buttonList.Add(new ButtonBinding(Button.LSTICKL, 1, -1));
			buttonList.Add(new ButtonBinding(Button.LSTICKU, 2, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKR, 1, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKD, 2, -1));
			axisList.Add(new AxisBinding(Axis.LSTICKH, 1, 1));
			axisList.Add(new AxisBinding(Axis.LSTICKV, 2, 1));
			axisList.Add(new AxisBinding(Axis.RSTICKH, 3, -1));
			axisList.Add(new AxisBinding(Axis.RSTICKV, 4, -1));
			axisList.Add(new AxisBinding(Axis.L2, 5, 1));
			axisList.Add(new AxisBinding(Axis.R2, 6, 1));
		}
		else if (controllerName.Contains("Wireless Controller"))
		{
			buttonList.Add(new ButtonBinding(Button.A, KeyCode.JoystickButton1));
			buttonList.Add(new ButtonBinding(Button.B, KeyCode.JoystickButton2));
			buttonList.Add(new ButtonBinding(Button.X, KeyCode.JoystickButton0));
			buttonList.Add(new ButtonBinding(Button.Y, KeyCode.JoystickButton3));
			buttonList.Add(new ButtonBinding(Button.L1, KeyCode.JoystickButton4));
			buttonList.Add(new ButtonBinding(Button.L2, KeyCode.JoystickButton6));
			buttonList.Add(new ButtonBinding(Button.L3, KeyCode.JoystickButton10));
			buttonList.Add(new ButtonBinding(Button.R1, KeyCode.JoystickButton5));
			buttonList.Add(new ButtonBinding(Button.R2, KeyCode.JoystickButton7));
			buttonList.Add(new ButtonBinding(Button.R3, KeyCode.JoystickButton11));
			buttonList.Add(new ButtonBinding(Button.START, KeyCode.JoystickButton8));
			buttonList.Add(new ButtonBinding(Button.BACK, KeyCode.JoystickButton9));
			buttonList.Add(new ButtonBinding(Button.DPADL, 7, -1));
			buttonList.Add(new ButtonBinding(Button.DPADU, 8, -1));
			buttonList.Add(new ButtonBinding(Button.DPADR, 7, 1));
			buttonList.Add(new ButtonBinding(Button.DPADD, 8, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKL, 1, -1));
			buttonList.Add(new ButtonBinding(Button.LSTICKU, 2, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKR, 1, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKD, 2, -1));
			axisList.Add(new AxisBinding(Axis.LSTICKH, 1, 1));
			axisList.Add(new AxisBinding(Axis.LSTICKV, 2, 1));
			axisList.Add(new AxisBinding(Axis.RSTICKH, 3, -1));
			axisList.Add(new AxisBinding(Axis.RSTICKV, 4, -1));
			axisList.Add(new AxisBinding(Axis.L2, 5, 1));
			axisList.Add(new AxisBinding(Axis.R2, 6, 1));
		}
		else if (controllerName.Contains("Sony PLAYSTATION(R)3"))
		{
			buttonList.Add(new ButtonBinding(Button.A, KeyCode.JoystickButton14));
			buttonList.Add(new ButtonBinding(Button.B, KeyCode.JoystickButton13));
			buttonList.Add(new ButtonBinding(Button.X, KeyCode.JoystickButton15));
			buttonList.Add(new ButtonBinding(Button.Y, KeyCode.JoystickButton12));
			buttonList.Add(new ButtonBinding(Button.L1, KeyCode.JoystickButton10));
			buttonList.Add(new ButtonBinding(Button.L2, KeyCode.JoystickButton8));
			buttonList.Add(new ButtonBinding(Button.L3, KeyCode.JoystickButton1));
			buttonList.Add(new ButtonBinding(Button.R1, KeyCode.JoystickButton11));
			buttonList.Add(new ButtonBinding(Button.R2, KeyCode.JoystickButton9));
			buttonList.Add(new ButtonBinding(Button.R3, KeyCode.JoystickButton2));
			buttonList.Add(new ButtonBinding(Button.START, KeyCode.JoystickButton0));
			buttonList.Add(new ButtonBinding(Button.BACK, KeyCode.JoystickButton3));
			buttonList.Add(new ButtonBinding(Button.DPADL, KeyCode.JoystickButton7));
			buttonList.Add(new ButtonBinding(Button.DPADU, KeyCode.JoystickButton4));
			buttonList.Add(new ButtonBinding(Button.DPADR, KeyCode.JoystickButton5));
			buttonList.Add(new ButtonBinding(Button.DPADD, KeyCode.JoystickButton6));
			buttonList.Add(new ButtonBinding(Button.LSTICKL, 1, -1));
			buttonList.Add(new ButtonBinding(Button.LSTICKU, 2, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKR, 1, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKD, 2, -1));
			axisList.Add(new AxisBinding(Axis.LSTICKH, 1, 1));
			axisList.Add(new AxisBinding(Axis.LSTICKV, 2, 1));
			axisList.Add(new AxisBinding(Axis.RSTICKH, 3, -1));
			axisList.Add(new AxisBinding(Axis.RSTICKV, 4, -1));
			axisList.Add(new AxisBinding(Axis.L2, KeyCode.JoystickButton8));
			axisList.Add(new AxisBinding(Axis.R2, KeyCode.JoystickButton9));
		}
		else
		{
			buttonList.Add(new ButtonBinding(Button.A, KeyCode.JoystickButton1));
			buttonList.Add(new ButtonBinding(Button.B, KeyCode.JoystickButton2));
			buttonList.Add(new ButtonBinding(Button.X, KeyCode.JoystickButton0));
			buttonList.Add(new ButtonBinding(Button.Y, KeyCode.JoystickButton3));
			buttonList.Add(new ButtonBinding(Button.L1, KeyCode.JoystickButton4));
			buttonList.Add(new ButtonBinding(Button.L2, KeyCode.JoystickButton6));
			buttonList.Add(new ButtonBinding(Button.L3, KeyCode.JoystickButton10));
			buttonList.Add(new ButtonBinding(Button.R1, KeyCode.JoystickButton5));
			buttonList.Add(new ButtonBinding(Button.R2, KeyCode.JoystickButton7));
			buttonList.Add(new ButtonBinding(Button.R3, KeyCode.JoystickButton11));
			buttonList.Add(new ButtonBinding(Button.START, KeyCode.JoystickButton8));
			buttonList.Add(new ButtonBinding(Button.BACK, KeyCode.JoystickButton9));
			buttonList.Add(new ButtonBinding(Button.DPADL, 5, -1));
			buttonList.Add(new ButtonBinding(Button.DPADU, 6, -1));
			buttonList.Add(new ButtonBinding(Button.DPADR, 5, 1));
			buttonList.Add(new ButtonBinding(Button.DPADD, 6, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKL, 1, -1));
			buttonList.Add(new ButtonBinding(Button.LSTICKU, 2, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKR, 1, 1));
			buttonList.Add(new ButtonBinding(Button.LSTICKD, 2, -1));
			axisList.Add(new AxisBinding(Axis.LSTICKH, 1, 1));
			axisList.Add(new AxisBinding(Axis.LSTICKV, 2, 1));
			axisList.Add(new AxisBinding(Axis.RSTICKH, 3, -1));
			axisList.Add(new AxisBinding(Axis.RSTICKV, 4, -1));
			axisList.Add(new AxisBinding(Axis.L2, KeyCode.JoystickButton6));
			axisList.Add(new AxisBinding(Axis.R2, KeyCode.JoystickButton7));
		}
	}

	private static void DefaultBinding()
	{
		List<ButtonBinding> list = new List<ButtonBinding>();
		List<AxisBinding> list2 = new List<AxisBinding>();
		list.Add(new ButtonBinding(Button.A, KeyCode.JoystickButton0));
		list.Add(new ButtonBinding(Button.B, KeyCode.JoystickButton1));
		list.Add(new ButtonBinding(Button.X, KeyCode.JoystickButton2));
		list.Add(new ButtonBinding(Button.Y, KeyCode.JoystickButton3));
		list.Add(new ButtonBinding(Button.L1, KeyCode.JoystickButton4));
		list.Add(new ButtonBinding(Button.R1, KeyCode.JoystickButton5));
		list.Add(new ButtonBinding(Button.L3, KeyCode.JoystickButton8));
		list.Add(new ButtonBinding(Button.R3, KeyCode.JoystickButton9));
		list.Add(new ButtonBinding(Button.START, KeyCode.JoystickButton10));
		list.Add(new ButtonBinding(Button.BACK, KeyCode.Escape));
		list.Add(new ButtonBinding(Button.BACK, KeyCode.JoystickButton11));
		list.Add(new ButtonBinding(Button.DPADL, 5, -1));
		list.Add(new ButtonBinding(Button.DPADU, 6, -1));
		list.Add(new ButtonBinding(Button.DPADR, 5, 1));
		list.Add(new ButtonBinding(Button.DPADD, 6, 1));
		list.Add(new ButtonBinding(Button.LSTICKL, 1, -1));
		list.Add(new ButtonBinding(Button.LSTICKU, 2, 1));
		list.Add(new ButtonBinding(Button.LSTICKR, 1, 1));
		list.Add(new ButtonBinding(Button.LSTICKD, 2, -1));
		list.Add(new ButtonBinding(Button.L2, 7, 1));
		list.Add(new ButtonBinding(Button.L2, 13, 1));
		list.Add(new ButtonBinding(Button.R2, 8, 1));
		list.Add(new ButtonBinding(Button.R2, 12, 1));
		list2.Add(new AxisBinding(Axis.LSTICKH, 1, 1));
		list2.Add(new AxisBinding(Axis.LSTICKV, 2, 1));
		list2.Add(new AxisBinding(Axis.RSTICKH, 3, -1));
		list2.Add(new AxisBinding(Axis.RSTICKV, 4, -1));
		list2.Add(new AxisBinding(Axis.RSTICKH, 14, -1));
		list2.Add(new AxisBinding(Axis.RSTICKV, 15, -1));
		list2.Add(new AxisBinding(Axis.L2, 7, 1));
		list2.Add(new AxisBinding(Axis.L2, 13, 1));
		list2.Add(new AxisBinding(Axis.R2, 8, 1));
		list2.Add(new AxisBinding(Axis.R2, 12, 1));
		buttons = (Lookup<Button, ButtonBinding>)list.ToLookup((ButtonBinding k) => k.Name(), (ButtonBinding v) => v);
		axes = (Lookup<Axis, AxisBinding>)list2.ToLookup((AxisBinding k) => k.Name(), (AxisBinding v) => v);
	}

	public static bool IsControllerConnected()
	{
		if (DismountGame.IsTVDevice())
		{
			return true;
		}
		if (lastChecked != Time.frameCount)
		{
			lastChecked = Time.frameCount;
			string[] joystickNames = Input.GetJoystickNames();
			connected = false;
			int num = -1;
			for (int i = 0; i < joystickNames.Length; i++)
			{
				if (connected)
				{
					break;
				}
				connected = !string.IsNullOrEmpty(joystickNames[i]) && !joystickNames[i].Equals("uinput-fpc");
				if (connected)
				{
					num = i;
				}
			}
			if (connected && !joystickNames[num].Equals(controllerName))
			{
				controllerName = joystickNames[num];
				DefaultBinding();
			}
		}
		return connected;
	}

	public static bool GetButtonDown(Button name)
	{
		if (!controllerEnabled)
		{
			return false;
		}
		IEnumerable<ButtonBinding> enumerable = buttons[name];
		foreach (ButtonBinding item in enumerable)
		{
			if (item.Down())
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetButtonUp(Button name)
	{
		if (!controllerEnabled)
		{
			return false;
		}
		IEnumerable<ButtonBinding> enumerable = buttons[name];
		foreach (ButtonBinding item in enumerable)
		{
			if (item.Up())
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetButton(Button name)
	{
		if (!controllerEnabled)
		{
			return false;
		}
		IEnumerable<ButtonBinding> enumerable = buttons[name];
		foreach (ButtonBinding item in enumerable)
		{
			if (item.Pressed())
			{
				return true;
			}
		}
		return false;
	}

	public static float GetAxisValue(Axis name, float threshold)
	{
		if (!controllerEnabled)
		{
			return 0f;
		}
		float num = 0f;
		if (IsControllerConnected())
		{
			IEnumerable<AxisBinding> enumerable = axes[name];
			foreach (AxisBinding item in enumerable)
			{
				num += item.Value(threshold);
			}
		}
		return Mathf.Clamp(num, -1f, 1f);
	}

	public void Update()
	{
	}

	public static bool GetMouseButton(int button)
	{
		return Input.GetMouseButton(button);
	}

	public static bool GetMouseButtonUp(int button)
	{
		return Input.GetMouseButtonUp(button);
	}

	public static bool GetMouseButtonDown(int button)
	{
		return Input.GetMouseButtonDown(button);
	}

	public static float GetMouseX()
	{
		return 0f;
	}

	public static float GetMouseY()
	{
		return 0f;
	}

	public static float GetMouseScrollWheel()
	{
		return 0f;
	}
}
