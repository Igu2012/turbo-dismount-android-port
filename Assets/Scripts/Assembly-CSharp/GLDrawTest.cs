#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

public class GLDrawTest : MonoBehaviour
{
	private delegate void PageMethod();

	public Material customMaterial;

	private GameObject testObjects;

	private MeshFilter[] meshFilters;

	private MeshRenderer[] meshRenderers;

	private Collider[] colliders;

	private int page;

	private List<PageMethod> pages = new List<PageMethod>();

	private List<string> titles = new List<string>();

	private List<Vector2> data;

	private float time;

	private float width = 1024f;

	private float height = 1024f;

	private float size = 1024f;

	private float unit = 64f;

	private float pageChangeTime;

	private void Awake()
	{
		testObjects = base.transform.Find("TestObjects").gameObject;
		meshFilters = testObjects.GetComponentsInChildren<MeshFilter>();
		meshRenderers = testObjects.GetComponentsInChildren<MeshRenderer>();
		colliders = testObjects.GetComponentsInChildren<Collider>();
		testObjects.SetActive(false);
		data = new List<Vector2>();
		for (int i = 0; i < 100; i++)
		{
			data.Add(Vector2.zero);
		}
		pages.Add(TestLines);
		titles.Add("Lines and curves");
		pages.Add(Test2dPrimitives);
		titles.Add("2D shapes");
		pages.Add(Test3dPrimitives);
		titles.Add("3D shapes");
		pages.Add(TestMetersGraphs);
		titles.Add("Meters and graphs");
		pages.Add(TestMeshes);
		titles.Add("Mesh visualization");
		pages.Add(TestBounds);
		titles.Add("AABB visualization");
		pages.Add(TestColliders);
		titles.Add("Collider visualization");
		pages.Add(TestStrings);
		titles.Add("Text rendering");
		pages.Add(TestCustomMaterial);
		titles.Add("Rendering with a custom material");
	}

