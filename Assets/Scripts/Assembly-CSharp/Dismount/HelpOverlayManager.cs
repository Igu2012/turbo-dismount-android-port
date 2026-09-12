#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	public class HelpOverlayManager : MonoBehaviour
	{
		[Serializable]
		public class HelpOverlaySequence
		{
			private const float inactivityDelay = 3f;

			private const float repeatDelay = 10f;

			private const float itemShowDuration = 3f;

			public string name;

			public List<HelpOverlayItem> items;

			private int currentItem = -1;

			private float nextItemTime = -1f;

			private int repeats;

			public void Start()
			{
				currentItem = -1;
				if (repeats > 0)
				{
					nextItemTime = Time.time + 10f;
				}
				else
				{
					nextItemTime = Time.time + 3f;
				}
			}

			public void Stop()
			{
				if (currentItem >= 0)
				{
					items[currentItem].HideNow();
				}
				nextItemTime = -1f;
				currentItem = -1;
			}

			public void Update()
			{
				if (nextItemTime < 0f)
				{
					return;
				}
				float time = Time.time;
				if (time > nextItemTime)
				{
					if (currentItem >= 0)
					{
						items[currentItem].Hide();
					}
					currentItem++;
					if (currentItem >= items.Count)
					{
						currentItem = -1;
						nextItemTime = Time.time + 10f;
						repeats++;
					}
					else
					{
						items[currentItem].Show();
						nextItemTime = Time.time + 3f;
					}
				}
			}
		}

		public List<HelpOverlaySequence> sequences;

		private HelpOverlaySequence currentSequence;

		private Vector3 prevMousePosition;

		public void RemoveRecordingHelpItems()
		{
			for (int i = 0; i < sequences.Count; i++)
			{
				HelpOverlaySequence helpOverlaySequence = sequences[i];
				if (!(helpOverlaySequence.name == "Replay"))
				{
					continue;
				}
				HelpOverlayItem helpOverlayItem = null;
				for (int j = 0; j < helpOverlaySequence.items.Count; j++)
				{
					HelpOverlayItem helpOverlayItem2 = helpOverlaySequence.items[j];
					if (helpOverlayItem2.name == "RecordingHelpItem")
					{
						helpOverlayItem = helpOverlayItem2;
						break;
					}
				}
				if (helpOverlayItem != null)
				{
					helpOverlaySequence.items.Remove(helpOverlayItem);
				}
			}
		}

		private void Update()
		{
			if (currentSequence != null)
			{
				currentSequence.Update();
			}
			Vector3 mousePosition = Input.mousePosition;
			bool flag = SXInputManager.GetMouseButton(0) || SXInputManager.GetMouseButton(1) || SXInputManager.GetMouseButton(2);
			if ((SXInputManager.GetMouseButtonDown(0) || SXInputManager.GetMouseButtonDown(1) || SXInputManager.GetMouseButtonDown(2) || (flag && (mousePosition != prevMousePosition || SXInputManager.GetMouseX() != 0f || SXInputManager.GetMouseY() != 0f))) && currentSequence != null)
			{
				currentSequence.Stop();
				currentSequence.Start();
			}
			prevMousePosition = mousePosition;
		}

		public void ShowSequence(string sequenceName)
		{
			if (currentSequence != null)
			{
				if (currentSequence.name == sequenceName)
				{
					return;
				}
				currentSequence.Stop();
			}
			currentSequence = null;
			foreach (HelpOverlaySequence sequence in sequences)
			{
				if (sequence.name == sequenceName)
				{
					currentSequence = sequence;
					break;
				}
			}
			if (currentSequence != null)
			{
				currentSequence.Start();
			}
		}

		public void StopSequence()
		{
			if (currentSequence != null)
			{
				currentSequence.Stop();
			}
			currentSequence = null;
		}
	}
}
