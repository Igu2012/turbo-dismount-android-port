#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SuperSplines/Spline")]
public class Spline : MonoBehaviour
{
	private struct SegmentParameter
	{
		public double normalizedParam;

		public int normalizedIndex;

		public SegmentParameter(int index, double param)
		{
			normalizedParam = param;
			normalizedIndex = index;
		}
	}

	public enum TangentMode
	{
		UseNormalizedTangents = 0,
		UseTangents = 1,
		UseNodeForwardVector = 2
	}

	public enum NormalMode
	{
		UseGlobalSplineNormal = 0,
		UseNodeNormal = 1,
		UseNodeUpVector = 2
	}

	public enum RotationMode
	{
		None = 0,
		Node = 1,
		Tangent = 2
	}

	public enum InterpolationMode
	{
		Hermite = 0,
		Bezier = 1,
		BSpline = 2,
		Linear = 3,
		CustomMatrix = 4
	}

	public enum UpdateMode
	{
		DontUpdate = 0,
		EveryFrame = 1,
		EveryXFrames = 2,
		EveryXSeconds = 3
	}

	private sealed class LengthData
	{
		public double[] subSegmentLength;

		public double[] subSegmentPosition;

		public double length;

		public void Calculate(Spline spline)
		{
			int num = spline.SegmentCount * spline.interpolationAccuracy;
			double num2 = 1.0 / (double)spline.interpolationAccuracy;
			subSegmentLength = new double[num];
			subSegmentPosition = new double[num];
			length = 0.0;
			for (int i = 0; i < num; i++)
			{
				subSegmentLength[i] = 0.0;
				subSegmentPosition[i] = 0.0;
			}
			for (int j = 0; j < spline.SegmentCount; j++)
			{
				for (int k = 0; k < spline.interpolationAccuracy; k++)
				{
					int num3 = j * spline.interpolationAccuracy + k;
					subSegmentLength[num3] = spline.GetSegmentLengthInternal(j * spline.NodesPerSegment, (double)k * num2, (double)(k + 1) * num2, 0.2 * num2);
					length += subSegmentLength[num3];
				}
			}
			for (int l = 0; l < spline.SegmentCount; l++)
			{
				for (int m = 0; m < spline.interpolationAccuracy; m++)
				{
					int num4 = l * spline.interpolationAccuracy + m;
					subSegmentLength[num4] /= length;
					if (num4 < subSegmentPosition.Length - 1)
					{
						subSegmentPosition[num4 + 1] = subSegmentPosition[num4] + subSegmentLength[num4];
					}
				}
			}
			SetupSplinePositions(spline);
		}

		private void SetupSplinePositions(Spline spline)
		{
			foreach (SplineNode item in spline.splineNodesInternal)
			{
				item.Parameters[spline].Reset();
			}
			for (int i = 0; i < subSegmentLength.Length; i++)
			{
				int index = (i - i % spline.interpolationAccuracy) / spline.interpolationAccuracy * spline.NodesPerSegment;
				SplineNode splineNode = spline.splineNodesInternal[index];
				splineNode.Parameters[spline].length += subSegmentLength[i];
			}
			for (int j = 0; j < spline.splineNodesInternal.Count - spline.NodesPerSegment; j += spline.NodesPerSegment)
			{
				NodeParameters nodeParameters = spline.splineNodesInternal[j].Parameters[spline];
				spline.splineNodesInternal[j + spline.NodesPerSegment].Parameters[spline].position = nodeParameters.position + nodeParameters.length;
			}
			if (spline.IsBezier)
			{
				for (int k = 0; k < spline.splineNodesInternal.Count - spline.NodesPerSegment; k += spline.NodesPerSegment)
				{
					spline.splineNodesInternal[k + 1].Parameters[spline].position = spline.splineNodesInternal[k].Parameters[spline].position;
					spline.splineNodesInternal[k + 2].Parameters[spline].position = spline.splineNodesInternal[k].Parameters[spline].position;
					spline.splineNodesInternal[k + 1].Parameters[spline].length = 0.0;
					spline.splineNodesInternal[k + 2].Parameters[spline].length = 0.0;
				}
			}
			if (!spline.AutoClose)
			{
				spline.splineNodesInternal[spline.splineNodesInternal.Count - 1].Parameters[spline].position = 1.0;
			}
		}
	}

