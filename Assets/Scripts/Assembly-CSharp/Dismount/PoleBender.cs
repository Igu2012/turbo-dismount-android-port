#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(MeshRenderer))]
	public class PoleBender : MonoBehaviour
	{
		public Transform node0;

		public Transform node1;

		public Transform node2;

		private Transform xf;

		private MeshRenderer mr;

		private Material mat;

		private int n0ID;

		private int n1ID;

		private int n2ID;

		private void Start()
		{
			xf = GetComponent<Transform>();
			mr = GetComponent<MeshRenderer>();
			mat = mr.material;
			n0ID = Shader.PropertyToID("_Node0");
			n1ID = Shader.PropertyToID("_Node1");
			n2ID = Shader.PropertyToID("_Node2");
		}

		private void FixedUpdate()
		{
			UpdateBend();
		}

		private void LateUpdate()
		{
			UpdateBend();
		}

		private void UpdateBend()
		{
			if (!(mat == null))
			{
				Vector3 vector = xf.InverseTransformPoint(node0.position);
				Vector3 vector2 = xf.InverseTransformPoint(node1.position);
				Vector3 vector3 = xf.InverseTransformPoint(node2.position);
				mat.SetVector(n0ID, new Vector4(vector.x, vector.y, vector.z));
				mat.SetVector(n1ID, new Vector4(vector2.x, vector2.y, vector2.z));
				mat.SetVector(n2ID, new Vector4(vector3.x, vector3.y, vector3.z));
			}
		}
	}
}
