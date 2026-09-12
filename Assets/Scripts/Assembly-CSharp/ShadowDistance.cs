#pragma warning disable 0618,0619
using Dismount;
using UnityEngine;

public class ShadowDistance : MonoBehaviour
{
	private float m_originalDistance;

	private float m_targetDistance;

	private float m_currentDistance;

	private Camera m_cam;

	private static int xsamples = 5;

	private static int ysamples = 5;

	private float[] dist = new float[xsamples * ysamples];

	private void Start()
	{
		m_originalDistance = QualitySettings.shadowDistance;
		m_targetDistance = m_originalDistance;
		m_currentDistance = m_originalDistance;
		m_cam = base.GetComponent<Camera>();
	}

	private void Update()
	{
		if (DismountGame.IsLowPerformanceDevice())
		{
			return;
		}
		string activeCameraId = DismountGame.cameraManager.GetActiveCameraId();
		float num = 8f;
		float maxShadowDistance = DismountGame.GetMaxShadowDistance();
		float a = maxShadowDistance;
		if (activeCameraId.Equals("1stPerson"))
		{
			a = num;
		}
		else if (DismountGame.cameraManager.GetCameraTarget(activeCameraId) != null)
		{
			Vector3 position = DismountGame.cameraManager.GetCameraTarget(activeCameraId).position;
			if (activeCameraId.Equals("Character"))
			{
				float num2 = 1f - (Vector3.Distance(position, m_cam.transform.position) - 2f) / 18f;
				a = Mathf.Lerp(num, maxShadowDistance, 1f - num2 * num2);
			}
			else if (activeCameraId.Equals("Vehicle"))
			{
				float num3 = 1f - (Vector3.Distance(position, m_cam.transform.position) - 5f) / 15f;
				a = Mathf.Lerp(num, maxShadowDistance, 1f - num3 * num3);
			}
			else if (activeCameraId.Equals("Scene"))
			{
				float num4 = 1f - Vector3.Distance(position, m_cam.transform.position) / 20f;
				a = Mathf.Lerp(num, maxShadowDistance, 1f - num4 * num4);
			}
		}
		m_targetDistance = Mathf.Min(Mathf.Max(a, num), maxShadowDistance);
		m_currentDistance = m_targetDistance;
		QualitySettings.shadowDistance = m_currentDistance;
	}
}
