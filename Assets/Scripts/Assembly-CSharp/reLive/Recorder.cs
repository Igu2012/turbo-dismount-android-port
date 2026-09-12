#pragma warning disable 0618,0619
using System.Collections.Generic;
using Dismount;
using Dismount.Vehicular;
using UnityEngine;

namespace reLive
{
	public class Recorder : MonoBehaviour
	{
		public GameObject SourceGameObject;

		public GameObject SourceParentObject;

		public string[] RecorderChannelNames = new string[0];

		private ParticleSystem cachedParticleSystem;

		private IRecorderChannel[] channels = new IRecorderChannel[0];

		private bool didJustActivate;

		private int startFrameOffset = -1;

		private int endFrameOffset = -1;

		private int recordFrame = -1;

		private int blockCount;

		private int frameCount;

		public int AllocatedMemory
		{
			get
			{
				int num = 0;
				for (int i = 0; i < channels.Length; i++)
				{
					num += channels[i].GetAllocatedMemory();
				}
				return num;
			}
		}

		public bool DidJustActivate
		{
			get
			{
				return didJustActivate;
			}
		}

		public int startFrameIndex
		{
			get
			{
				return startFrameOffset;
			}
		}

		public int endFrameIndex
		{
			get
			{
				return endFrameOffset;
			}
		}

		public void PrepareForRecording()
		{
			List<IRecorderChannel> list = new List<IRecorderChannel>();
			if ((bool)GetComponent<RecordLocalTransformation>())
			{
				list.Add(new LocalTransfomChannel());
				Object.Destroy(GetComponent<RecordLocalTransformation>());
			}
			else
			{
				list.Add(new TransformChannel());
			}
			if ((bool)GetComponent<ExtendedLocalEulers>())
			{
				list.Add(new ExtendedLocalEulersChannel());
				Object.Destroy(GetComponent<ExtendedLocalEulers>());
			}
			if ((bool)base.GetComponent<AudioSource>())
			{
				list.Add(new AudioSourceChannel());
			}
			if ((bool)base.GetComponent<Camera>())
			{
				list.Add(new CameraChannel());
			}
			if ((bool)base.GetComponent<ParticleSystem>())
			{
				cachedParticleSystem = base.GetComponent<ParticleSystem>();
				if ((bool)GetComponent<RecordIndividualParticles>())
				{
					list.Add(new IndividualParticleSystemChannel());
					base.gameObject.AddComponent<KeepInMainCamera>();
					Object.Destroy(GetComponent<RecordIndividualParticles>());
				}
				else
				{
					list.Add(new ParticleSystemChannel());
				}
				if (!GetComponent<KeepInMainCamera>())
				{
					base.gameObject.AddComponent<KeepInMainCamera>();
				}
			}
			RecordMaterialProperty[] components = base.gameObject.GetComponents<RecordMaterialProperty>();
			RecordMaterialProperty[] array = components;
			foreach (RecordMaterialProperty recordMaterialProperty in array)
			{
				if (recordMaterialProperty.PropertyType == RecordMaterialProperty.PType.Float)
				{
					MaterialFloatChannel materialFloatChannel = new MaterialFloatChannel();
					materialFloatChannel.MaterialIndex = recordMaterialProperty.MaterialIndex;
					materialFloatChannel.PropertyName = recordMaterialProperty.PropertyName;
					list.Add(materialFloatChannel);
				}
				else if (recordMaterialProperty.PropertyType == RecordMaterialProperty.PType.Color)
				{
					MaterialColorChannel materialColorChannel = new MaterialColorChannel();
					materialColorChannel.MaterialIndex = recordMaterialProperty.MaterialIndex;
					materialColorChannel.PropertyName = recordMaterialProperty.PropertyName;
					list.Add(materialColorChannel);
				}
				Object.Destroy(recordMaterialProperty);
			}
			if ((bool)GetComponent<RecordMaterialTexCoords>())
			{
				list.Add(new MaterialTexCoordsChannel());
				Object.Destroy(GetComponent<RecordMaterialTexCoords>());
			}
			if ((bool)GetComponent<RecordPoleBender>())
			{
				list.Add(new PoleBenderChannel());
				Object.Destroy(GetComponent<RecordPoleBender>());
			}
			if ((bool)GetComponent<DismountGameProxy>())
			{
				list.Add(new DismountDataChannel());
				Object.Destroy(GetComponent<DismountGameProxy>());
			}
			if ((bool)GetComponent<UISprite>())
			{
				list.Add(new UISpriteChannel());
			}
			if ((bool)GetComponent<Skidmark>())
			{
				list.Add(new SkidmarkChannel());
			}
			if ((bool)GetComponent<NPCWheelRotation>())
			{
				list.Add(new NPCWheelRotationChannel());
			}
			channels = list.ToArray();
			RecorderChannelNames = new string[channels.Length];
			for (int j = 0; j < channels.Length; j++)
			{
				IRecorderChannel recorderChannel = channels[j];
				RecorderChannelNames[j] = recorderChannel.GetType().ToString();
				recorderChannel.Init(SourceGameObject, base.gameObject);
				recorderChannel.AllocateBlock();
			}
			blockCount = 1;
		}

