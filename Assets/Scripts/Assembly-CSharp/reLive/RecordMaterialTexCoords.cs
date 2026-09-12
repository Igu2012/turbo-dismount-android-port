#pragma warning disable 0618,0619
using UnityEngine;

namespace reLive
{
	[RequireComponent(typeof(Renderer))]
	public class RecordMaterialTexCoords : MonoBehaviour
	{
		public int MaterialIndex;

		public bool Interpolate = true;
	}
}
