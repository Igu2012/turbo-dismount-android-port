#pragma warning disable 0618,0619
using UnityEngine;
using reLive;

public class Impact : MonoBehaviour
{
	private float maxAge = 0.1f;

	private float startAge;

	private float startScale = 0.3f;

	private float endScale = 0.5f;

	private void Start()
	{
		startAge = Time.time;
		GetComponent<RecordMeInReplay>().Warmup();
	}

	private void FixedUpdate()
	{
		float num = Time.time - startAge;
		if (num >= maxAge)
		{
			base.gameObject.SetActive(false);
		}
		float t = Mathf.Min(1f, num / maxAge);
		float num2 = Mathf.Lerp(startScale, endScale, t);
		base.transform.localScale = new Vector3(num2, num2, num2);
	}
}