	private struct Node
	{
		public float distance;

		public Vector3 position;

		public Quaternion rotation;

		public float customValue;
	}

	private delegate float DistanceFunction(Vector3 splinePos);

	private const float NodeStep = 0.5f;

	public List<SplineNode> splineNodesArray = new List<SplineNode>();

	private List<SplineNode> splineNodesInternal = new List<SplineNode>();

	public InterpolationMode interpolationMode;

	public RotationMode rotationMode = RotationMode.Tangent;

	public TangentMode tangentMode = TangentMode.UseTangents;

	public NormalMode normalMode;

	public UpdateMode updateMode;

	public int deltaFrames = 1;

	public float deltaTime = 0.1f;

	private int updateFrame;

	private float updateTime;

	public bool perNodeTension;

	public float tension = 0.5f;

	public Vector3 normal = Vector3.up;

	public bool autoClose;

	public int interpolationAccuracy = 5;

	private LengthData lengthData = new LengthData();

	private SplineInterpolator splineInterpolator = new HermiteInterpolator();

	private float length;

	private List<Node> nodes = new List<Node>();

	private bool precalculated;

	public float Length
	{
		get
		{
			return (float)lengthData.length;
		}
	}

	public bool AutoClose
	{
		get
		{
			return autoClose && interpolationMode != InterpolationMode.Bezier;
		}
	}

	public int NodesPerSegment
	{
		get
		{
			return (!IsBezier) ? 1 : 3;
		}
	}

	public int SegmentCount
	{
		get
		{
			return Mathf.Max((ControlNodeCount - 1) / NodesPerSegment, 0);
		}
	}

	public bool HasBeenUpdated
	{
		get
		{
			return updateFrame >= Time.frameCount - 1;
		}
	}

	public int UpdateFrame
	{
		get
		{
			return updateFrame;
		}
	}

	private int ControlNodeCount
	{
		get
		{
			return (!AutoClose) ? splineNodesInternal.Count : (splineNodesInternal.Count + 1);
		}
	}

	private double InvertedAccuracy
	{
		get
		{
			return 1.0 / (double)interpolationAccuracy;
		}
	}

	private bool IsBezier
	{
		get
		{
			return interpolationMode == InterpolationMode.Bezier;
		}
	}

	private bool HasNodes
	{
		get
		{
			return splineNodesInternal.Count > 0;
		}
	}

	public SplineNode[] SplineNodes
	{
		get
		{
			if (splineNodesInternal == null)
			{
				splineNodesInternal = new List<SplineNode>();
			}
			return splineNodesInternal.ToArray();
		}
	}

	public SplineNode[] SegmentNodes
	{
		get
		{
			if (!IsBezier)
			{
				return SplineNodes;
			}
			List<SplineNode> list = new List<SplineNode>();
			for (int i = 0; i < splineNodesInternal.Count; i += NodesPerSegment)
			{
				list.Add(splineNodesInternal[i]);
			}
			return list.ToArray();
		}
	}

	public SplineSegment[] SplineSegments
	{
		get
		{
			SplineSegment[] array = new SplineSegment[SegmentCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new SplineSegment(this, GetNode(i * NodesPerSegment, 0), GetNode(i * NodesPerSegment, NodesPerSegment));
			}
			return array;
		}
	}

	private void OnEnable()
	{
		UpdateSpline();
		Precalculate();
	}

	private void LateUpdate()
	{
		switch (updateMode)
		{
		default:
			return;
		case UpdateMode.DontUpdate:
			return;
		case UpdateMode.EveryXFrames:
			if (Time.frameCount % deltaFrames == 0)
			{
				break;
			}
			return;
		case UpdateMode.EveryXSeconds:
			if (deltaTime < Time.realtimeSinceStartup - updateTime)
			{
				updateTime = Time.realtimeSinceStartup;
				break;
			}
			return;
		case UpdateMode.EveryFrame:
			break;
		}
		UpdateSpline();
	}

