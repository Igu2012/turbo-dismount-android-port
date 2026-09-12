#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

public class GLDrawImpl
{
	private class SphereCreator
	{
		public enum Topology
		{
			IcosahedronSphere = 0,
			OctahedronSphere = 1,
			OctahedronHemisphere = 2
		}

		private struct Triangle
		{
			public int i0;

			public int i1;

			public int i2;

			public Triangle(int i0, int i1, int i2)
			{
				this.i0 = i0;
				this.i1 = i1;
				this.i2 = i2;
			}
		}

		private List<Vector3> vertices = new List<Vector3>();

		private List<int> indices = new List<int>();

		private Dictionary<long, int> midPointIndexCache = new Dictionary<long, int>();

		private int indexCount;

		public Vector3[] GetVertices()
		{
			return vertices.ToArray();
		}

		public int[] GetIndices()
		{
			return indices.ToArray();
		}

		private int AddVertex(Vector3 v)
		{
			vertices.Add(v.normalized);
			return indexCount++;
		}

		private int GetMidPoint(int i1, int i2)
		{
			bool flag = i1 < i2;
			long num = ((!flag) ? i2 : i1);
			long num2 = ((!flag) ? i1 : i2);
			long key = (num << 32) + num2;
			int value;
			if (midPointIndexCache.TryGetValue(key, out value))
			{
				return value;
			}
			Vector3 vector = vertices[i1];
			Vector3 vector2 = vertices[i2];
			Vector3 v = (vector + vector2) * 0.5f;
			value = AddVertex(v);
			midPointIndexCache.Add(key, value);
			return value;
		}

		public void Create(int recursionLevel, Topology topology)
		{
			vertices.Clear();
			indices.Clear();
			midPointIndexCache.Clear();
			indexCount = 0;
			List<Triangle> list = new List<Triangle>();
			switch (topology)
			{
			case Topology.IcosahedronSphere:
			{
				float num = (1f + Mathf.Sqrt(5f)) / 2f;
				AddVertex(new Vector3(-1f, num, 0f));
				AddVertex(new Vector3(1f, num, 0f));
				AddVertex(new Vector3(-1f, 0f - num, 0f));
				AddVertex(new Vector3(1f, 0f - num, 0f));
				AddVertex(new Vector3(0f, -1f, num));
				AddVertex(new Vector3(0f, 1f, num));
				AddVertex(new Vector3(0f, -1f, 0f - num));
				AddVertex(new Vector3(0f, 1f, 0f - num));
				AddVertex(new Vector3(num, 0f, -1f));
				AddVertex(new Vector3(num, 0f, 1f));
				AddVertex(new Vector3(0f - num, 0f, -1f));
				AddVertex(new Vector3(0f - num, 0f, 1f));
				list.Add(new Triangle(0, 11, 5));
				list.Add(new Triangle(0, 5, 1));
				list.Add(new Triangle(0, 1, 7));
				list.Add(new Triangle(0, 7, 10));
				list.Add(new Triangle(0, 10, 11));
				list.Add(new Triangle(1, 5, 9));
				list.Add(new Triangle(5, 11, 4));
				list.Add(new Triangle(11, 10, 2));
				list.Add(new Triangle(10, 7, 6));
				list.Add(new Triangle(7, 1, 8));
				list.Add(new Triangle(3, 9, 4));
				list.Add(new Triangle(3, 4, 2));
				list.Add(new Triangle(3, 2, 6));
				list.Add(new Triangle(3, 6, 8));
				list.Add(new Triangle(3, 8, 9));
				list.Add(new Triangle(4, 9, 5));
				list.Add(new Triangle(2, 4, 11));
				list.Add(new Triangle(6, 2, 10));
				list.Add(new Triangle(8, 6, 7));
				list.Add(new Triangle(9, 8, 1));
				break;
			}
			case Topology.OctahedronSphere:
				AddVertex(Vector3.up);
				AddVertex(Vector3.right);
				AddVertex(Vector3.forward);
				AddVertex(Vector3.left);
				AddVertex(Vector3.back);
				AddVertex(Vector3.down);
				list.Add(new Triangle(0, 4, 3));
				list.Add(new Triangle(0, 3, 2));
				list.Add(new Triangle(0, 2, 1));
				list.Add(new Triangle(0, 1, 4));
				list.Add(new Triangle(5, 1, 2));
				list.Add(new Triangle(5, 2, 3));
				list.Add(new Triangle(5, 3, 4));
				list.Add(new Triangle(5, 4, 1));
				break;
			case Topology.OctahedronHemisphere:
				AddVertex(Vector3.up);
				AddVertex(Vector3.right);
				AddVertex(Vector3.forward);
				AddVertex(Vector3.left);
				AddVertex(Vector3.back);
				list.Add(new Triangle(0, 4, 3));
				list.Add(new Triangle(0, 3, 2));
				list.Add(new Triangle(0, 2, 1));
				list.Add(new Triangle(0, 1, 4));
				break;
			}
			for (int i = 0; i < recursionLevel; i++)
			{
				List<Triangle> list2 = new List<Triangle>();
				foreach (Triangle item in list)
				{
					int midPoint = GetMidPoint(item.i0, item.i1);
					int midPoint2 = GetMidPoint(item.i1, item.i2);
					int midPoint3 = GetMidPoint(item.i2, item.i0);
					list2.Add(new Triangle(item.i0, midPoint, midPoint3));
					list2.Add(new Triangle(item.i1, midPoint2, midPoint));
					list2.Add(new Triangle(item.i2, midPoint3, midPoint2));
					list2.Add(new Triangle(midPoint, midPoint2, midPoint3));
				}
				list = list2;
			}
			foreach (Triangle item2 in list)
			{
				indices.Add(item2.i0);
				indices.Add(item2.i1);
				indices.Add(item2.i2);
			}
		}
	}

	private struct IncrementalTrig
	{
		public float cos;

		public float sin;

		private float cosd;

		private float sind;

		private float cosn;

		private float sinn;

		public void Init(int divs)
		{
			float f = (float)Math.PI * 2f / (float)divs;
			sin = 0f;
			cos = 1f;
			sind = Mathf.Sin(f);
			cosd = Mathf.Cos(f);
		}

		public void Init(float startAngle, int divs)
		{
			float delta = (float)Math.PI * 2f / (float)divs;
			Init(startAngle, delta);
		}

		public void Init(float startAngle, float delta)
		{
			sin = Mathf.Sin(startAngle);
			cos = Mathf.Cos(startAngle);
			sind = Mathf.Sin(delta);
			cosd = Mathf.Cos(delta);
		}

		public void Inc()
		{
			sinn = sin * cosd + cos * sind;
			cosn = cos * cosd - sin * sind;
			sin = sinn;
			cos = cosn;
		}
	}

	private class BStroke
	{
		private Vector3[] bezierPoints;

		public Vector3[] lineVertices;

		private Dictionary<int, Vector3[]> stripVertices = new Dictionary<int, Vector3[]>();

		public BStroke(bool bezier, Vector3[] points)
		{
			InitStroke(bezier, points);
		}

		private void InitStroke(bool bezier, Vector3[] points)
		{
			if (bezier)
			{
				bezierPoints = points;
				BakeLine();
			}
			else
			{
				lineVertices = points;
			}
			CleanLine();
		}

		private void CleanLine()
		{
			if (lineVertices.Length <= 1)
			{
				return;
			}
			List<int> list = new List<int>();
			Vector3 vector = lineVertices[0];
			for (int i = 1; i < lineVertices.Length; i++)
			{
				Vector3 vector2 = lineVertices[i];
				if ((vector2 - vector).sqrMagnitude < 1E-06f)
				{
					list.Add(i);
				}
				else
				{
					vector = vector2;
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			Vector3[] array = new Vector3[lineVertices.Length - list.Count];
			int num = 0;
			for (int j = 0; j < lineVertices.Length; j++)
			{
				if (!list.Contains(j))
				{
					array[num++] = lineVertices[j];
				}
			}
			lineVertices = array;
		}

		private void BakeLine()
		{
			int num = bezierPoints.Length / 3 - 1;
			if (num < 1)
			{
				if (bezierPoints.Length == 1)
				{
					lineVertices = new Vector3[1];
					lineVertices[0] = bezierPoints[0];
				}
				return;
			}
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 3;
				int num3 = (i + 1) * 3;
				Vector3 vector = bezierPoints[num2];
				Vector3 vector2 = vector + bezierPoints[num2 + 2];
				Vector3 vector3 = bezierPoints[num3];
				Vector3 vector4 = vector3 + bezierPoints[num3 + 1];
				float num4 = 0f;
				float num5 = 0f;
				Vector3 vector5 = vector2 - vector;
				Vector3 vector6 = vector4 - vector2;
				Vector3 to = vector3 - vector4;
				float magnitude = vector5.magnitude;
				float magnitude2 = vector6.magnitude;
				float magnitude3 = to.magnitude;
				num4 += magnitude + magnitude2 + magnitude3;
				float num6 = 0.001f;
				int num7 = 0;
				if (magnitude > num6 && magnitude2 > num6)
				{
					num5 += Vector3.Angle(vector5, vector6);
					num7++;
				}
				if (magnitude2 > num6 && magnitude3 > num6)
				{
					num5 += Vector3.Angle(vector6, to);
					num7++;
				}
				if (num7 < 2 && magnitude > num6 && magnitude3 > num6)
				{
					num5 += Vector3.Angle(vector5, to);
				}
				int value = (int)(num5 / 360f * 8f * Mathf.Max(1f, num4 * 8f));
				value = Mathf.Clamp(value, 1, 16);
				float num8 = 1f / (float)value;
				float num9 = 0f;
				int num10 = 0;
				while (num10 < value)
				{
					list.Add(CubicBezierInterpolate(vector, vector2, vector4, vector3, num9));
					num10++;
					num9 += num8;
				}
			}
			list.Add(bezierPoints[num * 3]);
			lineVertices = list.ToArray();
		}

		private void RoundCap(List<Vector3> tempVertices, Vector3 pos, Vector3 forward, Vector3 right, float radius, int strokeWidth)
		{
			IncrementalTrig incrementalTrig = default(IncrementalTrig);
			int num = strokeWidth * 2;
			float delta = (float)Math.PI / (float)num;
			incrementalTrig.Init(0f, delta);
			Vector3 item = pos + right * radius;
			for (int i = 0; i < num + 1; i++)
			{
				tempVertices.Add(item);
				tempVertices.Add(pos + incrementalTrig.cos * right * radius + incrementalTrig.sin * forward * radius);
				incrementalTrig.Inc();
			}
			tempVertices.Add(pos + right * radius);
			tempVertices.Add(pos + right * radius);
		}

		private void RoundJointRight(List<Vector3> tempVertices, Vector3 pos, Vector3 rFrom, Vector3 rTo, float radius, int strokeWidth)
		{
			float num = Vector3.Angle(rFrom, rTo);
			float num2 = 90f / (float)strokeWidth;
			int num3 = (int)(num / num2);
			float num4 = 1f / (float)num3;
			for (int i = 1; i < num3 + 1; i++)
			{
				float t = (float)i * num4;
				Vector3 vector = Vector3.Slerp(rFrom, rTo, t);
				tempVertices.Add(pos + rFrom * radius);
				tempVertices.Add(pos - vector * radius);
			}
			tempVertices.Add(pos - rTo * radius);
			tempVertices.Add(pos - rTo * radius);
		}

		private void RoundJointLeft(List<Vector3> tempVertices, Vector3 pos, Vector3 rFrom, Vector3 rTo, float radius, int strokeWidth)
		{
			float num = Vector3.Angle(rFrom, rTo);
			float num2 = 90f / (float)strokeWidth;
			int num3 = (int)(num / num2);
			float num4 = 1f / (float)num3;
			for (int i = 1; i < num3 + 1; i++)
			{
				float t = (float)i * num4;
				Vector3 vector = Vector3.Slerp(rFrom, rTo, t);
				tempVertices.Add(pos + vector * radius);
				tempVertices.Add(pos - rFrom * radius);
			}
			tempVertices.Add(pos + rTo * radius);
			tempVertices.Add(pos + rTo * radius);
		}

		private void BakeStrip(int strokeWidth)
		{
			List<Vector3> list = new List<Vector3>();
			if (lineVertices.Length < 1)
			{
				stripVertices[strokeWidth] = list.ToArray();
				return;
			}
			float num = (float)strokeWidth / 8f * 0.125f;
			if (lineVertices.Length == 1)
			{
				int num2 = strokeWidth * 4;
				Vector3 vector = lineVertices[0];
				float num3 = (float)Math.PI * 2f / (float)num2;
				float num4 = 0f;
				list.Add(vector);
				list.Add(vector);
				int num5 = 0;
				while (num5 < num2 + 1)
				{
					list.Add(vector + new Vector3(Mathf.Cos(num4) * num, Mathf.Sin(num4) * num, 0f));
					list.Add(vector);
					num5++;
					num4 += num3;
				}
				list.Add(vector);
				list.Add(vector);
				stripVertices[strokeWidth] = list.ToArray();
				return;
			}
			float num6 = 90f / Mathf.Clamp(strokeWidth, 1f, 8f);
			for (int i = 0; i < lineVertices.Length - 1; i++)
			{
				Vector3 vector = lineVertices[i];
				Vector3 vector2 = lineVertices[i + 1];
				Vector3 vector3 = vector2 - vector;
				if (vector3.sqrMagnitude < 1E-06f)
				{
					continue;
				}
				Vector3 normalized = vector3.normalized;
				Vector3 vector4 = Vector3.Cross(normalized, Vector3.forward);
				if (i == 0)
				{
					RoundCap(list, vector, -normalized, vector4, num, strokeWidth);
				}
				list.Add(vector + vector4 * num);
				list.Add(vector - vector4 * num);
				if (i < lineVertices.Length - 2)
				{
					Vector3 vector5 = lineVertices[i + 2];
					Vector3 vector6 = vector5 - vector2;
					if (vector6.sqrMagnitude > 1E-06f)
					{
						Vector3 normalized2 = vector6.normalized;
						Vector3 vector7 = Vector3.Cross(normalized2, Vector3.forward);
						float num7 = Vector3.Dot(vector4, normalized2);
						float num8 = Vector3.Angle(normalized, normalized2);
						if (num8 >= num6)
						{
							list.Add(vector2 + vector4 * num);
							list.Add(vector2 - vector4 * num);
							if (num7 > 0f)
							{
								RoundJointRight(list, vector2, vector4, vector7, num, strokeWidth);
							}
							else
							{
								RoundJointLeft(list, vector2, vector4, vector7, num, strokeWidth);
							}
						}
						else if (Vector3.Dot(vector4, vector7) < 0f)
						{
							list.Add(vector2 + vector4 * num);
							list.Add(vector2 - vector4 * num);
							RoundCap(list, vector2, normalized, -vector4, num, strokeWidth);
						}
					}
				}
				if (i == lineVertices.Length - 2)
				{
					list.Add(vector2 + vector4 * num);
					list.Add(vector2 - vector4 * num);
					RoundCap(list, vector2, normalized, -vector4, num, strokeWidth);
				}
			}
			stripVertices[strokeWidth] = list.ToArray();
		}

		public void Draw(Vector3 pos, Vector3 right, Vector3 up, int strokeWidth)
		{
			if (strokeWidth == 0)
			{
				DrawLine(pos, right, up);
				return;
			}
			strokeWidth = Mathf.Clamp(strokeWidth, 1, 16);
			if (!stripVertices.ContainsKey(strokeWidth))
			{
				BakeStrip(strokeWidth);
			}
			DrawStrip(pos, right, up, strokeWidth);
		}

		private void DrawLine(Vector3 pos, Vector3 right, Vector3 up)
		{
			if (lineVertices == null)
			{
				return;
			}
			if (lineVertices.Length == 1)
			{
				trig.Init(8);
				Vector3 vector = lineVertices[0];
				Vector3 vector2 = pos + right * vector.x + up * vector.y;
				Vector3 v = vector2 + trig.cos * right * 0.025f + trig.sin * up * 0.025f;
				for (int i = 0; i < 9; i++)
				{
					GL.Vertex(v);
					trig.Inc();
					v = vector2 + trig.cos * right * 0.025f + trig.sin * up * 0.025f;
					GL.Vertex(v);
				}
			}
			else
			{
				Vector3 vector3 = lineVertices[0];
				for (int j = 1; j < lineVertices.Length; j++)
				{
					GL.Vertex(pos + right * vector3.x + up * vector3.y);
					vector3 = lineVertices[j];
					GL.Vertex(pos + right * vector3.x + up * vector3.y);
				}
			}
		}

		private void DrawStrip(Vector3 pos, Vector3 right, Vector3 up, int strokeWidth)
		{
			if (!stripVertices.ContainsKey(strokeWidth))
			{
				return;
			}
			Vector3[] array = stripVertices[strokeWidth];
			if (array != null && array.Length >= 3)
			{
				for (int i = 0; i < array.Length; i++)
				{
					Vector3 vector = array[i];
					GL.Vertex(pos + right * vector.x + up * vector.y);
				}
			}
		}
	}

