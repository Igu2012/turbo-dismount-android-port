#pragma warning disable 0618,0619
namespace SXXcodeApi
{
	public class PlistElementInteger : PlistElement
	{
		public int value;

		public PlistElementInteger(int v)
		{
			value = v;
		}
	}
}
