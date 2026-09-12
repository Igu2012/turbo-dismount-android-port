#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

public class GizmoUtils
{
	private static Dictionary<char, List<Vector3>> lineFont;

	public static void DrawEllipse(Vector3 position, Vector3 right, Vector3 up, float radius, int divs = 36, bool clip = false)
	{
		float num = (float)Math.PI * 2f / (float)divs;
		if (radius <= 0f)
		{
			return;
		}
		float num2 = 0f;
		int num3 = 0;
		while (num3 < divs)
		{
			Vector3 vector = position + radius * (Mathf.Cos(num2) * right + Mathf.Sin(num2) * up);
			Vector3 vector2 = position + radius * (Mathf.Cos(num2 + num) * right + Mathf.Sin(num2 + num) * up);
			if (clip)
			{
				DrawClipLine(vector, vector2);
			}
			else
			{
				Gizmos.DrawLine(vector, vector2);
			}
			num3++;
			num2 += num;
		}
	}

	public static void DrawClipLine(Vector3 pos1, Vector3 pos2)
	{
		if ((pos1.x > 1f && pos2.x > 1f) || (pos1.x < 0f && pos2.x < 0f) || (pos1.y > 1f && pos2.y > 1f) || (pos1.y < 0f && pos2.y < 0f))
		{
			return;
		}
		bool flag = pos1.x >= 0f && pos1.x <= 1f && pos1.y >= 0f && pos1.y <= 1f;
		bool flag2 = pos2.x >= 0f && pos2.x <= 1f && pos2.y >= 0f && pos2.y <= 1f;
		if (flag && flag2)
		{
			Gizmos.DrawLine(pos1, pos2);
			return;
		}
		Vector3 vector = pos2 - pos1;
		float magnitude = vector.magnitude;
		float num = magnitude * magnitude;
		Vector3 vector2 = vector / magnitude;
		Vector3[] array = new Vector3[2];
		int num2 = 0;
		Vector3 vector3 = new Vector3(Mathf.Repeat(pos1.x, 1f), Mathf.Repeat(pos1.y, 1f), 0f);
		Vector3 zero = Vector3.zero;
		if (vector2.y > float.Epsilon || vector2.y < -float.Epsilon)
		{
			Vector3 vector4 = vector2 * (1f / Mathf.Abs(vector2.y));
			for (zero = ((!(vector2.y > 0f)) ? (pos1 + vector3.y * vector4) : (pos1 + (1f - vector3.y) * vector4)); (zero - pos1).sqrMagnitude < num; zero += vector4)
			{
				if (num2 >= 2)
				{
					break;
				}
				array[num2++] = zero;
			}
		}
		if (vector2.x > float.Epsilon || vector2.x < -float.Epsilon)
		{
			Vector3 vector5 = vector2 * (1f / Mathf.Abs(vector2.x));
			for (zero = ((!(vector2.x > 0f)) ? (pos1 + vector3.x * vector5) : (pos1 + (1f - vector3.x) * vector5)); (zero - pos1).sqrMagnitude < num; zero += vector5)
			{
				if (num2 >= 2)
				{
					break;
				}
				array[num2++] = zero;
			}
		}
		switch (num2)
		{
		case 1:
			if (flag)
			{
				Gizmos.DrawLine(pos1, array[0]);
			}
			else
			{
				Gizmos.DrawLine(pos2, array[0]);
			}
			break;
		case 2:
			Gizmos.DrawLine(array[0], array[1]);
			break;
		}
	}

	public static void DrawLineString(string s, Vector3 pos, Vector3 right, Vector3 up, Vector3 move, int align = 1)
	{
		char[] array = s.ToCharArray();
		Bounds bounds = default(Bounds);
		bounds.Encapsulate(right);
		bounds.Encapsulate(up);
		bounds.Encapsulate((array.Length - 1) * move);
		bounds.Encapsulate((array.Length - 1) * move + right);
		bounds.Encapsulate((array.Length - 1) * move + up);
		Vector3 center = bounds.center;
		Vector3 vector = pos;
		Vector3 size = bounds.size;
		align = Mathf.Clamp(align, 1, 9);
		switch (align)
		{
		case 1:
		case 4:
		case 7:
			vector.x += size.x * 0.5f;
			break;
		case 3:
		case 6:
		case 9:
			vector.x -= size.x * 0.5f;
			break;
		}
		switch (align)
		{
		case 1:
		case 2:
		case 3:
			vector.y += size.y * 0.5f;
			break;
		case 7:
		case 8:
		case 9:
			vector.y -= size.y * 0.5f;
			break;
		}
		Vector3 pos2 = vector - center;
		for (int i = 0; i < array.Length; i++)
		{
			DrawLineChar(array[i], pos2, right, up);
			pos2 += move;
		}
	}

