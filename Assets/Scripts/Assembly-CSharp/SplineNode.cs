#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SuperSplines/Spline Node")]
public class SplineNode : MonoBehaviour
{
	public sealed class NodeParameterRegister : Dictionary<Spline, NodeParameters>
	{
		public new NodeParameters this[Spline spline]
		{
			get
			{
				if (!ContainsKey(spline))
				{
					Add(spline, new NodeParameters(spline, 0f, 0f));
				}
				return base[spline];
			}
		}
	}

	public float customValue;

	public float tension = 1f;

	public Vector3 normal = Vector3.up;

	private NodeParameterRegister parameters = new NodeParameterRegister();

	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public Quaternion Rotation
	{
		get
		{
			return base.transform.rotation;
		}
		set
		{
			base.transform.rotation = value;
		}
	}

	public float CustomValue
	{
		get
		{
			return customValue;
		}
		set
		{
			customValue = value;
		}
	}

	public Vector3 TransformedNormal
	{
		get
		{
			return base.transform.TransformDirection(normal);
		}
	}

	public NodeParameterRegister Parameters
	{
		get
		{
			return parameters;
		}
	}
}