	private void OnRenderObject()
	{
		if ((Camera.current.cullingMask & (1 << base.gameObject.layer)) != 0 && Application.isPlaying)
		{
			pages[page]();
			if (time < pageChangeTime + 3f)
			{
				ShowTitle();
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			GLDraw.Begin();
			GL.LoadPixelMatrix();
			float num = 20f;
			float num2 = num;
			float num3 = num;
			GLDraw.RoundRect(new Rect(num2, num3 - 10f, 50f * num + 10f, 9f * num + 10f), num + 10f, ARGB(4279717703u), true);
			GLDraw.RoundRect(new Rect(num2, num3, 50f * num, 9f * num), num, ARGB(4280250207u), true);
			num2 += num;
			num3 += num;
			GLDraw.String(new Vector3(num2 + 0.5f, num3 - 0.5f), Vector3.right, Vector3.up, num, 10, false, 1, ARGB(1862270976u), "GLDraw is a static, stateless API for simple drawing from script.\nIt's meant for debugging, gizmos, and to quickly visualize data.\nGLDraw issues drawing commands with Unity's GL API.\nThe example scene displays various drawing functions.\nPress left / right or click on screen edges to change 'pages'.");
			GLDraw.String(new Vector3(num2, num3), Vector3.right, Vector3.up, num, 4, false, 1, ARGB(4291809231u), "GLDraw is a static, stateless API for simple drawing from script.\nIt's meant for debugging, gizmos, and to quickly visualize data.\nGLDraw issues drawing commands with Unity's GL API.\nThe example scene displays various drawing functions.\nPress left / right or click on screen edges to change 'pages'.");
			GLDraw.End();
		}
	}

	private void ShowTitle()
	{
		GLDraw.Begin();
		GLDraw.ScreenViewport();
		GL.LoadPixelMatrix();
		GLDraw.String(20f, height - 20f, height / 40f, 4, false, 7, ARGB(3489660927u), titles[page]);
		GLDraw.End();
	}

	private void Update()
	{
		time = Time.time;
		width = Screen.width;
		height = Screen.height;
		size = Mathf.Min(width, height);
		unit = size / 16f;
		float num = Mathf.Sin(time * (float)Math.PI);
		float num2 = Mathf.Sin(time * (float)Math.PI * 0.7f);
		data.RemoveAt(0);
		data.Add(Vec2(time, num * num2));
		base.transform.Rotate(Time.deltaTime * 5f, Time.deltaTime * 17f, Time.deltaTime * 7f, Space.Self);
		Vector3 position = new Vector3(Mathf.Sin(time * 0.05f * (float)Math.PI), 0f, 0f);
		base.transform.position = position;
		int num3 = page;
		if (Input.GetMouseButtonDown(0))
		{
			if (Input.mousePosition.x < (float)Screen.width * 0.5f)
			{
				page--;
			}
			else
			{
				page++;
			}
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			page--;
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			page++;
		}
		if (page < 0)
		{
			page = pages.Count - 1;
		}
		else if (page >= pages.Count)
		{
			page = 0;
		}
		if (page >= 4 && page <= 6)
		{
			testObjects.SetActive(true);
		}
		else
		{
			testObjects.SetActive(false);
		}
		if (page != num3)
		{
			pageChangeTime = time;
		}
		num3 = page;
	}

	private float PSin(float t)
	{
		return 0.5f + 0.5f * Mathf.Sin(t);
	}

	private float PCos(float t)
	{
		return 0.5f + 0.5f * Mathf.Cos(t);
	}

	private Color RGB(int hex, float a = 1f)
	{
		float num = 1f / 255f;
		return new Color((float)((hex >> 16) & 0xFF) * num, (float)((hex >> 8) & 0xFF) * num, (float)(hex & 0xFF) * num, a);
	}

	private Color ARGB(uint hex)
	{
		float num = 1f / 255f;
		return new Color((float)((hex >> 16) & 0xFF) * num, (float)((hex >> 8) & 0xFF) * num, (float)(hex & 0xFF) * num, (float)((hex >> 24) & 0xFF) * num);
	}

	private Vector3 VVec3(float x, float y)
	{
		return new Vector3(x * width, y * height);
	}

	private Vector3 SVec3(float x, float y)
	{
		return new Vector3(x, y) * size;
	}

	private Vector3 Vec3(float x, float y, float z)
	{
		return new Vector3(x, y, z);
	}

	private Vector3 Vec3(float x, float y)
	{
		return new Vector3(x, y);
	}

	private Vector2 Vec2(float x, float y)
	{
		return new Vector2(x, y);
	}

	private void TestLines()
	{
		Vector3[] array = new Vector3[16];
		for (int i = 0; i < array.Length; i++)
		{
			float f = time + (float)i / (float)array.Length * 5.7f;
			array[i] = new Vector3(4f * unit + (float)i * unit + Mathf.Cos(f) * unit, 2f * unit + PSin(i) * unit * 4f + Mathf.Sin(f) * unit, 0f);
		}
		GLDraw.Begin();
		GLDraw.ScreenViewport();
		GL.LoadPixelMatrix();
		GLDraw.Dots(array, 5f, ARGB(1073708863u));
		GLDraw.ContinuousLine(array, ARGB(4294934335u));
		for (int j = 0; j < array.Length; j++)
		{
			array[j].y += 2f * unit;
		}
		GLDraw.Dots(array, 5f, ARGB(1061126143u));
		GLDraw.CatmullRomSpline(array, ARGB(4282351615u));
		for (int k = 0; k < array.Length; k++)
		{
			array[k].y += 2f * unit;
		}
		GLDraw.Dots(array, 5f, ARGB(1073729280u));
		GLDraw.BSpline(array, ARGB(4294954752u));
		Vector3 vector = VVec3(0.5f + Mathf.Cos(time) * 0.1f, 0.75f + Mathf.Sin(time * 1.1f) * 0.1f);
		Vector3 vector2 = VVec3(0.5f + Mathf.Cos(time * 1.2f) * 0.1f, 0.75f + Mathf.Sin(time * 1.3f) * 0.1f);
		float num = (vector2 - vector).x * 0.5f;
		GLDraw.Circle(vector, 5f, ARGB(1059061567u), true);
		GLDraw.Circle(vector2, 5f, ARGB(1059061567u), true);
		GLDraw.BezierCurve(vector, Vec3(vector.x + num, vector.y), Vec3(vector.x + num, vector2.y), vector2, ARGB(4280287039u));
		GLDraw.End();
		GLDraw.Begin();
		for (int l = 0; l < 64; l++)
		{
			float num2 = (float)l / 64f * (float)Math.PI * 2f;
			uint num3 = (uint)(PCos(num2) * 255f);
			uint num4 = (uint)(PSin(num2) * 255f);
			uint num5 = 128 + (uint)(Mathf.Sin(num2 * 4f) * 127f);
			GLDraw.Line(base.transform.position, base.transform.position + Mathf.Cos(num2) * base.transform.right + Mathf.Sin(num2) * base.transform.up + Mathf.Sin(num2 * 4f) * base.transform.forward * 0.25f, ARGB(0xFF000000u | (num3 << 16) | (num4 << 8) | num5));
		}
		GLDraw.End();
	}

	private void Test2dPrimitives()
	{
		Vector3 vector = new Vector3(Mathf.Cos(time * 0.3f), Mathf.Sin(time * 0.3f), 0f);
		Vector3 vector2 = new Vector3(0f - vector.y, vector.x, 0f);
		GLDraw.Begin();
		GLDraw.ScreenViewport();
		GL.LoadPixelMatrix();
		GLDraw.Quad(VVec3(0.1f, 0.6f), VVec3(0.1f, 0.9f), VVec3(0.4f, 0.9f), VVec3(0.4f, 0.6f), ARGB(2134867967u), true);
		GLDraw.Quad(VVec3(0.1f, 0.6f), VVec3(0.1f, 0.9f), VVec3(0.4f, 0.9f), VVec3(0.4f, 0.6f), ARGB(4282351615u), false);
		GLDraw.Triangle(VVec3(0.6f, 0.6f), VVec3(0.75f, 0.9f), VVec3(0.9f, 0.6f), ARGB(2134867967u), true);
		GLDraw.Triangle(VVec3(0.6f, 0.6f), VVec3(0.75f, 0.9f), VVec3(0.9f, 0.6f), ARGB(4282351615u), false);
		GLDraw.Ellipse(VVec3(0.25f, 0.25f), vector * 0.2f * size, vector2 * 0.25f * size, ARGB(2134867967u), true);
		GLDraw.Ellipse(VVec3(0.25f, 0.25f), vector * 0.2f * size, vector2 * 0.25f * size, ARGB(4282351615u), false);
		GLDraw.Sector(VVec3(0.75f, 0.25f), vector * size * 0.2f, vector2 * size * 0.2f, 240f, -60f, ARGB(2134867967u), true);
		GLDraw.Sector(VVec3(0.75f, 0.25f), vector * size * 0.2f, vector2 * size * 0.2f, 240f, -60f, ARGB(4282351615u), false);
		GLDraw.End();
		GLDraw.Begin();
		GL.MultMatrix(base.transform.localToWorldMatrix);
		GLDraw.RoundRect(Vector3.left + Vector3.down, 2f * Vector3.right, 2f * Vector3.up, 0.1f, ARGB(2147450687u), true);
		GLDraw.RoundRect(Vector3.right + Vector3.down, 2f * Vector3.left, 2f * Vector3.up, 0.1f, ARGB(2134900607u), true);
		GLDraw.RoundRect(Vector3.left + Vector3.down, 2f * Vector3.right, 2f * Vector3.up, 0.1f, ARGB(4288659295u), false);
		GLDraw.End();
	}

	private void Test3dPrimitives()
	{
		float num = PSin(time * 2f);
		float num2 = PCos(time * 2f);
		GLDraw.Begin();
		GLDraw.Arrow(base.transform.position, base.transform.position + base.transform.right, 0.025f, 0.1f, ARGB(4294917919u));
		GLDraw.Arrow(base.transform.position, base.transform.position + base.transform.up, 0.025f, 0.1f, ARGB(4280287039u));
		GLDraw.Arrow(base.transform.position, base.transform.position + base.transform.forward, 0.025f, 0.1f, ARGB(4280238079u));
		GL.MultMatrix(base.transform.localToWorldMatrix);
		GLDraw.Box(Vector3.left * 2f, Vec3(0.5f + num2 * 0.2f, 0.5f + num * 0.2f, 0.5f), ARGB(2147450687u), true);
		GLDraw.Box(Vector3.left * 2f, Vec3(0.5f + num2 * 0.2f, 0.5f + num * 0.2f, 0.5f), ARGB(4294934335u), false);
		GLDraw.Ellipsoid(Vector3.left, Vec3(0.5f + num2 * 0.2f, 0.5f + num * 0.2f, 0.5f), ARGB(2147450687u), true);
		GLDraw.Ellipsoid(Vector3.left, Vec3(0.5f + num2 * 0.2f, 0.5f + num * 0.2f, 0.5f), ARGB(4294934335u), false);
		GLDraw.Capsule(Vector3.zero, Vector3.up, 1f + num * 0.2f, 0.25f + num2 * 0.1f, ARGB(2147450687u), true);
		GLDraw.Capsule(Vector3.zero, Vector3.up, 1f + num * 0.2f, 0.25f + num2 * 0.1f, ARGB(4294934335u), false);
		GLDraw.Cylinder(Vector3.right, Vector3.up, 1f + num * 0.2f, 0.25f + num2 * 0.1f, ARGB(2147450687u), true);
		GLDraw.Cylinder(Vector3.right, Vector3.up, 1f + num * 0.2f, 0.25f + num2 * 0.1f, ARGB(4294934335u), false);
		GLDraw.Cone(Vector3.right * 2f, Vector3.up * (0.5f + num * 0.1f), 0.25f + num2 * 0.1f, ARGB(2147450687u), true);
		GLDraw.Cone(Vector3.right * 2f, Vector3.up * (0.5f + num * 0.1f), 0.25f + num2 * 0.1f, ARGB(4294934335u), false);
		Vector2[] shape = new Vector2[9]
		{
			Vec2(-1f, 0f),
			Vec2(-0.75f, 0.2f),
			Vec2(-0.5f, 0.1f),
			Vec2(-0.25f, 0.3f),
			Vec2(0f, 0.2f),
			Vec2(0.25f, 0.3f),
			Vec2(0.5f, 0.1f),
			Vec2(0.75f, 0.2f),
			Vec2(1f, 0f)
		};
		GLDraw.LatheObject(Vector3.down * 1f, Vector3.left * (1f + num2 * 0.2f), ARGB(2147450687u), true, shape);
		GLDraw.LatheObject(Vector3.down * 1f, Vector3.left * (1f + num2 * 0.2f), ARGB(4294934335u), false, shape);
		GLDraw.End();
	}

	private float CurveFunc(float x)
	{
		return Mathf.Sin(x * 0.5f) * Mathf.Sin(x) * Mathf.Sin(2f * x) * Mathf.Sin(3f * x) + Mathf.Sin(0.6f * x);
	}

	private void TestMetersGraphs()
	{
		float num = Mathf.Sin(time * (float)Math.PI);
		float num2 = Mathf.Sin(time * (float)Math.PI * 0.7f);
		float fillAmount = 0.5f + 0.5f * num;
		float num3 = 0.5f + 0.5f * num * num2;
		GLDraw.Begin();
		GLDraw.ScreenViewport();
		GL.LoadPixelMatrix();
		float value = num3 * 9999f;
		GLDraw.RadialMeter(VVec3(0.15f, 0.75f), Vector3.right * unit * 3f, Vector3.up * unit * 3f, 0f, 10000f, 1000f, 0.001f, ARGB(4280229663u), ARGB(1610612735u), ARGB(4294917919u), value);
		GLDraw.String(VVec3(0.15f, 0.75f) - Vector3.up * unit * 1.5f, Vector3.right, Vector3.up, unit * 0.4f, 4, true, 5, ARGB(3221225471u), value.ToString("0000"));
		GLDraw.Pie(VVec3(0.85f, 0.75f), Vector3.right * unit * 3f, Vector3.up * unit * 3f, ARGB(4280229663u), ARGB(4294917919u), fillAmount);
		GLDraw.VerticalMeter(VVec3(0.1f, 0.1f) - Vector3.right * unit, Vector3.right * unit, Vector3.up * unit * 4f, -1f, 1f, 0.5f, 1f, false, ARGB(4280229663u), ARGB(1610612735u), ARGB(4294917919u), num);
		GLDraw.VerticalMeter(VVec3(0.1f, 0.1f) + Vector3.right * unit, Vector3.right * unit, Vector3.up * unit * 4f, -1f, 1f, 0.5f, 1f, true, ARGB(4280229663u), ARGB(1610612735u), ARGB(4294917919u), num);
		GLDraw.HorizontalMeter(VVec3(0.1f, 0.1f) + Vector3.right * unit * 3f + Vector3.up * 2f * unit, Vector3.right * unit * 4f, Vector3.up * unit, -1f, 1f, 0.5f, 1f, false, ARGB(4280229663u), ARGB(1610612735u), ARGB(4294917919u), num);
		GLDraw.HorizontalMeter(VVec3(0.1f, 0.1f) + Vector3.right * unit * 3f, Vector3.right * unit * 4f, Vector3.up * unit, -1f, 1f, 0.5f, 1f, true, ARGB(4280229663u), ARGB(1610612735u), ARGB(4294917919u), num);
		Vector2 minLimits = new Vector2(data[0].x, -1f);
		Vector2 maxLimits = new Vector2(data[data.Count - 1].x, 1f);
		if (data != null)
		{
			GLDraw.Graph(VVec3(0.6f, 0.1f), Vector3.right * unit * 8f, Vector3.up * unit * 4f, minLimits, maxLimits, new Vector2(0.5f, 0.25f), Vector2.one, ARGB(4280229663u), ARGB(1610612735u), GLDraw.GraphType.Area, ARGB(4294917919u), data.ToArray());
		}
		minLimits = new Vector2(Time.time, -1.6f);
		maxLimits = new Vector2(Time.time + (float)Math.PI * 2f, 1.6f);
		GLDraw.GraphLayout(VVec3(0.5f, 0.8f) - Vector3.right * unit * 3f - Vector3.up * unit * 2f, Vector3.right * unit * 6f, Vector3.up * unit * 4f, minLimits, maxLimits, Vector2.zero, Vector2.one, ARGB(0u), ARGB(1610612735u));
		GLDraw.GraphValues(VVec3(0.5f, 0.8f) - Vector3.right * unit * 3f - Vector3.up * unit * 2f, Vector3.right * unit * 6f, Vector3.up * unit * 4f, minLimits, maxLimits, GLDraw.GraphType.Line, ARGB(4282351615u), CurveFunc);
		GLDraw.GraphValues(VVec3(0.5f, 0.8f) - Vector3.right * unit * 3f - Vector3.up * unit * 2f, Vector3.right * unit * 6f, Vector3.up * unit * 4f, minLimits, maxLimits, GLDraw.GraphType.Dots, ARGB(4282351615u), CurveFunc, 10);
		GLDraw.End();
		GLDraw.Begin();
		GL.MultMatrix(base.transform.localToWorldMatrix);
		if (data != null)
		{
			GLDraw.Graph(Vector3.left - Vector3.up * 0.5f, Vector3.right * 2f, Vector3.up, Vector2.zero, Vector2.one, ARGB(0u), ARGB(1610612735u), GLDraw.GraphType.Line, ARGB(4282351615u), data.ToArray());
		}
		GLDraw.End();
	}

	private void TestMeshes()
	{
		MeshFilter[] array = meshFilters;
		foreach (MeshFilter meshFilter in array)
		{
			GLDraw.Begin();
			GL.MultMatrix(meshFilter.transform.localToWorldMatrix);
			GLDraw.Mesh(meshFilter.mesh, ARGB(2134867967u), false);
			GLDraw.End();
		}
	}

	private void TestBounds()
	{
		GLDraw.Begin();
		MeshRenderer[] array = meshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			GLDraw.Box(meshRenderer.bounds.center, meshRenderer.bounds.size, ARGB(1062182911u), true);
			GLDraw.Box(meshRenderer.bounds.center, meshRenderer.bounds.size, ARGB(2135924735u), false);
		}
		GLDraw.End();
	}

