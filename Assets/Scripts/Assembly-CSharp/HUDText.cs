#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/HUD Text")]
public class HUDText : MonoBehaviour
{
	protected class Entry
	{
		public float time;

		public float stay;

		public float offset;

		public float val;

		public UILabel label;

		public float movementStart
		{
			get
			{
				return time + stay;
			}
		}
	}

	public UIFont font;

	public UILabel.Effect effect;

	public AnimationCurve offsetCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(3f, 40f));

	public AnimationCurve alphaCurve = new AnimationCurve(new Keyframe(1f, 1f), new Keyframe(3f, 0f));

	public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 1f));

	private List<Entry> mList = new List<Entry>();

	private List<Entry> mUnused = new List<Entry>();

	private int counter;

	public bool isVisible
	{
		get
		{
			return mList.Count != 0;
		}
	}

	private static int Comparison(Entry a, Entry b)
	{
		if (a.movementStart < b.movementStart)
		{
			return -1;
		}
		if (a.movementStart > b.movementStart)
		{
			return 1;
		}
		return 0;
	}

	private Entry Create()
	{
		if (mUnused.Count > 0)
		{
			Entry entry = mUnused[mUnused.Count - 1];
			mUnused.RemoveAt(mUnused.Count - 1);
			entry.time = Time.realtimeSinceStartup;
			entry.label.depth = NGUITools.CalculateNextDepth(base.gameObject);
			NGUITools.SetActive(entry.label.gameObject, true);
			entry.offset = 0f;
			mList.Add(entry);
			return entry;
		}
		Entry entry2 = new Entry();
		entry2.time = Time.realtimeSinceStartup;
		entry2.label = NGUITools.AddWidget<UILabel>(base.gameObject);
		entry2.label.name = counter.ToString();
		entry2.label.effectStyle = effect;
		entry2.label.font = font;
		entry2.label.cachedTransform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		mList.Add(entry2);
		counter++;
		return entry2;
	}

	private void Delete(Entry ent)
	{
		mList.Remove(ent);
		mUnused.Add(ent);
		NGUITools.SetActive(ent.label.gameObject, false);
	}

	public void Add(object obj, Color c, float stayDuration)
	{
		if (!base.enabled)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		bool flag = false;
		float num = 0f;
		if (obj is float)
		{
			flag = true;
			num = (float)obj;
		}
		else if (obj is int)
		{
			flag = true;
			num = (int)obj;
		}
		if (flag)
		{
			if (num == 0f)
			{
				return;
			}
			int num2 = mList.Count;
			while (num2 > 0)
			{
				Entry entry = mList[--num2];
				if (!(entry.time + 1f < realtimeSinceStartup) && entry.val != 0f)
				{
					if (entry.val < 0f && num < 0f)
					{
						entry.val += num;
						entry.label.text = Mathf.RoundToInt(entry.val).ToString();
						return;
					}
					if (entry.val > 0f && num > 0f)
					{
						entry.val += num;
						entry.label.text = "+" + Mathf.RoundToInt(entry.val);
						return;
					}
				}
			}
		}
		Entry entry2 = Create();
		entry2.stay = stayDuration;
		entry2.label.color = c;
		entry2.val = num;
		if (flag)
		{
			entry2.label.text = ((!(num < 0f)) ? ("+" + Mathf.RoundToInt(entry2.val)) : Mathf.RoundToInt(entry2.val).ToString());
		}
		else
		{
			entry2.label.text = obj.ToString();
		}
		mList.Sort(Comparison);
	}

	private void OnDisable()
	{
		int num = mList.Count;
		while (num > 0)
		{
			Entry entry = mList[--num];
			if (entry.label != null)
			{
				entry.label.enabled = false;
			}
			else
			{
				mList.RemoveAt(num);
			}
		}
	}

	private void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		Keyframe[] keys = offsetCurve.keys;
		Keyframe[] keys2 = alphaCurve.keys;
		Keyframe[] keys3 = scaleCurve.keys;
		float time = keys[keys.Length - 1].time;
		float time2 = keys2[keys2.Length - 1].time;
		float time3 = keys3[keys3.Length - 1].time;
		float num = Mathf.Max(time3, Mathf.Max(time, time2));
		int num2 = mList.Count;
		while (num2 > 0)
		{
			Entry entry = mList[--num2];
			float num3 = realtimeSinceStartup - entry.movementStart;
			entry.offset = offsetCurve.Evaluate(num3);
			entry.label.alpha = alphaCurve.Evaluate(num3);
			float num4 = scaleCurve.Evaluate(realtimeSinceStartup - entry.time) * (float)entry.label.font.size;
			if (num4 < 0.001f)
			{
				num4 = 0.001f;
			}
			entry.label.cachedTransform.localScale = new Vector3(num4, num4, num4);
			if (num3 > num)
			{
				Delete(entry);
			}
			else
			{
				entry.label.enabled = true;
			}
		}
		float a = 0f;
		int num5 = mList.Count;
		while (num5 > 0)
		{
			Entry entry2 = mList[--num5];
			a = Mathf.Max(a, entry2.offset);
			entry2.label.cachedTransform.localPosition = new Vector3(0f, a, 0f);
			a += Mathf.Round(entry2.label.cachedTransform.localScale.y);
		}
	}
}
