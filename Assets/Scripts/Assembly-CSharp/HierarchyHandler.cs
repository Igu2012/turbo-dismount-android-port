#pragma warning disable 0618,0619
using UnityEngine;

[RequireComponent(typeof(ShatterTool))]
public class HierarchyHandler : MonoBehaviour
{
	public bool attachPieceToParent = true;

	public float maxPieceToParentDistance = 1f;

	public bool addRbToDetachedPieces = true;

	public bool attachChildrenToPieces = true;

	public float maxChildToPieceDistance = 1f;

	public bool addRbToDetachedChildren = true;

	private Transform parent;

	private Transform[] children;

	public void PreSplit(Plane[] planes)
	{
		if (base.transform.parent != null)
		{
			parent = base.transform.parent;
			base.transform.parent = null;
		}
		children = new Transform[base.transform.childCount];
		int num = 0;
		foreach (Transform item in base.transform)
		{
			children[num++] = item;
		}
		base.transform.DetachChildren();
	}

	public void PostSplit(GameObject[] newGameObjects)
	{
		ShatterTool[] array = new ShatterTool[newGameObjects.Length];
		for (int i = 0; i < newGameObjects.Length; i++)
		{
			array[i] = newGameObjects[i].GetComponent<ShatterTool>();
		}
		if (parent != null)
		{
			ShatterTool component = parent.GetComponent<ShatterTool>();
			if (component != null)
			{
				ShatterTool shatterTool = null;
				if (attachPieceToParent)
				{
					shatterTool = FindClosestPiece(component, array, maxPieceToParentDistance);
					if (shatterTool != null)
					{
						shatterTool.transform.parent = parent;
					}
				}
				if (addRbToDetachedPieces)
				{
					ShatterTool[] array2 = array;
					foreach (ShatterTool shatterTool2 in array2)
					{
						if (shatterTool2 != null && shatterTool2 != shatterTool)
						{
							shatterTool2.gameObject.AddComponent<Rigidbody>();
						}
					}
				}
			}
		}
		Transform[] array3 = children;
		foreach (Transform transform in array3)
		{
			ShatterTool component2 = transform.GetComponent<ShatterTool>();
			if (component2 != null)
			{
				ShatterTool shatterTool3 = FindClosestPiece(component2, array, maxChildToPieceDistance);
				if (attachChildrenToPieces && shatterTool3 != null)
				{
					transform.parent = shatterTool3.transform;
				}
				else if (addRbToDetachedChildren)
				{
					transform.gameObject.AddComponent<Rigidbody>();
				}
			}
		}
	}

	private ShatterTool FindClosestPiece(ShatterTool reference, ShatterTool[] pieces, float maxDistance)
	{
		Vector3 center = reference.Center;
		float num = maxDistance * maxDistance;
		ShatterTool shatterTool = null;
		float num2 = 0f;
		foreach (ShatterTool shatterTool2 in pieces)
		{
			if (shatterTool2 != null)
			{
				float sqrMagnitude = (center - shatterTool2.Center).sqrMagnitude;
				if (sqrMagnitude < num && (sqrMagnitude < num2 || shatterTool == null))
				{
					shatterTool = shatterTool2;
					num2 = sqrMagnitude;
				}
			}
		}
		return shatterTool;
	}
}
