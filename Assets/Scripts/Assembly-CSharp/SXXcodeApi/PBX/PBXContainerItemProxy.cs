#pragma warning disable 0618,0619
namespace SXXcodeApi.PBX
{
	internal class PBXContainerItemProxy : PBXObject
	{
		private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[1] { "containerPortal/*" });

		internal override PropertyCommentChecker checker
		{
			get
			{
				return checkerData;
			}
		}

		public static PBXContainerItemProxy Create(string containerRef, string proxyType, string remoteGlobalGUID, string remoteInfo)
		{
			PBXContainerItemProxy pBXContainerItemProxy = new PBXContainerItemProxy();
			pBXContainerItemProxy.guid = PBXGUID.Generate();
			pBXContainerItemProxy.SetPropertyString("isa", "PBXContainerItemProxy");
			pBXContainerItemProxy.SetPropertyString("containerPortal", containerRef);
			pBXContainerItemProxy.SetPropertyString("proxyType", proxyType);
			pBXContainerItemProxy.SetPropertyString("remoteGlobalIDString", remoteGlobalGUID);
			pBXContainerItemProxy.SetPropertyString("remoteInfo", remoteInfo);
			return pBXContainerItemProxy;
		}
	}
}
