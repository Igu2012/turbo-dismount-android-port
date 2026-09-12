#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount
{
	public class MultiplierVisualTweak : MonoBehaviour
	{
		[Serializable]
		public class MultiplierParams
		{
			public Color fillColor = Color.red;

			public Color outlineColor = Color.black;

			public float size = 60f;
		}

		public MultiplierParams[] paramsPerMultiplier;
	}
}
