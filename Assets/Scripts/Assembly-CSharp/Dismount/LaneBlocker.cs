#pragma warning disable 0618,0619
using Dismount.Vehicular;
using UnityEngine;

namespace Dismount
{
	public class LaneBlocker : MonoBehaviour
	{
		private struct Fist
		{
			public Rigidbody rigidbody;

			public Vector3 origin;

			public Vector3 up;
		}

		private enum State
		{
			Init = 0,
			Up = 1,
			Down = 2,
			Raising = 3,
			Lowering = 4
		}

		public AnimationCurve raiseAnimation = AnimationCurve.EaseInOut(0f, 0f, 0.4f, 1f);

		public AnimationCurve lowerAnimation = AnimationCurve.EaseInOut(0f, 1f, 2f, 0f);

		public float stayUpDuration = 3f;

		public float curveMaxYOffset = 1.52f;

		private Fist[] fists;

		private float yOffset = 1f;

		private float stateTime;

		private float raiseDuration = 1f;

		private float lowerDuration = 1f;

		private float raiseStartTime = -1f;

		private State state;

		private void OnDismountStarted()
		{
			for (int i = 0; i < fists.Length; i++)
			{
				fists[i].up = fists[i].rigidbody.transform.up;
				fists[i].origin = fists[i].rigidbody.position - curveMaxYOffset * fists[i].up;
			}
			Lower();
		}

		private void Awake()
		{
			Rigidbody[] componentsInChildren = base.gameObject.GetComponentsInChildren<Rigidbody>();
			fists = new Fist[componentsInChildren.Length];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				fists[i].rigidbody = componentsInChildren[i];
				fists[i].origin = componentsInChildren[i].position;
				fists[i].up = componentsInChildren[i].transform.up;
			}
			raiseDuration = raiseAnimation.keys[raiseAnimation.keys.Length - 1].time;
			lowerDuration = lowerAnimation.keys[lowerAnimation.keys.Length - 1].time;
			UpdateFists();
		}

		private void UpdateFists()
		{
			float num = curveMaxYOffset * yOffset;
			Fist[] array = fists;
			for (int i = 0; i < array.Length; i++)
			{
				Fist fist = array[i];
				fist.rigidbody.MovePosition(fist.origin + fist.up * num);
			}
		}

		private void FixedUpdate()
		{
			stateTime += Time.fixedDeltaTime;
			if (raiseStartTime > 0f && Time.fixedTime > raiseStartTime)
			{
				raiseStartTime = -1f;
				Raise(0f);
			}
			if (state == State.Down)
			{
				return;
			}
			if (state == State.Lowering)
			{
				yOffset = lowerAnimation.Evaluate(stateTime);
				UpdateFists();
				if (stateTime >= lowerDuration)
				{
					state = State.Down;
				}
			}
			else if (state == State.Raising)
			{
				yOffset = raiseAnimation.Evaluate(stateTime);
				UpdateFists();
				if (stateTime >= raiseDuration)
				{
					state = State.Up;
					stateTime = 0f;
				}
			}
			else if (state == State.Up && stateTime >= stayUpDuration)
			{
				Lower();
			}
		}

		private float FindTime(AnimationCurve curve, float value)
		{
			for (float num = 0f; num < curve.keys[curve.keys.Length - 1].time; num += 0.05f)
			{
				float num2 = curve.Evaluate(num);
				if (Mathf.Abs(num2 - value) <= 0.05f)
				{
					return num;
				}
			}
			return 0f;
		}

		private void Lower()
		{
			if (state != State.Down && state != State.Lowering)
			{
				stateTime = 0f;
				if (state == State.Raising)
				{
					stateTime = FindTime(lowerAnimation, yOffset);
				}
				state = State.Lowering;
			}
		}

		private void Raise(float delay)
		{
			if (delay > 0f)
			{
				raiseStartTime = Time.fixedTime + delay;
			}
			else if (state != State.Raising && state != State.Up)
			{
				stateTime = 0f;
				if (state == State.Lowering)
				{
					stateTime = FindTime(raiseAnimation, yOffset);
				}
				state = State.Raising;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!other.isTrigger || state == State.Raising || state == State.Up)
			{
				return;
			}
			Vehicle component = other.GetComponent<Vehicle>();
			if (component == null)
			{
				return;
			}
			Vector3 position = base.transform.position;
			Vector3 vector = other.ClosestPointOnBounds(position);
			float num = Vector3.Dot(position - vector, -base.transform.forward);
			if (num < 1f)
			{
				Raise(0f);
				return;
			}
			Vector3 velocity = other.GetComponent<Rigidbody>().velocity;
			float num2 = Vector3.Dot(velocity, -base.transform.forward);
			if (!(num2 <= 0f))
			{
				float num3 = num / num2;
				num3 -= raiseDuration * 0.2f;
				if (num3 < 0f)
				{
					num3 = 0f;
				}
				Raise(num3);
			}
		}
	}
}