	public void UpdateSpline()
	{
		switch (interpolationMode)
		{
		case InterpolationMode.Linear:
			if (!(splineInterpolator is LinearInterpolator))
			{
				splineInterpolator = new LinearInterpolator();
			}
			break;
		case InterpolationMode.Bezier:
			if (!(splineInterpolator is BezierInterpolator))
			{
				splineInterpolator = new BezierInterpolator();
			}
			break;
		case InterpolationMode.Hermite:
			if (!(splineInterpolator is HermiteInterpolator))
			{
				splineInterpolator = new HermiteInterpolator();
			}
			break;
		case InterpolationMode.BSpline:
			if (!(splineInterpolator is BSplineInterpolator))
			{
				splineInterpolator = new BSplineInterpolator();
			}
			break;
		}
		int num = 0;
		foreach (SplineNode item in splineNodesArray)
		{
			if (item != null)
			{
				num++;
			}
		}
		int relevantNodeCount = GetRelevantNodeCount(num);
		if (splineNodesInternal == null)
		{
			splineNodesInternal = new List<SplineNode>();
		}
		splineNodesInternal.Clear();
		if (EnoughNodes(relevantNodeCount))
		{
			splineNodesInternal.AddRange(splineNodesArray.GetRange(0, relevantNodeCount));
			splineNodesInternal.Remove(null);
			ReparameterizeCurve();
			updateFrame = Time.frameCount;
		}
	}

	public Vector3 GetPositionOnSpline(float param)
	{
		if (!HasNodes)
		{
			return Vector3.zero;
		}
		return GetPositionInternal(RecalculateParameter(param));
	}

	public Vector3 GetTangentToSpline(float param)
	{
		if (!HasNodes)
		{
			return Vector3.zero;
		}
		return GetTangentInternal(RecalculateParameter(param));
	}

	public Vector3 GetNormalToSpline(float param)
	{
		if (!HasNodes)
		{
			return Vector3.zero;
		}
		if (normalMode != NormalMode.UseGlobalSplineNormal)
		{
			return GetNormalInternal(RecalculateParameter(param));
		}
		return normal.normalized;
	}

	public Vector3 GetCurvatureOfSpline(float param)
	{
		if (!HasNodes)
		{
			return Vector3.zero;
		}
		return GetCurvatureInternal(RecalculateParameter(param));
	}

	public Quaternion GetOrientationOnSpline(float param)
	{
		if (!HasNodes)
		{
			return Quaternion.identity;
		}
		switch (rotationMode)
		{
		case RotationMode.Tangent:
		{
			SegmentParameter sParam = RecalculateParameter(param);
			Vector3 tangentInternal = GetTangentInternal(sParam);
			Vector3 normalInternal = GetNormalInternal(sParam);
			if (tangentInternal.sqrMagnitude == 0f || normalInternal.sqrMagnitude == 0f)
			{
				return Quaternion.identity;
			}
			return Quaternion.LookRotation(tangentInternal, normalInternal);
		}
		case RotationMode.Node:
			return GetRotationInternal(RecalculateParameter(param));
		default:
			return Quaternion.identity;
		}
	}

	public float GetCustomValueOnSpline(float param)
	{
		if (!HasNodes)
		{
			return 0f;
		}
		return GetValueInternal(RecalculateParameter(param));
	}

	private Vector3 GetPositionInternal(SegmentParameter sParam)
	{
		return splineInterpolator.InterpolateVector(this, sParam.normalizedParam, sParam.normalizedIndex, AutoClose, splineNodesInternal, 0);
	}

	private Vector3 GetTangentInternal(SegmentParameter sParam)
	{
		return splineInterpolator.InterpolateVector(this, sParam.normalizedParam, sParam.normalizedIndex, AutoClose, splineNodesInternal, 1);
	}

	private Vector3 GetNormalInternal(SegmentParameter sParam)
	{
		SplineNode d;
		SplineNode d2;
		SplineNode d3;
		SplineNode d4;
		splineInterpolator.GetNodeData(splineNodesInternal, sParam.normalizedIndex, AutoClose, out d, out d2, out d3, out d4);
		Vector3 v;
		Vector3 v2;
		Vector3 vector;
		Vector3 vector2;
		if (normalMode == NormalMode.UseNodeNormal)
		{
			v = d.transform.TransformDirection(d.normal).normalized;
			v2 = d2.transform.TransformDirection(d2.normal).normalized;
			vector = d3.transform.TransformDirection(d3.normal).normalized;
			vector2 = d4.transform.TransformDirection(d4.normal).normalized;
		}
		else
		{
			v = d.transform.up;
			v2 = d2.transform.up;
			vector = d3.transform.up;
			vector2 = d4.transform.up;
		}
		return splineInterpolator.InterpolateVector(sParam.normalizedParam, v, v2, vector.normalized, vector2.normalized, 0).normalized;
	}

