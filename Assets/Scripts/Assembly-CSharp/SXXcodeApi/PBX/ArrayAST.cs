#pragma warning disable 0618,0619
using System.Collections.Generic;

namespace SXXcodeApi.PBX
{
	internal class ArrayAST : ValueAST
	{
		public List<ValueAST> values = new List<ValueAST>();
	}
}
