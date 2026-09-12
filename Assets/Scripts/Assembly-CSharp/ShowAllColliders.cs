#pragma warning disable 0618,0619
using UnityEngine;

public class ShowAllColliders : MonoBehaviour
{
	public Transform root;

	public bool includeTriggers = true;

	private void OnDrawGizmos()
	{
		if (root == null)
		{
			return;
		}
		Collider[] componentsInChildren = root.GetComponentsInChildren<Collider>();
		GLDraw.Begin();
		foreach (Collider collider in componentsInChildren)
		{
			if (includeTriggers || !collider.isTrigger)
			{
				GL.MultMatrix(collider.transform.localToWorldMatrix);
				GLDraw.Collider(collider);
			}
		}
		GLDraw.End();
	}
}
