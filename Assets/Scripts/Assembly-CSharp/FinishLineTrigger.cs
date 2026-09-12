#pragma warning disable 0618,0619
using UnityEngine;

public class FinishLineTrigger : MonoBehaviour
{
	public DragStrip dragStripLogic;

	private float minZ;

	private void Start()
	{
		minZ = base.GetComponent<Collider>().bounds.min.z;
	}

	private void OnTriggerEnter(Collider other)
	{
		dragStripLogic.OnFinishLineCrossed(other, minZ);
	}
}
