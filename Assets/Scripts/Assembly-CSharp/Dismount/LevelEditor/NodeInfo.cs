#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dismount.LevelEditor
{
	public class NodeInfo
	{
		public int id = -1;

		public int parentId = -1;

		public string name = string.Empty;

		public int extraDataCount;

		public Vector3 localPosition = Vector3.zero;

		public Quaternion localRotation = Quaternion.identity;

		public Vector3 localScale = Vector3.one;

		public Dictionary<string, object> ToDictionary()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("id", id.ToString());
			dictionary.Add("parentId", parentId.ToString());
			dictionary.Add("name", name);
			dictionary.Add("extraDataCount", extraDataCount.ToString());
			dictionary.Add("pos.x", localPosition.x);
			dictionary.Add("pos.y", localPosition.y);
			dictionary.Add("pos.z", localPosition.z);
			dictionary.Add("rot.x", localRotation.x);
			dictionary.Add("rot.y", localRotation.y);
			dictionary.Add("rot.z", localRotation.z);
			dictionary.Add("rot.w", localRotation.w);
			dictionary.Add("scale.x", localScale.x);
			dictionary.Add("scale.y", localScale.y);
			dictionary.Add("scale.z", localScale.z);
			return dictionary;
		}

		public void FromDictionary(Dictionary<string, object> dict)
		{
			id = Convert.ToInt32(dict["id"]);
			parentId = Convert.ToInt32(dict["parentId"]);
			name = dict["name"].ToString();
			extraDataCount = Convert.ToInt32(dict["extraDataCount"]);
			localPosition.x = (float)Convert.ToDouble(dict["pos.x"]);
			localPosition.y = (float)Convert.ToDouble(dict["pos.y"]);
			localPosition.z = (float)Convert.ToDouble(dict["pos.z"]);
			localRotation.x = (float)Convert.ToDouble(dict["rot.x"]);
			localRotation.y = (float)Convert.ToDouble(dict["rot.y"]);
			localRotation.z = (float)Convert.ToDouble(dict["rot.z"]);
			localRotation.w = (float)Convert.ToDouble(dict["rot.w"]);
			localScale.x = (float)Convert.ToDouble(dict["scale.x"]);
			localScale.y = (float)Convert.ToDouble(dict["scale.y"]);
			localScale.z = (float)Convert.ToDouble(dict["scale.z"]);
		}
	}
}
