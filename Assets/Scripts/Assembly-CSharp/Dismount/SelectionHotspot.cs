#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class SelectionHotspot : MonoBehaviour
	{
		public GameItem.ItemCategory category;

		public GameItem item;

		[HideInInspector]
		public GameObject currentGameObject;

		private Transform visual;

		private Material visualMaterial;

		private Color originalColor;

		public Color hoverColor = new Color(0f, 0.44f, 0.75f, 1f);

		private int hover;

		private float hoverBlend;

		private ComboGizmo gizmo;

		public bool Hover
		{
			get
			{
				return hover > 0;
			}
			set
			{
				hover = (value ? 2 : 0);
				if (gizmo != null)
				{
					GizmoManager.ActivateGizmo(gizmo);
				}
				if (DismountGame.level != null)
				{
					DismountGame.level.activeObstacleHotspot = this;
				}
			}
		}

		private void Awake()
		{
			if ((bool)item && item.itemCategory != category)
			{
				Debug.LogError("Initial item doesn't match item category for hotspot: " + base.name);
			}
			Transform transform = base.transform.Find("ComboGizmo");
			if ((bool)transform)
			{
				gizmo = transform.gameObject.GetComponent<ComboGizmo>();
				GizmoManager.AddGizmo(gizmo);
			}
			visual = base.transform.Find("Visual");
			if ((bool)visual)
			{
				visualMaterial = visual.GetComponent<Renderer>().material;
				originalColor = visualMaterial.color;
			}
		}

		private void Update()
		{
			hover--;
			if (hover > 0)
			{
				hoverBlend += Time.deltaTime * 5f;
				if (hoverBlend > 1f)
				{
					hoverBlend = 1f;
				}
			}
			else
			{
				hoverBlend -= Time.deltaTime * 5f;
				if (hoverBlend < 0f)
				{
					hoverBlend = 0f;
				}
			}
			if (visual != null)
			{
				Color color = Color.Lerp(originalColor, hoverColor, hoverBlend);
				visualMaterial.color = new Color(color.r, color.g, color.b, 1f);
				visual.localScale = Vector3.Lerp(Vector3.one, new Vector3(1.2f, 1f, 1.2f), hoverBlend);
			}
		}
	}
}
