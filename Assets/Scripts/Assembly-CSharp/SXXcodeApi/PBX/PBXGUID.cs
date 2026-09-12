#pragma warning disable 0618,0619
using System;

namespace SXXcodeApi.PBX
{
	internal class PBXGUID
	{
		internal delegate string GuidGenerator();

		private static GuidGenerator guidGenerator = DefaultGuidGenerator;

		internal static string DefaultGuidGenerator()
		{
			return Guid.NewGuid().ToString("N").Substring(8)
				.ToUpper();
		}

		internal static void SetGuidGenerator(GuidGenerator generator)
		{
			guidGenerator = generator;
		}

		public static string Generate()
		{
			return guidGenerator();
		}
	}
}
