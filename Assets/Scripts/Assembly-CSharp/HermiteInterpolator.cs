#pragma warning disable 0618,0619
using System.Collections.Generic;
using QuaternionUtilities;
using UnityEngine;

public class HermiteInterpolator : SplineInterpolator
{
	public HermiteInterpolator()
	{
		base.CoefficientMatrix = new double[16]
		{
			2.0, -3.0, 0.0, 1.0, -2.0, 3.0, 0.0, 0.0, 1.0, -2.0,
			1.0, 0.0, 1.0, -1.0, 0.0, 0.0
		};
		base.NodeIndices = new int[4] { 0, 1, -1, 2 };
	}

	public override Vector3 InterpolateVector(Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder)
	{
		SplineNode d;
		SplineNode d2;
		SplineNode d3;
		SplineNode d4;
		GetNodeData(nodes, index, autoClose, out d, out d2, out d3, out d4);
		Vector3 P = d3.Position;
		Vector3 P2 = d4.Position;
		RecalcVectors(spline, d, d2, ref P, ref P2);
		return InterpolateVector(t, d.Position, d2.Position, P, P2, derivationOrder);
	}

	public override Vector3 InterpolateVector(double t, int index, bool autoClose, IList<Vector3> nodes, int derivationOrder)
	{
		Vector3 d;
		Vector3 d2;
		Vector3 d3;
		Vector3 d4;
		GetNodeData(nodes, index, autoClose, out d, out d2, out d3, out d4);
		RecalcVectors(d, d2, ref d3, ref d4);
		return InterpolateVector(t, d, d2, d3, d4, derivationOrder);
	}

	public override float InterpolateValue(Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder)
	{
		SplineNode d;
		SplineNode d2;
		SplineNode d3;
		SplineNode d4;
		GetNodeData(nodes, index, autoClose, out d, out d2, out d3, out d4);
		float P = d3.CustomValue;
		float P2 = d4.CustomValue;
		RecalcScalars(spline, d, d2, ref P, ref P2);
		return InterpolateValue(t, d.CustomValue, d2.CustomValue, P, P2, derivationOrder);
	}

	public override float InterpolateValue(double t, int index, bool autoClose, IList<float> nodes, int derivationOrder)
	{
		float d;
		float d2;
		float d3;
		float d4;
		GetNodeData(nodes, index, autoClose, out d, out d2, out d3, out d4);
		RecalcScalars(d, d2, ref d3, ref d4);
		return InterpolateValue(t, d, d2, d3, d4, derivationOrder);
	}

	public override Quaternion InterpolateRotation(Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder)
	{
		SplineNode d;
		SplineNode d2;
		SplineNode d3;
		SplineNode d4;
		GetNodeData(nodes, index, autoClose, out d, out d2, out d3, out d4);
		Quaternion Q = d3.Rotation;
		Quaternion Q2 = d4.Rotation;
		RecalcRotations(d.Rotation, d2.Rotation, ref Q, ref Q2);
		return InterpolateRotation(t, d.Rotation, d2.Rotation, Q, Q2, derivationOrder);
	}

	public void RecalcVectors(Spline spline, SplineNode node0, SplineNode node1, ref Vector3 P2, ref Vector3 P3)
	{
		float tension;
		float tension2;
		if (spline.perNodeTension)
		{
			tension = node0.tension;
			tension2 = node1.tension;
		}
		else
		{
			tension = spline.tension;
			tension2 = spline.tension;
		}
		if (spline.tangentMode == Spline.TangentMode.UseNodeForwardVector)
		{
			P2 = node0.transform.forward * tension;
			P3 = node1.transform.forward * tension2;
			return;
		}
		P2 = node1.Position - P2;
		P3 -= node0.Position;
		if (spline.tangentMode != Spline.TangentMode.UseTangents)
		{
			P2.Normalize();
			P3.Normalize();
		}
		P2 *= tension;
		P3 *= tension2;
	}

	public void RecalcVectors(Vector3 P0, Vector3 P1, ref Vector3 P2, ref Vector3 P3)
	{
		float num = 0.5f;
		P2 = P1 - P2;
		P3 -= P0;
		P2 *= num;
		P3 *= num;
	}

	public void RecalcScalars(Spline spline, SplineNode node0, SplineNode node1, ref float P2, ref float P3)
	{
		float tension;
		float tension2;
		if (spline.perNodeTension)
		{
			tension = node0.tension;
			tension2 = node1.tension;
		}
		else
		{
			tension = spline.tension;
			tension2 = spline.tension;
		}
		P2 = node1.customValue - P2;
		P3 -= node0.customValue;
		P2 *= tension;
		P3 *= tension2;
	}

	public void RecalcScalars(float P0, float P1, ref float P2, ref float P3)
	{
		float num = 0.5f;
		P2 = P1 - P2;
		P3 -= P0;
		P2 *= num;
		P3 *= num;
	}

	public void RecalcRotations(Quaternion Q0, Quaternion Q1, ref Quaternion Q2, ref Quaternion Q3)
	{
		Q2 = QuaternionUtils.GetSquadIntermediate(Q0, Q1, Q2);
		Q3 = QuaternionUtils.GetSquadIntermediate(Q1, Q2, Q3);
	}
}
