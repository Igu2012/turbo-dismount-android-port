#pragma warning disable 0618,0619
using System.Text;

namespace SXXcodeApi.PBX
{
	internal abstract class SectionBase
	{
		public abstract void AddObject(string key, PBXElementDict value);

		public abstract void WriteSection(StringBuilder sb, GUIDToCommentMap comments);
	}
}
