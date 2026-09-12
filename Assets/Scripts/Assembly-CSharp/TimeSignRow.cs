#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

public class TimeSignRow : MonoBehaviour
{
	private List<TimeSignDigit> digits = new List<TimeSignDigit>();

	private Dictionary<char, int> mapping = new Dictionary<char, int>();

	private void Start()
	{
		for (int i = 0; i < 6; i++)
		{
			GameObject gameObject = base.transform.Find("D" + i).gameObject;
			digits.Add(gameObject.GetComponent<TimeSignDigit>());
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
		mapping.Add('d', 12);
	}

	public void Print(string text)
	{
		char[] array = text.ToCharArray();
		for (int i = 0; i < 6; i++)
		{
			int number = 10;
			if (i < array.Length && mapping.ContainsKey(array[i]))
			{
				number = mapping[array[i]];
			}
			digits[i].SetNumber(number);
		}
	}
}
