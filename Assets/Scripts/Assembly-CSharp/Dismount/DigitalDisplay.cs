#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	public class DigitalDisplay : MonoBehaviour
	{
		private List<DigitalDisplayDigit> digits = new List<DigitalDisplayDigit>();

		private Dictionary<char, int> mapping = new Dictionary<char, int>();

		private void Start()
		{
			for (int i = 0; i < 100; i++)
			{
				Transform transform = base.transform.Find("D" + i.ToString("00"));
				if ((bool)transform)
				{
					DigitalDisplayDigit component = transform.GetComponent<DigitalDisplayDigit>();
					if ((bool)component)
					{
						digits.Add(component);
						component.SetIndex(10);
					}
					else
					{
						Debug.LogWarning("Missing DigitalDisplayDigit component on " + transform.name);
					}
				}
			}
			mapping.Add('0', 0);
			mapping.Add('1', 1);
			mapping.Add('2', 2);
			mapping.Add('3', 3);
			mapping.Add('4', 4);
			mapping.Add('5', 5);
			mapping.Add('6', 6);
			mapping.Add('7', 7);
			mapping.Add('8', 8);
			mapping.Add('9', 9);
			mapping.Add(' ', 10);
			mapping.Add('.', 11);
			mapping.Add(':', 12);
			mapping.Add('G', 13);
		}

		public void Print(char[] chars)
		{
			int num = chars.Length - 1;
			int num2 = 0;
			while (num2 < chars.Length && num2 < digits.Count)
			{
				int index = 10;
				if (mapping.ContainsKey(chars[num]))
				{
					index = mapping[chars[num]];
				}
				digits[num2].SetIndex(index);
				num2++;
				num--;
			}
		}
	}
}