	public static void DrawLineChar(char c, Vector3 pos, Vector3 right, Vector3 up)
	{
		if (lineFont == null)
		{
			InitFont();
		}
		if (!lineFont.ContainsKey(c) || c == ' ')
		{
			return;
		}
		List<Vector3> list = lineFont[c];
		for (int i = 0; i < list.Count - 1; i++)
		{
			Vector3 vector = list[i];
			Vector3 vector2 = list[i + 1];
			if (!(vector.z < 0f) && !(vector2.z < 0f))
			{
				vector = pos + right * vector.x + up * vector.y;
				vector2 = pos + right * vector2.x + up * vector2.y;
				Gizmos.DrawLine(vector, vector2);
			}
		}
	}

	private static void InitFont()
	{
		lineFont = new Dictionary<char, List<Vector3>>();
		List<Vector3> list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0.5f, 0f));
		lineFont.Add('A', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 0.5f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		lineFont.Add('B', list);
		list = new List<Vector3>();
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		lineFont.Add('C', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0f, 0f, 0f));
		lineFont.Add('D', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 0.5f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		lineFont.Add('E', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 0.5f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		lineFont.Add('F', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0.5f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		lineFont.Add('G', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(1f, 0f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0.5f, 0f));
		lineFont.Add('H', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0.5f, 0f, 0f));
		list.Add(new Vector3(0.5f, 1f, 0f));
		lineFont.Add('I', list);
		list = new List<Vector3>();
		list.Add(new Vector3(1f, 1f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		lineFont.Add('J', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0.33333f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		lineFont.Add('K', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		lineFont.Add('L', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0.5f, 0.5f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		lineFont.Add('M', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		lineFont.Add('N', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		lineFont.Add('O', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(0f, 0.5f, 0f));
		lineFont.Add('P', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0.5f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		lineFont.Add('Q', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(0f, 0.5f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0.5f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		lineFont.Add('R', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(0.25f, 0.5f, 0f));
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		lineFont.Add('S', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0.5f, 0f, 0f));
		list.Add(new Vector3(0.5f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		lineFont.Add('T', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		lineFont.Add('U', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0.5f, 0f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		lineFont.Add('V', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(0.5f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		lineFont.Add('W', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		lineFont.Add('X', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0.5f, 0.5f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0.5f, 0.5f, 0f));
		list.Add(new Vector3(0.5f, 0f, 0f));
		lineFont.Add('Y', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		lineFont.Add('Z', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		lineFont.Add('0', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0.5f, 0f, 0f));
		list.Add(new Vector3(0.5f, 1f, 0f));
		list.Add(new Vector3(0.25f, 0.75f, 0f));
		lineFont.Add('1', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(0.25f, 0.5f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		lineFont.Add('2', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(0.5f, 0.5f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		lineFont.Add('3', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		lineFont.Add('4', list);
		list = new List<Vector3>();
		list.Add(new Vector3(1f, 1f, 0f));
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(0f, 0.5f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		lineFont.Add('5', list);
		list = new List<Vector3>();
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(0.25f, 0.5f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		lineFont.Add('6', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 1f, 0f));
		list.Add(new Vector3(1f, 1f, 0f));
		list.Add(new Vector3(0.5f, 0f, 0f));
		lineFont.Add('7', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0.25f, 0.5f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(0f, 0f, -1f));
		list.Add(new Vector3(0.25f, 0.5f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		lineFont.Add('8', list);
		list = new List<Vector3>();
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(0.75f, 0.5f, 0f));
		list.Add(new Vector3(0.25f, 0.5f, 0f));
		list.Add(new Vector3(0f, 0.75f, 0f));
		list.Add(new Vector3(0.25f, 1f, 0f));
		list.Add(new Vector3(0.75f, 1f, 0f));
		list.Add(new Vector3(1f, 0.75f, 0f));
		list.Add(new Vector3(1f, 0.25f, 0f));
		list.Add(new Vector3(0.75f, 0f, 0f));
		list.Add(new Vector3(0.25f, 0f, 0f));
		list.Add(new Vector3(0f, 0.25f, 0f));
		lineFont.Add('9', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0f, 0f));
		list.Add(new Vector3(1f, 0f, 0f));
		lineFont.Add('_', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0f, 0.5f, 0f));
		list.Add(new Vector3(1f, 0.5f, 0f));
		lineFont.Add('-', list);
		list = new List<Vector3>();
		list.Add(new Vector3(0.375f, 0f, 0f));
		list.Add(new Vector3(0.375f, 0.25f, 0f));
		list.Add(new Vector3(0.625f, 0.25f, 0f));
		list.Add(new Vector3(0.625f, 0f, 0f));
		list.Add(new Vector3(0.375f, 0f, 0f));
		lineFont.Add('.', list);
	}
}