	private Vector3 GetCurvatureInternal(SegmentParameter sParam)
	{
		return splineInterpolator.InterpolateVector(this, sParam.normalizedParam, sParam.normalizedIndex, AutoClose, splineNodesInternal, 2);
	}

	private float GetValueInternal(SegmentParameter sParam)
	{
		return splineInterpolator.InterpolateValue(this, sParam.normalizedParam, sParam.normalizedIndex, AutoClose, splineNodesInternal, 0);
	}

	private Quaternion GetRotationInternal(SegmentParameter sParam)
	{
		return splineInterpolator.InterpolateRotation(this, sParam.normalizedParam, sParam.normalizedIndex, AutoClose, splineNodesInternal, 0);
	}

	public SplineSegment GetSplineSegment(float param)
	{
		param = Mathf.Clamp01(param);
		SplineSegment[] splineSegments = SplineSegments;
		foreach (SplineSegment splineSegment in splineSegments)
		{
			if (splineSegment.IsParameterInRange(param))
			{
				return splineSegment;
			}
		}
		return null;
	}

	public float ConvertNormalizedParameterToDistance(float param)
	{
		return Length * param;
	}

	public float ConvertDistanceToNormalizedParameter(float param)
	{
		return (!(Length <= 0f)) ? (param / Length) : 0f;
	}

	public GameObject AddSplineNode()
	{
		if (splineNodesArray.Count > 0)
		{
			return AddSplineNode(splineNodesArray[splineNodesArray.Count - 1]);
		}
		return AddSplineNode(null);
	}

	public GameObject AddSplineNode(float normalizedParam)
	{
		if (SplineNodes.Length == 0)
		{
			return AddSplineNode();
		}
		SplineNode precedingNode = null;
		SplineNode[] splineNodes = SplineNodes;
		foreach (SplineNode splineNode in splineNodes)
		{
			if (splineNode.Parameters[this].position >= (double)normalizedParam)
			{
				return AddSplineNode(precedingNode);
			}
			precedingNode = splineNode;
		}
		return AddSplineNode(splineNodesArray[splineNodesArray.Count - 1]);
	}

	public GameObject AddSplineNode(SplineNode precedingNode)
	{
		GameObject gameObject = new GameObject();
		SplineNode item = gameObject.AddComponent<SplineNode>();
		int num = ((!(precedingNode == null)) ? (splineNodesArray.IndexOf(precedingNode) + 1) : 0);
		if (num == -1)
		{
			throw new ArgumentException("The SplineNode referenced by \"percedingNode\" is not part of the spline " + base.gameObject.name);
		}
		splineNodesArray.Insert(num, item);
		UpdateSpline();
		return gameObject;
	}

	public void RemoveSplineNode(GameObject gObject)
	{
		SplineNode component = gObject.GetComponent<SplineNode>();
		if (component != null)
		{
			RemoveSplineNode(component);
		}
	}

	public void RemoveSplineNode(SplineNode splineNode)
	{
		splineNodesArray.Remove(splineNode);
		UpdateSpline();
	}

	private SegmentParameter RecalculateParameter(double param)
	{
		if (param <= 0.0)
		{
			return new SegmentParameter(0, 0.0);
		}
		if (param > 1.0)
		{
			return new SegmentParameter(MaxNodeIndex(), 1.0);
		}
		double invertedAccuracy = InvertedAccuracy;
		if (lengthData == null)
		{
			lengthData = new LengthData();
		}
		if (lengthData.subSegmentPosition == null)
		{
			lengthData.Calculate(this);
		}
		for (int num = lengthData.subSegmentPosition.Length - 1; num >= 0; num--)
		{
			if (lengthData.subSegmentPosition[num] < param)
			{
				int num2 = num - num % interpolationAccuracy;
				int num3 = num2 * NodesPerSegment / interpolationAccuracy;
				double param2 = invertedAccuracy * ((double)(num - num2) + (param - lengthData.subSegmentPosition[num]) / lengthData.subSegmentLength[num]);
				if (num3 >= ControlNodeCount - 1)
				{
					return new SegmentParameter(MaxNodeIndex(), 1.0);
				}
				return new SegmentParameter(num3, param2);
			}
		}
		return new SegmentParameter(MaxNodeIndex(), 1.0);
	}

