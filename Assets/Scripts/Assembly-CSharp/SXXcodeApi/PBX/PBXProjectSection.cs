#pragma warning disable 0618,0619
using System.Collections.Generic;

namespace SXXcodeApi.PBX
{
	internal class PBXProjectSection : KnownSectionBase<PBXProjectObject>
	{
		public PBXProjectObject project
		{
			get
			{
				using (IEnumerator<KeyValuePair<string, PBXProjectObject>> enumerator = GetEntries().GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						return enumerator.Current.Value;
					}
				}
				return null;
			}
		}

		public PBXProjectSection()
			: base("PBXProject")
		{
		}
	}
}
