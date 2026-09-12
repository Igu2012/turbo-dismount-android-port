#pragma warning disable 0618,0619
using UnityEngine;

[AddComponentMenu("NGUI/Examples/UI Equipment Slot")]
public class UIEquipmentSlot : UIItemSlot
{
	public InvEquipment equipment;

	public InvBaseItem.Slot slot;

	protected override InvGameItem observedItem
	{
		get
		{
			return (!(equipment != null)) ? null : equipment.GetItem(slot);
		}
	}

	protected override InvGameItem Replace(InvGameItem item)
	{
		return (!(equipment != null)) ? item : equipment.Replace(slot, item);
	}
}
