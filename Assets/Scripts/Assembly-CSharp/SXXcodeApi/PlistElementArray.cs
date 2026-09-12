#pragma warning disable 0618,0619
using System.Collections.Generic;

namespace SXXcodeApi
{
	public class PlistElementArray : PlistElement
	{
		public List<PlistElement> values = new List<PlistElement>();

		public void AddString(string val)
		{
			values.Add(new PlistElementString(val));
		}

		public void AddInteger(int val)
		{
			values.Add(new PlistElementInteger(val));
		}

		public void AddBoolean(bool val)
		{
			values.Add(new PlistElementBoolean(val));
		}

		public PlistElementArray AddArray()
		{
			PlistElementArray plistElementArray = new PlistElementArray();
			values.Add(plistElementArray);
			return plistElementArray;
		}

		public PlistElementDict AddDict()
		{
			PlistElementDict plistElementDict = new PlistElementDict();
			values.Add(plistElementDict);
			return plistElementDict;
		}
	}
}
