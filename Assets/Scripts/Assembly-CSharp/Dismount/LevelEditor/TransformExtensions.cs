#pragma warning disable 0618,0619
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dismount.LevelEditor
{
	public static class TransformExtensions
	{
		public static string GetPath(this Transform current)
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<Transform> list = new List<Transform>();
			list.Add(current);
			Transform parent = current.parent;
			while ((bool)parent)
			{
				list.Add(parent);
				parent = parent.parent;
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				stringBuilder.Append("/");
				stringBuilder.Append(list[num].name);
			}
			return stringBuilder.ToString();
		}

		public static int GetDepth(this Transform current)
		{
			int num = 0;
			Transform parent = current.parent;
			while ((bool)parent)
			{
				num++;
				parent = parent.parent;
			}
			return num;
		}
	}
}
