#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

public class ShatterScheduler : MonoBehaviour
{
	[SerializeField]
	private int frameCooldown = 1;

	private List<IShatterTask> tasks = new List<IShatterTask>();

	private int framesSinceLastTask;

	public void AddTask(IShatterTask task)
	{
		tasks.Add(task);
	}

	public void Update()
	{
		framesSinceLastTask++;
		if (frameCooldown == 0)
		{
			foreach (IShatterTask task in tasks)
			{
				task.Run();
			}
			tasks.Clear();
			framesSinceLastTask = 0;
		}
		else if (framesSinceLastTask >= frameCooldown && tasks.Count >= 1)
		{
			tasks[0].Run();
			tasks.RemoveAt(0);
			framesSinceLastTask = 0;
		}
	}
}
