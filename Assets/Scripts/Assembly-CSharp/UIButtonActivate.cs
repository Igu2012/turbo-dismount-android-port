#pragma warning disable 0618,0619
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Activate")]
public class UIButtonActivate : MonoBehaviour
{
	public GameObject target;

	public bool state = true;

	private void OnClick()
	{
		if (target != null)
		{
			NGUITools.SetActive(target, state);
		}
	}
}
