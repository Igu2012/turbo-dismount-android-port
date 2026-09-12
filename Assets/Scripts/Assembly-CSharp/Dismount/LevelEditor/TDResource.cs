#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.LevelEditor
{
	public class TDResource : MonoBehaviour
	{
		[SerializeField]
		private string _id = "0";

		[SerializeField]
		private string _assetPath = string.Empty;

		[SerializeField]
		private string _originalAssetPath = string.Empty;

		[SerializeField]
		private string _resourceType = string.Empty;

		public string id
		{
			get
			{
				return _id;
			}
		}

		public string assetPath
		{
			get
			{
				return _assetPath;
			}
		}

		public string originalAssetPath
		{
			get
			{
				return _originalAssetPath;
			}
		}

		public string resourceType
		{
			get
			{
				return _resourceType;
			}
			set
			{
				_resourceType = value;
			}
		}

		public void AssignId(string newId, string newAssetPath)
		{
			if (!string.IsNullOrEmpty(newId))
			{
				_id = newId;
				_assetPath = newAssetPath;
				_originalAssetPath = newAssetPath;
			}
			else
			{
				_assetPath = newAssetPath;
			}
		}

		public bool IsOriginalAssetPath()
		{
			if (string.IsNullOrEmpty(_originalAssetPath))
			{
				return true;
			}
			return string.Compare(_assetPath, _originalAssetPath) == 0;
		}
	}
}
