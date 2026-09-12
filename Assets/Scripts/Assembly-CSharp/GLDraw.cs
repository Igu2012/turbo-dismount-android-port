#pragma warning disable 0618,0619
using UnityEngine;

public class GLDraw
{
	public enum GraphType
	{
		Dots = 0,
		Line = 1,
		Area = 2
	}

	public delegate float CurveFunction(float x);

	private GLDrawImpl mImpl;

	private static GLDrawImpl impl
	{
		get
		{
			return GLDrawImpl.instance;
		}
	}

	public static void Begin(bool useStencil = false)
	{
		impl.Begin(useStencil);
	}

	public static void Begin(Material material, int pass)
	{
		impl.Begin(material, pass);
	}

	public static void End()
	{
		impl.End();
	}

	public static void StencilReset()
	{
		impl.StencilReset();
	}

	public static void StencilInc()
	{
		impl.StencilInc();
	}

	public static void StencilDec()
	{
		impl.StencilDec();
	}

	public static void StencilSet(int value)
	{
		impl.StencilSet(value);
	}

	public static void ScreenViewport()
	{
		impl.ScreenViewport();
	}

	public static void Line(Vector3 v0, Vector3 v1, Color color)
	{
		impl.Line(v0, v1, color);
	}

	public static void Line(Vector3 v0, Vector3 v1, Color c0, Color c1)
	{
		impl.Line(v0, v1, c0, c1);
	}

	public static void Lines(Vector3[] vertices, Color color)
	{
		impl.Lines(vertices, color);
	}

	public static void Lines(Vector3[] vertices, Color[] colors)
	{
		impl.Lines(vertices, colors);
	}

	public static void ContinuousLine(Vector3[] vertices, Color color)
	{
		impl.ContinuousLine(vertices, color);
	}

	public static void ContinuousLine(Vector3[] vertices, Color[] colors)
	{
		impl.ContinuousLine(vertices, colors);
	}

