#pragma warning disable 0618,0619
using System.Reflection;
using Dismount.Vehicular;
using UnityEngine;

public class GlobalAdjust : MonoBehaviour
{
	public Car car;

	private static GlobalAdjust gInstance;

	private void Awake()
	{
		if (gInstance != null)
		{
			Object.Destroy(gInstance.gameObject);
			return;
		}
		gInstance = this;
		Object.DontDestroyOnLoad(this);
		Car componentInChildren = base.transform.GetComponentInChildren<Car>();
		if ((bool)componentInChildren)
		{
			car = componentInChildren;
			componentInChildren.gameObject.SetActive(false);
		}
	}

	public void SetParameters()
	{
		if ((bool)gInstance)
		{
			return;
		}
		Car car = Object.FindObjectOfType<Car>();
		if (!(car.gameObject == this))
		{
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public;
			FieldInfo[] fields = this.car.GetType().GetFields(bindingAttr);
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				fieldInfo.SetValue(car, fieldInfo.GetValue(this.car));
			}
		}
	}
}
