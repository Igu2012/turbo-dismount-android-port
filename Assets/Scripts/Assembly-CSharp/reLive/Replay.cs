#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using Dismount;
using Dismount.Vehicular;
using UnityEngine;

namespace reLive
{
	public class Replay : MonoBehaviour
	{
		public enum ReplayActivity
		{
			Idle = 0,
			Record = 1,
			Playback = 2
		}

		public float RequestedRecordInterval = 0.08f;

		public int MaxReplayFrames = 1024;

		[HideInInspector]
		public static int BlockSize = 16;

		public float PlaybackSpeed = 1f;

		public bool RecordOnStart;

		public bool PlaybackInFixedUpdate;

		public bool SmoothPlayback = true;

		public bool SetCameraOnPlayback = true;

		public List<GameObject> GameObjectsToBeRecorded = new List<GameObject>();

		public GameObject MessageListener;

		private GameObject replayCloneRoot;

		private Dictionary<GameObject, GameObject> recordedObjects = new Dictionary<GameObject, GameObject>();

		private List<Recorder> recorders = new List<Recorder>();

		private float[] frameTimestamps;

		private List<int> updateFrames = new List<int>();

		private bool updateWasCalled = true;

		private ReplayActivity activity;

		private bool isRunning;

		private int recordFrame = -1;

		private int playbackFrame;

		private int frameCount;

		private float recordInterval = 0.08f;

		private int recordFrameInterval = 5;

		private int recordFrameIntervalCounter;

		private float recordTime;

		private float playbackTime;

		private bool playbackDidReachEnd;

		private static Replay instance;

		private Dictionary<Component, int> componentRemovedLookup = new Dictionary<Component, int>();

		private float snapshotFrameTime = -1f;

		private bool recordPostSnapshot;

		public List<int> UpdateFrames
		{
			get
			{
				return updateFrames;
			}
		}

		public ReplayActivity Activity
		{
			get
			{
				return activity;
			}
		}

		public bool IsRunning
		{
			get
			{
				return isRunning;
			}
		}

		public bool HasRecording
		{
			get
			{
				return frameCount > 0;
			}
		}

		public int PlaybackFrame
		{
			get
			{
				return playbackFrame;
			}
		}

		public int FrameCount
		{
			get
			{
				return frameCount;
			}
		}

		public float PlaybackTime
		{
			get
			{
				return playbackTime;
			}
		}

		public float RecordInterval
		{
			get
			{
				return recordInterval;
			}
		}

		public float Duration
		{
			get
			{
				if (frameCount == 0)
				{
					return 0f;
				}
				return frameTimestamps[frameCount - 1];
			}
		}

		public int AllocatedMemory
		{
			get
			{
				int num = 0;
				foreach (Recorder recorder in recorders)
				{
					num += recorder.AllocatedMemory;
				}
				return num;
			}
		}

		public static Replay Instance
		{
			get
			{
				return instance;
			}
		}

		private void Awake()
		{
			if (!instance)
			{
				instance = this;
			}
			else
			{
				Debug.LogWarning("More than one Replay added to the scene.");
			}
			frameTimestamps = new float[MaxReplayFrames];
			RecorderChannel<AudioSourceChannel.Frame>.PreallocPools(233, 566);
			RecorderChannel<CameraChannel.Frame>.PreallocPools(1, 33);
			RecorderChannel<DismountDataChannel.Frame>.PreallocPools(33, 33);
			RecorderChannel<ExtendedLocalEulersChannel.Frame>.PreallocPools(2000, 2666);
			RecorderChannel<IndividualParticleSystemChannel.Frame>.PreallocPools(3, 3);
			RecorderChannel<LocalTransfomChannel.Frame>.PreallocPools(2666, 10000);
			RecorderChannel<MaterialColorChannel.Frame>.PreallocPools(66, 500);
			RecorderChannel<MaterialFloatChannel.Frame>.PreallocPools(6, 33);
			RecorderChannel<MaterialTexCoordsChannel.Frame>.PreallocPools(6, 33);
			RecorderChannel<ParticleSystemChannel.Frame>.PreallocPools(3, 3);
			RecorderChannel<TransformChannel.Frame>.PreallocPools(5000, 8333);
			RecorderChannel<UISpriteChannel.Frame>.PreallocPools(33, 333);
		}

