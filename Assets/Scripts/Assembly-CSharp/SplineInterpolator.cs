#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using QuaternionUtilities;
using UnityEngine;

[Serializable]
public class SplineInterpolator
{
	protected double[] coefficientMatrix;

	protected int[] nodeIndices;

	public double[] CoefficientMatrix
	{
		get
		{
			return coefficientMatrix;
		}
		set
		{
			CheckMatrix(value);
			coefficientMatrix = value;
		}
	}

	public int[] NodeIndices
	{
		get
		{
			return nodeIndices;
		}
		set
		{
			CheckIndices(value);
			nodeIndices = value;
		}
	}

	public Matrix4x4 CoefficientMatrix4x4
	{
		get
		{
			Matrix4x4 result = default(Matrix4x4);
			result[0] = (float)coefficientMatrix[0];
			result[1] = (float)coefficientMatrix[1];
			result[2] = (float)coefficientMatrix[2];
			result[3] = (float)coefficientMatrix[3];
			result[4] = (float)coefficientMatrix[4];
			result[5] = (float)coefficientMatrix[5];
			result[6] = (float)coefficientMatrix[6];
			result[7] = (float)coefficientMatrix[7];
			result[8] = (float)coefficientMatrix[8];
			result[9] = (float)coefficientMatrix[9];
			result[10] = (float)coefficientMatrix[10];
			result[11] = (float)coefficientMatrix[11];
			result[12] = (float)coefficientMatrix[12];
			result[13] = (float)coefficientMatrix[13];
			result[14] = (float)coefficientMatrix[14];
			result[15] = (float)coefficientMatrix[15];
			return result;
		}
		set
		{
			coefficientMatrix[0] = value[0];
			coefficientMatrix[1] = value[1];
			coefficientMatrix[2] = value[2];
			coefficientMatrix[3] = value[3];
			coefficientMatrix[4] = value[4];
			coefficientMatrix[5] = value[5];
			coefficientMatrix[6] = value[6];
			coefficientMatrix[7] = value[7];
			coefficientMatrix[8] = value[8];
			coefficientMatrix[9] = value[9];
			coefficientMatrix[10] = value[10];
			coefficientMatrix[11] = value[11];
			coefficientMatrix[12] = value[12];
			coefficientMatrix[13] = value[13];
			coefficientMatrix[14] = value[14];
			coefficientMatrix[15] = value[15];
		}
	}

	protected SplineInterpolator()
	{
	}

	public SplineInterpolator(double[] coefficientMatrix, int[] nodeIndices)
	{
		CheckMatrix(coefficientMatrix);
		CheckIndices(nodeIndices);
		this.coefficientMatrix = coefficientMatrix;
		this.nodeIndices = nodeIndices;
	}

	public virtual Vector3 InterpolateVector(double t, int index, bool autoClose, IList<Vector3> nodes, int derivationOrder)
	{
		Vector3 d;
		Vector3 d2;
		Vector3 d3;
		Vector3 d4;
		GetNodeData(nodes, index, autoClose, out d, out d2, out d3, out d4);
		return InterpolateVector(t, d, d2, d3, d4, derivationOrder);
	}

	public virtual Vector3 InterpolateVector(Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder)
	{
		SplineNode d;
		SplineNode d2;
		SplineNode d3;
		SplineNode d4;
		GetNodeData(nodes, index, autoClose, out d, out d2, out d3, out d4);
		return InterpolateVector(t, d.Position, d2.Position, d3.Position, d4.Position, derivationOrder);
	}

	public virtual float InterpolateValue(double t, int index, bool autoClose, IList<float> nodes, int derivationOrder)
	{
		float d;
		float d2;
		float d3;
		float d4;
		GetNodeData(nodes, index, autoClose, out d, out d2, out d3, out d4);
		return InterpolateValue(t, d, d2, d3, d4, derivationOrder);
	}

	public virtual float InterpolateValue(Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder)
	{
		SplineNode d;
		SplineNode d2;
		SplineNode d3;
		SplineNode d4;
		GetNodeData(nodes, index, autoClose, out d, out d2, out d3, out d4);
		return InterpolateValue(t, d.CustomValue, d2.CustomValue, d3.CustomValue, d4.CustomValue, derivationOrder);
	}

	public virtual Quaternion InterpolateRotation(double t, int index, bool autoClose, IList<Quaternion> nodes, int derivationOrder)
	{
		Quaternion d;
		Quaternion d2;
		Quaternion d3;
		Quaternion d4;
		GetNodeData(nodes, index, autoClose, out d, out d2, out d3, out d4);
		return InterpolateRotation(t, d, d2, d3, d4, derivationOrder);
	}

	public virtual Quaternion InterpolateRotation(Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder)
	{
		SplineNode d;
		SplineNode d2;
		SplineNode d3;
		SplineNode d4;
		GetNodeData(nodes, index, autoClose, out d, out d2, out d3, out d4);
		return InterpolateRotation(t, d.Rotation, d2.Rotation, d3.Rotation, d4.Rotation, derivationOrder);
	}

	public Vector3 InterpolateVector(double t, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int derivationOrder)
	{
		float b;
		float b2;
		float b3;
		float b4;
		GetCoefficients(t, out b, out b2, out b3, out b4, derivationOrder);
		return b * v0 + b2 * v1 + b3 * v2 + b4 * v3;
	}

