#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount.Vehicular
{
	public class EngineSoundField : MonoBehaviour
	{
		public SoundField field;

		[HideInInspector]
		public float maxVolumeRpm = -1f;

		[HideInInspector]
		public float muteStartRpm = 800f;

		[HideInInspector]
		public float muteEndRpm = 1000f;

		[Range(0f, 1f)]
		public float minThrottleVolume = 0.8f;

		[Range(0f, 1f)]
		public float minRpmVolume = 1f;

		public bool exaggerateLoadShift;

		private float gRpm;

		private float gLoad;

		public int gizmoDrawLayer = -1;

		private void Awake()
		{
			field = GetComponent<SoundField>();
		}

		public void Play()
		{
			if (!(field == null))
			{
				field.Play();
			}
		}

		public void Stop()
		{
			if (!(field == null))
			{
				field.Stop();
			}
		}

		public void UpdateSound(float throttle, float rpm, float load)
		{
			if (!(field == null))
			{
				if (exaggerateLoadShift)
				{
					load = 0.5f + load * 0.5f;
					load = load * load * (3f - 2f * load);
					load = -1f + 2f * load;
				}
				float num = Mathf.Lerp(minThrottleVolume, 1f, throttle);
				float num2 = Mathf.Lerp(minRpmVolume, 1f, rpm / maxVolumeRpm);
				float num3 = 1f;
				if (rpm < muteEndRpm)
				{
					num3 = Mathf.InverseLerp(muteStartRpm, muteEndRpm, rpm);
				}
				float volumeMultiplier = num * num2 * num3;
				field.UpdateSound(rpm, load, volumeMultiplier);
				gLoad = Mathf.Clamp(load, field.minY, field.maxY);
				gRpm = Mathf.Clamp(rpm, field.minX, field.maxX);
			}
		}

		public void OnDrawGizmos()
		{
			if (gizmoDrawLayer >= 0)
			{
				float num = (float)Screen.height / 20f;
				GLDraw.Begin();
				GL.LoadPixelMatrix();
				Draw(Mathf.Clamp(gizmoDrawLayer, 0, field.layers.Length - 1), new Vector3(2f * num, 8f * num, 0f), Vector3.right * num * 10f, Vector3.up * num * 10f);
				GLDraw.End();
			}
		}

		private void FindGridSteps(ref float min, ref float max, float scale, ref float step)
		{
			step = scale / 10f;
			float num = 1f;
			while (step > 10f)
			{
				num *= 10f;
				step /= 10f;
			}
			while (step < 1f)
			{
				num /= 10f;
				step *= 10f;
			}
			if (step > 5f)
			{
				step = 10f * num;
			}
			else if (step > 2.5f)
			{
				step = 5f * num;
			}
			else if (step > 1.5f)
			{
				step = 2f * num;
			}
			else
			{
				step = num;
			}
			min = Mathf.Floor(min / step) * step;
			max = Mathf.Ceil(max / step) * step;
		}

		public void Draw(int drawLayer, Vector3 pos, Vector3 right, Vector3 up)
		{
			Color color = new Color(0.125f, 0.125f, 0.125f, 1f);
			Color color2 = new Color(1f, 1f, 1f, 0.15f);
			Color color3 = new Color(1f, 1f, 1f, 0.3f);
			Color color4 = new Color(1f, 1f, 1f, 0.75f);
			Color color5 = new Color(1f, 0.4f, 0.2f, 1f);
			Color color6 = new Color(1f, 0.4f, 0.2f, 1f);
			Color color7 = new Color(1f, 0.4f, 0.2f, 0.15f);
			Color color8 = new Color(1f, 1f, 1f, 0.1f);
			Color color9 = new Color(0.2f, 0.6f, 1f, 1f);
			Color color10 = new Color(0.2f, 0.6f, 1f, 0.4f);
			Color color11 = new Color(0.2f, 0.6f, 1f, 0.15f);
			drawLayer = Mathf.Clamp(drawLayer, 0, field.layers.Length - 1);
			SoundFieldLayer soundFieldLayer = field.layers[drawLayer];
			SoundFieldSample[] samples = soundFieldLayer.samples;
			float num = Mathf.Clamp(gLoad, field.minY, field.maxY);
			float num2 = Mathf.Clamp(gRpm, field.minX, field.maxX);
			soundFieldLayer.UpdateSampleScale();
			float num3 = (field.maxX - field.minX) / field.xScale;
			float num4 = (field.maxY - field.minY) / field.yScale;
			GLDraw.Rect(pos - right * 0.1f - up * 0.1f, right * (num3 + 0.2f), up * (num4 + 0.2f), color, true);
			GLDraw.String(pos + right * num3 * 0.5f + up * (num4 + 0.05f), right, up, 0.025f, 4, false, 5, color4, field.name.ToUpper() + " - " + soundFieldLayer.name.ToUpper());
			Vector3 pos2 = pos + right * num3 * 0.5f - up * 0.05f;
			GLDraw.String(pos2, right, up, 0.025f, 4, false, 5, color4, "RPM");
			pos2 = pos + right * (num3 + 0.05f) + up * 0.5f * num4;
			GLDraw.String(pos2, right, up, 0.025f, 4, false, 5, color4, "L\nO\nA\nD");
			float min = field.minX;
			float max = field.maxX;
			float xScale = field.xScale;
			float step = 1f;
			FindGridSteps(ref min, ref max, xScale, ref step);
			float min2 = field.minY;
			float max2 = field.maxY;
			float yScale = field.yScale;
			float step2 = 1f;
			FindGridSteps(ref min2, ref max2, yScale, ref step2);
			float num5 = (min - field.minX) / xScale;
			float num6 = (max - field.minX) / xScale;
			float num7 = (min2 - field.minY) / yScale;
			float num8 = (max2 - field.minY) / yScale;
			GLDraw.Line(pos + right * num5 + up * num7, pos + right * num6 + up * num7, color3);
			GLDraw.Line(pos + right * num5 + up * num8, pos + right * num6 + up * num8, color3);
			GLDraw.Line(pos + right * num5 + up * num7, pos + right * num5 + up * num8, color3);
			GLDraw.Line(pos + right * num6 + up * num7, pos + right * num6 + up * num8, color3);
			for (float num9 = min; num9 < max + 0.5f * step; num9 += step)
			{
				float num10 = (num9 - field.minX) / xScale;
				GLDraw.Line(pos + right * num10 + up * num7, pos + right * num10 + up * num8, color2);
			}
			for (float num11 = min2; num11 < max2 + 0.5f * step2; num11 += step2)
			{
				float num12 = (num11 - field.minY) / yScale;
				GLDraw.Line(pos + right * num7 + up * num12, pos + right * num6 + up * num12, color2);
			}
			foreach (SoundFieldSample soundFieldSample in samples)
			{
				Vector3 vector = (soundFieldSample.x - field.minX) / field.xScale * right + (soundFieldSample.y - field.minY) / field.yScale * up;
				if (!soundFieldSample.isActiveAndEnabled)
				{
					float num13 = soundFieldSample.originalVolume * 0.1f;
					GLDraw.Ellipse(pos + vector, right * num13, up * num13, color8, false);
					continue;
				}
				Color color12 = color9;
				Color color13 = color6;
				if (soundFieldSample.mix > 0.025f)
				{
					color12 = color10;
					color13 = color6;
				}
				else if (soundFieldSample.mix > 0f)
				{
					color12 = Color.Lerp(color11, color10, Mathf.InverseLerp(0f, 0.025f, soundFieldSample.mix));
					color13 = Color.Lerp(color7, color6, Mathf.InverseLerp(0f, 0.025f, soundFieldSample.mix));
				}
				else
				{
					color12 = color11;
					color13 = color7;
				}
				float num14 = soundFieldLayer.volumeMultiplier * soundFieldSample.originalVolume * 0.1f;
				GLDraw.Ellipse(pos + vector, right * num14, up * num14, color12, false);
				if (soundFieldSample.width > 0f || soundFieldSample.height > 0f)
				{
					float num15 = soundFieldSample.scaledWidth * 0.5f;
					float num16 = soundFieldSample.scaledHeight * 0.5f;
					Vector3 vector2 = pos + vector - right * num15 - up * num16;
					Vector3 vector3 = right * 2f * num15;
					Vector3 vector4 = up * 2f * num16;
					GLDraw.Line(vector2, vector2 + vector4, color13);
					GLDraw.Line(vector2, vector2 + vector3, color13);
					GLDraw.Line(vector2 + vector4, vector2 + vector4 + vector3, color13);
					GLDraw.Line(vector2 + vector3, vector2 + vector4 + vector3, color13);
				}
				num14 = soundFieldLayer.volumeMultiplier * soundFieldSample.mix * soundFieldSample.originalVolume * 0.1f;
				GLDraw.Ellipse(pos + vector, right * num14, up * num14, color9, true);
			}
			if (Application.isPlaying)
			{
				float num17 = Mathf.Clamp(field.node.x, field.minX, field.maxX);
				float num18 = Mathf.Clamp(field.node.y, field.minY, field.maxY);
				Vector3 vector5 = new Vector3((num17 - field.minX) / field.xScale, (num18 - field.minY) / field.yScale, 0f);
				if (soundFieldLayer.maxDistance > 0f)
				{
					float maxDistance = soundFieldLayer.maxDistance;
					GLDraw.Ellipse(pos + vector5.x * right + vector5.y * up, up * maxDistance, right * maxDistance, color7, false, 144);
				}
				GLDraw.Ellipse(pos + vector5.x * right + vector5.y * up, right * 0.01f, up * 0.01f, color5, true);
				string s = num2.ToString("0");
				string s2 = num.ToString("0.00");
				GLDraw.String(pos + vector5.x * right - 0.05f * up, right, up, 0.0125f, 0, true, 5, color5, s);
				GLDraw.String(pos + (num3 + 0.05f) * right + vector5.y * up, right, up, 0.0125f, 0, true, 5, color5, s2);
			}
		}
	}
}
