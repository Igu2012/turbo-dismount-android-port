#pragma warning disable 0618,0619
using UnityEngine;

namespace CarEngineTest
{
	public class PersistentState : MonoBehaviour
	{
		public int vehicleIndex;

		public int trackIndex;

		public bool dismountControls = true;

		public bool showDebug;

		private static PersistentState instance;

		private void Start()
		{
			if ((bool)instance)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}
}