		public static void ReclaimChannelDataItems()
		{
			RecorderChannel<AudioSourceChannel.Frame>.ReclaimPoolItems();
			RecorderChannel<CameraChannel.Frame>.ReclaimPoolItems();
			RecorderChannel<DismountDataChannel.Frame>.ReclaimPoolItems();
			RecorderChannel<ExtendedLocalEulersChannel.Frame>.ReclaimPoolItems();
			RecorderChannel<IndividualParticleSystemChannel.Frame>.ReclaimPoolItems();
			RecorderChannel<LocalTransfomChannel.Frame>.ReclaimPoolItems();
			RecorderChannel<MaterialColorChannel.Frame>.ReclaimPoolItems();
			RecorderChannel<MaterialFloatChannel.Frame>.ReclaimPoolItems();
			RecorderChannel<MaterialTexCoordsChannel.Frame>.ReclaimPoolItems();
			RecorderChannel<ParticleSystemChannel.Frame>.ReclaimPoolItems();
			RecorderChannel<TransformChannel.Frame>.ReclaimPoolItems();
			RecorderChannel<UISpriteChannel.Frame>.ReclaimPoolItems();
		}

		private void Start()
		{
			if (RecordOnStart)
			{
				DoStartRecording();
			}
		}

		private void Update()
		{
			updateWasCalled = true;
			if (!PlaybackInFixedUpdate)
			{
				DoPlaybackFrame();
			}
		}

		private void FixedUpdate()
		{
			if (PlaybackInFixedUpdate)
			{
				DoPlaybackFrame();
			}
			DoRecordFrame();
		}

		public void StartRecording()
		{
			DoStartRecording();
			LogMemoryStart();
		}

		public void StopRecording()
		{
			if (activity != ReplayActivity.Record)
			{
				return;
			}
			foreach (Recorder recorder in recorders)
			{
				recorder.PostprocessRecording();
			}
			LogMemoryStop();
			GameObjectsToBeRecorded.Clear();
			activity = ReplayActivity.Idle;
			isRunning = false;
			recordPostSnapshot = false;
			snapshotFrameTime = -1f;
		}

		public void StopRecordingDontPostprocess()
		{
			if (activity == ReplayActivity.Record)
			{
				LogMemoryStop();
				GameObjectsToBeRecorded.Clear();
				activity = ReplayActivity.Idle;
				isRunning = false;
			}
		}

		public void StartPlayback()
		{
			DoStartPlayback(0f, true);
		}

		public void SeekPlayback(float time, bool startPlayback)
		{
			DoStartPlayback(time, startPlayback);
		}

		public void StopPlayback()
		{
			if (activity == ReplayActivity.Playback)
			{
				for (int i = 0; i < recorders.Count; i++)
				{
					recorders[i].Freeze();
				}
				isRunning = false;
			}
		}

		public void ResumePlayback()
		{
			if (!HasRecording)
			{
				return;
			}
			if (activity == ReplayActivity.Idle)
			{
				StartPlayback();
				return;
			}
			if (activity == ReplayActivity.Playback)
			{
				isRunning = true;
				return;
			}
			foreach (GameObject key in recordedObjects.Keys)
			{
				key.SetActive(false);
			}
			replayCloneRoot.SetActive(true);
			activity = ReplayActivity.Playback;
			isRunning = true;
		}

		public void AddObjectForRecording(GameObject sourceObject)
		{
			if (activity != ReplayActivity.Record || sourceObject == null)
			{
				return;
			}
			if (recordedObjects.ContainsKey(sourceObject))
			{
				Debug.LogWarning("GameObject already recorded: " + sourceObject.name);
				return;
			}
			GameObject gameObject = InstantiateReplayClone(sourceObject);
			if (!gameObject)
			{
				Debug.LogWarning("Could not instantiate replay clone for: " + sourceObject.name);
				return;
			}
			Transform[] componentsInChildren = sourceObject.transform.GetComponentsInChildren<Transform>(true);
			Transform[] componentsInChildren2 = gameObject.transform.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				GameObject gameObject2 = componentsInChildren2[i].gameObject;
				if (gameObject2.GetComponent<DontNeedRecorder>() == null)
				{
					Recorder recorder = gameObject2.AddComponent<Recorder>();
					recorder.SourceGameObject = componentsInChildren[i].gameObject;
					if ((bool)componentsInChildren[i].parent)
					{
						recorder.SourceParentObject = componentsInChildren[i].parent.gameObject;
					}
				}
			}
			foreach (Transform transform in componentsInChildren2)
			{
				if (!(transform.GetComponent<DontNeedRecorder>() == null))
				{
					continue;
				}
				string text = LayerMask.LayerToName(transform.gameObject.layer);
				Transform transform2 = replayCloneRoot.transform.Find(text);
				if (!transform2)
				{
					GameObject gameObject3 = new GameObject(text);
					gameObject3.layer = transform.gameObject.layer;
					transform2 = gameObject3.transform;
					transform2.parent = replayCloneRoot.transform;
					if (text == "HUD")
					{
						gameObject3.AddComponent<UIPanel>();
					}
				}
				AudioSource component = transform.GetComponent<AudioSource>();
				if (component != null)
				{
					component.Stop();
				}
				transform.parent = transform2;
			}
			foreach (Transform transform3 in componentsInChildren2)
			{
				ProcessReplayClone(transform3.gameObject);
			}
		}