	public float InterpolateValue(double t, float v0, float v1, float v2, float v3, int derivationOrder)
	{
		float b;
		float b2;
		float b3;
		float b4;
		GetCoefficients(t, out b, out b2, out b3, out b4, derivationOrder);
		return b * v0 + b2 * v1 + b3 * v2 + b4 * v3;
	}

	public Quaternion InterpolateRotation(double t, Quaternion q0, Quaternion q1, Quaternion q2, Quaternion q3, int derivationOrder)
	{
		if (Quaternion.Dot(q0, q1) < 0f)
		{
			q1 = q1.Negative();
		}
		if (Quaternion.Dot(q1, q2) < 0f)
		{
			q2 = q2.Negative();
		}
		if (Quaternion.Dot(q2, q3) < 0f)
		{
			q3 = q3.Negative();
		}
		float b;
		float b2;
		float b3;
		float b4;
		GetCoefficients(t, out b, out b2, out b3, out b4, derivationOrder);
		Vector3 vector = new Vector3(q0.x, q0.y, q0.z);
		Vector3 vector2 = new Vector3(q1.x, q1.y, q1.z);
		Vector3 vector3 = new Vector3(q2.x, q2.y, q2.z);
		Vector3 vector4 = new Vector3(q3.x, q3.y, q3.z);
		Vector3 vector5 = b * vector + b2 * vector2 + b3 * vector3 + b4 * vector4;
		float w = b * q0.w + b2 * q1.w + b3 * q2.w + b4 * q3.w;
		Quaternion q4 = new Quaternion(vector5.x, vector5.y, vector5.z, w);
		return q4.Normalized();
	}

	public void GetNodeData<T>(IList<T> array, int index, bool autoClose, out T d0, out T d1, out T d2, out T d3)
	{
		int count = array.Count;
		d0 = array[GetNodeIndex(autoClose, count, index, nodeIndices[0])];
		d1 = array[GetNodeIndex(autoClose, count, index, nodeIndices[1])];
		d2 = array[GetNodeIndex(autoClose, count, index, nodeIndices[2])];
		d3 = array[GetNodeIndex(autoClose, count, index, nodeIndices[3])];
	}

	private int GetNodeIndex(bool autoClose, int arrayLength, int index, int offset)
	{
		int num = index + offset;
		if (autoClose)
		{
			return (num % arrayLength + arrayLength) % arrayLength;
		}
		return Mathf.Clamp(num, 0, arrayLength - 1);
	}

	private void GetCoefficients(double t, out float b0, out float b1, out float b2, out float b3, int derivationOrder)
	{
		switch (derivationOrder)
		{
		case 0:
			GetCoefficients(t, out b0, out b1, out b2, out b3);
			return;
		case 1:
			GetCoefficientsFirstDerivative(t, out b0, out b1, out b2, out b3);
			return;
		case 2:
			GetCoefficientsSecondDerivative(t, out b0, out b1, out b2, out b3);
			return;
		}
		b0 = 0f;
		b1 = 0f;
		b2 = 0f;
		b3 = 0f;
	}

	private void GetCoefficients(double t, out float b0, out float b1, out float b2, out float b3)
	{
		double num = t * t;
		double num2 = num * t;
		b0 = (float)(coefficientMatrix[0] * num2 + coefficientMatrix[1] * num + coefficientMatrix[2] * t + coefficientMatrix[3]);
		b1 = (float)(coefficientMatrix[4] * num2 + coefficientMatrix[5] * num + coefficientMatrix[6] * t + coefficientMatrix[7]);
		b2 = (float)(coefficientMatrix[8] * num2 + coefficientMatrix[9] * num + coefficientMatrix[10] * t + coefficientMatrix[11]);
		b3 = (float)(coefficientMatrix[12] * num2 + coefficientMatrix[13] * num + coefficientMatrix[14] * t + coefficientMatrix[15]);
	}

	private void GetCoefficientsFirstDerivative(double t, out float b0, out float b1, out float b2, out float b3)
	{
		double num = t * t;
		t *= 2.0;
		num *= 3.0;
		b0 = (float)(coefficientMatrix[0] * num + coefficientMatrix[1] * t + coefficientMatrix[2]);
		b1 = (float)(coefficientMatrix[4] * num + coefficientMatrix[5] * t + coefficientMatrix[6]);
		b2 = (float)(coefficientMatrix[8] * num + coefficientMatrix[9] * t + coefficientMatrix[10]);
		b3 = (float)(coefficientMatrix[12] * num + coefficientMatrix[13] * t + coefficientMatrix[14]);
	}

	private void GetCoefficientsSecondDerivative(double t, out float b0, out float b1, out float b2, out float b3)
	{
		t *= 6.0;
		b0 = (float)(coefficientMatrix[0] * t + coefficientMatrix[1] * 2.0);
		b1 = (float)(coefficientMatrix[4] * t + coefficientMatrix[5] * 2.0);
		b2 = (float)(coefficientMatrix[8] * t + coefficientMatrix[9] * 2.0);
		b3 = (float)(coefficientMatrix[12] * t + coefficientMatrix[13] * 2.0);
	}

	private void CheckMatrix(double[] coefficientMatrix)
	{
		if (coefficientMatrix.Length != 16)
		{
			throw new ArgumentException("The coefficientMatrix-array must contain exactly 16 doubles!");
		}
	}

	private void CheckIndices(int[] nodeIndices)
	{
		if (nodeIndices.Length != 4)
		{
			throw new ArgumentException("nodeIndices-array must contain exactly 4 ints!");
		}
	}
}
