#pragma warning disable 0618,0619
using UnityEngine;

namespace QuaternionUtilities
{
	public static class QuaternionUtils
	{
		public static Quaternion Exponential(this Quaternion q)
		{
			return GetQuatExp(q);
		}

		public static Quaternion Logarithm(this Quaternion q)
		{
			return GetQuatLog(q);
		}

		public static Quaternion Conjugate(this Quaternion q)
		{
			return GetQuatConjugate(q);
		}

		public static Quaternion Negative(this Quaternion q)
		{
			return GetQuatNegative(q);
		}

		public static Quaternion Normalized(this Quaternion q)
		{
			float num = 1f / (q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
			Quaternion result = new Quaternion(q.x, q.y, q.z, q.w);
			result.x *= num;
			result.y *= num;
			result.z *= num;
			result.w *= num;
			return result;
		}

		public static Quaternion GetSquadIntermediate(Quaternion q0, Quaternion q1, Quaternion q2)
		{
			Quaternion quatConjugate = GetQuatConjugate(q1);
			Quaternion quatLog = GetQuatLog(quatConjugate * q0);
			Quaternion quatLog2 = GetQuatLog(quatConjugate * q2);
			Quaternion q3 = new Quaternion(-0.25f * (quatLog.x + quatLog2.x), -0.25f * (quatLog.y + quatLog2.y), -0.25f * (quatLog.z + quatLog2.z), -0.25f * (quatLog.w + quatLog2.w));
			return q1 * GetQuatExp(q3);
		}

		public static Quaternion GetQuatLog(Quaternion q)
		{
			Quaternion result = q;
			result.w = 0f;
			if (Mathf.Abs(q.w) < 1f)
			{
				float num = Mathf.Acos(q.w);
				float num2 = Mathf.Sin(num);
				if (Mathf.Abs(num2) > 0.0001f)
				{
					float num3 = num / num2;
					result.x = q.x * num3;
					result.y = q.y * num3;
					result.z = q.z * num3;
				}
			}
			return result;
		}

		public static Quaternion GetQuatExp(Quaternion q)
		{
			Quaternion result = q;
			float num = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z);
			float num2 = Mathf.Sin(num);
			result.w = Mathf.Cos(num);
			if (Mathf.Abs(num2) > 0.0001f)
			{
				float num3 = num2 / num;
				result.x = num3 * q.x;
				result.y = num3 * q.y;
				result.z = num3 * q.z;
			}
			return result;
		}

		public static Quaternion GetQuatConjugate(Quaternion q)
		{
			return new Quaternion(0f - q.x, 0f - q.y, 0f - q.z, q.w);
		}

		public static Quaternion GetQuatNegative(Quaternion q)
		{
			return new Quaternion(0f - q.x, 0f - q.y, 0f - q.z, 0f - q.w);
		}
	}
}