	public static void BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Color color, int divs = 64)
	{
		impl.BezierCurve(p0, p1, p2, p3, color, divs);
	}

	public static void BezierCurves(Vector3[] points, Color color, int divs = 64)
	{
		impl.BezierCurves(points, color, divs);
	}

	public static void BSpline(Vector3[] points, Color color, int divs = 64)
	{
		impl.BSpline(points, color, divs);
	}

	public static void CatmullRomSpline(Vector3[] points, Color color, int divs = 64)
	{
		impl.CatmullRomSpline(points, color, divs);
	}

	public static void Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Color color, bool fill)
	{
		impl.Triangle(v0, v1, v2, color, fill);
	}

	public static void Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Color c0, Color c1, Color c2, bool fill)
	{
		impl.Triangle(v0, v1, v2, c0, c1, c2, fill);
	}

	public static void Triangles(Vector3[] vertices, Color color, bool fill)
	{
		impl.Triangles(vertices, color, fill);
	}

	public static void Triangles(Vector3[] vertices, Color[] colors, bool fill)
	{
		impl.Triangles(vertices, colors, fill);
	}

	public static void Quad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color color, bool fill)
	{
		impl.Quad(v0, v1, v2, v3, color, fill);
	}

	public static void Quad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color c0, Color c1, Color c2, Color c3, bool fill)
	{
		impl.Quad(v0, v1, v2, v3, c0, c1, c2, c3, fill);
	}

	public static void Quad(Vector3[] vertices, Color color, bool fill)
	{
		impl.Quads(vertices, color, fill);
	}

	public static void Quads(Vector3[] vertices, Color[] colors, bool fill)
	{
		impl.Quads(vertices, colors, fill);
	}

	public static void Rect(Rect rect, Color color, bool fill)
	{
		Rect(new Vector3(rect.min.x, rect.min.y), Vector3.right * rect.size.x, Vector3.up * rect.size.y, color, fill);
	}

	public static void Rect(Vector3 position, Vector3 right, Vector3 up, Color color, bool fill)
	{
		impl.Quad(position, position + up, position + right + up, position + right, color, fill);
	}

	public static void RoundRect(Rect rect, float radius, Color color, bool fill)
	{
		RoundRect(new Vector3(rect.min.x, rect.min.y), Vector3.right * rect.size.x, Vector3.up * rect.size.y, radius, color, fill);
	}

	public static void RoundRect(Vector3 position, Vector3 right, Vector3 up, float radius, Color color, bool fill)
	{
		impl.RoundRect(position, right, up, radius, color, fill);
	}

	public static void Ellipse(Vector3 position, Vector3 right, Vector3 up, Color color, bool fill, int divs = 64)
	{
		impl.Ellipse(position, right, up, color, fill, divs);
	}

	public static void EllipseRim(Vector3 position, Vector3 right, Vector3 up, float innerRadius, Color color, int divs = 64)
	{
		impl.EllipseRim(position, right, up, innerRadius, color, divs);
	}

	public static void Circle(Vector3 position, float radius, Color color, bool fill, int divs = 64)
	{
		impl.Ellipse(position, Vector3.right * radius, Vector3.up * radius, color, fill, divs);
	}

	public static void CircleRim(Vector3 position, float radius, float innerRadius, Color color, int divs = 64)
	{
		impl.EllipseRim(position, Vector3.right * radius, Vector3.up * radius, innerRadius, color, divs);
	}

	public static void Sector(Vector3 position, Vector3 right, Vector3 up, float startAngle, float endAngle, Color color, bool fill, int divs = 64)
	{
		impl.Sector(position, right, up, startAngle, endAngle, color, fill, divs);
	}

	public static void Dots(Vector3[] vertices, float size, Color color)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			impl.Ellipse(vertices[i], Vector3.right * size, Vector3.up * size, color, true, 16);
		}
	}

	public static void Dots(Vector3[] vertices, Vector3 right, Vector3 up, float size, Color color)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			impl.Ellipse(vertices[i], right * size, up * size, color, true, 16);
		}
	}

	public static void Cube(Vector3 center, float radius, Color color, bool fill)
	{
		impl.Box(center, Vector3.one * radius, color, fill);
	}

	public static void Box(Vector3 center, Vector3 size, Color color, bool fill)
	{
		impl.Box(center, size, color, fill);
	}

	public static void Sphere(Vector3 center, float radius, Color color, bool fill)
	{
		impl.Ellipsoid(center, Vector3.one * radius, color, fill);
	}

	public static void Ellipsoid(Vector3 center, Vector3 size, Color color, bool fill)
	{
		impl.Ellipsoid(center, size, color, fill);
	}

	public static void Capsule(Vector3 center, Vector3 axis, float height, float radius, Color color, bool fill)
	{
		impl.Capsule(center, axis, height, radius, color, fill);
	}

	public static void LatheObject(Vector3 position, Vector3 axis, Color color, bool fill, params Vector2[] shape)
	{
		impl.LatheObject(position, axis, color, fill, shape);
	}

	public static void Cone(Vector3 position, Vector3 axis, float radius, Color color, bool fill)
	{
		impl.Cone(position, axis, radius, color, fill);
	}

	public static void Cylinder(Vector3 center, Vector3 axis, float height, float radius, Color color, bool fill)
	{
		impl.Cylinder(center, axis, height, radius, color, fill);
	}

	public static void Arrow(Vector3 p0, Vector3 p1, float diameter, float capSize, Color color)
	{
		impl.Arrow(p0, p1, diameter, capSize, color);
	}

	public static void RadialMeter(Vector3 position, Vector3 right, Vector3 up, float minLimit, float maxLimit, float value)
	{
		impl.RadialMeter(position, right, up, 225f, -45f, minLimit, maxLimit, 0f, 1f, Color.black, Color.white, Color.red, value);
	}

	public static void RadialMeter(Vector3 position, Vector3 right, Vector3 up, float minLimit, float maxLimit, float labelMultiplier, float value)
	{
		impl.RadialMeter(position, right, up, 225f, -45f, minLimit, maxLimit, 0f, labelMultiplier, Color.black, Color.white, Color.red, value);
	}

	public static void RadialMeter(Vector3 position, Vector3 right, Vector3 up, float minLimit, float maxLimit, float ruleStep, float labelMultiplier, Color background, Color scale, Color indicator, float value)
	{
		impl.RadialMeter(position, right, up, 225f, -45f, minLimit, maxLimit, ruleStep, labelMultiplier, background, scale, indicator, value);
	}

	public static void RadialMeter(Vector3 position, Vector3 right, Vector3 up, float minAngle, float maxAngle, float minLimit, float maxLimit, float ruleStep, float labelMultiplier, Color background, Color scale, Color indicator, float value)
	{
		impl.RadialMeter(position, right, up, minAngle, maxAngle, minLimit, maxLimit, ruleStep, labelMultiplier, background, scale, indicator, value);
	}

	public static void VerticalMeter(Vector3 position, Vector3 right, Vector3 up, float minLimit, float maxLimit, bool fill, float value)
	{
		impl.VerticalMeter(position, right, up, minLimit, maxLimit, 0f, 1f, fill, Color.black, Color.white, Color.red, value);
	}

	public static void VerticalMeter(Vector3 position, Vector3 right, Vector3 up, float minLimit, float maxLimit, float labelMultiplier, bool fill, float value)
	{
		impl.VerticalMeter(position, right, up, minLimit, maxLimit, 0f, labelMultiplier, fill, Color.black, Color.white, Color.red, value);
	}

	public static void VerticalMeter(Vector3 position, Vector3 right, Vector3 up, float minLimit, float maxLimit, float ruleStep, float labelMultiplier, bool fill, Color background, Color rule, Color indicator, float value)
	{
		impl.VerticalMeter(position, right, up, minLimit, maxLimit, ruleStep, labelMultiplier, fill, background, rule, indicator, value);
	}

	public static void HorizontalMeter(Vector3 position, Vector3 right, Vector3 up, float minLimit, float maxLimit, bool fill, float value)
	{
		impl.HorizontalMeter(position, right, up, minLimit, maxLimit, 0f, 1f, fill, Color.black, Color.white, Color.red, value);
	}

	public static void HorizontalMeter(Vector3 position, Vector3 right, Vector3 up, float minLimit, float maxLimit, float labelMultiplier, bool fill, float value)
	{
		impl.HorizontalMeter(position, right, up, minLimit, maxLimit, 0f, labelMultiplier, fill, Color.black, Color.white, Color.red, value);
	}

	public static void HorizontalMeter(Vector3 position, Vector3 right, Vector3 up, float minLimit, float maxLimit, float ruleStep, float labelMultiplier, bool fill, Color background, Color rule, Color indicator, float value)
	{
		impl.HorizontalMeter(position, right, up, minLimit, maxLimit, ruleStep, labelMultiplier, fill, background, rule, indicator, value);
	}

	public static void Pie(Vector3 position, Vector3 right, Vector3 up, Color background, Color sector, float fillAmount, int divs = 64)
	{
		impl.Pie(position, right, up, background, sector, fillAmount, divs);
	}

	public static void Pie(Vector3 position, Vector3 right, Vector3 up, float fillAmount, int divs = 64)
	{
		impl.Pie(position, right, up, Color.black, Color.red, fillAmount, divs);
	}

	public static void GraphLayout(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits)
	{
		impl.GraphLayout(position, right, up, minLimits, maxLimits, Vector2.zero, Vector2.one, Color.black, Color.white);
	}

	public static void GraphLayout(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, Vector2 labelMultipliers)
	{
		impl.GraphLayout(position, right, up, minLimits, maxLimits, Vector2.zero, labelMultipliers, Color.black, Color.white);
	}

	public static void GraphLayout(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, Vector2 labelMultipliers, Color background, Color rule)
	{
		impl.GraphLayout(position, right, up, minLimits, maxLimits, Vector2.zero, labelMultipliers, background, rule);
	}

	public static void GraphLayout(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, Vector2 ruleSteps, Vector2 labelMultipliers, Color background, Color rule)
	{
		impl.GraphLayout(position, right, up, minLimits, maxLimits, ruleSteps, labelMultipliers, background, rule);
	}

	public static void GraphValues(Vector3 position, Vector3 right, Vector3 up, Vector2[] values)
	{
		Vector2 minValue;
		Vector2 maxValue;
		FindMinMax(values, out minValue, out maxValue);
		impl.GraphValues(position, right, up, minValue, maxValue, GraphType.Line, Color.red, values);
	}

	public static void GraphValues(Vector3 position, Vector3 right, Vector3 up, GraphType type, Color graph, Vector2[] values)
	{
		Vector2 minValue;
		Vector2 maxValue;
		FindMinMax(values, out minValue, out maxValue);
		impl.GraphValues(position, right, up, minValue, maxValue, type, graph, values);
	}

	public static void GraphValues(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, GraphType type, Color graph, Vector2[] values)
	{
		impl.GraphValues(position, right, up, minLimits, maxLimits, type, graph, values);
	}

	public static void Graph(Vector3 position, Vector3 right, Vector3 up, Vector2[] values)
	{
		Vector2 minValue;
		Vector2 maxValue;
		FindMinMax(values, out minValue, out maxValue);
		impl.GraphLayout(position, right, up, minValue, maxValue, Vector2.zero, Vector2.one, Color.black, Color.white);
		impl.GraphValues(position, right, up, minValue, maxValue, GraphType.Line, Color.red, values);
	}

	public static void Graph(Vector3 position, Vector3 right, Vector3 up, Vector2 ruleSteps, Vector2 labelMultipliers, GraphType type, Vector2[] values)
	{
		Vector2 minValue;
		Vector2 maxValue;
		FindMinMax(values, out minValue, out maxValue);
		impl.GraphLayout(position, right, up, minValue, maxValue, ruleSteps, labelMultipliers, Color.black, Color.white);
		impl.GraphValues(position, right, up, minValue, maxValue, type, Color.red, values);
	}

	public static void Graph(Vector3 position, Vector3 right, Vector3 up, Vector2 ruleSteps, Vector2 labelMultipliers, Color background, Color rule, GraphType type, Color graph, Vector2[] values)
	{
		Vector2 minValue;
		Vector2 maxValue;
		FindMinMax(values, out minValue, out maxValue);
		impl.GraphLayout(position, right, up, minValue, maxValue, ruleSteps, labelMultipliers, background, rule);
		impl.GraphValues(position, right, up, minValue, maxValue, type, graph, values);
	}

	public static void Graph(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, Vector2 ruleSteps, Vector2 labelMultipliers, Color background, Color rule, GraphType type, Color graph, Vector2[] values)
	{
		impl.GraphLayout(position, right, up, minLimits, maxLimits, ruleSteps, labelMultipliers, background, rule);
		impl.GraphValues(position, right, up, minLimits, maxLimits, type, graph, values);
	}

	public static void GraphValues(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, CurveFunction function, int samples = 100)
	{
		impl.GraphValues(position, right, up, minLimits, maxLimits, GraphType.Line, Color.red, function, samples);
	}

	public static void GraphValues(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, GraphType type, Color graph, CurveFunction function, int samples = 100)
	{
		impl.GraphValues(position, right, up, minLimits, maxLimits, type, graph, function, samples);
	}

	public static void Graph(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, CurveFunction function, int samples = 100)
	{
		impl.GraphLayout(position, right, up, minLimits, maxLimits, Vector2.zero, Vector2.one, Color.black, Color.white);
		impl.GraphValues(position, right, up, minLimits, maxLimits, GraphType.Line, Color.red, function, samples);
	}

	public static void Graph(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, Vector2 ruleSteps, Vector2 labelMultipliers, GraphType type, CurveFunction function, int samples = 100)
	{
		impl.GraphLayout(position, right, up, minLimits, maxLimits, ruleSteps, labelMultipliers, Color.black, Color.white);
		impl.GraphValues(position, right, up, minLimits, maxLimits, type, Color.red, function, samples);
	}

	public static void Graph(Vector3 position, Vector3 right, Vector3 up, Vector2 minLimits, Vector2 maxLimits, Vector2 ruleSteps, Vector2 labelMultipliers, Color background, Color rule, GraphType type, Color graph, CurveFunction function, int samples = 100)
	{
		impl.GraphLayout(position, right, up, minLimits, maxLimits, ruleSteps, labelMultipliers, background, rule);
		impl.GraphValues(position, right, up, minLimits, maxLimits, type, graph, function, samples);
	}

	public static void Mesh(Vector3[] vertices, int[] indices, Color color, bool fill)
	{
		impl.Mesh(vertices, indices, color, fill);
	}

	public static void Mesh(Vector3[] vertices, int[] indices, Color[] colors, bool fill)
	{
		impl.Mesh(vertices, indices, colors, fill);
	}

	public static void Mesh(Mesh mesh, Color color, bool fill)
	{
		impl.Mesh(mesh.vertices, mesh.triangles, color, fill);
	}

	public static void Mesh(Mesh mesh, bool fill)
	{
		impl.Mesh(mesh, fill);
	}

	public static void Mesh(Mesh mesh)
	{
		impl.Mesh(mesh);
	}

	public static void Collider(BoxCollider collider)
	{
		impl.Collider(collider);
	}

	public static void Collider(SphereCollider collider)
	{
		impl.Collider(collider);
	}

	public static void Collider(CapsuleCollider collider)
	{
		impl.Collider(collider);
	}

	public static void Collider(WheelCollider collider)
	{
		impl.Collider(collider);
	}

	public static void Collider(Collider collider)
	{
		impl.Collider(collider);
	}

	public static void String(float x, float y, float size, string s)
	{
		impl.String(new Vector3(x, y, 0f), Vector3.right, Vector3.up, size, 4, false, 1, Color.white, s);
	}

	public static void String(float x, float y, float size, int strokeWidth, Color color, string s)
	{
		impl.String(new Vector3(x, y, 0f), Vector3.right, Vector3.up, size, strokeWidth, false, 1, color, s);
	}

	public static void String(float x, float y, float size, int strokeWidth, bool monospace, int align, Color color, string s)
	{
		impl.String(new Vector3(x, y, 0f), Vector3.right, Vector3.up, size, strokeWidth, monospace, align, color, s);
	}

	public static void String(Vector3 pos, Vector3 right, Vector3 up, float size, int strokeWidth, bool monospace, int align, Color color, string s)
	{
		impl.String(pos, right, up, size, strokeWidth, monospace, align, color, s);
	}

	public static void Focus(Vector3 position, float diameter, Color dimColor)
	{
		impl.Focus(position, diameter, dimColor);
	}

	private static void FindMinMax(Vector2[] values, out Vector2 minValue, out Vector2 maxValue)
	{
		Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
		for (int i = 0; i < values.Length; i++)
		{
			Vector2 vector3 = values[i];
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
		minValue = vector;
		maxValue = vector2;
	}
}
