#pragma warning disable 0618,0619
namespace SXXcodeApi
{
	public class PlistElementString : PlistElement
	{
		public string value;

		public PlistElementString(string v)
		{
			value = v;
		}
	}
}
