#pragma warning disable 0618,0619
using UnityEngine;

public class SXSlider : MonoBehaviour
{
	private void adjust(float delta)
	{
		UISlider component = GetComponent<UISlider>();
		float num = Mathf.Clamp01(component.sliderValue + delta);
		component.eventReceiver.SendMessage(component.functionName, num);
		component.sliderValue = num;
	}

	private void OnVolumeInc()
	{
		adjust(0.1f);
	}

	private void OnVolumeDec()
	{
		adjust(-0.1f);
	}
}
