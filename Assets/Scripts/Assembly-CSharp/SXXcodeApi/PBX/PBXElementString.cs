#pragma warning disable 0618,0619
namespace SXXcodeApi.PBX
{
	internal class PBXElementString : PBXElement
	{
		public string value;

		public PBXElementString(string v)
		{
			value = v;
		}
	}
}
