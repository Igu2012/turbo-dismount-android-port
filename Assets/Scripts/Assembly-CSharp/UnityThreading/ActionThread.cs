#pragma warning disable 0618,0619
using System;
using System.Collections;

namespace UnityThreading
{
	public sealed class ActionThread : ThreadBase
	{
		private Action<ActionThread> action;

		public ActionThread(Action<ActionThread> action)
			: this(action, true)
		{
		}

		public ActionThread(Action<ActionThread> action, bool autoStartThread)
			: base("ActionThread", Dispatcher.Current, false)
		{
			this.action = action;
			if (autoStartThread)
			{
				Start();
			}
		}

		protected override IEnumerator Do()
		{
			action(this);
			return null;
		}
	}
}
