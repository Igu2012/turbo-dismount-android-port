#pragma warning disable 0618,0619
using System.Collections.Generic;

namespace reLive
{
	public class ArrayPool<T> where T : new()
	{
		private Stack<T[]> items = new Stack<T[]>();

		private Stack<T[]> allItems = new Stack<T[]>();

		public T[] Get()
		{
			if (items.Count == 0)
			{
				T[] array = allocate();
				allItems.Push(array);
				return array;
			}
			return items.Pop();
		}

		public void Free(T[] item)
		{
			items.Push(item);
		}

		private T[] allocate()
		{
			T[] array = new T[Replay.BlockSize];
			for (int i = 0; i < Replay.BlockSize; i++)
			{
				array[i] = new T();
			}
			return array;
		}

		public void Prealloc(int numberOfItems)
		{
			for (int i = 0; i < numberOfItems; i++)
			{
				T[] t = allocate();
				items.Push(t);
				allItems.Push(t);
			}
		}

		public void ReclaimItems()
		{
			items = new Stack<T[]>(allItems);
		}
	}
}
