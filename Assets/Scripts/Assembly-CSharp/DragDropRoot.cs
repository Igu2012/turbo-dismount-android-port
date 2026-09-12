#pragma warning disable 0618,0619
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Drag and Drop Root")]
public class DragDropRoot : MonoBehaviour
{
	public static Transform root;

	private void Awake()
	{
		root = base.transform;
	}
}
