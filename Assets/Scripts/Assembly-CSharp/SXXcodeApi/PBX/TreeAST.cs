#pragma warning disable 0618,0619
using System.Collections.Generic;

namespace SXXcodeApi.PBX
{
	internal class TreeAST : ValueAST
	{
		public List<KeyValueAST> values = new List<KeyValueAST>();
	}
}