	private class Glyph
	{
		public List<BStroke> strokes;

		public float width = 1f;

		public float offset;

		public Glyph(params BStroke[] strokes)
		{
			this.strokes = new List<BStroke>(strokes);
			CalculateSpacing();
		}

		public void AddStroke(BStroke stroke)
		{
			if (strokes == null)
			{
				strokes = new List<BStroke>();
			}
			strokes.Add(stroke);
			CalculateSpacing();
		}

		private void CalculateSpacing()
		{
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			for (int i = 0; i < strokes.Count; i++)
			{
				Vector3[] lineVertices = strokes[i].lineVertices;
				for (int j = 0; j < lineVertices.Length; j++)
				{
					Vector3 vector3 = lineVertices[j];
					if (vector3.x < vector.x)
					{
						vector.x = vector3.x;
					}
					if (vector3.x > vector2.x)
					{
						vector2.x = vector3.x;
					}
					if (vector3.y < vector.y)
					{
						vector.y = vector3.y;
					}
					if (vector3.y > vector2.y)
					{
						vector2.y = vector3.y;
					}
				}
			}
			width = vector2.x - vector.x;
			offset = vector.x;
		}
	}

	private class BFont
	{
		private const float monospaceWidth = 1f;

		private const float spaceWidth = 0.5f;

		private const float hSpacing = 0.25f;

		private const float vSpacing = 0.5f;

		private Dictionary<char, Glyph> glyphs = new Dictionary<char, Glyph>();

		public void Add16(char c, Glyph g)
		{
			glyphs.Add(c, g);
		}

		public void Add16(char c, params int[] xypoints)
		{
			Add(c, 0.0625f, xypoints);
		}

		public void Add32(char c, params int[] xypoints)
		{
			Add(c, 1f / 32f, xypoints);
		}

		public void Add100(char c, params int[] xypoints)
		{
			Add(c, 0.01f, xypoints);
		}

		public void Add(char c, float divSize, params int[] xypoints)
		{
			int num = 65535;
			int num2 = 65534;
			int num3 = 0;
			int num4 = 0;
			Glyph glyph = new Glyph();
			for (int i = 0; i < xypoints.Length; i++)
			{
				num4 = i;
				if ((xypoints[i] == num || xypoints[i] == num2) && i != 0)
				{
					if (num4 < num3 + 3)
					{
						num3 = num4;
						continue;
					}
					AddStroke(glyph, divSize, xypoints, num3, num4);
					num3 = num4;
				}
			}
			num4++;
			if (num4 > num3 + 2)
			{
				AddStroke(glyph, divSize, xypoints, num3, num4);
			}
			if (glyph.strokes.Count > 0)
			{
				glyphs.Add(c, glyph);
			}
		}

		private void AddStroke(Glyph g, float div, int[] xypoints, int startIndex, int endIndex)
		{
			int num = 65535;
			bool bezier = xypoints[startIndex] == num;
			startIndex++;
			int num2 = endIndex - startIndex;
			Vector3[] array = new Vector3[num2 / 2];
			for (int i = 0; i < num2; i += 2)
			{
				array[i >> 1] = new Vector3((float)xypoints[startIndex + i] * div, (float)xypoints[startIndex + i + 1] * div);
			}
			g.AddStroke(new BStroke(bezier, array));
		}

		private float DrawChar(char c, Vector3 pos, Vector3 right, Vector3 up, int strokeWidth, bool monospace)
		{
			if (c == ' ')
			{
				if (monospace)
				{
					return 1f;
				}
				return 0.5f;
			}
			Glyph glyph = glyphs[c];
			float num = ((!monospace) ? 1f : 0f);
			for (int i = 0; i < glyph.strokes.Count; i++)
			{
				glyph.strokes[i].Draw(pos - right * glyph.offset * num, right, up, strokeWidth);
			}
			return (!monospace) ? glyph.width : 1f;
		}

		public void DrawString(Vector3 pos, Vector3 right, Vector3 up, float size, int strokeWidth, bool monospace, int align, Color color, string s)
		{
			char[] array = s.ToCharArray();
			List<float> list = new List<float>();
			Vector3 up2 = Vector3.up;
			float num = 0f;
			foreach (char c in array)
			{
				switch (c)
				{
				case '\n':
					num -= 0.25f;
					if (num > up2.x)
					{
						up2.x = num;
					}
					up2.y += 1.5f;
					list.Add(num);
					num = 0f;
					break;
				case ' ':
					num += ((!monospace) ? 0.75f : 1f);
					break;
				default:
					if (glyphs.ContainsKey(c))
					{
						Glyph glyph = glyphs[c];
						num += ((!monospace) ? (glyph.width + 0.25f) : 1f);
					}
					break;
				}
			}
			num -= 0.25f;
			if (num > up2.x)
			{
				up2.x = num;
			}
			list.Add(num);
			right = size * right;
			up = size * up;
			align = Mathf.Clamp(align, 1, 9);
			Vector3 zero = Vector3.zero;
			switch (align)
			{
			case 4:
			case 5:
			case 6:
				zero.y -= up2.y * 0.5f;
				break;
			case 7:
			case 8:
			case 9:
				zero.y -= up2.y;
				break;
			}
			zero.y += (float)(list.Count - 1) * 1.5f;
			int num2 = 0;
			switch (align)
			{
			case 2:
			case 5:
			case 8:
				zero.x -= list[num2] * 0.5f;
				break;
			case 3:
			case 6:
			case 9:
				zero.x -= list[num2];
				break;
			}
			if (strokeWidth == 0)
			{
				GL.Begin(1);
			}
			else
			{
				GL.Begin(5);
			}
			GL.Color(color);
			foreach (char c2 in array)
			{
				switch (c2)
				{
				case '\n':
					num2++;
					zero.x = 0f;
					zero.y -= 1.5f;
					switch (align)
					{
					case 2:
					case 5:
					case 8:
						zero.x -= list[num2] * 0.5f;
						break;
					case 3:
					case 6:
					case 9:
						zero.x -= list[num2];
						break;
					}
					break;
				case ' ':
					zero.x += ((!monospace) ? 0.75f : 1f);
					break;
				default:
					if (glyphs.ContainsKey(c2))
					{
						float num3 = DrawChar(c2, pos + zero.x * right + zero.y * up, right, up, strokeWidth, monospace);
						zero.x += ((!monospace) ? (num3 + 0.25f) : 1f);
					}
					break;
				}
			}
			GL.End();
		}
	}

	private static GLDrawImpl mInstance = null;

	private bool initialized;

	private Material defaultMaterial;

	private Material currMaterial;

	private int currPass;

	private int stencilRef = 1;

	private Vector3[] cubeVertices;

	private int[] cubeIndices;

	private Vector3[] sphereVertices;

	private int[] sphereIndices;

	private Vector3[] hemisphereVertices;

	private int[] hemisphereIndices;

	private BFont font;

	private static IncrementalTrig trig;

	private Color colliderFillColor = new Color(0.1f, 1f, 0.2f, 0.075f);

	private Color colliderEdgeColor = new Color(0.1f, 1f, 0.2f, 0.5f);

	private Color meshDefaultColor = new Color(1f, 1f, 1f, 1f);

	private Vector3 fanCenter = Vector3.zero;

	private Vector3 prevLineVertex = Vector3.zero;

	private Vector2[] coneLathe = new Vector2[3]
	{
		Vector2.zero,
		Vector2.zero,
		Vector2.zero
	};

	private Vector2[] cylinderLathe = new Vector2[4]
	{
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		Vector2.zero
	};

	private Vector2[] arrowLathe = new Vector2[5]
	{
		Vector2.zero,
		Vector2.up,
		Vector2.right,
		Vector2.right,
		Vector2.right
	};

	private static Vector3[] bspCoeffs = new Vector3[4]
	{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero
	};

