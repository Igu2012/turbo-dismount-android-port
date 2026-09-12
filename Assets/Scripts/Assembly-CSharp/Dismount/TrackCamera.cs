#pragma warning disable 0618,0619
using System;
using UnityEngine;

namespace Dismount
{
	[RequireComponent(typeof(Camera))]
	public class TrackCamera : MonoBehaviour
	{
		[Serializable]
		public class Track
		{
			public Spline controlTrack;

			public Spline cameraTrack;

			[Range(-1f, 1f)]
			public float cameraTrackOffset;

			[Range(0f, 1f)]
			public float cameraTrackLinearVsS;
		}

		private const float minTargetSize = 2f;

		private const float maxTargetSize = 35f;

		public Track[] tracks;

		[HideInInspector]
		public Transform primaryTarget;

		[HideInInspector]
		public Transform secondaryTarget;

		private Transform thisTransform;

		private Camera thisCamera;

		[Range(2f, 35f)]
		public float targetSize = 20f;

		private float currTargetSize = 20f;

		private bool replayMode;

		private float smoothingWeight = 1f;

		private bool pinching;

		private float pinchReferenceSize = 20f;

		private float pinchReferenceTargetSize = 20f;

		private int currTrackIndex = -1;

		private float currTrackParam = -1f;

		public int previewTrack;

		[Range(0f, 1f)]
		public float previewControlParam;

		[Range(-1f, 1f)]
		public float previewControlLeftRight;

		private void Start()
		{
			thisTransform = base.transform;
			thisCamera = base.GetComponent<Camera>();
			if (tracks == null || tracks.Length < 1)
			{
				Debug.LogError("TrackCamera (" + base.name + "): no tracks defined");
				return;
			}
			for (int i = 0; i < tracks.Length; i++)
			{
				if (tracks[i].controlTrack == null)
				{
					Debug.LogError("TrackCamera (" + base.name + "): control track missing on track[" + i + "]");
					return;
				}
				if (tracks[i].cameraTrack == null)
				{
					Debug.LogError("TrackCamera (" + base.name + "): camera track missing on track[" + i + "]");
					return;
				}
			}
			currTargetSize = targetSize;
		}

		private void EnterReplayMode()
		{
			replayMode = true;
		}

		private void ExitReplayMode()
		{
			replayMode = false;
		}

		private void Update()
		{
			bool flag = pinching;
			if (Input.touchCount == 2)
			{
				pinching = true;
			}
			else
			{
				pinching = false;
			}
			if (pinching)
			{
				Vector3 vector = thisCamera.ViewportToWorldPoint(new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, thisCamera.nearClipPlane));
				Vector3 vector2 = thisCamera.ViewportToWorldPoint(new Vector3(Input.touches[1].position.x, Input.touches[1].position.y, thisCamera.nearClipPlane));
				float magnitude = (vector2 - vector).magnitude;
				if (!flag)
				{
					pinchReferenceSize = magnitude;
					pinchReferenceTargetSize = currTargetSize;
				}
				targetSize = pinchReferenceSize / magnitude * pinchReferenceTargetSize;
			}
			if (SXInputManager.GetMouseScrollWheel() < 0f && (bool)DismountGame.uiManager && DismountGame.uiManager.CurrentState != UIManager.State.SetupVehicle)
			{
				targetSize *= 1.1f;
			}
			if (SXInputManager.GetMouseScrollWheel() > 0f && (bool)DismountGame.uiManager && DismountGame.uiManager.CurrentState != UIManager.State.SetupVehicle)
			{
				targetSize /= 1.1f;
			}
			if ((bool)DismountGame.uiManager)
			{
				float threshold = 0.2f;
				float num = Time.smoothDeltaTime * 25f;
				UIManager.State currentState = DismountGame.uiManager.CurrentState;
				if (currentState != UIManager.State.LevelSelect && currentState != UIManager.State.SetupCharacter && currentState != UIManager.State.SetupVehicle && currentState != UIManager.State.SetupObstacle && currentState != UIManager.State.CustomizeCharacter && currentState != UIManager.State.Paused)
				{
					float axisValue = SXInputManager.GetAxisValue(SXInputManager.Axis.LSTICKV, threshold);
					targetSize -= axisValue * num;
				}
			}
			targetSize = Mathf.Clamp(targetSize, 2f, 35f);
		}

		private void LateUpdate()
		{
			if (replayMode)
			{
				smoothingWeight = 5f * Time.deltaTime;
				if (smoothingWeight > 1f)
				{
					smoothingWeight = 1f;
				}
				UpdateCamera();
			}
		}

		private void FixedUpdate()
		{
			if (!replayMode)
			{
				smoothingWeight = 5f * Time.fixedDeltaTime;
				UpdateCamera();
			}
		}

		private float sCurve(float t, float s)
		{
			float num = t * 2f;
			if (num < 1f)
			{
				return 0.5f * Mathf.Pow(num, 1f + 2f * s);
			}
			return 0.5f * (2f - Mathf.Pow(2f - num, 1f + 2f * s));
		}

