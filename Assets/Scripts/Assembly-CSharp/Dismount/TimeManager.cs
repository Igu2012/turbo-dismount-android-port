#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class TimeManager : MonoBehaviour
	{
		private float internalTime;

		private float internalDeltaTime;

		private float internalFixedTime;

		private bool internalIsPaused;

		private static TimeManager instance;

		public bool showGUI;

		public static bool isPaused
		{
			get
			{
				return Instance.internalIsPaused;
			}
			set
			{
				Instance.internalIsPaused = value;
			}
		}

		public static float time
		{
			get
			{
				return Instance.internalTime;
			}
		}

		public static float deltaTime
		{
			get
			{
				return Instance.internalDeltaTime;
			}
		}

		public static float fixedTime
		{
			get
			{
				return Instance.internalFixedTime;
			}
		}

		public static float fixedDeltaTime
		{
			get
			{
				return Time.fixedDeltaTime;
			}
		}

		public float realTime
		{
			get
			{
				return Time.time;
			}
		}

		public float realDeltaTime
		{
			get
			{
				return Time.deltaTime;
			}
		}

		public float realFixedTime
		{
			get
			{
				return Time.fixedTime;
			}
		}

		public float realFixedDeltaTime
		{
			get
			{
				return Time.fixedDeltaTime;
			}
		}

		private static TimeManager Instance
		{
			get
			{
				if (!instance)
				{
					GameObject gameObject = new GameObject("TimeManager");
					gameObject.transform.position = Vector3.zero;
					gameObject.name = "TimeManager";
					instance = gameObject.AddComponent<TimeManager>();
				}
				return instance;
			}
		}

		private void Awake()
		{
			if (!instance)
			{
				instance = this;
			}
			else
			{
				Debug.LogWarning("More than one TimeManager added to the scene.");
			}
		}

		private void Update()
		{
			float num = internalTime;
			if (!isPaused)
			{
				internalTime += Time.deltaTime;
			}
			internalDeltaTime = internalTime - num;
		}

		private void FixedUpdate()
		{
			if (!isPaused)
			{
				internalFixedTime += Time.fixedDeltaTime;
			}
		}
	}
}
