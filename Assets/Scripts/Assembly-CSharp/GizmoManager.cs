#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

public class GizmoManager : MonoBehaviour
{
	private static List<ComboGizmo> gizmos = new List<ComboGizmo>();

	public static ComboGizmo currentlyActiveGizmo;

	public static void AddGizmo(ComboGizmo gizmo)
	{
		gizmos.Add(gizmo);
		gizmo.gameObject.SetActive(false);
	}

	public static void ActivateGizmo(ComboGizmo gizmo)
	{
		if ((bool)currentlyActiveGizmo)
		{
			if (currentlyActiveGizmo.IsDragging())
			{
				return;
			}
			currentlyActiveGizmo.gameObject.SetActive(false);
		}
		currentlyActiveGizmo = gizmo;
	}

	public static void Clear()
	{
		gizmos.Clear();
		currentlyActiveGizmo = null;
	}
}
