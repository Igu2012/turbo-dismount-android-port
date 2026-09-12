#pragma warning disable 0618,0619
using UnityEngine;

namespace reLive
{
	[RequireComponent(typeof(Renderer))]
	public class RecordMaterialProperty : MonoBehaviour
	{
		public enum PType
		{
			Float = 0,
			Color = 1
		}

		public int MaterialIndex;

		public PType PropertyType;

		public string PropertyName = "_Color";
	}
}
