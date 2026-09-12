#pragma warning disable 0618,0619
using UnityEngine;

namespace reLive
{
	public interface IRecorderChannel
	{
		void Init(GameObject sourceObject, GameObject targetObject);

		void RecordFrame(int frameIndex);

		void PlaybackFrame(int frameIndex, float playbackSpeed);

		void PlaybackFrameInterpolated(int prevFrameIndex, int nextFrameIndex, float delta, float playbackSpeed);

		void AllocateBlock();

		void PostProcessBlock();

		void PostProcessRecording();

		int GetAllocatedMemory();
	}
}
