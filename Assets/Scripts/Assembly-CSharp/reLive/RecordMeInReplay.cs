#pragma warning disable 0618,0619
using UnityEngine;

namespace reLive
{
	public class RecordMeInReplay : MonoBehaviour
	{
		private bool warmedUp;

		private void Start()
		{
			if (!warmedUp)
			{
				Warmup();
			}
		}

		public void Warmup()
		{
			Replay.Instance.AddObjectForRecording(base.gameObject);
			warmedUp = true;
		}

		private void OnDestroy()
		{
			Replay.Instance.RemoveRecordedObject(base.gameObject);
		}
	}
}