		public void SourceDestroyed()
		{
			endFrameOffset = startFrameOffset + recordFrame;
			SourceGameObject = null;
		}

		public void Freeze()
		{
			if ((bool)base.GetComponent<AudioSource>() && base.GetComponent<AudioSource>().enabled)
			{
				base.GetComponent<AudioSource>().pitch = 0f;
			}
			if ((bool)cachedParticleSystem && cachedParticleSystem.isPlaying)
			{
				cachedParticleSystem.playbackSpeed = 0f;
			}
		}

		public void RecordFrame(int currReplayFrame)
		{
			if (startFrameOffset < 0)
			{
				startFrameOffset = currReplayFrame;
			}
			if (endFrameOffset >= 0)
			{
				return;
			}
			recordFrame = currReplayFrame - startFrameOffset;
			if (!SourceGameObject || recordFrame < 0 || recordFrame > frameCount)
			{
				return;
			}
			frameCount = recordFrame + 1;
			int num = recordFrame;
			int num2 = num / Replay.BlockSize;
			num %= Replay.BlockSize;
			if (num2 >= blockCount)
			{
				for (int i = 0; i < channels.Length; i++)
				{
					IRecorderChannel recorderChannel = channels[i];
					recorderChannel.PostProcessBlock();
					recorderChannel.AllocateBlock();
				}
				blockCount++;
			}
			int num3 = channels.Length;
			for (int j = 0; j < num3; j++)
			{
				channels[j].RecordFrame(num);
			}
		}

		public void PostprocessRecording()
		{
			if (endFrameOffset == -1)
			{
				endFrameOffset = startFrameOffset + recordFrame;
			}
			for (int i = 0; i < channels.Length; i++)
			{
				IRecorderChannel recorderChannel = channels[i];
				recorderChannel.PostProcessRecording();
			}
		}

		public void PlaybackFrame(int currReplayFrame, float playbackSpeed)
		{
			int num = currReplayFrame - startFrameOffset;
			if (num < 0 || num >= frameCount)
			{
				if (base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(false);
				}
				return;
			}
			int frameIndex = num;
			didJustActivate = false;
			bool activeInHierarchy = base.gameObject.activeInHierarchy;
			channels[0].PlaybackFrame(frameIndex, playbackSpeed);
			didJustActivate = !activeInHierarchy && base.gameObject.activeInHierarchy;
			for (int i = 1; i < channels.Length; i++)
			{
				channels[i].PlaybackFrame(frameIndex, playbackSpeed);
			}
			if ((bool)cachedParticleSystem)
			{
				cachedParticleSystem.playbackSpeed = playbackSpeed;
			}
		}

		public void PlaybackFrameInterpolated(int prevReplayFrame, int nextReplayFrame, float delta, float playbackSpeed)
		{
			int num = prevReplayFrame - startFrameOffset;
			int num2 = nextReplayFrame - startFrameOffset;
			if (num < 0 || num2 >= frameCount)
			{
				if (base.gameObject.activeInHierarchy)
				{
					base.gameObject.SetActive(false);
				}
				return;
			}
			int prevFrameIndex = num;
			int nextFrameIndex = num2;
			didJustActivate = false;
			bool activeInHierarchy = base.gameObject.activeInHierarchy;
			channels[0].PlaybackFrameInterpolated(prevFrameIndex, nextFrameIndex, delta, playbackSpeed);
			didJustActivate = !activeInHierarchy && base.gameObject.activeInHierarchy;
			for (int i = 1; i < channels.Length; i++)
			{
				channels[i].PlaybackFrameInterpolated(prevFrameIndex, nextFrameIndex, delta, playbackSpeed);
			}
		}
	}
}
