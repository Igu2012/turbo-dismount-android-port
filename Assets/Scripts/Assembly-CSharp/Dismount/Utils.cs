#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Dismount
{
	public class Utils
	{
		private const float dismountBarFillSpeed = 3f;

		private const string customPrefix = "custom_";

		public static string NumberString(float number, int decimals = 2)
		{
			NumberFormatInfo numberFormat = new CultureInfo("en-US", false).NumberFormat;
			numberFormat.NumberDecimalDigits = decimals;
			numberFormat.NumberGroupSeparator = " ";
			return number.ToString("N", numberFormat);
		}

		public static string ScoreString(int number)
		{
			NumberFormatInfo numberFormat = new CultureInfo("en-US", false).NumberFormat;
			numberFormat.NumberDecimalDigits = 0;
			numberFormat.NumberGroupSeparator = ",";
			return number.ToString("N", numberFormat);
		}

		public static void SetVisible(GameObject go, bool visible)
		{
			MeshRenderer[] componentsInChildren = go.GetComponentsInChildren<MeshRenderer>();
			MeshRenderer[] array = componentsInChildren;
			foreach (MeshRenderer meshRenderer in array)
			{
				meshRenderer.enabled = visible;
			}
		}

		public static void SetSubtreeLayer(Transform t, int layer)
		{
			t.gameObject.layer = layer;
			for (int i = 0; i < t.childCount; i++)
			{
				Transform child = t.GetChild(i);
				SetSubtreeLayer(child, layer);
			}
		}

		public static Transform FindChildRecursive(string name, Transform parentTransform)
		{
			Transform transform = parentTransform.Find(name);
			if ((bool)transform)
			{
				return transform;
			}
			for (int i = 0; i < parentTransform.childCount; i++)
			{
				Transform child = parentTransform.GetChild(i);
				transform = FindChildRecursive(name, child);
				if ((bool)transform)
				{
					return transform;
				}
			}
			return null;
		}

		public static void FindChildrenRecursive(string name, Transform parentTransform, ref List<Transform> children)
		{
			if (children == null)
			{
				children = new List<Transform>();
			}
			Transform transform = parentTransform.Find(name);
			if ((bool)transform)
			{
				children.Add(transform);
			}
			for (int i = 0; i < parentTransform.childCount; i++)
			{
				Transform child = parentTransform.GetChild(i);
				FindChildrenRecursive(name, child, ref children);
			}
		}

		public static void SendMessage(string targetObjectName, string message, UnityEngine.Object parameter = null)
		{
			GameObject gameObject = GameObject.Find(targetObjectName);
			if ((bool)gameObject)
			{
				gameObject.SendMessage(message, parameter, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				DismountGame.instance.gameObject.SendMessage(message, parameter, SendMessageOptions.DontRequireReceiver);
			}
		}

		public static void BroadcastMessage(string targetObjectName, string message, UnityEngine.Object parameter = null)
		{
			GameObject gameObject = GameObject.Find(targetObjectName);
			if ((bool)gameObject)
			{
				gameObject.BroadcastMessage(message, parameter, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				DismountGame.instance.gameObject.BroadcastMessage(message, parameter, SendMessageOptions.DontRequireReceiver);
			}
		}

		public static Transform FindParent(string name, Transform transform)
		{
			if (!transform)
			{
				return null;
			}
			Transform transform2 = transform;
			while (transform2.parent != null)
			{
				if (transform2.name.CompareTo(name) == 0)
				{
					return transform2;
				}
				transform2 = transform2.parent;
			}
			return null;
		}

		public static bool Maybe()
		{
			return UnityEngine.Random.Range(0, 2) == 1;
		}

		public static bool Odds(float fraction)
		{
			return UnityEngine.Random.Range(0f, 1f) < fraction;
		}

		public static object RandomPick(params object[] objects)
		{
			int num = UnityEngine.Random.Range(0, objects.Length);
			return objects[num];
		}

		public static object RandomSlotPick(params object[] objects)
		{
			int num = 0;
			for (int i = 0; i < objects.Length; i += 2)
			{
				int num2 = (int)objects[i + 1];
				num += num2;
			}
			int num3 = UnityEngine.Random.Range(0, num);
			int num4 = 0;
			for (int j = 0; j < objects.Length; j += 2)
			{
				int num5 = (int)objects[j + 1];
				if (num3 >= num4 && num3 < num4 + num5)
				{
					return objects[j];
				}
				num4 += num5;
			}
			return objects[0];
		}

		public static string GetQualitySetting()
		{
			int qualityLevel = QualitySettings.GetQualityLevel();
			return QualitySettings.names[qualityLevel];
		}

		public static void SetQualitySetting(string settingName)
		{
			string[] names = QualitySettings.names;
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i].Equals(settingName))
				{
					Debug.Log("Setting quality level " + settingName);
					QualitySettings.SetQualityLevel(i, false);
					break;
				}
			}
		}

		public static string ConvertSpeedString(float speed, bool metric)
		{
			if (metric)
			{
				return Mathf.FloorToInt(speed * 3.6f).ToString("000' km/h'");
			}
			return Mathf.FloorToInt(speed * 2.23694f).ToString("000' mph'");
		}

		public static string ConvertAltitudeString(float altitude, bool metric)
		{
			if (metric)
			{
				return Mathf.FloorToInt(altitude).ToString("000' m'");
			}
			return Mathf.FloorToInt(altitude * 3.28084f).ToString("000' ft'");
		}

		public static float DismountBarFillFunctionSetup(float time, float maxValue)
		{
			if (Mathf.Repeat(time * 3f, (float)Math.PI * 5f) < (float)Math.PI * 2f)
			{
				return 0f;
			}
			return maxValue - Mathf.Abs(Mathf.Cos(time * 3f)) * maxValue;
		}

		public static float DismountBarFillFunction(float time, float maxValue)
		{
			return maxValue - Mathf.Abs(Mathf.Cos(time * 3f)) * maxValue;
		}

		public static float DismountBarFillInverseFunction(float value, float maxValue)
		{
			float num = Mathf.Clamp(value, 0f, maxValue);
			num = maxValue - num;
			float f = (num /= maxValue);
			float num2 = Mathf.Acos(f);
			return num2 / 3f;
		}

		public static float ResetBarFillFunction(float time, float maxValue)
		{
			if (time > (float)Math.PI / 10f)
			{
				return maxValue;
			}
			return maxValue - Mathf.Abs(Mathf.Cos(time * 5f)) * maxValue;
		}

		public static bool IsSystemMetric()
		{
			if (RegionInfo.CurrentRegion != null)
			{
				return RegionInfo.CurrentRegion.IsMetric;
			}
			return false;
		}

		public static T FindFirstComponentUpInHierarchy<T>(Transform transform) where T : Component
		{
			while ((bool)transform)
			{
				T component = transform.GetComponent<T>();
				if ((bool)component)
				{
					return component;
				}
				transform = transform.parent;
			}
			return (T)null;
		}

		public static string Md5Sum(string strToEncrypt)
		{
			UTF8Encoding uTF8Encoding = new UTF8Encoding();
			byte[] bytes = uTF8Encoding.GetBytes(strToEncrypt);
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			byte[] array = mD5CryptoServiceProvider.ComputeHash(bytes);
			string text = string.Empty;
			for (int i = 0; i < array.Length; i++)
			{
				text += Convert.ToString(array[i], 16).PadLeft(2, '0');
			}
			return text.PadLeft(32, '0');
		}

		public static string GetCustomLevelKey(string filename)
		{
			return "custom_" + Md5Sum(filename);
		}

		public static string GetCustomLevelFilename(string key)
		{
			return key.Substring("custom_".Length);
		}

		public static bool IsCustomLevelKey(string key)
		{
			return key.StartsWith("custom_");
		}

		public static string GetItemIdFromFilename(string filename)
		{
			string[] array = filename.Split('/');
			if (array.Length >= 2)
			{
				return array[array.Length - 2];
			}
			return filename;
		}

		public static void LoadImageIntoTexture(string filename, Texture2D texture)
		{
			try
			{
				byte[] array = File.ReadAllBytes(filename);
				if (array.Length <= 10485760)
				{
					texture.LoadImage(array);
				}
			}
			catch (Exception)
			{
				Debug.Log("Error reading file " + filename);
			}
		}

		public static string StripMinorMinorFromVersion(string version)
		{
			return version.Remove(version.LastIndexOf('.'));
		}
	}
}
