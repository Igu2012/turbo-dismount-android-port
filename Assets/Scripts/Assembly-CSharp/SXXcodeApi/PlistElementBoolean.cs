#pragma warning disable 0618,0619
namespace SXXcodeApi
{
	public class PlistElementBoolean : PlistElement
	{
		public bool value;

		public PlistElementBoolean(bool v)
		{
			value = v;
		}
	}
}
