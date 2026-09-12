#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace reLive
{
	public class RecorderChannel<T> : IRecorderChannel where T : new()
	{
		protected const int blockOverhead = 12;

		protected Replay replay;

		protected Recorder recorder;

		protected GameObject sourceGameObject;

		protected GameObject targetGameObject;

		protected List<SequentialBlock<T>> blocks = new List<SequentialBlock<T>>();

		protected T[] runningBlockFrames;

		protected int allocatedMemory;

		private T lastPlaybackFrame;

		private float lastPlaybackSpeed;

		public static ArrayPool<T> framePool = new ArrayPool<T>();

		public static Pool<SequentialBlock<T>> sequentialBlockPool = new Pool<SequentialBlock<T>>();

		public virtual void Init(GameObject sourceObject, GameObject targetObject)
		{
			replay = Replay.Instance;
			recorder = targetObject.GetComponent<Recorder>();
			sourceGameObject = sourceObject;
			targetGameObject = targetObject;
		}

		public virtual void RecordFrame(int frameIndex)
		{
			frameIndex %= Replay.BlockSize;
			T frame = runningBlockFrames[frameIndex];
			RecordFrame(ref frame);
			runningBlockFrames[frameIndex] = frame;
		}

		public virtual void PlaybackFrame(int frameIndex, float playbackSpeed)
		{
			T frame = GetPlaybackFrame(blocks, frameIndex);
			if (!object.ReferenceEquals(frame, lastPlaybackFrame) || playbackSpeed != lastPlaybackSpeed)
			{
				PlaybackFrame(ref frame, playbackSpeed);
				lastPlaybackFrame = frame;
				lastPlaybackSpeed = playbackSpeed;
			}
		}

		public virtual void PlaybackFrameInterpolated(int prevFrameIndex, int nextFrameIndex, float delta, float playbackSpeed)
		{
			T prevFrame = GetPlaybackFrame(blocks, prevFrameIndex);
			T nextFrame = GetPlaybackFrame(blocks, nextFrameIndex);
			if (!object.ReferenceEquals(prevFrame, nextFrame) || !object.ReferenceEquals(prevFrame, lastPlaybackFrame) || playbackSpeed != lastPlaybackSpeed)
			{
				PlaybackFrameInterpolated(ref prevFrame, ref nextFrame, delta, playbackSpeed);
				lastPlaybackFrame = prevFrame;
				lastPlaybackSpeed = playbackSpeed;
			}
		}

		public static void PreallocPools(int framePoolSize, int sequentialBlockPoolSize)
		{
			framePool.Prealloc(framePoolSize);
			sequentialBlockPool.Prealloc(sequentialBlockPoolSize);
		}

		public static void ReclaimPoolItems()
		{
			framePool.ReclaimItems();
			sequentialBlockPool.ReclaimItems();
		}

		public virtual void AllocateBlock()
		{
			SequentialBlock<T> sequentialBlock = sequentialBlockPool.Get();
			if (runningBlockFrames == null)
			{
				runningBlockFrames = framePool.Get();
			}
			sequentialBlock.dataBlockIndex = blocks.Count;
			blocks.Add(sequentialBlock);
		}

		public virtual void PostProcessBlock()
		{
			int num = blocks.Count - 1;
			SequentialBlock<T> sequentialBlock = blocks[num];
			if (num == 0)
			{
				sequentialBlock.frames = runningBlockFrames;
				runningBlockFrames = null;
				return;
			}
			int dataBlockIndex = blocks[num - 1].dataBlockIndex;
			SequentialBlock<T> sequentialBlock2 = blocks[dataBlockIndex];
			T frame = sequentialBlock2.frames[Replay.BlockSize - 1];
			for (int i = 0; i < Replay.BlockSize; i++)
			{
				T frame2 = runningBlockFrames[i];
				if (!CheckSimilar(ref frame, ref frame2))
				{
					sequentialBlock.frames = runningBlockFrames;
					runningBlockFrames = null;
					return;
				}
			}
			sequentialBlock.dataBlockIndex = dataBlockIndex;
		}

		public virtual void PostProcessRecording()
		{
			int index = blocks.Count - 1;
			SequentialBlock<T> sequentialBlock = blocks[index];
			sequentialBlock.frames = runningBlockFrames;
			runningBlockFrames = null;
		}

		public virtual int GetAllocatedMemory()
		{
			return allocatedMemory;
		}

		protected virtual void RecordFrame(ref T frame)
		{
		}

		protected virtual void PlaybackFrame(ref T frame, float playbackSpeed)
		{
		}

		protected virtual void PlaybackFrameInterpolated(ref T prevFrame, ref T nextFrame, float delta, float playbackSpeed)
		{
		}

		protected virtual bool CheckSimilar(ref T frame1, ref T frame2)
		{
			return false;
		}

		protected virtual int GetFrameSize()
		{
			return 0;
		}

		protected T GetPlaybackFrame(List<SequentialBlock<T>> blocks, int frameIndex)
		{
			int num = frameIndex / Replay.BlockSize;
			if (num != blocks[num].dataBlockIndex)
			{
				num = blocks[num].dataBlockIndex;
				frameIndex = Replay.BlockSize - 1;
			}
			else
			{
				frameIndex %= Replay.BlockSize;
			}
			return blocks[num].frames[frameIndex];
		}

		protected void SetPlaybackFrame(List<SequentialBlock<T>> blocks, int frameIndex, T frame)
		{
			int num = frameIndex / Replay.BlockSize;
			if (num != blocks[num].dataBlockIndex)
			{
				num = blocks[num].dataBlockIndex;
				frameIndex = Replay.BlockSize - 1;
			}
			else
			{
				frameIndex %= Replay.BlockSize;
			}
			blocks[num].frames[frameIndex] = frame;
		}
	}
}