	private SplineNode GetNode(int idxNode, int idxOffset)
	{
		idxNode += idxOffset;
		if (AutoClose)
		{
			return splineNodesInternal[(idxNode % splineNodesInternal.Count + splineNodesInternal.Count) % splineNodesInternal.Count];
		}
		return splineNodesInternal[Mathf.Clamp(idxNode, 0, splineNodesInternal.Count - 1)];
	}

	private void ReparameterizeCurve()
	{
		if (lengthData == null)
		{
			lengthData = new LengthData();
		}
		lengthData.Calculate(this);
	}

	private int MaxNodeIndex()
	{
		return ControlNodeCount - NodesPerSegment - 1;
	}

	private int GetRelevantNodeCount(int nodeCount)
	{
		int num = nodeCount;
		if (IsBezier)
		{
			num = ((nodeCount >= 7) ? (num - (nodeCount - 4) % 3) : (num - nodeCount % 4));
		}
		return num;
	}

	private bool EnoughNodes(int nodeCount)
	{
		if (IsBezier)
		{
			return nodeCount >= 4;
		}
		return nodeCount >= 2;
	}

	public float GetClosestPointParam(Vector3 point, int iterations, float start = 0f, float end = 1f, float step = 0.01f)
	{
		return GetClosestPointParamIntern((Vector3 splinePos) => (point - splinePos).sqrMagnitude, iterations, start, end, step);
	}

	public float GetClosestPointParamToRay(Ray ray, int iterations, float start = 0f, float end = 1f, float step = 0.01f)
	{
		return GetClosestPointParamIntern((Vector3 splinePos) => Vector3.Cross(ray.direction, splinePos - ray.origin).sqrMagnitude, iterations, start, end, step);
	}

	public float GetClosestPointParamToPlane(Plane plane, int iterations, float start = 0f, float end = 1f, float step = 0.01f)
	{
		return GetClosestPointParamIntern((Vector3 splinePos) => Mathf.Abs(plane.GetDistanceToPoint(splinePos)), iterations, start, end, step);
	}

	private float GetClosestPointParamIntern(DistanceFunction distFnc, int iterations, float start, float end, float step)
	{
		iterations = Mathf.Clamp(iterations, 0, 5);
		float closestPointParamOnSegmentIntern = GetClosestPointParamOnSegmentIntern(distFnc, start, end, step);
		for (int i = 0; i < iterations; i++)
		{
			float num = Mathf.Pow(10f, 0f - ((float)i + 2f));
			start = Mathf.Clamp01(closestPointParamOnSegmentIntern - num);
			end = Mathf.Clamp01(closestPointParamOnSegmentIntern + num);
			step = num * 0.1f;
			closestPointParamOnSegmentIntern = GetClosestPointParamOnSegmentIntern(distFnc, start, end, step);
		}
		return closestPointParamOnSegmentIntern;
	}

	private float GetClosestPointParamOnSegmentIntern(DistanceFunction distFnc, float start, float end, float step)
	{
		float num = float.PositiveInfinity;
		float result = 0f;
		for (float num2 = start; num2 <= end; num2 += step)
		{
			float num3 = distFnc(GetPositionOnSpline(num2));
			if (num > num3)
			{
				num = num3;
				result = num2;
			}
		}
		return result;
	}

	private void OnDrawGizmosSelected()
	{
		UpdateSpline();
		if (!HasNodes)
		{
			return;
		}
		DrawSplineGizmo(new Color(1f, 0.5f, 0f, 1f));
		Gizmos.color = new Color(1f, 0.5f, 0f, 0.75f);
		int num = -1;
		foreach (SplineNode item in splineNodesInternal)
		{
			num++;
			if (IsBezier && num % 3 != 0)
			{
				Gizmos.color = new Color(0.8f, 1f, 0.1f, 0.7f);
			}
			else
			{
				Gizmos.color = new Color(1f, 0.5f, 0f, 0.75f);
			}
			Gizmos.DrawSphere(item.Position, GetSizeMultiplier(item) * 1.5f);
		}
		DrawPrecalculatedSplineNodes();
	}