		public void RemoveRecordedObject(GameObject recordedObject)
		{
			if (recordedObject == null)
			{
				return;
			}
			Transform[] componentsInChildren = recordedObject.transform.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				GameObject key = componentsInChildren[i].gameObject;
				if (!recordedObjects.ContainsKey(key))
				{
					continue;
				}
				GameObject gameObject = recordedObjects[key];
				if ((bool)gameObject)
				{
					Recorder component = gameObject.GetComponent<Recorder>();
					component.SourceDestroyed();
					if (component.startFrameIndex < 0)
					{
						recorders.Remove(component);
						UnityEngine.Object.Destroy(gameObject);
					}
				}
				recordedObjects.Remove(key);
			}
		}

		public GameObject GetReplayCloneFor(GameObject recordedObject)
		{
			if (recordedObjects.ContainsKey(recordedObject))
			{
				return recordedObjects[recordedObject];
			}
			return null;
		}

		public GameObject GetSourceObjectFor(GameObject replayClone)
		{
			Recorder component = replayClone.GetComponent<Recorder>();
			if ((bool)component)
			{
				return component.SourceGameObject;
			}
			return null;
		}

		private void ClearRecording()
		{
			if ((bool)replayCloneRoot)
			{
				UnityEngine.Object.Destroy(replayCloneRoot);
			}
			recorders.Clear();
			recordedObjects.Clear();
		}

		private void DoStartRecording()
		{
			activity = ReplayActivity.Record;
			recordFrameInterval = Mathf.CeilToInt(RequestedRecordInterval / Time.fixedDeltaTime);
			if (recordFrameInterval < 1)
			{
				recordFrameInterval = 1;
			}
			recordInterval = (float)recordFrameInterval * Time.fixedDeltaTime;
			ClearRecording();
			recordFrame = -1;
			recordTime = 0f;
			frameCount = 0;
			recordFrameIntervalCounter = 0;
			PrepareForRecording();
			AddObjectsFromList();
			updateFrames.Clear();
			updateWasCalled = true;
			isRunning = true;
		}

		private void PrepareForPlayback()
		{
			if (!HasRecording || activity == ReplayActivity.Playback)
			{
				return;
			}
			if (activity == ReplayActivity.Record)
			{
				StopRecording();
			}
			List<GameObject> list = new List<GameObject>();
			foreach (GameObject key in recordedObjects.Keys)
			{
				if (key == null)
				{
					Debug.LogWarning("Recorded object was destroyed without notifying Replay. Clone was " + recordedObjects[key].name);
					list.Add(key);
				}
				else
				{
					key.SetActive(false);
				}
			}
			foreach (GameObject item in list)
			{
				RemoveRecordedObject(item);
			}
			replayCloneRoot.SetActive(true);
		}

		private void DoStartPlayback(float time, bool running)
		{
			if (!HasRecording)
			{
				return;
			}
			if (activity != ReplayActivity.Playback)
			{
				PrepareForPlayback();
			}
			activity = ReplayActivity.Playback;
			isRunning = running;
			if (time <= 0f)
			{
				playbackFrame = 0;
				playbackTime = 0f;
				DoPlaybackFreezeFrame();
				return;
			}
			if (time >= Duration)
			{
				playbackFrame = frameCount - 1;
				playbackTime = frameTimestamps[playbackFrame];
				DoPlaybackFreezeFrame();
				return;
			}
			int num = 0;
			int num2 = frameCount - 1;
			while (num <= num2)
			{
				int num3 = num + num2 >> 1;
				float num4 = frameTimestamps[num3];
				if (num4 > time)
				{
					num2 = num3 - 1;
					continue;
				}
				if (num4 < time)
				{
					num = num3 + 1;
					continue;
				}
				playbackFrame = num3;
				playbackTime = frameTimestamps[playbackFrame];
			}
			playbackFrame = num;
			playbackTime = time;
			if (!isRunning)
			{
				DoPlaybackFreezeFrame();
			}
		}

		private static bool CheckRequireComponent(Component a, Component b)
		{
			Attribute[] customAttributes = Attribute.GetCustomAttributes(a.GetType());
			Attribute[] array = customAttributes;
			foreach (Attribute attribute in array)
			{
				if (attribute is RequireComponent)
				{
					RequireComponent requireComponent = (RequireComponent)attribute;
					if (requireComponent.m_Type0 == b.GetType() || requireComponent.m_Type1 == b.GetType() || requireComponent.m_Type2 == b.GetType())
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool CheckKnownTypeDependency(Component a, Component b)
		{
			if (b is Rigidbody && a is Joint)
			{
				return true;
			}
			return false;
		}

		private static int CompareComponentsForRemoval(Component a, Component b)
		{
			if (CheckKnownTypeDependency(a, b))
			{
				return -1;
			}
			if (CheckKnownTypeDependency(b, a))
			{
				return 1;
			}
			if (CheckRequireComponent(a, b))
			{
				return -1;
			}
			if (CheckRequireComponent(b, a))
			{
				return 1;
			}
			return 0;
		}

		private GameObject InstantiateReplayClone(GameObject sourceGameObject)
		{
			return (GameObject)UnityEngine.Object.Instantiate(sourceGameObject);
		}

		private void RemoveKnownDependencies(GameObject gameObject)
		{
			componentRemovedLookup.Clear();
			JointFriction[] components = gameObject.GetComponents<JointFriction>();
			for (int i = 0; i < components.Length; i++)
			{
				UnityEngine.Object.Destroy(components[i]);
				componentRemovedLookup[components[i]] = 1;
			}
			BodyPart[] components2 = gameObject.GetComponents<BodyPart>();
			for (int j = 0; j < components2.Length; j++)
			{
				UnityEngine.Object.Destroy(components2[j]);
				componentRemovedLookup[components2[j]] = 1;
			}
			DynamicPhysicsPart[] components3 = gameObject.GetComponents<DynamicPhysicsPart>();
			for (int k = 0; k < components3.Length; k++)
			{
				UnityEngine.Object.Destroy(components3[k]);
				componentRemovedLookup[components3[k]] = 1;
			}
			Joint[] components4 = gameObject.GetComponents<Joint>();
			for (int l = 0; l < components4.Length; l++)
			{
				UnityEngine.Object.Destroy(components4[l]);
				componentRemovedLookup[components4[l]] = 1;
			}
			if (gameObject.GetComponent<Rigidbody>() != null)
			{
				UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
				componentRemovedLookup[gameObject.GetComponent<Rigidbody>()] = 1;
			}
			Collider[] components5 = gameObject.GetComponents<Collider>();
			for (int m = 0; m < components5.Length; m++)
			{
				UnityEngine.Object.Destroy(components5[m]);
				componentRemovedLookup[components5[m]] = 1;
			}
		}

		private void ProcessReplayClone(GameObject cloneGameObject)
		{
			bool flag = false;
			if ((bool)cloneGameObject.GetComponent<DoNotRecordMeInReplay>())
			{
				UnityEngine.Object.Destroy(cloneGameObject);
				return;
			}
			Recorder component = cloneGameObject.GetComponent<Recorder>();
			Component[] components = cloneGameObject.GetComponents<Component>();
			Component[] array = components;
			foreach (Component component2 in array)
			{
				if (component2.GetType() == typeof(MeshRenderer) || component2.GetType() == typeof(SkinnedMeshRenderer) || component2.GetType() == typeof(AudioSource) || component2.GetType() == typeof(Camera) || component2.GetType() == typeof(Light) || component2.GetType() == typeof(Projector) || component2.GetType() == typeof(ParticleSystem) || component2.GetType() == typeof(RetainHierarchy) || component2.GetType() == typeof(RecordMeInReplay) || component2.GetType() == typeof(TreatAsSingleObject) || component2.GetType() == typeof(RecordLocalTransformation) || component2.GetType() == typeof(DismountGameProxy) || component2.GetType() == typeof(NPCWheelRotation) || component2.GetType() == typeof(NPCWheel) || component2.GetType() == typeof(UISprite))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				UnityEngine.Object.Destroy(cloneGameObject);
				return;
			}
			if (component != null)
			{
				cloneGameObject.name = component.SourceGameObject.name + cloneGameObject.GetInstanceID();
			}
			else
			{
				cloneGameObject.name += cloneGameObject.GetInstanceID();
			}
			RemoveKnownDependencies(cloneGameObject);
			List<Component> list = new List<Component>();
			Component component3 = cloneGameObject.GetComponent("Flare Layer");
			Component[] array2 = components;
			foreach (Component component4 in array2)
			{
				if (!(component4 == null) && !componentRemovedLookup.ContainsKey(component4) && component4.GetType() != typeof(Recorder) && component4.GetType() != typeof(Transform) && component4.GetType() != typeof(MeshFilter) && component4.GetType() != typeof(MeshRenderer) && component4.GetType() != typeof(SkinnedMeshRenderer) && component4.GetType() != typeof(AudioSource) && component4.GetType() != typeof(AudioListener) && component4.GetType() != typeof(Camera) && component4.GetType() != typeof(Camera) && component4.GetType() != typeof(Light) && component4.GetType() != typeof(Projector) && component4.GetType() != typeof(ProjectorBox) && component4.GetType() != typeof(ProjectorSphere) && !(component4 == component3) && component4.GetType() != typeof(Skybox) && component4.GetType() != typeof(ParticleSystem) && component4.GetType() != typeof(ParticleSystemRenderer) && component4.GetType() != typeof(RecordIndividualParticles) && component4.GetType() != typeof(RecordMaterialProperty) && component4.GetType() != typeof(RecordMaterialTexCoords) && component4.GetType() != typeof(RecordLossyScale) && component4.GetType() != typeof(RetainHierarchy) && component4.GetType() != typeof(RecordLocalTransformation) && component4.GetType() != typeof(ExtendedLocalEulers) && component4.GetType() != typeof(NPCWheelRotation) && component4.GetType() != typeof(NPCWheel) && component4.GetType() != typeof(DismountGameProxy) && component4.GetType() != typeof(UISprite) && component4.GetType() != typeof(Face) && component4.GetType() != typeof(LookAtCamera) && component4.GetType() != typeof(Skidmark) && component4.GetType() != typeof(RecordPoleBender))
				{
					list.Add(component4);
				}
			}
			list.Sort(CompareComponentsForRemoval);
			foreach (Component item in list)
			{
				UnityEngine.Object.Destroy(item);
			}
			if (component != null)
			{
				component.PrepareForRecording();
				recorders.Add(component);
				recordedObjects.Add(component.SourceGameObject, cloneGameObject);
				if ((bool)cloneGameObject.GetComponent<RecordLocalTransformation>() && (bool)component.SourceParentObject)
				{
					GameObject gameObject = recordedObjects[component.SourceParentObject];
					cloneGameObject.transform.parent = gameObject.transform;
				}
			}
		}

		private void PrepareForRecording()
		{
			GameObject gameObject = GameObject.Find("ReplayCloneRoot");
			if (!gameObject)
			{
				gameObject = new GameObject("ReplayCloneRoot");
				gameObject.transform.position = Vector3.zero;
				gameObject.transform.rotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
			}
			replayCloneRoot = gameObject;
			replayCloneRoot.SetActive(false);
			for (int i = 0; i < frameTimestamps.Length; i++)
			{
				frameTimestamps[i] = 0f;
			}
		}

		private void AddObjectsFromList()
		{
			foreach (GameObject item in GameObjectsToBeRecorded)
			{
				AddObjectForRecording(item);
			}
		}

		private void SendReplayMessage(string message)
		{
			if ((bool)MessageListener)
			{
				MessageListener.SendMessage(message, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
			}
		}

		private void DoPlaybackFreezeFrame()
		{
			if (activity == ReplayActivity.Playback)
			{
				for (int i = 0; i < recorders.Count; i++)
				{
					recorders[i].PlaybackFrame(playbackFrame, 0f);
				}
			}
		}

		private void DoPlaybackFrame()
		{
			if (activity != ReplayActivity.Playback || !isRunning)
			{
				return;
			}
			bool flag = false;
			int num = playbackFrame;
			if (PlaybackInFixedUpdate)
			{
				playbackTime += Time.fixedDeltaTime * PlaybackSpeed;
			}
			else
			{
				playbackTime += Time.deltaTime * PlaybackSpeed;
			}
			if (PlaybackSpeed > 0f)
			{
				if (playbackTime > frameTimestamps[frameCount - 1])
				{
					playbackFrame = frameCount - 1;
					playbackTime = frameTimestamps[frameCount - 1];
					flag = true;
				}
				else
				{
					while (frameTimestamps[playbackFrame] < playbackTime && playbackFrame < frameCount - 1)
					{
						playbackFrame++;
					}
				}
			}
			else if (PlaybackSpeed < 0f)
			{
				if (playbackTime < 0f)
				{
					playbackFrame = 0;
					playbackTime = frameTimestamps[0];
					flag = true;
				}
				else
				{
					while (frameTimestamps[playbackFrame] > playbackTime && playbackFrame > 0)
					{
						playbackFrame--;
					}
					if (++playbackFrame > frameCount - 1)
					{
						playbackFrame = frameCount - 1;
					}
				}
			}
			if (flag)
			{
				for (int i = 0; i < recorders.Count; i++)
				{
					recorders[i].PlaybackFrame(playbackFrame, 0f);
				}
			}
			else if (playbackFrame > 0 && playbackTime < frameTimestamps[playbackFrame] && SmoothPlayback)
			{
				float num2 = frameTimestamps[playbackFrame] - frameTimestamps[playbackFrame - 1];
				float delta = (playbackTime - frameTimestamps[playbackFrame - 1]) / num2;
				for (int j = 0; j < recorders.Count; j++)
				{
					recorders[j].PlaybackFrameInterpolated(playbackFrame - 1, playbackFrame, delta, PlaybackSpeed);
				}
			}
			else
			{
				for (int k = 0; k < recorders.Count; k++)
				{
					recorders[k].PlaybackFrame(playbackFrame, PlaybackSpeed);
				}
			}
			if (flag && playbackFrame == num && !playbackDidReachEnd)
			{
				playbackDidReachEnd = true;
				if (PlaybackSpeed < 0f)
				{
					SendReplayMessage("ReplayPlaybackReachedBeginning");
				}
				else
				{
					SendReplayMessage("ReplayPlaybackReachedEnd");
				}
			}
			else if (!flag)
			{
				playbackDidReachEnd = false;
			}
		}

		public void RecordSnapshotFrame()
		{
			if (activity == ReplayActivity.Record && isRunning)
			{
				if (snapshotFrameTime >= 0f)
				{
					recordPostSnapshot = true;
					return;
				}
				snapshotFrameTime = recordTime + Time.fixedDeltaTime * 0.5f;
				RecordTimestampedFrame(snapshotFrameTime);
			}
		}

		private void DoRecordFrame()
		{
			if (activity == ReplayActivity.Record && isRunning)
			{
				if (recordPostSnapshot)
				{
					RecordTimestampedFrame(snapshotFrameTime);
					snapshotFrameTime = -1f;
					recordPostSnapshot = false;
				}
				if (recordFrame > -1)
				{
					recordFrameIntervalCounter--;
					recordTime += Time.fixedDeltaTime;
				}
				if (recordFrameIntervalCounter <= 0)
				{
					RecordTimestampedFrame(recordTime);
					recordFrameIntervalCounter = recordFrameInterval;
				}
			}
		}

		private void RecordTimestampedFrame(float timestamp)
		{
			if (recordFrame < MaxReplayFrames - 1)
			{
				recordFrame++;
				frameTimestamps[recordFrame] = timestamp;
				for (int i = 0; i < recorders.Count; i++)
				{
					recorders[i].RecordFrame(recordFrame);
				}
				frameCount = recordFrame + 1;
				LogMemoryFrame();
				if (updateWasCalled)
				{
					updateFrames.Add(recordFrame);
					updateWasCalled = false;
				}
				if (frameCount >= MaxReplayFrames)
				{
					SendReplayMessage("ReplayRecordingReachedMaxFrames");
					StopRecording();
				}
			}
		}

		private void LogMemoryStart()
		{
		}

		private void LogMemoryFrame()
		{
		}

		private void LogMemoryStop()
		{
		}
	}
}
