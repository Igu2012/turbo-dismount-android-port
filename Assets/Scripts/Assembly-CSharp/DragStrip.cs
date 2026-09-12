#pragma warning disable 0618,0619
using System;
using System.Collections.Generic;
using Dismount;
using Dismount.LevelEditor;
using Dismount.Vehicular;
using UnityEngine;

public class DragStrip : MonoBehaviour
{
	[Serializable]
	public class Opponent
	{
		public GameObject gameObjectPrefab;

		public float maxSpeed = 40f;

		public float maxAcceleration = 4f;

		public float maxDeceleration = -12f;
	}

	private float dragStartTime = -1f;

	private float leftTime;

	private float rightTime;

	private float leftSpeed = -1f;

	private float rightSpeed = -1f;

	private bool leftRunning;

	private bool rightRunning;

	private int winner = -1;

	private float winningTime = -1f;

	private bool checkWinner = true;

	public TimeSignRow leftTimeSign;

	public TimeSignRow leftSpeedSign;

	public TimeSignRow rightTimeSign;

	public TimeSignRow rightSpeedSign;

	public MovingObject opponent;

	private NPCVehicleData opponentVehicleData;

	private float opponentMaxSpeed = 40f;

	private float opponentMaxAcceleration = 4f;

	private float opponentMaxDeceleration = -12f;

	private float opponentAcceleration;

	private int opponentIndex;

	private bool metricSpeed;

	public List<Opponent> opponents = new List<Opponent>();

	private void Awake()
	{
		opponentIndex = Prefs.GetInt("level.dragstrip.opponent", 0);
		Opponent opponent = opponents[opponentIndex];
		Spline component = GameObject.Find("OpponentPath").GetComponent<Spline>();
		Vector3 positionOnSplineFast = component.GetPositionOnSplineFast(0f);
		Quaternion orientationOnSplineFast = component.GetOrientationOnSplineFast(0f);
		GameObject gameObject = UnityEngine.Object.Instantiate(opponent.gameObjectPrefab.gameObject, positionOnSplineFast, orientationOnSplineFast) as GameObject;
		this.opponent = gameObject.GetComponent<MovingObject>();
		opponentVehicleData = this.opponent.GetComponent<NPCVehicleData>();
		gameObject.transform.parent = GameObject.Find("SceneDynamic").transform;
		this.opponent.GetComponent<Rigidbody>().isKinematic = true;
		opponentVehicleData.velocity = 0f;
		this.opponent.SetFollowPath(component);
		opponentMaxSpeed = opponent.maxSpeed;
		opponentMaxAcceleration = opponent.maxAcceleration;
		opponentMaxDeceleration = opponent.maxDeceleration;
	}

	public void OnDismountStarted()
	{
		dragStartTime = Time.fixedTime;
		leftRunning = true;
		rightRunning = true;
		UpdateTime(leftTimeSign, 0f);
		UpdateSpeed(leftSpeedSign, -1f);
		UpdateTime(rightTimeSign, 0f);
		UpdateSpeed(rightSpeedSign, -1f);
		opponentMaxSpeed = RandomizeByFactor(opponentMaxSpeed, 0.7f, 1.3f);
		opponentMaxAcceleration = RandomizeByFactor(opponentMaxAcceleration, 0.7f, 1.3f);
		opponentMaxDeceleration = RandomizeByFactor(opponentMaxDeceleration, 0.7f, 1.3f);
		opponentAcceleration = opponentMaxAcceleration;
		metricSpeed = DismountGame.playerState.metricUnits;
	}

	public void OnDismountReset()
	{
		if (opponents.Count <= 1)
		{
			opponentIndex = 0;
		}
		else
		{
			int num = opponentIndex;
			int num2 = 0;
			while (opponentIndex == num && num2++ < 10)
			{
				opponentIndex = UnityEngine.Random.Range(0, opponents.Count);
			}
		}
		Prefs.SetInt("level.dragstrip.opponent", opponentIndex);
		Prefs.Save();
	}

	private float RandomizeByFactor(float val, float min, float max)
	{
		return val + UnityEngine.Random.Range(val * min, val * max);
	}