	private void DrawSplineGizmo(Color curveColor)
	{
		switch (interpolationMode)
		{
		case InterpolationMode.Bezier:
		case InterpolationMode.BSpline:
		{
			Gizmos.color = new Color(curveColor.r, curveColor.g, curveColor.b, curveColor.a * 0.25f);
			Gizmos.color = new Color(0.8f, 1f, 0.1f, curveColor.a * 0.25f);
			for (int i = 0; i < ControlNodeCount - 1; i++)
			{
				Gizmos.DrawLine(GetNode(i, 0).Position, GetNode(i, 1).Position);
				if (i % 3 == 0 && IsBezier)
				{
					i++;
				}
			}
			break;
		}
		}
		Gizmos.color = curveColor;
		for (int j = 0; j < ControlNodeCount - 1; j += NodesPerSegment)
		{
			Vector3 vector = GetPositionInternal(new SegmentParameter(j, 0.0));
			for (float num = ((!IsBezier) ? 0.1f : 0.025f); num < 1.0005f; num += ((!IsBezier) ? 0.1f : 0.025f))
			{
				Vector3 positionInternal = GetPositionInternal(new SegmentParameter(j, num));
				Gizmos.DrawLine(vector, positionInternal);
				vector = positionInternal;
			}
		}
	}

	private float GetSizeMultiplier(SplineNode node)
	{
		if (!Camera.current.orthographic)
		{
			Plane plane = default(Plane);
			float enter = 0f;
			plane.SetNormalAndPosition(Camera.current.transform.forward, Camera.current.transform.position);
			plane.Raycast(new Ray(node.Position, Camera.current.transform.forward), out enter);
			return enter * 0.0075f;
		}
		return Camera.current.orthographicSize * (3f / 160f);
	}

	private double GetSegmentLengthInternal(int idxFirstPoint, double startValue, double endValue, double step)
	{
		double num = 0.0;
		Vector3 positionInternal = GetPositionInternal(new SegmentParameter(idxFirstPoint, startValue));
		double num2 = positionInternal.x;
		double num3 = positionInternal.y;
		double num4 = positionInternal.z;
		for (double num5 = startValue + step; num5 < endValue + step * 0.5; num5 += step)
		{
			positionInternal = GetPositionInternal(new SegmentParameter(idxFirstPoint, num5));
			double num6 = num2 - (double)positionInternal.x;
			double num7 = num3 - (double)positionInternal.y;
			double num8 = num4 - (double)positionInternal.z;
			num += Math.Sqrt(num6 * num6 + num7 * num7 + num8 * num8);
			num2 = positionInternal.x;
			num3 = positionInternal.y;
			num4 = positionInternal.z;
		}
		return num;
	}

	public void Precalculate(bool redo = false)
	{
		if (precalculated && !redo)
		{
			return;
		}
		precalculated = true;
		nodes.Clear();
		length = Length;
		if (!(length <= 0f))
		{
			int num = 0;
			float num2 = 0f;
			Node item = default(Node);
			while (num2 < length)
			{
				num2 = (item.distance = (float)num * 0.5f);
				float param = num2 / length;
				item.position = GetPositionOnSpline(param);
				item.rotation = GetOrientationOnSpline(param);
				item.customValue = GetCustomValueOnSpline(param);
				nodes.Add(item);
				num++;
			}
			if (nodes[nodes.Count - 1].distance < length)
			{
				Node item2 = default(Node);
				item2.distance = length;
				float param2 = 1f;
				item2.position = GetPositionOnSpline(param2);
				item2.rotation = GetOrientationOnSpline(param2);
				item2.customValue = GetCustomValueOnSpline(param2);
				nodes.Add(item2);
			}
		}
	}