	public static GLDrawImpl instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new GLDrawImpl();
				mInstance.Init();
			}
			return mInstance;
		}
	}

	public void Begin(bool useStencil = false)
	{
		Begin(defaultMaterial, useStencil ? 1 : 0);
	}

	public void Begin(Material material, int pass)
	{
		if (!initialized)
		{
			Init();
		}
		StencilReset();
		GL.PushMatrix();
		currMaterial = material;
		if (currMaterial == null)
		{
			currMaterial = defaultMaterial;
		}
		currPass = pass;
		currMaterial.SetPass(pass);
	}

	public void End()
	{
		GL.PopMatrix();
	}

	public void StencilReset()
	{
		StencilSet(1);
	}

	public void StencilInc()
	{
		StencilSet(stencilRef + 1);
	}

	public void StencilDec()
	{
		StencilSet(stencilRef - 1);
	}

	public void StencilSet(int value)
	{
		stencilRef = Mathf.Clamp(value, 1, 255);
		defaultMaterial.SetInt("_StencilRef", stencilRef);
	}

	public void ScreenViewport()
	{
		GL.Viewport(new Rect(0f, 0f, Screen.width, Screen.height));
	}

	public void Line(Vector3 v0, Vector3 v1, Color color)
	{
		GL.Begin(1);
		GL.Color(color);
		GL.Vertex(v0);
		GL.Vertex(v1);
		GL.End();
	}

	public void Line(Vector3 v0, Vector3 v1, Color c0, Color c1)
	{
		GL.Begin(1);
		GL.Color(c0);
		GL.Vertex(v0);
		GL.Color(c1);
		GL.Vertex(v1);
		GL.End();
	}

	public void Lines(Vector3[] vertices, Color color)
	{
		GL.Begin(1);
		GL.Color(color);
		for (int i = 0; i < vertices.Length; i++)
		{
			GL.Vertex(vertices[i]);
		}
		GL.End();
	}

	public void Lines(Vector3[] vertices, Color[] colors)
	{
		GL.Begin(1);
		for (int i = 0; i < vertices.Length; i++)
		{
			GL.Color(colors[i]);
			GL.Vertex(vertices[i]);
		}
		GL.End();
	}

	public void ContinuousLine(Vector3[] vertices, Color color)
	{
		GL.Begin(1);
		GL.Color(color);
		for (int i = 0; i < vertices.Length - 1; i++)
		{
			GL.Vertex(vertices[i]);
			GL.Vertex(vertices[i + 1]);
		}
		GL.End();
	}

	public void ContinuousLine(Vector3[] vertices, Color[] colors)
	{
		GL.Begin(1);
		for (int i = 0; i < vertices.Length - 1; i++)
		{
			GL.Color(colors[i]);
			GL.Vertex(vertices[i]);
			GL.Color(colors[i + 1]);
			GL.Vertex(vertices[i + 1]);
		}
		GL.End();
	}

	public void BezierCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color, int divs)
	{
		GL.Begin(1);
		GL.Color(color);
		float num = 1f / (float)divs;
		float num2 = num;
		Vector3 v = a;
		int num3 = 0;
		while (num3 < divs)
		{
			Vector3 vector = CubicBezierInterpolate(a, b, c, d, num2);
			GL.Vertex(v);
			GL.Vertex(vector);
			v = vector;
			num3++;
			num2 += num;
		}
		GL.End();
	}

	public void BezierCurves(Vector3[] points, Color color, int divs)
	{
		if (points.Length < 4)
		{
			return;
		}
		int num = points.Length / 4 * 4;
		GL.Begin(1);
		GL.Color(color);
		float num2 = 1f / (float)divs;
		Vector3 v = Vector3.zero;
		for (int i = 0; i < num; i += 4)
		{
			float num3 = num2;
			v = points[i];
			int num4 = 0;
			while (num4 < divs - 1)
			{
				Vector3 vector = CubicBezierInterpolate(points[i], points[i + 1], points[i + 2], points[i + 3], num3);
				GL.Vertex(v);
				GL.Vertex(vector);
				v = vector;
				num4++;
				num3 += num2;
			}
		}
		GL.Vertex(v);
		GL.Vertex(points[num - 1]);
		GL.End();
	}

	public void BSpline(Vector3[] points, Color color, int divs)
	{
		if (points.Length < 1)
		{
			return;
		}
		GL.Begin(1);
		GL.Color(color);
		float num = 1f / (float)divs;
		Vector3 v = points[0];
		int max = points.Length - 1;
		for (int i = -1; i < points.Length; i++)
		{
			Vector3 p = points[Mathf.Clamp(i - 1, 0, max)];
			Vector3 p2 = points[Mathf.Clamp(i, 0, max)];
			Vector3 p3 = points[Mathf.Clamp(i + 1, 0, max)];
			Vector3 p4 = points[Mathf.Clamp(i + 2, 0, max)];
			float num2 = num;
			int num3 = 0;
			while (num3 < divs - 1)
			{
				Vector3 vector = BSPInterpolate(p, p2, p3, p4, num2);
				GL.Vertex(v);
				GL.Vertex(vector);
				v = vector;
				num3++;
				num2 += num;
			}
		}
		GL.Vertex(v);
		GL.Vertex(points[points.Length - 1]);
		GL.End();
	}

	public void CatmullRomSpline(Vector3[] points, Color color, int divs)
	{
		if (points.Length < 1)
		{
			return;
		}
		GL.Begin(1);
		GL.Color(color);
		float num = 1f / (float)divs;
		Vector3 v = Vector3.zero;
		int max = points.Length - 1;
		for (int i = 0; i < points.Length - 1; i++)
		{
			Vector3 p = points[Mathf.Clamp(i - 1, 0, max)];
			Vector3 vector = points[Mathf.Clamp(i, 0, max)];
			Vector3 p2 = points[Mathf.Clamp(i + 1, 0, max)];
			Vector3 p3 = points[Mathf.Clamp(i + 2, 0, max)];
			float num2 = num;
			v = vector;
			int num3 = 0;
			while (num3 < divs - 1)
			{
				Vector3 vector2 = CatmullRomInterpolate(p, vector, p2, p3, num2);
				GL.Vertex(v);
				GL.Vertex(vector2);
				v = vector2;
				num3++;
				num2 += num;
			}
		}
		GL.Vertex(v);
		GL.Vertex(points[points.Length - 1]);
		GL.End();
	}

	public void Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Color color, bool fill)
	{
		if (fill)
		{
			GL.Begin(4);
			GL.Color(color);
			GL.Vertex(v0);
			GL.Vertex(v1);
			GL.Vertex(v2);
			GL.End();
		}
		else
		{
			GL.Begin(1);
			GL.Color(color);
			GL.Vertex(v0);
			GL.Vertex(v1);
			GL.Vertex(v1);
			GL.Vertex(v2);
			GL.Vertex(v2);
			GL.Vertex(v0);
			GL.End();
		}
	}

	public void Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Color c0, Color c1, Color c2, bool fill)
	{
		if (fill)
		{
			GL.Begin(4);
			GL.Color(c0);
			GL.Vertex(v0);
			GL.Color(c1);
			GL.Vertex(v1);
			GL.Color(c2);
			GL.Vertex(v2);
			GL.End();
		}
		else
		{
			GL.Begin(1);
			GL.Color(c0);
			GL.Vertex(v0);
			GL.Color(c1);
			GL.Vertex(v1);
			GL.Vertex(v1);
			GL.Color(c2);
			GL.Vertex(v2);
			GL.Vertex(v2);
			GL.Color(c0);
			GL.Vertex(v0);
			GL.End();
		}
	}

	public void Triangles(Vector3[] vertices, Color color, bool fill)
	{
		if (fill)
		{
			GL.Begin(4);
			GL.Color(color);
			for (int i = 0; i < vertices.Length; i++)
			{
				GL.Vertex(vertices[i]);
			}
			GL.End();
			return;
		}
		GL.Begin(1);
		GL.Color(color);
		for (int j = 0; j < vertices.Length - 2; j++)
		{
			GL.Vertex(vertices[j]);
			GL.Vertex(vertices[j + 1]);
			GL.Vertex(vertices[j + 1]);
			GL.Vertex(vertices[j + 2]);
			GL.Vertex(vertices[j + 2]);
			GL.Vertex(vertices[j]);
		}
		GL.End();
	}

	public void Triangles(Vector3[] vertices, Color[] colors, bool fill)
	{
		if (fill)
		{
			GL.Begin(4);
			for (int i = 0; i < vertices.Length; i++)
			{
				GL.Color(colors[i]);
				GL.Vertex(vertices[i]);
			}
			GL.End();
			return;
		}
		GL.Begin(1);
		for (int j = 0; j < vertices.Length - 2; j++)
		{
			GL.Color(colors[j]);
			GL.Vertex(vertices[j]);
			GL.Color(colors[j + 1]);
			GL.Vertex(vertices[j + 1]);
			GL.Vertex(vertices[j + 1]);
			GL.Color(colors[j + 2]);
			GL.Vertex(vertices[j + 2]);
			GL.Vertex(vertices[j + 2]);
			GL.Color(colors[j]);
			GL.Vertex(vertices[j]);
		}
		GL.End();
	}

	public void Quad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color color, bool fill)
	{
		if (fill)
		{
			GL.Begin(7);
			GL.Color(color);
			GL.Vertex(v0);
			GL.Vertex(v1);
			GL.Vertex(v2);
			GL.Vertex(v3);
			GL.End();
		}
		else
		{
			GL.Begin(1);
			GL.Color(color);
			GL.Vertex(v0);
			GL.Vertex(v1);
			GL.Vertex(v1);
			GL.Vertex(v2);
			GL.Vertex(v2);
			GL.Vertex(v3);
			GL.Vertex(v3);
			GL.Vertex(v0);
			GL.End();
		}
	}

	public void Quad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color c0, Color c1, Color c2, Color c3, bool fill)
	{
		if (fill)
		{
			GL.Begin(7);
			GL.Color(c0);
			GL.Vertex(v0);
			GL.Color(c1);
			GL.Vertex(v1);
			GL.Color(c2);
			GL.Vertex(v2);
			GL.Color(c3);
			GL.Vertex(v3);
			GL.End();
		}
		else
		{
			GL.Begin(1);
			GL.Color(c0);
			GL.Vertex(v0);
			GL.Color(c1);
			GL.Vertex(v1);
			GL.Vertex(v1);
			GL.Color(c2);
			GL.Vertex(v2);
			GL.Vertex(v2);
			GL.Color(c3);
			GL.Vertex(v3);
			GL.Vertex(v3);
			GL.Color(c0);
			GL.Vertex(v0);
			GL.End();
		}
	}

	public void Quads(Vector3[] vertices, Color color, bool fill)
	{
		if (fill)
		{
			GL.Begin(7);
			GL.Color(color);
			for (int i = 0; i < vertices.Length; i++)
			{
				GL.Vertex(vertices[i]);
			}
			GL.End();
			return;
		}
		GL.Begin(1);
		GL.Color(color);
		for (int j = 0; j < vertices.Length - 3; j++)
		{
			GL.Vertex(vertices[j]);
			GL.Vertex(vertices[j + 1]);
			GL.Vertex(vertices[j + 1]);
			GL.Vertex(vertices[j + 2]);
			GL.Vertex(vertices[j + 2]);
			GL.Vertex(vertices[j + 3]);
			GL.Vertex(vertices[j + 3]);
			GL.Vertex(vertices[j]);
		}
		GL.End();
	}

	public void Quads(Vector3[] vertices, Color[] colors, bool fill)
	{
		if (fill)
		{
			GL.Begin(7);
			for (int i = 0; i < vertices.Length; i++)
			{
				GL.Color(colors[i]);
				GL.Vertex(vertices[i]);
			}
			GL.End();
			return;
		}
		GL.Begin(1);
		for (int j = 0; j < vertices.Length - 3; j++)
		{
			GL.Color(colors[j]);
			GL.Vertex(vertices[j]);
			GL.Color(colors[j + 1]);
			GL.Vertex(vertices[j + 1]);
			GL.Vertex(vertices[j + 1]);
			GL.Color(colors[j + 2]);
			GL.Vertex(vertices[j + 2]);
			GL.Vertex(vertices[j + 2]);
			GL.Color(colors[j + 3]);
			GL.Vertex(vertices[j + 3]);
			GL.Vertex(vertices[j + 3]);
			GL.Color(colors[j]);
			GL.Vertex(vertices[j]);
		}
		GL.End();
	}

	private void FanCenter(Vector3 v)
	{
		fanCenter = v;
	}

	private void FanVertex(Vector3 v)
	{
		GL.Vertex(v);
		GL.Vertex(fanCenter);
	}

	private void PrevLineVertex(Vector3 v)
	{
		prevLineVertex = v;
	}

	private void LineVertex(Vector3 v)
	{
		GL.Vertex(prevLineVertex);
		GL.Vertex(v);
		prevLineVertex = v;
	}

	public void RoundRect(Vector3 position, Vector3 right, Vector3 up, float radius, Color color, bool fill)
	{
		trig.Init(4.712389f, 32);
		Vector3 normalized = up.normalized;
		Vector3 normalized2 = right.normalized;
		if (fill)
		{
			FanCenter(position + 0.5f * right + 0.5f * up);
			GL.Begin(5);
			GL.Color(color);
			FanVertex(position + normalized2 * radius);
			FanVertex(position + right - normalized2 * radius);
			Vector3 vector = position + right - normalized2 * radius + normalized * radius;
			for (int i = 0; i < 8; i++)
			{
				FanVertex(vector + trig.cos * normalized2 * radius + trig.sin * normalized * radius);
				trig.Inc();
			}
			FanVertex(position + right + normalized * radius);
			FanVertex(position + right + up - normalized * radius);
			vector = position + right + up - normalized2 * radius - normalized * radius;
			for (int j = 0; j < 8; j++)
			{
				FanVertex(vector + trig.cos * normalized2 * radius + trig.sin * normalized * radius);
				trig.Inc();
			}
			FanVertex(position + right + up - normalized2 * radius);
			FanVertex(position + up + normalized2 * radius);
			vector = position + up + normalized2 * radius - normalized * radius;
			for (int k = 0; k < 8; k++)
			{
				FanVertex(vector + trig.cos * normalized2 * radius + trig.sin * normalized * radius);
				trig.Inc();
			}
			FanVertex(position + up - normalized * radius);
			FanVertex(position + normalized * radius);
			vector = position + normalized2 * radius + normalized * radius;
			for (int l = 0; l < 9; l++)
			{
				FanVertex(vector + trig.cos * normalized2 * radius + trig.sin * normalized * radius);
				trig.Inc();
			}
		}
		else
		{
			GL.Begin(1);
			GL.Color(color);
			PrevLineVertex(position + normalized2 * radius);
			LineVertex(position + right - normalized2 * radius);
			Vector3 vector2 = position + right - normalized2 * radius + normalized * radius;
			for (int m = 0; m < 8; m++)
			{
				LineVertex(vector2 + trig.cos * normalized2 * radius + trig.sin * normalized * radius);
				trig.Inc();
			}
			LineVertex(position + right + normalized * radius);
			LineVertex(position + right + up - normalized * radius);
			vector2 = position + right + up - normalized2 * radius - normalized * radius;
			for (int n = 0; n < 8; n++)
			{
				LineVertex(vector2 + trig.cos * normalized2 * radius + trig.sin * normalized * radius);
				trig.Inc();
			}
			LineVertex(position + right + up - normalized2 * radius);
			LineVertex(position + up + normalized2 * radius);
			vector2 = position + up + normalized2 * radius - normalized * radius;
			for (int num = 0; num < 8; num++)
			{
				LineVertex(vector2 + trig.cos * normalized2 * radius + trig.sin * normalized * radius);
				trig.Inc();
			}
			LineVertex(position + up - normalized * radius);
			LineVertex(position + normalized * radius);
			vector2 = position + normalized2 * radius + normalized * radius;
			for (int num2 = 0; num2 < 9; num2++)
			{
				LineVertex(vector2 + trig.cos * normalized2 * radius + trig.sin * normalized * radius);
				trig.Inc();
			}
		}
		GL.End();
	}

	public void Ellipse(Vector3 position, Vector3 right, Vector3 up, Color color, bool fill, int divs = 64)
	{
		trig.Init(divs);
		if (fill)
		{
			GL.Begin(5);
			GL.Color(color);
			for (int i = 0; i < divs + 1; i++)
			{
				GL.Vertex(position + (trig.cos * right + trig.sin * up));
				GL.Vertex(position);
				trig.Inc();
			}
			GL.End();
			return;
		}
		GL.Begin(1);
		GL.Color(color);
		Vector3 v = position + trig.cos * right + trig.sin * up;
		for (int j = 0; j < divs; j++)
		{
			GL.Vertex(v);
			trig.Inc();
			v = position + trig.cos * right + trig.sin * up;
			GL.Vertex(v);
		}
		GL.End();
	}

	public void EllipseRim(Vector3 position, Vector3 right, Vector3 up, float innerRadius, Color color, int divs = 64)
	{
		innerRadius = Mathf.Clamp01(innerRadius);
		trig.Init(divs);
		GL.Begin(5);
		GL.Color(color);
		for (int i = 0; i < divs + 1; i++)
		{
			Vector3 vector = trig.cos * right + trig.sin * up;
			GL.Vertex(position + vector);
			GL.Vertex(position + vector * innerRadius);
			trig.Inc();
		}
		GL.End();
	}

	public void Sector(Vector3 position, Vector3 right, Vector3 up, float startAngle, float endAngle, Color color, bool fill, int divs = 64)
	{
		startAngle = startAngle * (float)Math.PI / 180f;
		endAngle = endAngle * (float)Math.PI / 180f;
		if (startAngle > endAngle)
		{
			float num = endAngle;
			endAngle = startAngle;
			startAngle = num;
		}
		float delta = (endAngle - startAngle) / (float)divs;
		trig.Init(startAngle, delta);
		if (fill)
		{
			GL.Begin(5);
			GL.Color(color);
			for (int i = 0; i < divs + 1; i++)
			{
				GL.Vertex(position + (trig.cos * right + trig.sin * up));
				GL.Vertex(position);
				trig.Inc();
			}
			GL.End();
			return;
		}
		GL.Begin(1);
		GL.Color(color);
		Vector3 v = position + trig.cos * right + trig.sin * up;
		for (int j = 0; j < divs; j++)
		{
			GL.Vertex(v);
			trig.Inc();
			v = position + trig.cos * right + trig.sin * up;
			GL.Vertex(v);
		}
		GL.End();
	}

	public void Box(Vector3 center, Vector3 size, Color color, bool fill)
	{
		Vector3 vector = size * 0.5f;
		if (!fill)
		{
			GL.wireframe = true;
		}
		GL.Begin(7);
		GL.Color(color);
		for (int i = 0; i < cubeIndices.Length; i++)
		{
			Vector3 vector2 = cubeVertices[cubeIndices[i]];
			GL.Vertex(center + new Vector3(vector.x * vector2.x, vector.y * vector2.y, vector.z * vector2.z));
		}
		GL.End();
		if (!fill)
		{
			GL.wireframe = false;
		}
	}

	public void Ellipsoid(Vector3 center, Vector3 size, Color color, bool fill)
	{
		Vector3 vector = size * 0.5f;
		if (!fill)
		{
			GL.wireframe = true;
		}
		GL.Begin(4);
		GL.Color(color);
		for (int i = 0; i < sphereIndices.Length; i++)
		{
			Vector3 vector2 = sphereVertices[sphereIndices[i]];
			GL.Vertex(center + new Vector3(vector.x * vector2.x, vector.y * vector2.y, vector.z * vector2.z));
		}
		GL.End();
		if (!fill)
		{
			GL.wireframe = false;
		}
	}

	public void Capsule(Vector3 center, Vector3 axis, float height, float radius, Color color, bool fill)
	{
		height *= 0.5f;
		height -= radius;
		if (height < 0f)
		{
			height = 0f;
		}
		Vector3 @base;
		Vector3 base2;
		FindBaseAxes(axis, out @base, out base2);
		if (!fill)
		{
			GL.wireframe = true;
		}
		Vector3 vector = center - axis * height;
		Vector3 vector2 = center + axis * height;
		if (height > 0f)
		{
			trig.Init(16);
			GL.Begin(5);
			GL.Color(color);
			for (int i = 0; i < 17; i++)
			{
				GL.Vertex(vector2 + trig.cos * @base * radius + trig.sin * base2 * radius);
				GL.Vertex(vector + trig.cos * @base * radius + trig.sin * base2 * radius);
				trig.Inc();
			}
			GL.End();
		}
		GL.Begin(4);
		GL.Color(color);
		for (int j = 0; j < hemisphereIndices.Length; j++)
		{
			Vector3 vector3 = hemisphereVertices[hemisphereIndices[j]];
			GL.Vertex(vector2 + vector3.x * base2 * radius + vector3.y * axis * radius + vector3.z * @base * radius);
		}
		for (int k = 0; k < hemisphereIndices.Length; k++)
		{
			Vector3 vector4 = hemisphereVertices[hemisphereIndices[k]];
			GL.Vertex(vector + vector4.x * @base * radius - vector4.y * axis * radius + vector4.z * base2 * radius);
		}
		GL.End();
		if (!fill)
		{
			GL.wireframe = false;
		}
	}

	public void LatheObject(Vector3 position, Vector3 axis, Color color, bool fill, params Vector2[] shape)
	{
		if (shape.Length < 2)
		{
			return;
		}
		Vector3 @base;
		Vector3 base2;
		FindBaseAxes(axis, out @base, out base2);
		if (!fill)
		{
			GL.wireframe = true;
		}
		for (int i = 0; i < shape.Length - 1; i++)
		{
			GL.Begin(5);
			GL.Color(color);
			Vector3 vector = position + shape[i].x * axis;
			Vector3 vector2 = position + shape[i + 1].x * axis;
			trig.Init(32);
			for (int j = 0; j < 33; j++)
			{
				GL.Vertex(vector2 + trig.cos * shape[i + 1].y * @base + trig.sin * shape[i + 1].y * base2);
				GL.Vertex(vector + trig.cos * shape[i].y * @base + trig.sin * shape[i].y * base2);
				trig.Inc();
			}
			GL.End();
		}
		if (!fill)
		{
			GL.wireframe = false;
		}
	}

	public void Cone(Vector3 position, Vector3 axis, float radius, Color color, bool fill)
	{
		coneLathe[1].y = radius;
		coneLathe[2].x = axis.magnitude;
		LatheObject(position, axis, color, fill, coneLathe);
	}

	public void Cylinder(Vector3 center, Vector3 axis, float height, float radius, Color color, bool fill)
	{
		height *= 0.5f;
		cylinderLathe[0].x = 0f - height;
		cylinderLathe[1].x = 0f - height;
		cylinderLathe[1].y = radius;
		cylinderLathe[2].x = height;
		cylinderLathe[2].y = radius;
		cylinderLathe[3].x = height;
		LatheObject(center, axis, color, fill, cylinderLathe);
	}

	public void Arrow(Vector3 v0, Vector3 v1, float diameter, float capSize, Color color)
	{
		Vector3 vector = v1 - v0;
		float magnitude = vector.magnitude;
		if (!(magnitude <= 0f))
		{
			vector /= magnitude;
			if (magnitude < capSize)
			{
				float num = magnitude / capSize;
				Cone(v0, vector * capSize * num, capSize * 0.5f * num, color, true);
				return;
			}
			if (diameter <= 0f)
			{
				Line(v0, v0 + vector * (magnitude - capSize), color);
				Cone(v1 - vector * capSize, vector * capSize, capSize * 0.5f, color, true);
				return;
			}
			float y = diameter * 0.5f;
			arrowLathe[1].y = y;
			arrowLathe[2].x = magnitude - capSize;
			arrowLathe[2].y = y;
			arrowLathe[3].x = magnitude - capSize;
			arrowLathe[3].y = capSize * 0.5f;
			arrowLathe[4].x = magnitude;
			arrowLathe[4].y = 0f;
			LatheObject(v0, vector.normalized, color, true, arrowLathe);
		}
	}

	public void RadialMeter(Vector3 position, Vector3 right, Vector3 up, float minAngle, float maxAngle, float minLimit, float maxLimit, float ruleStep, float labelMultiplier, Color background, Color rule, Color indicator, float value)
	{
		Ellipse(position, right, up, background, true);
		Sector(position, right, up, minAngle, maxAngle, rule, false, 48);
		float firstRule = 0f;
		int ruleCount = 0;
		FindRules(ref minLimit, ref maxLimit, ref ruleStep, out firstRule, out ruleCount);
		if (ruleCount > 0)
		{
			GL.Begin(1);
			GL.Color(rule);
			for (int i = 0; i < ruleCount; i++)
			{
				float t = Mathf.InverseLerp(minLimit, maxLimit, firstRule + (float)i * ruleStep);
				float f = Mathf.Lerp(minAngle, maxAngle, t) / 180f * (float)Math.PI;
				GL.Vertex(position + Mathf.Cos(f) * right + Mathf.Sin(f) * up);
				GL.Vertex(position + Mathf.Cos(f) * right * 0.85f + Mathf.Sin(f) * up * 0.85f);
			}
			GL.End();
		}
		if (labelMultiplier != 0f)
		{
			float num = up.magnitude * 0.04f;
			Vector3 up2 = up.normalized * num;
			Vector3 right2 = right.normalized * num;
			for (int j = 0; j < ruleCount; j++)
			{
				float num2 = firstRule + (float)j * ruleStep;
				float t2 = Mathf.InverseLerp(minLimit, maxLimit, num2);
				float f2 = Mathf.Lerp(minAngle, maxAngle, t2) / 180f * (float)Math.PI;
				Vector3 pos = position + Mathf.Cos(f2) * right * 0.75f + Mathf.Sin(f2) * up * 0.75f;
				String(pos, right2, up2, 2f, 0, false, 5, rule, (num2 * labelMultiplier).ToString("0.###"));
			}
		}
		GL.Begin(1);
		GL.Color(indicator);
		float f3 = Mathf.Lerp(minAngle, maxAngle, Mathf.InverseLerp(minLimit, maxLimit, value)) / 180f * (float)Math.PI;
		GL.Vertex(position + (Mathf.Cos(f3) * -0.25f * right + Mathf.Sin(f3) * up * -0.25f));
		GL.Vertex(position + (Mathf.Cos(f3) * right + Mathf.Sin(f3) * up));
		GL.End();
	}

	public void VerticalMeter(Vector3 position, Vector3 right, Vector3 up, float minLimit, float maxLimit, float ruleStep, float labelMultiplier, bool fill, Color background, Color rule, Color indicator, float value)
	{
		Quad(position, position + up, position + up + right, position + right, background, true);
		float firstRule = 0f;
		int ruleCount = 0;
		FindRules(ref minLimit, ref maxLimit, ref ruleStep, out firstRule, out ruleCount);
		if (ruleCount > 0)
		{
			GL.Begin(1);
			GL.Color(rule);
			for (int i = 0; i < ruleCount; i++)
			{
				float num = Mathf.InverseLerp(minLimit, maxLimit, firstRule + (float)i * ruleStep);
				GL.Vertex(position + num * up);
				GL.Vertex(position + right + num * up);
			}
			GL.End();
		}
		if (labelMultiplier != 0f)
		{
			float num2 = up.magnitude * 0.02f;
			Vector3 up2 = up.normalized * num2;
			Vector3 vector = right.normalized * num2;
			for (int j = 0; j < ruleCount; j++)
			{
				float num3 = firstRule + (float)j * ruleStep;
				float num4 = Mathf.InverseLerp(minLimit, maxLimit, num3);
				Vector3 vector2 = position + num4 * up;
				String(vector2 - vector, vector, up2, 2f, 0, false, 6, rule, (num3 * labelMultiplier).ToString("0.###"));
			}
		}
		if (fill)
		{
			float num5 = Mathf.InverseLerp(minLimit, maxLimit, 0f);
			float num6 = Mathf.InverseLerp(minLimit, maxLimit, value);
			if (num5 > num6)
			{
				float num7 = num6;
				num6 = num5;
				num5 = num7;
			}
			Vector3 vector3 = position + num5 * up;
			Quad(vector3, vector3 + (num6 - num5) * up, vector3 + (num6 - num5) * up + right, vector3 + right, indicator, true);
		}
		else
		{
			float num8 = Mathf.InverseLerp(minLimit, maxLimit, value);
			GL.Begin(1);
			GL.Color(indicator);
			GL.Vertex(position + num8 * up);
			GL.Vertex(position + right + num8 * up);
			GL.End();
		}
	}

	public void HorizontalMeter(Vector3 position, Vector3 right, Vector3 up, float minLimit, float maxLimit, float ruleStep, float labelMultiplier, bool fill, Color background, Color rule, Color indicator, float value)
	{
		Quad(position, position + up, position + up + right, position + right, background, true);
		float firstRule = 0f;
		int ruleCount = 0;
		FindRules(ref minLimit, ref maxLimit, ref ruleStep, out firstRule, out ruleCount);
		if (ruleCount > 0)
		{
			GL.Begin(1);
			GL.Color(rule);
			for (int i = 0; i < ruleCount; i++)
			{
				float num = Mathf.InverseLerp(minLimit, maxLimit, firstRule + (float)i * ruleStep);
				GL.Vertex(position + num * right);
				GL.Vertex(position + up + num * right);
			}
			GL.End();
		}
		if (labelMultiplier != 0f)
		{
			float num2 = up.magnitude * 0.08f;
			Vector3 vector = up.normalized * num2;
			Vector3 right2 = right.normalized * num2;
			for (int j = 0; j < ruleCount; j++)
			{
				float num3 = firstRule + (float)j * ruleStep;
				float num4 = Mathf.InverseLerp(minLimit, maxLimit, num3);
				Vector3 vector2 = position + num4 * right;
				String(vector2 - vector, right2, vector, 2f, 0, false, 8, rule, (num3 * labelMultiplier).ToString("0.###"));
			}
		}
		if (fill)
		{
			float num5 = Mathf.InverseLerp(minLimit, maxLimit, 0f);
			float num6 = Mathf.InverseLerp(minLimit, maxLimit, value);
			if (num5 > num6)
			{
				float num7 = num6;
				num6 = num5;
				num5 = num7;
			}
			Vector3 vector3 = position + num5 * right;
			Quad(vector3, vector3 + up, vector3 + up + (num6 - num5) * right, vector3 + (num6 - num5) * right, indicator, true);
		}
		else
		{
			float num8 = Mathf.InverseLerp(minLimit, maxLimit, value);
			GL.Begin(1);
			GL.Color(indicator);
			GL.Vertex(position + num8 * right);
			GL.Vertex(position + up + num8 * right);
			GL.End();
		}
	}

	public void Pie(Vector3 position, Vector3 right, Vector3 up, Color background, Color sector, float fillAmount, int divs = 64)
	{
		fillAmount = Mathf.Clamp01(fillAmount);
		Ellipse(position, right, up, background, true);
		Sector(position, right * 0.95f, up * 0.95f, 90f, 90f - fillAmount * 360f, sector, true, 1 + (int)(fillAmount * 63f));
	}

	public void GraphLayout(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, Vector2 ruleSteps, Vector2 labelMultipliers, Color background, Color rule)
	{
		Quad(position, position + up, position + up + right, position + right, background, true);
		Vector2 firstRule = Vector2.zero;
		int ruleCountX = 0;
		int ruleCountY = 0;
		FindRules(ref minLimits, ref maxLimits, ref ruleSteps, out firstRule, out ruleCountX, out ruleCountY);
		if (ruleCountY > 0)
		{
			GL.Begin(1);
			GL.Color(rule);
			for (int i = 0; i < ruleCountY; i++)
			{
				float num = Mathf.InverseLerp(minLimits.y, maxLimits.y, firstRule.y + (float)i * ruleSteps.y);
				GL.Vertex(position + num * up);
				GL.Vertex(position + right + num * up);
			}
			GL.End();
		}
		if (ruleCountX > 0)
		{
			GL.Begin(1);
			GL.Color(rule);
			for (int j = 0; j < ruleCountX; j++)
			{
				float num2 = Mathf.InverseLerp(minLimits.x, maxLimits.x, firstRule.x + (float)j * ruleSteps.x);
				GL.Vertex(position + num2 * right);
				GL.Vertex(position + num2 * right + up);
			}
			GL.End();
		}
		float num3 = up.magnitude * 0.02f;
		Vector3 vector = up.normalized * num3;
		Vector3 vector2 = right.normalized * num3;
		if (labelMultipliers.x != 0f)
		{
			for (int k = 0; k < ruleCountX; k++)
			{
				float num4 = firstRule.x + (float)k * ruleSteps.x;
				float num5 = Mathf.InverseLerp(minLimits.x, maxLimits.x, num4);
				Vector3 vector3 = position + num5 * right;
				String(vector3 - vector, vector2, vector, 2f, 0, false, 8, rule, (num4 * labelMultipliers.x).ToString("0.###"));
			}
		}
		if (labelMultipliers.y != 0f)
		{
			for (int l = 0; l < ruleCountY; l++)
			{
				float num6 = firstRule.y + (float)l * ruleSteps.y;
				float num7 = Mathf.InverseLerp(minLimits.y, maxLimits.y, num6);
				Vector3 vector4 = position + num7 * up;
				String(vector4 - vector2, vector2, vector, 2f, 0, false, 6, rule, (num6 * labelMultipliers.y).ToString("0.###"));
			}
		}
	}

	private bool IsInside(ref Vector2 v, ref Vector2 minLimits, ref Vector2 maxLimits)
	{
		if (v.x < minLimits.x - 0.0001f || v.x > maxLimits.x + 0.0001f || v.y < minLimits.y - 0.0001f || v.y > maxLimits.y + 0.0001f)
		{
			return false;
		}
		return true;
	}

	public void GraphValues(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, GLDraw.GraphType type, Color graph, Vector2[] values)
	{
		FixRange(ref minLimits, ref maxLimits);
		switch (type)
		{
		case GLDraw.GraphType.Dots:
		{
			float magnitude = up.magnitude;
			Vector3 up2 = up * 0.02f;
			Vector3 right2 = right.normalized * magnitude * 0.02f;
			for (int j = 0; j < values.Length; j++)
			{
				Vector2 v3 = values[j];
				if (IsInside(ref v3, ref minLimits, ref maxLimits))
				{
					Vector3 position2 = position + Mathf.InverseLerp(minLimits.x, maxLimits.x, v3.x) * right + Mathf.InverseLerp(minLimits.y, maxLimits.y, v3.y) * up;
					Ellipse(position2, right2, up2, graph, true, 8);
				}
			}
			return;
		}
		case GLDraw.GraphType.Line:
		{
			if (values.Length <= 1)
			{
				break;
			}
			GL.Begin(1);
			GL.Color(graph);
			for (int i = 0; i < values.Length - 1; i++)
			{
				Vector2 v = values[i];
				Vector2 v2 = values[i + 1];
				if (IsInside(ref v, ref minLimits, ref maxLimits) && IsInside(ref v2, ref minLimits, ref maxLimits))
				{
					GL.Vertex(position + Mathf.InverseLerp(minLimits.x, maxLimits.x, v.x) * right + Mathf.InverseLerp(minLimits.y, maxLimits.y, v.y) * up);
					GL.Vertex(position + Mathf.InverseLerp(minLimits.x, maxLimits.x, v2.x) * right + Mathf.InverseLerp(minLimits.y, maxLimits.y, v2.y) * up);
				}
			}
			GL.End();
			return;
		}
		}
		if (type != GLDraw.GraphType.Area || values.Length <= 1)
		{
			return;
		}
		GL.Begin(5);
		GL.Color(graph);
		for (int k = 0; k < values.Length; k++)
		{
			Vector2 v4 = values[k];
			Vector2 v5 = new Vector2(values[k].x, 0f);
			if (IsInside(ref v4, ref minLimits, ref maxLimits) && IsInside(ref v5, ref minLimits, ref maxLimits))
			{
				float num = Mathf.InverseLerp(minLimits.x, maxLimits.x, v4.x);
				float num2 = Mathf.InverseLerp(minLimits.y, maxLimits.y, 0f);
				float num3 = Mathf.InverseLerp(minLimits.y, maxLimits.y, v4.y);
				if (num3 > num2)
				{
					GL.Vertex(position + num * right + num2 * up);
					GL.Vertex(position + num * right + num3 * up);
				}
				else
				{
					GL.Vertex(position + num * right + num3 * up);
					GL.Vertex(position + num * right + num2 * up);
				}
			}
		}
		GL.End();
	}

	public void GraphValues(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, GLDraw.GraphType type, Color graph, GLDraw.CurveFunction function, int samples)
	{
		FixRange(ref minLimits, ref maxLimits);
		if (samples < 2)
		{
			samples = 2;
		}
		float num = minLimits.x;
		float num2 = (maxLimits.x - minLimits.x) / (float)(samples - 1);
		switch (type)
		{
		case GLDraw.GraphType.Dots:
		{
			float magnitude = up.magnitude;
			Vector3 up2 = up * 0.02f;
			Vector3 right2 = right.normalized * magnitude * 0.02f;
			for (int k = 0; k < samples; k++)
			{
				Vector2 v5 = new Vector2(num, function(num));
				if (IsInside(ref v5, ref minLimits, ref maxLimits))
				{
					Vector3 position2 = position + Mathf.InverseLerp(minLimits.x, maxLimits.x, v5.x) * right + Mathf.InverseLerp(minLimits.y, maxLimits.y, v5.y) * up;
					Ellipse(position2, right2, up2, graph, true, 8);
				}
				num += num2;
			}
			break;
		}
		case GLDraw.GraphType.Line:
		{
			GL.Begin(1);
			GL.Color(graph);
			for (int j = 0; j < samples; j++)
			{
				Vector2 v3 = new Vector2(num, function(num));
				Vector2 v4 = new Vector2(num + num2, function(num + num2));
				if (IsInside(ref v3, ref minLimits, ref maxLimits) && IsInside(ref v4, ref minLimits, ref maxLimits))
				{
					GL.Vertex(position + Mathf.InverseLerp(minLimits.x, maxLimits.x, v3.x) * right + Mathf.InverseLerp(minLimits.y, maxLimits.y, v3.y) * up);
					GL.Vertex(position + Mathf.InverseLerp(minLimits.x, maxLimits.x, v4.x) * right + Mathf.InverseLerp(minLimits.y, maxLimits.y, v4.y) * up);
				}
				num += num2;
			}
			GL.End();
			break;
		}
		case GLDraw.GraphType.Area:
		{
			GL.Begin(5);
			GL.Color(graph);
			for (int i = 0; i < samples; i++)
			{
				Vector2 v = new Vector2(num, function(num));
				Vector2 v2 = new Vector2(num, 0f);
				if (IsInside(ref v, ref minLimits, ref maxLimits) && IsInside(ref v2, ref minLimits, ref maxLimits))
				{
					float num3 = Mathf.InverseLerp(minLimits.x, maxLimits.x, v.x);
					float num4 = Mathf.InverseLerp(minLimits.y, maxLimits.y, 0f);
					float num5 = Mathf.InverseLerp(minLimits.y, maxLimits.y, v.y);
					if (num5 > num4)
					{
						GL.Vertex(position + num3 * right + num4 * up);
						GL.Vertex(position + num3 * right + num5 * up);
					}
					else
					{
						GL.Vertex(position + num3 * right + num5 * up);
						GL.Vertex(position + num3 * right + num4 * up);
					}
				}
			}
			GL.End();
			break;
		}
		}
	}

	public void Mesh(Vector3[] vertices, int[] indices, Color color, bool fill)
	{
		if (!fill)
		{
			GL.wireframe = true;
		}
		GL.Begin(4);
		GL.Color(color);
		for (int i = 0; i < indices.Length; i++)
		{
			GL.Vertex(vertices[indices[i]]);
		}
		GL.End();
		if (!fill)
		{
			GL.wireframe = false;
		}
	}

	public void Mesh(Vector3[] vertices, int[] indices, Color[] colors, bool fill)
	{
		if (!fill)
		{
			GL.wireframe = true;
		}
		GL.Begin(4);
		for (int i = 0; i < indices.Length; i++)
		{
			GL.Color(colors[indices[i]]);
			GL.Vertex(vertices[indices[i]]);
		}
		GL.End();
		if (!fill)
		{
			GL.wireframe = false;
		}
	}

	public void Mesh(Mesh mesh, bool fill)
	{
		if (mesh.colors != null && mesh.colors.Length > 0)
		{
			Mesh(mesh.vertices, mesh.triangles, mesh.colors, fill);
		}
		else
		{
			Mesh(mesh.vertices, mesh.triangles, meshDefaultColor, fill);
		}
	}

	public void Mesh(Mesh mesh)
	{
		if (mesh.colors != null && mesh.colors.Length > 0)
		{
			Mesh(mesh.vertices, mesh.triangles, mesh.colors, true);
		}
		else
		{
			Mesh(mesh.vertices, mesh.triangles, meshDefaultColor, true);
		}
	}

	public void Collider(BoxCollider collider)
	{
		Box(collider.center, collider.size, colliderFillColor, true);
		Box(collider.center, collider.size, colliderEdgeColor, false);
	}

	public void Collider(SphereCollider collider)
	{
		Vector3 center = collider.center;
		float radius = collider.radius;
		Ellipsoid(center, Vector3.one * 2f * radius, colliderFillColor, true);
		Ellipse(center, Vector3.right * radius, Vector3.up * radius, colliderEdgeColor, false, 32);
		Ellipse(center, Vector3.right * radius, Vector3.forward * radius, colliderEdgeColor, false, 32);
		Ellipse(center, Vector3.forward * radius, Vector3.up * radius, colliderEdgeColor, false, 32);
	}

	public void Collider(CapsuleCollider collider)
	{
		Vector3 center = collider.center;
		int direction = collider.direction;
		float height = collider.height;
		float radius = collider.radius;
		direction = Mathf.Clamp(direction, 0, 2);
		Vector3 vector = Vector3.right;
		switch (direction)
		{
		case 1:
			vector = Vector3.up;
			break;
		case 2:
			vector = Vector3.forward;
			break;
		}
		Capsule(center, vector, height, radius, colliderFillColor, true);
		height -= radius * 2f;
		if (height < 0f)
		{
			height = 0f;
		}
		Vector3 zero = Vector3.zero;
		Vector3 vector2 = vector;
		Vector3 vector3;
		switch (direction)
		{
		case 0:
			vector3 = Vector3.down;
			zero = Vector3.forward;
			break;
		case 1:
			vector3 = Vector3.right;
			zero = Vector3.forward;
			break;
		default:
			vector3 = Vector3.right;
			zero = Vector3.down;
			break;
		}
		Vector3 vector4 = vector2 * height * 0.5f;
		vector2 *= radius;
		vector3 *= radius;
		zero *= radius;
		Sector(center + vector4, vector3, vector2, 0f, 180f, colliderEdgeColor, false, 16);
		Sector(center + vector4, zero, vector2, 0f, 180f, colliderEdgeColor, false, 16);
		Sector(center - vector4, vector3, -vector2, 0f, 180f, colliderEdgeColor, false, 16);
		Sector(center - vector4, zero, -vector2, 0f, 180f, colliderEdgeColor, false, 16);
		Ellipse(center + vector4, vector3, zero, colliderEdgeColor, false, 32);
		Ellipse(center - vector4, vector3, zero, colliderEdgeColor, false, 32);
		if (height > 0f)
		{
			Line(center + vector4 + vector3, center - vector4 + vector3, colliderEdgeColor);
			Line(center + vector4 - vector3, center - vector4 - vector3, colliderEdgeColor);
			Line(center + vector4 + zero, center - vector4 + zero, colliderEdgeColor);
			Line(center + vector4 - zero, center - vector4 - zero, colliderEdgeColor);
		}
	}

	public void Collider(WheelCollider collider)
	{
		Vector3 center = collider.center;
		float suspensionDistance = collider.suspensionDistance;
		float radius = collider.radius;
		float f = collider.steerAngle / 180f * (float)Math.PI;
		Vector3 vector = new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
		Ellipse(center, vector * radius, Vector3.up * radius, colliderFillColor, true, 32);
		Ellipse(center, -vector * radius, Vector3.up * radius, colliderFillColor, true, 32);
		Ellipse(center, vector * radius, Vector3.up * radius, colliderEdgeColor, false, 32);
		Line(center, center + Vector3.down * (radius + suspensionDistance), colliderEdgeColor);
	}

	public void Collider(Collider collider)
	{
		if (collider is BoxCollider)
		{
			Collider(collider as BoxCollider);
		}
		else if (collider is SphereCollider)
		{
			Collider(collider as SphereCollider);
		}
		else if (collider is CapsuleCollider)
		{
			Collider(collider as CapsuleCollider);
		}
		else if (collider is WheelCollider)
		{
			Collider(collider as WheelCollider);
		}
	}

	public void String(Vector3 pos, Vector3 right, Vector3 up, float size, int strokeWidth, bool monospace, int align, Color color, string s)
	{
		if (currMaterial == defaultMaterial)
		{
			currMaterial.SetPass(1);
		}
		StencilInc();
		font.DrawString(pos, right, up, size, strokeWidth, monospace, align, color, s);
		if (currMaterial == defaultMaterial)
		{
			currMaterial.SetPass(currPass);
		}
	}

	public void Focus(Vector3 position, float diameter, Color dimColor)
	{
		float num = diameter * 0.5f;
		Camera current = Camera.current;
		Vector3 vector = current.WorldToScreenPoint(position);
		if (vector.z < 0f)
		{
			return;
		}
		vector.z = 0f;
		Transform transform = current.transform;
		Vector3 vector2 = current.WorldToScreenPoint(position + transform.right * num);
		Vector3 vector3 = current.WorldToScreenPoint(position - transform.right * num);
		vector2.z = 0f;
		vector3.z = 0f;
		num = (vector2 - vector3).magnitude * 0.5f;
		float num2 = 1.5f * num;
		float a = 0f;
		a = Mathf.Max(a, 0f - vector.x);
		a = Mathf.Max(a, vector.x - (float)Screen.width);
		a = Mathf.Max(a, 0f - vector.y);
		a = Mathf.Max(a, vector.y - (float)Screen.height);
		if (!(a > num2))
		{
			float num3 = 0.33f;
			if (a > num)
			{
				num3 *= 1f - Mathf.InverseLerp(num, num2, a);
			}
			Color c = dimColor;
			c.a = 0f;
			Vector3 vector4 = Vector3.up * num;
			Vector3 vector5 = Vector3.right * num;
			GL.PushMatrix();
			GL.LoadPixelMatrix();
			defaultMaterial.SetPass(1);
			trig.Init(64);
			GL.Begin(5);
			GL.Color(c);
			for (int i = 0; i < 65; i++)
			{
				GL.Vertex(vector + (trig.cos * vector5 + trig.sin * vector4));
				GL.Vertex(vector);
				trig.Inc();
			}
			GL.End();
			Vector3 vector6 = Vector3.up * num2;
			Vector3 vector7 = Vector3.right * num2;
			trig.Init(64);
			GL.Begin(5);
			for (int j = 0; j < 65; j++)
			{
				GL.Color(dimColor);
				GL.Vertex(vector + (trig.cos * vector7 + trig.sin * vector6));
				GL.Color(c);
				GL.Vertex(vector + (trig.cos * vector5 + trig.sin * vector4));
				trig.Inc();
			}
			GL.End();
			vector4 = Vector3.up * Screen.height;
			vector5 = Vector3.right * Screen.width;
			Quad(Vector3.zero, vector4, vector5 + vector4, vector5, dimColor, true);
			currMaterial.SetPass(currPass);
			GL.PopMatrix();
		}
	}

	private void Init()
	{
		CreateDefaultMaterial();
		StencilReset();
		trig = default(IncrementalTrig);
		CreateArrays();
		CreateFont();
		initialized = true;
	}

	private void CreateDefaultMaterial()
	{
		Shader shader = Shader.Find("GLDraw/Default");
		defaultMaterial = new Material(shader);
		defaultMaterial.hideFlags = HideFlags.HideAndDontSave;
	}

	private void CreateArrays()
	{
		cubeVertices = new Vector3[8]
		{
			new Vector3(-1f, 1f, -1f),
			new Vector3(-1f, 1f, 1f),
			new Vector3(1f, 1f, 1f),
			new Vector3(1f, 1f, -1f),
			new Vector3(-1f, -1f, -1f),
			new Vector3(-1f, -1f, 1f),
			new Vector3(1f, -1f, 1f),
			new Vector3(1f, -1f, -1f)
		};
		cubeIndices = new int[24]
		{
			0, 1, 2, 3, 0, 4, 5, 1, 1, 5,
			6, 2, 2, 6, 7, 3, 3, 7, 4, 0,
			7, 6, 5, 4
		};
		SphereCreator sphereCreator = new SphereCreator();
		sphereCreator.Create(2, SphereCreator.Topology.IcosahedronSphere);
		sphereVertices = sphereCreator.GetVertices();
		sphereIndices = sphereCreator.GetIndices();
		sphereCreator.Create(2, SphereCreator.Topology.OctahedronHemisphere);
		hemisphereVertices = sphereCreator.GetVertices();
		hemisphereIndices = sphereCreator.GetIndices();
	}

	private void CreateFont()
	{
		font = new BFont();
		int num = 65535;
		int num2 = 65534;
		BFont bFont = font;
		int[] obj = new int[8] { 0, 8, 16, 8, 4, 0, 8, 0 };
		obj[0] = num2;
		obj[5] = num2;
		bFont.Add16('!', obj);
		BFont bFont2 = font;
		int[] obj2 = new int[10] { 0, 6, 16, 6, 12, 0, 10, 16, 10, 12 };
		obj2[0] = num2;
		obj2[5] = num2;
		bFont2.Add16('"', obj2);
		BFont bFont3 = font;
		int[] obj3 = new int[20]
		{
			0, 6, 14, 4, 2, 0, 12, 14, 10, 2,
			0, 2, 11, 14, 11, 0, 2, 5, 14, 5
		};
		obj3[0] = num2;
		obj3[5] = num2;
		obj3[10] = num2;
		obj3[15] = num2;
		bFont3.Add16('#', obj3);
		BFont bFont4 = font;
		int[] obj4 = new int[36]
		{
			0, 12, 14, 4, 0, -4, 0, 8, 14, 4,
			0, -6, 0, 8, 8, -6, 0, 6, 0, 8,
			2, 6, 0, -4, 0, 4, 2, 4, 0, -4,
			0, 0, 8, 16, 8, 0
		};
		obj4[0] = num;
		obj4[31] = num2;
		bFont4.Add16('$', obj4);
		BFont bFont5 = font;
		int[] obj5 = new int[43]
		{
			0, 3, 2, 13, 14, 0, 4, 14, -3, 0,
			3, 0, 4, 10, 3, 0, -3, 0, 4, 14,
			-3, 0, 3, 0, 0, 12, 6, 0, 0, 3,
			0, 12, 2, 3, 0, -3, 0, 12, 6, -3,
			0, 0, 0
		};
		obj5[0] = num2;
		obj5[5] = num;
		obj5[24] = num;
		bFont5.Add16('%', obj5);
		BFont bFont6 = font;
		int[] obj6 = new int[44]
		{
			0, 28, 0, 0, 0, 0, 0, 13, 20, 0,
			0, -3, 4, 16, 32, -7, 0, 3, 0, 22,
			28, 0, 2, 0, 0, 0, 13, 20, 0, 0,
			-8, -6, 16, 0, -14, 0, 6, 0, 28, 8,
			0, 0, 0, 0
		};
		obj6[0] = num;
		obj6[25] = num;
		bFont6.Add32('&', obj6);
		BFont bFont7 = font;
		int[] obj7 = new int[5] { 0, 8, 16, 8, 12 };
		obj7[0] = num2;
		bFont7.Add16('\'', obj7);
		BFont bFont8 = font;
		int[] obj8 = new int[19]
		{
			0, 11, 16, 0, 0, 0, 0, 5, 8, 0,
			6, 0, -6, 11, 0, 0, 0, 0, 0
		};
		obj8[0] = num;
		bFont8.Add16('(', obj8);
		BFont bFont9 = font;
		int[] obj9 = new int[19]
		{
			0, 5, 16, 0, 0, 0, 0, 11, 8, 0,
			6, 0, -6, 5, 0, 0, 0, 0, 0
		};
		obj9[0] = num;
		bFont9.Add16(')', obj9);
		BFont bFont10 = font;
		int[] obj10 = new int[15]
		{
			0, 8, 14, 8, 2, 0, 2, 12, 14, 4,
			0, 2, 4, 14, 12
		};
		obj10[0] = num2;
		obj10[5] = num2;
		obj10[10] = num2;
		bFont10.Add16('*', obj10);
		BFont bFont11 = font;
		int[] obj11 = new int[10] { 0, 8, 12, 8, 4, 0, 4, 8, 12, 8 };
		obj11[0] = num2;
		obj11[5] = num2;
		bFont11.Add16('+', obj11);
		BFont bFont12 = font;
		int[] obj12 = new int[13]
		{
			0, 6, -2, 0, 0, 1, 0, 10, 2, 0,
			-1, 0, 0
		};
		obj12[0] = num;
		bFont12.Add16(',', obj12);
		BFont bFont13 = font;
		int[] obj13 = new int[5] { 0, 4, 8, 12, 8 };
		obj13[0] = num2;
		bFont13.Add16('-', obj13);
		font.Add16('.', num2, 8, 0);
		font.Add16('/', num2, 4, 0, 12, 16);
		BFont bFont14 = font;
		int[] obj14 = new int[19]
		{
			0, 8, 16, -8, 0, 8, 0, 8, 0, 8,
			0, -8, 0, 8, 16, -8, 0, 8, 0
		};
		obj14[0] = num;
		bFont14.Add16('0', obj14);
		BFont bFont15 = font;
		int[] obj15 = new int[7] { 0, 6, 12, 10, 16, 10, 0 };
		obj15[0] = num2;
		bFont15.Add16('1', obj15);
		BFont bFont16 = font;
		int[] obj16 = new int[31]
		{
			0, 3, 16, 0, 0, 0, 0, 7, 16, 0,
			0, 2, 0, 12, 12, 0, 3, 0, -5, 3,
			0, 0, 8, 0, 0, 12, 0, 0, 0, 0,
			0
		};
		obj16[0] = num;
		bFont16.Add16('2', obj16);
		BFont bFont17 = font;
		int[] obj17 = new int[44]
		{
			0, 3, 16, 0, 0, 0, 0, 8, 16, 0,
			0, 6, 0, 8, 8, 6, 0, 0, 0, 6,
			8, 0, 0, 0, 0, 0, 8, 8, 0, 0,
			6, 0, 8, 0, 6, 0, 0, 0, 3, 0,
			0, 0, 0, 0
		};
		obj17[0] = num;
		obj17[25] = num;
		bFont17.Add16('3', obj17);
		BFont bFont18 = font;
		int[] obj18 = new int[9] { 0, 11, 0, 11, 16, 2, 4, 14, 4 };
		obj18[0] = num2;
		bFont18.Add16('4', obj18);
		BFont bFont19 = font;
		int[] obj19 = new int[37]
		{
			0, 12, 16, 0, 0, 0, 0, 3, 16, 0,
			0, 0, 0, 3, 10, 0, 0, 0, 0, 7,
			10, 0, 0, 7, 0, 7, 0, 7, 0, 0,
			0, 3, 0, 0, 0, 0, 0
		};
		obj19[0] = num;
		bFont19.Add16('5', obj19);
		BFont bFont20 = font;
		int[] obj20 = new int[37]
		{
			0, 12, 16, 0, 0, -6, 0, 3, 8, 0,
			4, 0, 0, 3, 5, 0, 0, 0, -3, 8,
			0, -3, 0, 7, 0, 8, 10, 7, 0, -4,
			0, 3, 5, 0, 2, 0, 0
		};
		obj20[0] = num;
		bFont20.Add16('6', obj20);
		BFont bFont21 = font;
		int[] obj21 = new int[7] { 0, 3, 16, 13, 16, 6, 0 };
		obj21[0] = num2;
		bFont21.Add16('7', obj21);
		BFont bFont22 = font;
		int[] obj22 = new int[31]
		{
			0, 16, 18, 0, 0, -11, 0, 16, 32, -11,
			0, 11, 0, 16, 18, 11, 0, -14, 0, 16,
			0, -14, 0, 14, 0, 16, 18, 14, 0, 0,
			0
		};
		obj22[0] = num;
		bFont22.Add32('8', obj22);
		BFont bFont23 = font;
		int[] obj23 = new int[37]
		{
			0, 4, 0, 0, 0, 6, 0, 13, 8, 0,
			-4, 0, 0, 13, 11, 0, 0, 0, 3, 8,
			16, 3, 0, -7, 0, 8, 6, -7, 0, 4,
			0, 13, 11, 0, -2, 0, 0
		};
		obj23[0] = num;
		bFont23.Add16('9', obj23);
		BFont bFont24 = font;
		int[] obj24 = new int[6] { 0, 8, 12, 0, 8, 4 };
		obj24[0] = num2;
		obj24[3] = num2;
		bFont24.Add16(':', obj24);
		BFont bFont25 = font;
		int[] obj25 = new int[16]
		{
			0, 6, -2, 0, 0, 1, 0, 10, 2, 0,
			-1, 0, 0, 0, 8, 12
		};
		obj25[0] = num;
		obj25[13] = num2;
		bFont25.Add16(';', obj25);
		BFont bFont26 = font;
		int[] obj26 = new int[7] { 0, 11, 3, 5, 8, 11, 13 };
		obj26[0] = num2;
		bFont26.Add16('<', obj26);
		BFont bFont27 = font;
		int[] obj27 = new int[10] { 0, 4, 10, 12, 10, 0, 4, 6, 12, 6 };
		obj27[0] = num2;
		obj27[5] = num2;
		bFont27.Add16('=', obj27);
		BFont bFont28 = font;
		int[] obj28 = new int[7] { 0, 5, 13, 11, 8, 5, 3 };
		obj28[0] = num2;
		bFont28.Add16('>', obj28);
		BFont bFont29 = font;
		int[] obj29 = new int[28]
		{
			0, 3, 16, 0, 0, 0, 0, 8, 16, 0,
			0, 2, 0, 13, 12, 0, 3, 0, -4, 8,
			4, 0, 4, 0, 0, 0, 8, 0
		};
		obj29[0] = num;
		obj29[25] = num2;
		bFont29.Add16('?', obj29);
		BFont bFont30 = font;
		int[] obj30 = new int[43]
		{
			0, 14, 12, 0, 0, -4, 0, 6, 8, 0,
			4, 0, -4, 14, 4, -4, 0, 0, 0, 14,
			12, 0, 0, 0, 2, 8, 16, 6, 0, -9,
			0, 8, 0, -9, 0, 0, 0, 13, 0, 0,
			0, 0, 0
		};
		obj30[0] = num;
		bFont30.Add16('@', obj30);
		BFont bFont31 = font;
		int[] obj31 = new int[12]
		{
			0, 2, 0, 8, 16, 14, 0, 0, 5, 8,
			11, 8
		};
		obj31[0] = num2;
		obj31[7] = num2;
		bFont31.Add16('A', obj31);
		BFont bFont32 = font;
		int[] obj32 = new int[50]
		{
			0, 3, 0, 0, 0, 0, 0, 3, 16, 0,
			0, 0, 0, 9, 16, 0, 0, 6, 0, 9,
			8, 6, 0, 0, 0, 3, 8, 0, 0, 0,
			0, 0, 9, 8, 0, 0, 6, 0, 9, 0,
			6, 0, 0, 0, 3, 0, 0, 0, 0, 0
		};
		obj32[0] = num;
		obj32[31] = num;
		bFont32.Add16('B', obj32);
		BFont bFont33 = font;
		int[] obj33 = new int[25]
		{
			0, 14, 16, 0, 0, 0, 0, 9, 16, 0,
			0, -9, 0, 9, 0, -9, 0, 0, 0, 14,
			0, 0, 0, 0, 0
		};
		obj33[0] = num;
		bFont33.Add16('C', obj33);
		BFont bFont34 = font;
		int[] obj34 = new int[31]
		{
			0, 2, 0, 0, 0, 0, 0, 2, 16, 0,
			0, 0, 0, 7, 16, 0, 0, 9, 0, 7,
			0, 9, 0, 0, 0, 2, 0, 0, 0, 0,
			0
		};
		obj34[0] = num;
		bFont34.Add16('D', obj34);
		BFont bFont35 = font;
		int[] obj35 = new int[14]
		{
			0, 13, 16, 3, 16, 3, 0, 13, 0, 0,
			3, 8, 9, 8
		};
		obj35[0] = num2;
		obj35[9] = num2;
		bFont35.Add16('E', obj35);
		BFont bFont36 = font;
		int[] obj36 = new int[12]
		{
			0, 13, 16, 3, 16, 3, 0, 0, 3, 8,
			9, 8
		};
		obj36[0] = num2;
		obj36[7] = num2;
		bFont36.Add16('F', obj36);
		BFont bFont37 = font;
		int[] obj37 = new int[37]
		{
			0, 14, 16, 0, 0, 0, 0, 9, 16, 0,
			0, -9, 0, 9, 0, -9, 0, 0, 0, 14,
			0, 0, 0, 0, 0, 14, 8, 0, 0, 0,
			0, 9, 8, 0, 0, 0, 0
		};
		obj37[0] = num;
		bFont37.Add16('G', obj37);
		BFont bFont38 = font;
		int[] obj38 = new int[15]
		{
			0, 3, 16, 3, 0, 0, 13, 16, 13, 0,
			0, 3, 8, 13, 8
		};
		obj38[0] = num2;
		obj38[5] = num2;
		obj38[10] = num2;
		bFont38.Add16('H', obj38);
		font.Add16('I', num2, 8, 16, 8, 0);
		BFont bFont39 = font;
		int[] obj39 = new int[25]
		{
			0, 13, 16, 0, 0, 0, 0, 13, 8, 0,
			0, 0, -5, 8, 0, 4, 0, 0, 0, 3,
			0, 0, 0, 0, 0
		};
		obj39[0] = num;
		bFont39.Add16('J', obj39);
		BFont bFont40 = font;
		int[] obj40 = new int[15]
		{
			0, 3, 16, 3, 0, 0, 3, 6, 13, 16,
			0, 5, 8, 13, 0
		};
		obj40[0] = num2;
		obj40[5] = num2;
		obj40[10] = num2;
		bFont40.Add16('K', obj40);
		BFont bFont41 = font;
		int[] obj41 = new int[7] { 0, 3, 16, 3, 0, 13, 0 };
		obj41[0] = num2;
		bFont41.Add16('L', obj41);
		BFont bFont42 = font;
		int[] obj42 = new int[11]
		{
			0, 2, 0, 3, 16, 8, 6, 13, 16, 14,
			0
		};
		obj42[0] = num2;
		bFont42.Add16('M', obj42);
		BFont bFont43 = font;
		int[] obj43 = new int[9] { 0, 3, 0, 3, 16, 13, 0, 13, 16 };
		obj43[0] = num2;
		bFont43.Add16('N', obj43);
		BFont bFont44 = font;
		int[] obj44 = new int[19]
		{
			0, 8, 16, 0, 0, 9, 0, 8, 0, 9,
			0, -9, 0, 8, 16, -9, 0, 0, 0
		};
		obj44[0] = num;
		bFont44.Add16('O', obj44);
		BFont bFont45 = font;
		int[] obj45 = new int[31]
		{
			0, 3, 0, 0, 0, 0, 0, 3, 16, 0,
			0, 0, 0, 9, 16, 0, 0, 6, 0, 9,
			8, 6, 0, 0, 0, 3, 8, 0, 0, 0,
			0
		};
		obj45[0] = num;
		bFont45.Add16('P', obj45);
		BFont bFont46 = font;
		int[] obj46 = new int[24]
		{
			0, 8, 16, 0, 0, 9, 0, 8, 0, 9,
			0, -9, 0, 8, 16, -9, 0, 0, 0, 0,
			8, 8, 14, 0
		};
		obj46[0] = num;
		obj46[19] = num2;
		bFont46.Add16('Q', obj46);
		BFont bFont47 = font;
		int[] obj47 = new int[36]
		{
			0, 3, 0, 0, 0, 0, 0, 3, 16, 0,
			0, 0, 0, 9, 16, 0, 0, 6, 0, 9,
			8, 6, 0, 0, 0, 3, 8, 0, 0, 0,
			0, 0, 9, 8, 14, 0
		};
		obj47[0] = num;
		obj47[31] = num2;
		bFont47.Add16('R', obj47);
		BFont bFont48 = font;
		int[] obj48 = new int[37]
		{
			0, 13, 16, 0, 0, 0, 0, 8, 16, 0,
			0, -4, 0, 2, 12, 0, 2, 0, -7, 14,
			4, 0, 7, 0, -2, 8, 0, 4, 0, 0,
			0, 2, 0, 0, 0, 0, 0
		};
		obj48[0] = num;
		bFont48.Add16('S', obj48);
		BFont bFont49 = font;
		int[] obj49 = new int[10] { 0, 3, 16, 13, 16, 0, 8, 0, 8, 16 };
		obj49[0] = num2;
		obj49[5] = num2;
		bFont49.Add16('T', obj49);
		BFont bFont50 = font;
		int[] obj50 = new int[31]
		{
			0, 3, 16, 0, 0, 0, 0, 3, 8, 0,
			0, 0, -5, 8, 0, -4, 0, 4, 0, 13,
			8, 0, -5, 0, 0, 13, 16, 0, 0, 0,
			0
		};
		obj50[0] = num;
		bFont50.Add16('U', obj50);
		BFont bFont51 = font;
		int[] obj51 = new int[7] { 0, 3, 16, 8, 0, 13, 16 };
		obj51[0] = num2;
		bFont51.Add16('V', obj51);
		BFont bFont52 = font;
		int[] obj52 = new int[11]
		{
			0, 2, 16, 3, 0, 8, 10, 13, 0, 14,
			16
		};
		obj52[0] = num2;
		bFont52.Add16('W', obj52);
		BFont bFont53 = font;
		int[] obj53 = new int[10] { 0, 3, 0, 13, 16, 0, 3, 16, 13, 0 };
		obj53[0] = num2;
		obj53[5] = num2;
		bFont53.Add16('X', obj53);
		BFont bFont54 = font;
		int[] obj54 = new int[12]
		{
			0, 3, 16, 8, 8, 13, 16, 0, 8, 0,
			8, 8
		};
		obj54[0] = num2;
		obj54[7] = num2;
		bFont54.Add16('Y', obj54);
		BFont bFont55 = font;
		int[] obj55 = new int[9] { 0, 3, 16, 13, 16, 3, 0, 13, 0 };
		obj55[0] = num2;
		bFont55.Add16('Z', obj55);
		BFont bFont56 = font;
		int[] obj56 = new int[9] { 0, 11, 16, 5, 16, 5, 0, 11, 0 };
		obj56[0] = num2;
		bFont56.Add16('[', obj56);
		font.Add16('\\', num2, 4, 16, 12, 0);
		BFont bFont57 = font;
		int[] obj57 = new int[9] { 0, 5, 16, 11, 16, 11, 0, 5, 0 };
		obj57[0] = num2;
		bFont57.Add16(']', obj57);
		BFont bFont58 = font;
		int[] obj58 = new int[7] { 0, 4, 12, 8, 16, 12, 12 };
		obj58[0] = num2;
		bFont58.Add16('^', obj58);
		font.Add16('_', num2, 2, 0, 14, 0);
		BFont bFont59 = font;
		int[] obj59 = new int[5] { 0, 6, 16, 10, 12 };
		obj59[0] = num2;
		bFont59.Add16('`', obj59);
		BFont bFont60 = font;
		int[] obj60 = new int[37]
		{
			0, 12, 0, 0, 0, 0, 0, 12, 10, 0,
			0, 0, 0, 8, 10, 0, 0, -3, 0, 3,
			5, 0, 3, 0, -3, 8, 0, -3, 0, 0,
			0, 12, 0, 0, 0, 0, 0
		};
		obj60[0] = num;
		bFont60.Add16('a', obj60);
		BFont bFont61 = font;
		int[] obj61 = new int[37]
		{
			0, 4, 14, 0, 0, 0, 0, 4, 0, 0,
			0, 0, 0, 8, 0, 0, 0, 3, 0, 13,
			5, 0, -3, 0, 3, 8, 10, 3, 0, 0,
			0, 4, 10, 0, 0, 0, 0
		};
		obj61[0] = num;
		bFont61.Add16('b', obj61);
		BFont bFont62 = font;
		int[] obj62 = new int[31]
		{
			0, 12, 10, 0, 0, 0, 0, 8, 10, 3,
			0, -3, 0, 3, 5, 0, 3, 0, -3, 8,
			0, -3, 0, 3, 0, 12, 0, 0, 0, 0,
			0
		};
		obj62[0] = num;
		bFont62.Add16('c', obj62);
		BFont bFont63 = font;
		int[] obj63 = new int[37]
		{
			0, 12, 14, 0, 0, 0, 0, 12, 0, 0,
			0, 0, 0, 8, 0, 0, 0, -3, 0, 3,
			5, 0, -3, 0, 3, 8, 10, -3, 0, 3,
			0, 12, 10, 0, 0, 0, 0
		};
		obj63[0] = num;
		bFont63.Add16('d', obj63);
		BFont bFont64 = font;
		int[] obj64 = new int[37]
		{
			0, 3, 5, 0, 0, 0, 0, 13, 5, 0,
			0, 0, 3, 8, 10, 3, 0, -3, 0, 3,
			5, 0, 3, 0, -3, 8, 0, -3, 0, 0,
			0, 12, 0, 0, 0, 0, 0
		};
		obj64[0] = num;
		bFont64.Add16('e', obj64);
		BFont bFont65 = font;
		int[] obj65 = new int[24]
		{
			0, 10, 14, 0, 0, -3, 0, 5, 9, 0,
			3, 0, 0, 5, 0, 0, 3, 0, 0, 0,
			5, 8, 10, 8
		};
		obj65[0] = num;
		obj65[19] = num2;
		bFont65.Add16('f', obj65);
		BFont bFont66 = font;
		int[] obj66 = new int[49]
		{
			0, 12, 0, 0, 0, 0, 0, 8, 0, 3,
			0, -3, 0, 3, 5, 0, -3, 0, 3, 8,
			10, -3, 0, 0, 0, 12, 10, 0, 0, 0,
			0, 12, 0, 0, 0, 0, -3, 7, -5, 3,
			0, 0, 0, 5, -5, 0, 0, 0, 0
		};
		obj66[0] = num;
		bFont66.Add16('g', obj66);
		BFont bFont67 = font;
		int[] obj67 = new int[30]
		{
			0, 4, 14, 4, 0, 0, 4, 10, 0, 0,
			0, 0, 8, 10, 0, 0, 3, 0, 13, 5,
			0, 3, 0, -3, 13, 0, 0, 0, 0, 0
		};
		obj67[0] = num2;
		obj67[5] = num;
		bFont67.Add16('h', obj67);
		BFont bFont68 = font;
		int[] obj68 = new int[8] { 0, 8, 10, 8, 0, 0, 8, 14 };
		obj68[0] = num2;
		obj68[5] = num2;
		bFont68.Add16('i', obj68);
		BFont bFont69 = font;
		int[] obj69 = new int[22]
		{
			0, 11, 10, 0, 0, 0, 0, 11, 0, 0,
			0, 0, -3, 6, -5, 3, 0, 0, 0, 0,
			11, 14
		};
		obj69[0] = num;
		obj69[19] = num2;
		bFont69.Add16('j', obj69);
		BFont bFont70 = font;
		int[] obj70 = new int[15]
		{
			0, 8, 28, 8, 0, 0, 8, 8, 24, 20,
			0, 12, 11, 24, 0
		};
		obj70[0] = num2;
		obj70[5] = num2;
		obj70[10] = num2;
		bFont70.Add32('k', obj70);
		BFont bFont71 = font;
		int[] obj71 = new int[19]
		{
			0, 6, 14, 0, 0, 0, 0, 6, 5, 0,
			0, 0, -3, 11, 0, -3, 0, 0, 0
		};
		obj71[0] = num;
		bFont71.Add16('l', obj71);
		BFont bFont72 = font;
		int[] obj72 = new int[56]
		{
			0, 2, 0, 0, 0, 0, 0, 2, 10, 0,
			0, 0, 0, 5, 10, -2, 0, 2, 0, 8,
			5, 0, 3, 0, 0, 8, 0, 0, 0, 0,
			0, 0, 8, 5, 0, 0, 0, 3, 11, 10,
			-2, 0, 2, 0, 14, 5, 0, 3, 0, 0,
			14, 0, 0, 0, 0, 0
		};
		obj72[0] = num;
		obj72[31] = num;
		bFont72.Add16('m', obj72);
		BFont bFont73 = font;
		int[] obj73 = new int[31]
		{
			0, 4, 0, 0, 0, 0, 0, 4, 10, 0,
			0, 0, 0, 8, 10, 0, 0, 3, 0, 13,
			5, 0, 3, 0, 0, 13, 0, 0, 0, 0,
			0
		};
		obj73[0] = num;
		bFont73.Add16('n', obj73);
		BFont bFont74 = font;
		int[] obj74 = new int[31]
		{
			0, 8, 10, 0, 0, -3, 0, 3, 5, 0,
			3, 0, -3, 8, 0, -3, 0, 3, 0, 13,
			5, 0, -3, 0, 3, 8, 10, 3, 0, 0,
			0
		};
		obj74[0] = num;
		bFont74.Add16('o', obj74);
		BFont bFont75 = font;
		int[] obj75 = new int[37]
		{
			0, 4, -5, 0, 0, 0, 0, 4, 10, 0,
			0, 0, 0, 8, 10, 0, 0, 3, 0, 13,
			5, 0, 3, 0, -3, 8, 0, 3, 0, 0,
			0, 4, 0, 0, 0, 0, 0
		};
		obj75[0] = num;
		bFont75.Add16('p', obj75);
		BFont bFont76 = font;
		int[] obj76 = new int[37]
		{
			0, 12, -5, 0, 0, 0, 0, 12, 10, 0,
			0, 0, 0, 8, 10, 0, 0, -3, 0, 3,
			5, 0, 3, 0, -3, 8, 0, -3, 0, 0,
			0, 12, 0, 0, 0, 0, 0
		};
		obj76[0] = num;
		bFont76.Add16('q', obj76);
		BFont bFont77 = font;
		int[] obj77 = new int[25]
		{
			0, 11, 10, 0, 0, 0, 0, 8, 10, 0,
			0, -3, 0, 3, 5, 0, 3, 0, 0, 3,
			0, 0, 0, 0, 0
		};
		obj77[0] = num;
		bFont77.Add16('r', obj77);
		BFont bFont78 = font;
		int[] obj78 = new int[31]
		{
			0, 11, 10, 0, 0, 0, 0, 8, 10, 0,
			0, -6, 0, 8, 5, -6, 0, 6, 0, 8,
			0, 6, 0, 0, 0, 4, 0, 0, 0, 0,
			0
		};
		obj78[0] = num;
		bFont78.Add16('s', obj78);
		BFont bFont79 = font;
		int[] obj79 = new int[24]
		{
			0, 6, 14, 0, 0, 0, 0, 6, 5, 0,
			0, 0, -3, 11, 0, -3, 0, 0, 0, 0,
			6, 10, 11, 10
		};
		obj79[0] = num;
		obj79[19] = num2;
		bFont79.Add16('t', obj79);
		BFont bFont80 = font;
		int[] obj80 = new int[31]
		{
			0, 12, 10, 0, 0, 0, 0, 12, 0, 0,
			0, 0, 0, 8, 0, 0, 0, -3, 0, 3,
			5, 0, -3, 0, 0, 3, 10, 0, 0, 0,
			0
		};
		obj80[0] = num;
		bFont80.Add16('u', obj80);
		BFont bFont81 = font;
		int[] obj81 = new int[7] { 0, 3, 10, 8, 0, 13, 10 };
		obj81[0] = num2;
		bFont81.Add16('v', obj81);
		BFont bFont82 = font;
		int[] obj82 = new int[11]
		{
			0, 2, 10, 4, 0, 8, 8, 12, 0, 14,
			10
		};
		obj82[0] = num2;
		bFont82.Add16('w', obj82);
		BFont bFont83 = font;
		int[] obj83 = new int[10] { 0, 3, 0, 13, 10, 0, 3, 10, 13, 0 };
		obj83[0] = num2;
		obj83[5] = num2;
		bFont83.Add16('x', obj83);
		BFont bFont84 = font;
		int[] obj84 = new int[50]
		{
			0, 3, 10, 0, 0, 0, 0, 3, 5, 0,
			0, 0, -3, 8, 0, -3, 0, 3, 0, 12,
			0, 0, 0, 0, 0, 0, 12, 10, 0, 0,
			0, 0, 12, 0, 0, 0, 0, -3, 7, -5,
			3, 0, 0, 0, 5, -5, 0, 0, 0, 0
		};
		obj84[0] = num;
		obj84[25] = num;
		bFont84.Add16('y', obj84);
		BFont bFont85 = font;
		int[] obj85 = new int[9] { 0, 3, 10, 13, 10, 3, 0, 13, 0 };
		obj85[0] = num2;
		bFont85.Add16('z', obj85);
		BFont bFont86 = font;
		int[] obj86 = new int[19]
		{
			0, 11, 16, 0, 0, -8, 0, 5, 8, 6,
			1, 6, -1, 11, 0, -8, 0, 0, 0
		};
		obj86[0] = num;
		bFont86.Add16('{', obj86);
		font.Add16('|', num2, 8, 0, 8, 16);
		BFont bFont87 = font;
		int[] obj87 = new int[19]
		{
			0, 5, 16, 0, 0, 8, 0, 11, 8, -6,
			1, -6, -1, 5, 0, 8, 0, 0, 0
		};
		obj87[0] = num;
		bFont87.Add16('}', obj87);
		BFont bFont88 = font;
		int[] obj88 = new int[13]
		{
			0, 3, 8, 0, 0, 4, 6, 13, 8, -4,
			-6, 0, 0
		};
		obj88[0] = num;
		bFont88.Add16('~', obj88);
	}

	private void FindBaseAxes(Vector3 normal, out Vector3 base1, out Vector3 base2)
	{
		Vector3 vector = new Vector3(normal.y, 0f - normal.x, 0f);
		Vector3 vector2 = new Vector3(0f - normal.z, 0f, normal.x);
		base1 = (vector + vector2).normalized;
		base2 = Vector3.Cross(normal.normalized, base1);
	}

	private void FixRange(ref float minValue, ref float maxValue)
	{
		if (minValue > maxValue)
		{
			float num = minValue;
			minValue = maxValue;
			maxValue = num;
		}
	}

	private void FixRange(ref Vector2 minValue, ref Vector2 maxValue)
	{
		if (minValue.x > maxValue.x)
		{
			float x = minValue.x;
			minValue.x = maxValue.x;
			maxValue.x = x;
		}
		if (minValue.y > maxValue.y)
		{
			float y = minValue.y;
			minValue.y = maxValue.y;
			maxValue.y = y;
		}
	}

	private void FindRules(ref Vector2 minValue, ref Vector2 maxValue, ref Vector2 ruleStep, out Vector2 firstRule, out int ruleCountX, out int ruleCountY)
	{
		FixRange(ref minValue, ref maxValue);
		float x = minValue.x;
		float x2 = maxValue.x;
		float ruleStep2 = ruleStep.x;
		firstRule = Vector2.zero;
		int num;
		int num2;
		if (ruleStep2 < 0f || x >= x2)
		{
			ruleCountX = 0;
		}
		else
		{
			if (ruleStep2 == 0f)
			{
				FindRuleStep(x2 - x, ref ruleStep2);
				ruleStep.x = ruleStep2;
			}
			num = Mathf.CeilToInt(x / ruleStep2);
			num2 = Mathf.FloorToInt(x2 / ruleStep2);
			if (num2 < 0)
			{
				num2--;
			}
			firstRule.x = (float)num * ruleStep2;
			ruleCountX = num2 - num + 1;
		}
		x = minValue.y;
		x2 = maxValue.y;
		ruleStep2 = ruleStep.y;
		if (ruleStep2 < 0f || x >= x2)
		{
			ruleCountY = 0;
			return;
		}
		if (ruleStep2 == 0f)
		{
			FindRuleStep(x2 - x, ref ruleStep2);
			ruleStep.y = ruleStep2;
		}
		num = Mathf.CeilToInt(x / ruleStep2);
		num2 = Mathf.FloorToInt(x2 / ruleStep2);
		if (num2 < 0)
		{
			num2--;
		}
		firstRule.y = (float)num * ruleStep2;
		ruleCountY = num2 - num + 1;
	}

	private void FindRules(ref float minValue, ref float maxValue, ref float ruleStep, out float firstRule, out int ruleCount)
	{
		FixRange(ref minValue, ref maxValue);
		firstRule = 0f;
		ruleCount = 0;
		if (!(ruleStep < 0f) && !(minValue >= maxValue))
		{
			if (ruleStep == 0f)
			{
				FindRuleStep(maxValue - minValue, ref ruleStep);
			}
			int num = Mathf.CeilToInt(minValue / ruleStep);
			int num2 = Mathf.FloorToInt(maxValue / ruleStep);
			if (num2 < 0)
			{
				num2--;
			}
			firstRule = (float)num * ruleStep;
			ruleCount = num2 - num + 1;
		}
	}

	private void FindRuleStep(float range, ref float ruleStep)
	{
		ruleStep = range / 10f;
		float num = 1f;
		while (ruleStep > 10f)
		{
			num *= 10f;
			ruleStep /= 10f;
		}
		while (ruleStep < 1f)
		{
			num /= 10f;
			ruleStep *= 10f;
		}
		if (ruleStep > 5f)
		{
			ruleStep = 10f * num;
		}
		else if (ruleStep > 2.5f)
		{
			ruleStep = 5f * num;
		}
		else if (ruleStep > 1.5f)
		{
			ruleStep = 2f * num;
		}
		else
		{
			ruleStep = num;
		}
	}

	private static Vector3 CubicBezierInterpolate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		Vector3 vector = Vector3.Lerp(p0, p1, t);
		Vector3 vector2 = Vector3.Lerp(p1, p2, t);
		Vector3 to = Vector3.Lerp(p2, p3, t);
		Vector3 vector3 = Vector3.Lerp(vector, vector2, t);
		Vector3 to2 = Vector3.Lerp(vector2, to, t);
		return Vector3.Lerp(vector3, to2, t);
	}

	private static Vector3 BSPInterpolate(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
	{
		bspCoeffs[0] = (-p1 + 3f * p2 - 3f * p3 + p4) * (1f / 6f);
		bspCoeffs[1] = (3f * p1 - 6f * p2 + 3f * p3) * (1f / 6f);
		bspCoeffs[2] = (-3f * p1 + 3f * p3) * (1f / 6f);
		bspCoeffs[3] = (p1 + 4f * p2 + p3) * (1f / 6f);
		return (bspCoeffs[2] + t * (bspCoeffs[1] + t * bspCoeffs[0])) * t + bspCoeffs[3];
	}

	private static Vector3 CatmullRomInterpolate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t);
	}
}
