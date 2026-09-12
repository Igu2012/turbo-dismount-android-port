#pragma warning disable 0618,0619
using System.Collections.Generic;

namespace reLive
{
	public class Pool<T> where T : new()
	{
		private Stack<T> items = new Stack<T>();

		private Stack<T> allItems = new Stack<T>();

		public T Get()
		{
			if (items.Count == 0)
			{
				T val = new T();
				allItems.Push(val);
				return val;
			}
			return items.Pop();
		}

		public void Prealloc(int numberOfItems)
		{
			for (int i = 0; i < numberOfItems; i++)
			{
				T t = new T();
				allItems.Push(t);
				items.Push(t);
			}
		}

		public void ReclaimItems()
		{
			items = new Stack<T>(allItems);
		}
	}
}
