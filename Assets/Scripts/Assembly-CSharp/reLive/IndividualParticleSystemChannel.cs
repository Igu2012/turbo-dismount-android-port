#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

namespace reLive
{
	public class IndividualParticleSystemChannel : RecorderChannel<IndividualParticleSystemChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public ParticleSystem.Particle[] particles;

			public int[] prevFrameParticles;

			public int[] nextFrameParticles;

			public int particleCount;
		}

		public class ParticleLookupEntry
		{
			public int index;

			public float startLifeTime;
		}

		private const int particleSize = 52;

		private const int frameFieldSize = 28;

		private int maxRecordedParticles = 1024;

		private ParticleSystem sourceParticleSystem;

		private ParticleSystem targetParticleSystem;

		private ParticleSystem.Particle[] tempParticles;

		private static Pool<ParticleLookupEntry> lookupEntryPool;

		private static Pool<List<ParticleLookupEntry>> lookupEntryListPool;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			if (sourceObject.name.CompareTo("FlameParticleSystem") == 0)
			{
				maxRecordedParticles = 2048;
			}
			sourceParticleSystem = sourceGameObject.GetComponent<ParticleSystem>();
			targetParticleSystem = targetGameObject.GetComponent<ParticleSystem>();
			targetParticleSystem.playbackSpeed = 0f;
			tempParticles = new ParticleSystem.Particle[maxRecordedParticles];
			allocatedMemory += maxRecordedParticles * 52;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.particleCount = sourceParticleSystem.particleCount;
			if (frame.particleCount > maxRecordedParticles)
			{
				frame.particleCount = maxRecordedParticles;
			}
			frame.particles = new ParticleSystem.Particle[frame.particleCount];
			frame.particleCount = sourceParticleSystem.GetParticles(frame.particles);
			if (sourceParticleSystem.simulationSpace == ParticleSystemSimulationSpace.Local)
			{
				for (int i = 0; i < frame.particleCount; i++)
				{
					frame.particles[i].position = sourceParticleSystem.transform.TransformPoint(frame.particles[i].position);
				}
			}
			allocatedMemory += frame.particleCount * 52;
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			targetParticleSystem.simulationSpace = ParticleSystemSimulationSpace.World;
			targetParticleSystem.SetParticles(frame.particles, frame.particleCount);
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			targetParticleSystem.simulationSpace = ParticleSystemSimulationSpace.World;
			Frame frame = ((!(playbackSpeed < 0f)) ? prevFrame : nextFrame);
			frame.particles.CopyTo(tempParticles, 0);
			if (frame.particleCount == 0)
			{
				targetParticleSystem.SetParticles(tempParticles, 0);
				return;
			}
			if (playbackSpeed < 0f)
			{
				for (int i = 0; i < frame.particleCount; i++)
				{
					int num = frame.prevFrameParticles[i];
					if (num >= 0)
					{
						tempParticles[i].color = Color32.Lerp(prevFrame.particles[num].color, frame.particles[i].color, delta);
						tempParticles[i].remainingLifetime = Mathf.Lerp(prevFrame.particles[num].remainingLifetime, frame.particles[i].remainingLifetime, delta);
						tempParticles[i].position = Vector3.Lerp(prevFrame.particles[num].position, frame.particles[i].position, delta);
						tempParticles[i].rotation = Mathf.Lerp(prevFrame.particles[num].rotation, frame.particles[i].rotation, delta);
						tempParticles[i].size = Mathf.Lerp(prevFrame.particles[num].size, frame.particles[i].size, delta);
					}
				}
			}
			else
			{
				for (int j = 0; j < frame.particleCount; j++)
				{
					int num2 = frame.nextFrameParticles[j];
					if (num2 >= 0)
					{
						tempParticles[j].color = Color32.Lerp(frame.particles[j].color, nextFrame.particles[num2].color, delta);
						tempParticles[j].remainingLifetime = Mathf.Lerp(frame.particles[j].remainingLifetime, nextFrame.particles[num2].remainingLifetime, delta);
						tempParticles[j].position = Vector3.Lerp(frame.particles[j].position, nextFrame.particles[num2].position, delta);
						tempParticles[j].rotation = Mathf.Lerp(frame.particles[j].rotation, nextFrame.particles[num2].rotation, delta);
						tempParticles[j].size = Mathf.Lerp(frame.particles[j].size, nextFrame.particles[num2].size, delta);
					}
				}
			}
			targetParticleSystem.SetParticles(tempParticles, frame.particleCount);
		}

		public override void AllocateBlock()
		{
			SequentialBlock<Frame> sequentialBlock = new SequentialBlock<Frame>();
			if (runningBlockFrames == null)
			{
				runningBlockFrames = new Frame[Replay.BlockSize];
				for (int i = 0; i < Replay.BlockSize; i++)
				{
					runningBlockFrames[i] = new Frame();
					runningBlockFrames[i].particleCount = 0;
				}
				allocatedMemory += Replay.BlockSize * 28;
			}
			sequentialBlock.dataBlockIndex = blocks.Count;
			blocks.Add(sequentialBlock);
			allocatedMemory += 12;
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.particleCount == 0 && frame2.particleCount == 0)
			{
				return true;
			}
			return false;
		}

		public override void PostProcessRecording()
		{
			if (lookupEntryPool == null)
			{
				lookupEntryPool = new Pool<ParticleLookupEntry>();
				lookupEntryListPool = new Pool<List<ParticleLookupEntry>>();
				lookupEntryPool.Prealloc(100);
				lookupEntryListPool.Prealloc(100);
			}
			base.PostProcessRecording();
			int num = -1;
			int num2 = 1;
			int num3 = blocks.Count * Replay.BlockSize;
			int num4 = 0;
			num4 = 0;
			while (num4 < num3)
			{
				Frame playbackFrame = GetPlaybackFrame(blocks, num4);
				if (playbackFrame.particleCount != 0)
				{
					if (num >= 0)
					{
						Frame playbackFrame2 = GetPlaybackFrame(blocks, num);
						playbackFrame.prevFrameParticles = new int[playbackFrame.particleCount];
						Dictionary<uint, List<ParticleLookupEntry>> dictionary = new Dictionary<uint, List<ParticleLookupEntry>>();
						for (int i = 0; i < playbackFrame2.particleCount; i++)
						{
							uint randomSeed = playbackFrame2.particles[i].randomSeed;
							List<ParticleLookupEntry> value = null;
							if (!dictionary.TryGetValue(randomSeed, out value))
							{
								value = lookupEntryListPool.Get();
								value.Clear();
								dictionary[randomSeed] = value;
							}
							ParticleLookupEntry particleLookupEntry = lookupEntryPool.Get();
							particleLookupEntry.index = i;
							particleLookupEntry.startLifeTime = playbackFrame2.particles[i].startLifetime;
							value.Add(particleLookupEntry);
						}
						for (int j = 0; j < playbackFrame.particleCount; j++)
						{
							playbackFrame.prevFrameParticles[j] = -1;
							List<ParticleLookupEntry> value2 = null;
							if (!dictionary.TryGetValue(playbackFrame.particles[j].randomSeed, out value2))
							{
								continue;
							}
							for (int k = 0; k < value2.Count; k++)
							{
								if (playbackFrame.particles[j].startLifetime == value2[k].startLifeTime)
								{
									playbackFrame.prevFrameParticles[j] = value2[k].index;
									break;
								}
							}
						}
						lookupEntryPool.ReclaimItems();
						lookupEntryListPool.ReclaimItems();
					}
					if (num2 < num3)
					{
						Frame playbackFrame3 = GetPlaybackFrame(blocks, num2);
						playbackFrame.nextFrameParticles = new int[playbackFrame.particleCount];
						Dictionary<uint, List<ParticleLookupEntry>> dictionary2 = new Dictionary<uint, List<ParticleLookupEntry>>();
						for (int l = 0; l < playbackFrame3.particleCount; l++)
						{
							uint randomSeed2 = playbackFrame3.particles[l].randomSeed;
							List<ParticleLookupEntry> value3 = null;
							if (!dictionary2.TryGetValue(randomSeed2, out value3))
							{
								value3 = lookupEntryListPool.Get();
								value3.Clear();
								dictionary2[randomSeed2] = value3;
							}
							ParticleLookupEntry particleLookupEntry2 = lookupEntryPool.Get();
							particleLookupEntry2.index = l;
							particleLookupEntry2.startLifeTime = playbackFrame3.particles[l].startLifetime;
							value3.Add(particleLookupEntry2);
						}
						for (int m = 0; m < playbackFrame.particleCount; m++)
						{
							playbackFrame.nextFrameParticles[m] = -1;
							List<ParticleLookupEntry> value4 = null;
							if (!dictionary2.TryGetValue(playbackFrame.particles[m].randomSeed, out value4))
							{
								continue;
							}
							for (int n = 0; n < value4.Count; n++)
							{
								if (playbackFrame.particles[m].startLifetime == value4[n].startLifeTime)
								{
									playbackFrame.nextFrameParticles[m] = value4[n].index;
									break;
								}
							}
						}
						lookupEntryPool.ReclaimItems();
						lookupEntryListPool.ReclaimItems();
					}
					SetPlaybackFrame(blocks, num4, playbackFrame);
				}
				num4++;
				num++;
				num2++;
			}
			int startFrameIndex = recorder.startFrameIndex;
			int endFrameIndex = recorder.endFrameIndex;
			int[,] array = new int[17, maxRecordedParticles];
			List<int> updateFrames = replay.UpdateFrames;
			num4 = 0;
			while (num4 < updateFrames.Count - 1 && updateFrames[num4] < endFrameIndex)
			{
				if (updateFrames[num4] < startFrameIndex)
				{
					num4++;
					continue;
				}
				int num5 = updateFrames[num4] - startFrameIndex;
				int num6 = updateFrames[num4 + 1] - startFrameIndex;
				if (num6 - num5 <= 1)
				{
					num4++;
					continue;
				}
				if (num6 - num5 >= 16)
				{
					num4++;
					continue;
				}
				float num7 = 1f / (float)(num6 - num5);
				float num8 = 0f;
				Frame playbackFrame4 = GetPlaybackFrame(blocks, num5);
				Frame playbackFrame5 = GetPlaybackFrame(blocks, num6);
				int particleCount = playbackFrame4.particleCount;
				for (int num9 = 0; num9 < num6 - num5 + 1; num9++)
				{
					for (int num10 = 0; num10 < particleCount; num10++)
					{
						array[num9, num10] = -1;
					}
				}
				for (int num11 = 0; num11 < num6 - num5; num11++)
				{
					Frame playbackFrame6 = GetPlaybackFrame(blocks, num5 + num11);
					if (playbackFrame6.nextFrameParticles == null)
					{
						for (int num12 = 0; num12 < particleCount; num12++)
						{
							array[0, num12] = -1;
						}
						break;
					}
					if (num11 == 0)
					{
						for (int num13 = 0; num13 < particleCount; num13++)
						{
							array[num11, num13] = num13;
						}
					}
					for (int num14 = 0; num14 < particleCount; num14++)
					{
						if (array[0, num14] != -1)
						{
							array[num11 + 1, num14] = playbackFrame6.nextFrameParticles[array[num11, num14]];
							if (array[num11 + 1, num14] == -1)
							{
								array[0, num14] = -1;
							}
						}
					}
				}
				int num15 = num5 + 1;
				int num16 = 1;
				while (num15 < num6)
				{
					Frame playbackFrame7 = GetPlaybackFrame(blocks, num15);
					num8 += num7;
					for (int num17 = 0; num17 < particleCount; num17++)
					{
						if (array[0, num17] != -1)
						{
							int num18 = num17;
							int num19 = array[num6 - num5, num17];
							int num20 = array[num16, num17];
							playbackFrame7.particles[num20].color = Color32.Lerp(playbackFrame4.particles[num18].color, playbackFrame5.particles[num19].color, num8);
							playbackFrame7.particles[num20].remainingLifetime = Mathf.Lerp(playbackFrame4.particles[num18].remainingLifetime, playbackFrame5.particles[num19].remainingLifetime, num8);
							playbackFrame7.particles[num20].position = Vector3.Lerp(playbackFrame4.particles[num18].position, playbackFrame5.particles[num19].position, num8);
							playbackFrame7.particles[num20].rotation = Mathf.Lerp(playbackFrame4.particles[num18].rotation, playbackFrame5.particles[num19].rotation, num8);
							playbackFrame7.particles[num20].size = Mathf.Lerp(playbackFrame4.particles[num18].size, playbackFrame5.particles[num19].size, num8);
						}
					}
					SetPlaybackFrame(blocks, num15, playbackFrame7);
					num15++;
					num16++;
				}
				num4++;
			}
		}
	}
}