		private void UpdateCamera()
		{
			if (tracks == null || tracks.Length < 1 || primaryTarget == null)
			{
				return;
			}
			if (pinching)
			{
				currTargetSize = Mathf.Lerp(currTargetSize, targetSize, smoothingWeight * 2f);
			}
			else
			{
				currTargetSize = Mathf.Lerp(currTargetSize, targetSize, smoothingWeight);
			}
			Vector3 position = primaryTarget.position;
			float num = float.MaxValue;
			int num2 = 0;
			float num3 = 0f;
			for (int i = 0; i < tracks.Length; i++)
			{
				float closestPointParamFast = tracks[i].controlTrack.GetClosestPointParamFast(position, 0f, 1f);
				Vector3 positionOnSplineFast = tracks[i].controlTrack.GetPositionOnSplineFast(closestPointParamFast);
				float sqrMagnitude = (position - positionOnSplineFast).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					num2 = i;
					num3 = closestPointParamFast;
				}
			}
			Track track = tracks[num2];
			if (num2 != currTrackIndex)
			{
				currTrackIndex = num2;
				currTrackParam = num3;
			}
			else if (Mathf.Abs(currTrackParam - num3) > 0.25f)
			{
				if (track.controlTrack.autoClose)
				{
					if (currTrackParam > 0.9f && num3 < 0.1f)
					{
						currTrackParam = Mathf.Lerp(currTrackParam, num3 + 1f, smoothingWeight);
						currTrackParam = Mathf.Repeat(currTrackParam, 1f);
					}
					else if (currTrackParam < 0.1f && num3 > 0.9f)
					{
						currTrackParam = Mathf.Lerp(currTrackParam + 1f, num3, smoothingWeight);
						currTrackParam = Mathf.Repeat(currTrackParam, 1f);
					}
					else
					{
						currTrackParam = num3;
					}
				}
				else
				{
					currTrackParam = num3;
				}
			}
			else
			{
				currTrackParam = Mathf.Lerp(currTrackParam, num3, smoothingWeight);
			}
			float num4 = currTrackParam;
			num4 += Mathf.Clamp(track.cameraTrackOffset, -1f, 1f);
			num4 = ((!track.cameraTrack.autoClose) ? Mathf.Clamp01(num4) : Mathf.Repeat(num4, 1f));
			float cameraTrackLinearVsS = track.cameraTrackLinearVsS;
			if (cameraTrackLinearVsS > 0f)
			{
				num4 = sCurve(num4, Mathf.Clamp01(cameraTrackLinearVsS));
			}
			thisTransform.position = track.cameraTrack.GetPositionOnSplineFast(num4);
			Vector3 vector = primaryTarget.position;
			if (secondaryTarget != null)
			{
				float magnitude = (primaryTarget.position - thisTransform.position).magnitude;
				Vector3 normalized = (secondaryTarget.position - thisTransform.position).normalized;
				Vector3 vector2 = thisTransform.position + normalized * magnitude;
				Vector3 vector3 = vector2 - primaryTarget.position;
				vector3.y *= 0.7f;
				float magnitude2 = vector3.magnitude;
				vector3 /= magnitude2;
				float num5 = Mathf.Clamp(0.5f * magnitude2, 0f, 0.66f * currTargetSize);
				float num6 = Mathf.InverseLerp(2f, 35f, currTargetSize);
				float num7 = 1f - num6;
				num6 = 1f - num7 * num7 * num7 * num7;
				num5 = num6 * num5;
				vector = primaryTarget.position + num5 * vector3;
			}
			thisTransform.LookAt(vector, Vector3.up);
			float magnitude3 = (thisTransform.position - vector).magnitude;
			float num8 = Mathf.Atan(currTargetSize * 0.5f / magnitude3);
			thisCamera.fieldOfView = Mathf.Clamp(num8 * 2f * 180f / (float)Math.PI, 5f, 120f);
		}

		private void OnDrawGizmosSelected()
		{
			if (tracks != null && tracks.Length >= 1)
			{
				Track track = tracks[Mathf.Clamp(previewTrack, 0, tracks.Length - 1)];
				Spline controlTrack = track.controlTrack;
				Spline cameraTrack = track.cameraTrack;
				if (!(controlTrack == null) && !(cameraTrack == null))
				{
					float cameraTrackLinearVsS = track.cameraTrackLinearVsS;
					Vector3 vector = cameraTrack.GetPositionOnSplineFast(0f) + Vector3.right * 2f;
					Color color = new Color(0.5f, 0.5f, 0.5f, 1f);
					Color color2 = new Color(1f, 0f, 0f, 1f);
					float num = previewControlParam;
					float num2 = num + Mathf.Clamp(track.cameraTrackOffset, -1f, 1f);
					num2 = ((!track.cameraTrack.autoClose) ? Mathf.Clamp01(num2) : Mathf.Repeat(num2, 1f));
					Gizmos.color = color;
					Vector3 positionOnSplineFast = controlTrack.GetPositionOnSplineFast(num);
					Vector3 normalized = controlTrack.GetTangentToSpline(num).normalized;
					Vector3 vector2 = Vector3.Cross(normalized, Vector3.up);
					Gizmos.DrawLine(positionOnSplineFast - 50f * vector2, positionOnSplineFast + 50f * vector2);
					positionOnSplineFast -= previewControlLeftRight * vector2 * 50f;
					Gizmos.DrawSphere(positionOnSplineFast, 1f);
					Gizmos.color = color2;
					Vector3 positionOnSplineFast2 = cameraTrack.GetPositionOnSplineFast(sCurve(num2, cameraTrackLinearVsS));
					base.transform.position = positionOnSplineFast2;
					base.transform.LookAt(positionOnSplineFast);
					float magnitude = (positionOnSplineFast2 - positionOnSplineFast).magnitude;
					float num3 = Mathf.Atan(Mathf.Clamp(targetSize, 2f, 35f) * 0.5f / magnitude);
					base.GetComponent<Camera>().fieldOfView = Mathf.Clamp(num3 * 2f * 180f / (float)Math.PI, 5f, 120f);
					Gizmos.DrawSphere(positionOnSplineFast2, 1f);
				}
			}
		}
	}
}
