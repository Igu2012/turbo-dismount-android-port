#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace reLive
{
	public class PoleBenderChannel : RecorderChannel<PoleBenderChannel.Frame>
	{
		[Serializable]
		public class Frame
		{
			public Vector4 node0;

			public Vector4 node1;

			public Vector4 node2;
		}

		private Material sourceMaterial;

		private Material targetMaterial;

		private int node0Id;

		private int node1Id;

		private int node2Id;

		public override void Init(GameObject sourceObject, GameObject targetObject)
		{
			base.Init(sourceObject, targetObject);
			sourceMaterial = sourceGameObject.GetComponent<Renderer>().material;
			targetMaterial = targetGameObject.GetComponent<Renderer>().material;
			node0Id = Shader.PropertyToID("_Node0");
			node1Id = Shader.PropertyToID("_Node1");
			node2Id = Shader.PropertyToID("_Node2");
		}

		protected override int GetFrameSize()
		{
			return 48;
		}

		protected override void RecordFrame(ref Frame frame)
		{
			frame.node0 = sourceMaterial.GetVector(node0Id);
			frame.node1 = sourceMaterial.GetVector(node1Id);
			frame.node2 = sourceMaterial.GetVector(node2Id);
		}

		protected override void PlaybackFrame(ref Frame frame, float playbackSpeed)
		{
			targetMaterial.SetVector(node0Id, frame.node0);
			targetMaterial.SetVector(node1Id, frame.node1);
			targetMaterial.SetVector(node2Id, frame.node2);
		}

		protected override void PlaybackFrameInterpolated(ref Frame prevFrame, ref Frame nextFrame, float delta, float playbackSpeed)
		{
			targetMaterial.SetVector(node0Id, Vector4.Lerp(prevFrame.node0, nextFrame.node0, delta));
			targetMaterial.SetVector(node1Id, Vector4.Lerp(prevFrame.node1, nextFrame.node1, delta));
			targetMaterial.SetVector(node2Id, Vector4.Lerp(prevFrame.node2, nextFrame.node2, delta));
		}

		protected override bool CheckSimilar(ref Frame frame1, ref Frame frame2)
		{
			if (frame1.node0 == frame2.node0 && frame1.node1 == frame2.node1 && frame1.node2 == frame2.node2)
			{
				return true;
			}
			return false;
		}
	}
}
