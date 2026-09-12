#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	public class StateManager : MonoBehaviour
	{
		public enum State
		{
			Init = 0,
			Splash = 1,
			LevelLoader = 2,
			MainMenu = 3,
			SetupScene = 4,
			Dismount = 5,
			ReplayMode = 6,
			LevelIntro = 7
		}

		private State mCurrentState;

		private State mPreviousState;

		private Dictionary<State, GameObject> gameStateObjects = new Dictionary<State, GameObject>();

		public State currentState
		{
			get
			{
				return mCurrentState;
			}
		}

		public State previousState
		{
			get
			{
				return mPreviousState;
			}
		}

		public GameObject currentStateObject
		{
			get
			{
				return gameStateObjects[mCurrentState];
			}
		}

		public void Awake()
		{
			Transform transform = base.transform.Find("GameStates");
			foreach (int value in Enum.GetValues(typeof(State)))
			{
				string text = ((State)value).ToString();
				if (value != 0)
				{
					Transform transform2 = transform.Find(text);
					if (!transform2)
					{
						Debug.LogError("Missing game state object for " + text);
					}
					gameStateObjects.Add((State)value, transform2.gameObject);
					gameStateObjects[(State)value].SetActive(false);
				}
			}
		}

		public void ChangeState(State nextState)
		{
			if (nextState != State.Init)
			{
				if (mCurrentState != State.Init)
				{
					gameStateObjects[mCurrentState].BroadcastMessage("ExitState", SendMessageOptions.DontRequireReceiver);
				}
				mPreviousState = mCurrentState;
				mCurrentState = nextState;
				gameStateObjects[mCurrentState].SetActive(true);
				gameStateObjects[mCurrentState].BroadcastMessage("EnterState", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
