#pragma warning disable 0618,0619
using System.ComponentModel;
using System.Text.RegularExpressions;
using Rewired.Platforms;
using Rewired.Utils;
using Rewired.Utils.Interfaces;
using UnityEngine;

namespace Rewired
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class InputManager : InputManager_Base
	{
		protected override void DetectPlatform()
		{
			editorPlatform = EditorPlatform.None;
			platform = Rewired.Platforms.Platform.Unknown;
			webplayerPlatform = WebplayerPlatform.None;
			isEditor = false;
			string deviceName = SystemInfo.deviceName ?? string.Empty;
			string deviceModel = SystemInfo.deviceModel ?? string.Empty;
			platform = Rewired.Platforms.Platform.Android;
			if (CheckDeviceName("OUYA", deviceName, deviceModel))
			{
				platform = Rewired.Platforms.Platform.Ouya;
			}
			else if (CheckDeviceName("Amazon AFT.*", deviceName, deviceModel))
			{
				platform = Rewired.Platforms.Platform.AmazonFireTV;
			}
			else if (CheckDeviceName("razer Forge", deviceName, deviceModel))
			{
				platform = Rewired.Platforms.Platform.RazerForgeTV;
			}
		}

		protected override void CheckRecompile()
		{
		}

		protected override string GetFocusedEditorWindowTitle()
		{
			return string.Empty;
		}

		protected override IExternalTools GetExternalTools()
		{
			return new ExternalTools();
		}

		private bool CheckDeviceName(string searchPattern, string deviceName, string deviceModel)
		{
			return Regex.IsMatch(deviceName, searchPattern, RegexOptions.IgnoreCase) || Regex.IsMatch(deviceModel, searchPattern, RegexOptions.IgnoreCase);
		}
	}
}