	public void OnFinishLineCrossed(Collider other, float finishLineZ)
	{
		if (other.isTrigger)
		{
			return;
		}
		if (rightRunning && (bool)other.attachedRigidbody.gameObject.GetComponent<Vehicle>())
		{
			rightRunning = false;
			rightSpeed = other.attachedRigidbody.velocity.magnitude;
			rightTime = Time.fixedTime - dragStartTime;
			float z = other.bounds.max.z;
			z -= finishLineZ;
			rightTime -= z / rightSpeed;
			UpdateTime(rightTimeSign, rightTime);
			UpdateSpeed(rightSpeedSign, rightSpeed);
			if (winner < 0)
			{
				winner = 2;
				winningTime = Time.fixedTime;
			}
			DismountGame.playerState.statistics.DragStripFinishLineCrossed(rightTime, rightSpeed);
		}
		if (leftRunning && (bool)other.attachedRigidbody.gameObject.GetComponent<MovingObject>())
		{
			leftRunning = false;
			leftSpeed = opponentVehicleData.velocity;
			leftTime = Time.fixedTime - dragStartTime;
			float z2 = other.bounds.max.z;
			z2 -= finishLineZ;
			leftTime -= z2 / leftSpeed;
			UpdateTime(leftTimeSign, leftTime);
			UpdateSpeed(leftSpeedSign, leftSpeed);
			opponentAcceleration = opponentMaxDeceleration;
			if (winner < 0)
			{
				winner = 1;
				winningTime = Time.fixedTime;
			}
		}
	}

	private void FixedUpdate()
	{
		float fixedTime = Time.fixedTime;
		float num = fixedTime - dragStartTime;
		opponentVehicleData.velocity += opponentAcceleration * Time.fixedDeltaTime;
		if (winner > 0 && checkWinner)
		{
			checkWinner = false;
			if (!leftRunning && !rightRunning)
			{
				winner = ((leftTime < rightTime) ? 1 : 2);
			}
		}
		if (leftRunning)
		{
			opponentAcceleration = (1f - opponentVehicleData.velocity / opponentMaxSpeed) * opponentMaxAcceleration;
		}
		else
		{
			opponentAcceleration = opponentVehicleData.velocity / opponentMaxSpeed * opponentMaxDeceleration;
		}
		if (leftRunning)
		{
			leftTime = num;
			UpdateTime(leftTimeSign, leftTime);
		}
		if (rightRunning)
		{
			rightTime = num;
			UpdateTime(rightTimeSign, rightTime);
		}
		if (winner <= 0)
		{
			return;
		}
		if (fixedTime > winningTime + 2f)
		{
			if (winner == 1)
			{
				UpdateTime(leftTimeSign, leftTime);
			}
			else
			{
				UpdateTime(rightTimeSign, rightTime);
			}
			winner = 0;
			return;
		}
		int num2 = (int)((fixedTime - winningTime) * 4f);
		bool flag = (num2 & 1) == 0;
		if (winner == 1)
		{
			if (flag)
			{
				UpdateTime(leftTimeSign, leftTime);
			}
			else
			{
				UpdateTime(leftTimeSign, -1f);
			}
		}
		else if (flag)
		{
			UpdateTime(rightTimeSign, rightTime);
		}
		else
		{
			UpdateTime(rightTimeSign, -1f);
		}
	}

	private void UpdateTime(TimeSignRow sign, float time)
	{
		if (time < 0f)
		{
			sign.Print("  d   ");
			return;
		}
		time = Mathf.Clamp(time, 0f, 99.999f);
		sign.Print(time.ToString("00.000"));
	}

	private void UpdateSpeed(TimeSignRow sign, float speed)
	{
		if (speed < 0f)
		{
			sign.Print("   d  ");
			return;
		}
		speed = ((!metricSpeed) ? (Mathf.Clamp(speed, 0f, 999.99f) * 2.23694f) : (Mathf.Clamp(speed, 0f, 999.99f) * 3.6f));
		sign.Print(speed.ToString("000.00"));
	}
}