	private void TestColliders()
	{
		Collider[] array = colliders;
		foreach (Collider collider in array)
		{
			GLDraw.Begin();
			GL.MultMatrix(collider.transform.localToWorldMatrix);
			GLDraw.Collider(collider);
			GLDraw.End();
		}
	}

	private void TestStrings()
	{
		GLDraw.Begin();
		GLDraw.ScreenViewport();
		GL.LoadPixelMatrix();
		GLDraw.String(new Vector3(width - 10f, height - 10f), Vector3.right, Vector3.up, unit * 0.5f, 4, true, 9, ARGB(4282351615u), "Monospace\nABCDEFGHIJ\nabcdefghij\n0123456789");
		GLDraw.String(Vec3(width - 10f, 10f), Vector3.right, Vector3.up, unit * 0.5f, 4, false, 3, ARGB(4282351615u), "Proportional\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nabcdefghijklmnopqrstuvwxyz\n0123456789!\"#$%&'()*+,-./\n:;<=>?@[\\]^_`{|}~");
		GLDraw.String(Vec3(unit, unit), Vector3.right, Vector3.up, unit * 4f, 8, false, 1, ARGB(1061126143u), "BIG");
		GLDraw.String(Vec3(unit, unit), Vector3.right, Vector3.up, unit * 4f, 4, false, 1, ARGB(1061126143u), "BIG");
		GLDraw.String(Vec3(unit, unit), Vector3.right, Vector3.up, unit * 4f, 0, false, 1, ARGB(4282351615u), "BIG");
		int align = Mathf.FloorToInt(Mathf.Repeat(Time.time * 0.5f, 9f) + 1f);
		GLDraw.String(VVec3(0.25f, 0.75f), Vector3.right, Vector3.up, unit * 0.5f, 4, false, align, Color.white, "The quick\nbrown fox\njumps over\nthe lazy dog");
		GLDraw.Circle(VVec3(0.25f, 0.75f), 10f, RGB(16727839), true);
		GLDraw.String(VVec3(0.25f, 0.75f), Vector3.right, Vector3.up, 10f, 6, false, 5, Color.black, align.ToString());
		GLDraw.End();
		GLDraw.Begin();
		GL.MultMatrix(base.transform.localToWorldMatrix);
		Color[] array = new Color[5]
		{
			RGB(12458808),
			RGB(15230532),
			RGB(13225854),
			RGB(6516830),
			RGB(108943)
		};
		int num = (int)(Time.time * 2f) % array.Length;
		GLDraw.String(Vector3.zero, Vector3.right, Vector3.up, 0.6f, 4, false, 5, array[num++ % array.Length], "DISCO");
		GLDraw.StencilDec();
		GLDraw.String(Vector3.zero, Vector3.right, Vector3.up, 0.6f, 7, false, 5, array[num++ % array.Length], "DISCO");
		GLDraw.StencilDec();
		GLDraw.String(Vector3.zero, Vector3.right, Vector3.up, 0.6f, 10, false, 5, array[num++ % array.Length], "DISCO");
		GLDraw.StencilDec();
		GLDraw.String(Vector3.zero, Vector3.right, Vector3.up, 0.6f, 13, false, 5, array[num++ % array.Length], "DISCO");
		GLDraw.StencilDec();
		GLDraw.String(Vector3.zero, Vector3.right, Vector3.up, 0.6f, 16, false, 5, array[num++ % array.Length], "DISCO");
		GLDraw.String(Vector3.zero, Vector3.left, Vector3.up, 0.6f, 4, false, 5, array[4], "DISCO");
		GLDraw.End();
	}

	private void TestCustomMaterial()
	{
		Color[] array = new Color[4]
		{
			RGB(12458808),
			RGB(15230532),
			RGB(13225854),
			RGB(108943)
		};
		for (int i = 0; i < meshFilters.Length; i++)
		{
			MeshFilter meshFilter = meshFilters[i];
			Color color = array[i % array.Length];
			GLDraw.Begin(customMaterial, 0);
			GL.MultMatrix(meshFilter.transform.localToWorldMatrix);
			GLDraw.Mesh(meshFilter.mesh, color, true);
			GLDraw.End();
		}
	}
}