	public Vector3 GetPositionOnSplineFast(float param)
	{
		if (nodes.Count < 2)
		{
			return GetPositionOnSpline(param);
		}
		int leftIndex;
		int rightIndex;
		float delta;
		GetInterpolationParams(param * length, out leftIndex, out rightIndex, out delta);
		if (leftIndex < 0 || rightIndex > nodes.Count - 1)
		{
			Debug.Log(leftIndex + " - " + rightIndex + " (" + nodes.Count + ")");
		}
		return Vector3.Lerp(nodes[leftIndex].position, nodes[rightIndex].position, delta);
	}

	public Quaternion GetOrientationOnSplineFast(float param)
	{
		if (nodes.Count < 2)
		{
			return GetOrientationOnSpline(param);
		}
		int leftIndex;
		int rightIndex;
		float delta;
		GetInterpolationParams(param * length, out leftIndex, out rightIndex, out delta);
		return Quaternion.Lerp(nodes[leftIndex].rotation, nodes[rightIndex].rotation, delta);
	}

	public float GetCustomValueOnSplineFast(float param)
	{
		if (nodes.Count < 2)
		{
			return GetCustomValueOnSpline(param);
		}
		int leftIndex;
		int rightIndex;
		float delta;
		GetInterpolationParams(param * length, out leftIndex, out rightIndex, out delta);
		return Mathf.Lerp(nodes[leftIndex].customValue, nodes[rightIndex].customValue, delta);
	}

	private void GetInterpolationParams(float distance, out int leftIndex, out int rightIndex, out float delta)
	{
		distance = Mathf.Clamp(distance, 0f, length);
		float num = distance / 0.5f;
		leftIndex = (int)num;
		if (leftIndex >= nodes.Count - 1)
		{
			leftIndex = nodes.Count - 2;
			rightIndex = leftIndex + 1;
			delta = 1f;
		}
		else if (leftIndex == nodes.Count - 2)
		{
			rightIndex = leftIndex + 1;
			float num2 = nodes[rightIndex].distance - nodes[leftIndex].distance;
			delta = (distance - nodes[leftIndex].distance) / num2;
		}
		else
		{
			rightIndex = leftIndex + 1;
			delta = num - (float)leftIndex;
		}
	}

	public float GetClosestPointParamFast(Vector3 point, float start = 0f, float end = 1f, float targetResolution = 0.0001f)
	{
		int num = 0;
		float num2 = end - start;
		float num3 = start + num2 * 0.5f;
		float num4 = (GetPositionOnSplineFast(num3) - point).sqrMagnitude;
		do
		{
			num2 *= 0.5f;
			float num5 = num3 - num2;
			float num6 = num3 + num2;
			float sqrMagnitude = (GetPositionOnSplineFast(num5) - point).sqrMagnitude;
			float sqrMagnitude2 = (GetPositionOnSplineFast(num6) - point).sqrMagnitude;
			if (sqrMagnitude < num4)
			{
				num3 = num5;
				num4 = sqrMagnitude;
			}
			if (sqrMagnitude2 < num4)
			{
				num3 = num6;
				num4 = sqrMagnitude2;
			}
		}
		while (num2 > targetResolution && ++num < 20);
		return num3;
	}

	private void DrawPrecalculatedSplineNodes()
	{
		if (nodes.Count >= 2)
		{
			float num = 0.25f;
			if (num > 0.25f)
			{
				num = 0.25f;
			}
			Gizmos.color = new Color(0.25f, 1f, 0.25f, 0.5f);
			Gizmos.DrawSphere(nodes[0].position, 0.5f * num);
			Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.5f);
			Gizmos.DrawSphere(nodes[nodes.Count - 1].position, 0.5f * num);
			for (int i = 0; i < nodes.Count; i++)
			{
				Vector3 position = nodes[i].position;
				Quaternion rotation = nodes[i].rotation;
				Gizmos.color = new Color(1f, 0.25f, 0.25f, 1f);
				Gizmos.DrawLine(position, position + rotation * Vector3.right * num);
				Gizmos.color = new Color(0.25f, 1f, 0.25f, 1f);
				Gizmos.DrawLine(position, position + rotation * Vector3.up * num);
				Gizmos.color = new Color(0.25f, 0.2f, 1f, 1f);
				Gizmos.DrawLine(position, position + rotation * Vector3.forward * num);
			}
		}
	}
}
