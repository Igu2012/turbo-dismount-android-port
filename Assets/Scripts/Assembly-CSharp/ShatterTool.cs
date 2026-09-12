#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ShatterTool : MonoBehaviour
{
	[SerializeField]
	private int generation = 1;

	[SerializeField]
	private int generationLimit = 3;

	[SerializeField]
	private int cuts = 2;

	[SerializeField]
	private bool fillCut = true;

	[SerializeField]
	private bool sendPreSplitMessage;

	[SerializeField]
	private bool sendPostSplitMessage;

	[SerializeField]
	private HullType internalHullType;

	private IHull hull;

	private Vector3 center;

	public int Generation
	{
		get
		{
			return generation;
		}
		set
		{
			generation = Mathf.Max(value, 1);
		}
	}

	public int GenerationLimit
	{
		get
		{
			return generationLimit;
		}
		set
		{
			generationLimit = Mathf.Max(value, 1);
		}
	}

	public int Cuts
	{
		get
		{
			return cuts;
		}
		set
		{
			cuts = Mathf.Max(value, 1);
		}
	}

	public bool FillCut
	{
		get
		{
			return fillCut;
		}
		set
		{
			fillCut = value;
		}
	}

	public bool SendPreSplitMessage
	{
		get
		{
			return sendPreSplitMessage;
		}
		set
		{
			sendPreSplitMessage = value;
		}
	}

	public bool SendPostSplitMessage
	{
		get
		{
			return sendPostSplitMessage;
		}
		set
		{
			sendPostSplitMessage = value;
		}
	}

	public HullType InternalHullType
	{
		get
		{
			return internalHullType;
		}
		set
		{
			internalHullType = value;
		}
	}

	public bool IsFirstGeneration
	{
		get
		{
			return generation == 1;
		}
	}

	public bool IsLastGeneration
	{
		get
		{
			return generation >= generationLimit;
		}
	}

	public Vector3 Center
	{
		get
		{
			return base.transform.TransformPoint(center);
		}
	}

	private void CalculateCenter()
	{
		center = GetComponent<MeshFilter>().sharedMesh.bounds.center;
	}

	public void Start()
	{
		Mesh sharedMesh = GetComponent<MeshFilter>().sharedMesh;
		if (hull == null)
		{
			if (internalHullType == HullType.FastHull)
			{
				hull = new FastHull(sharedMesh);
			}
			else if (internalHullType == HullType.LegacyHull)
			{
				hull = new LegacyHull(sharedMesh);
			}
		}
		CalculateCenter();
	}

	public void Shatter(Vector3 point)
	{
		if (!IsLastGeneration)
		{
			generation++;
			Plane[] array = new Plane[cuts];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Plane(Random.onUnitSphere, point);
			}
			Split(array);
		}
	}

	public void Split(Plane[] planes)
	{
		if (planes == null || planes.Length <= 0 || hull == null || hull.IsEmpty)
		{
			return;
		}
		UvMapper component = GetComponent<UvMapper>();
		if (component != null)
		{
			if (sendPreSplitMessage)
			{
				SendMessage("PreSplit", planes, SendMessageOptions.DontRequireReceiver);
			}
			Vector3[] points;
			Vector3[] normals;
			ConvertPlanesToLocalspace(planes, out points, out normals);
			IList<IHull> newHulls;
			CreateNewHulls(component, points, normals, out newHulls);
			GameObject[] newGameObjects;
			CreateNewGameObjects(newHulls, out newGameObjects);
			if (sendPostSplitMessage)
			{
				SendMessage("PostSplit", newGameObjects, SendMessageOptions.DontRequireReceiver);
			}
			Object.Destroy(base.gameObject);
		}
		else
		{
			Debug.LogWarning(base.name + " has no UvMapper attached! Please attach a UvMapper to use the ShatterTool.", this);
		}
	}

	private void ConvertPlanesToLocalspace(Plane[] planes, out Vector3[] points, out Vector3[] normals)
	{
		points = new Vector3[planes.Length];
		normals = new Vector3[planes.Length];
		for (int i = 0; i < planes.Length; i++)
		{
			Plane plane = planes[i];
			Vector3 vector = base.transform.InverseTransformPoint(plane.normal * (0f - plane.distance));
			Vector3 vector2 = base.transform.InverseTransformDirection(plane.normal);
			vector2.Scale(base.transform.localScale);
			vector2.Normalize();
			points[i] = vector;
			normals[i] = vector2;
		}
	}

	private void CreateNewHulls(UvMapper uvMapper, Vector3[] points, Vector3[] normals, out IList<IHull> newHulls)
	{
		newHulls = new List<IHull>();
		newHulls.Add(this.hull);
		for (int i = 0; i < points.Length; i++)
		{
			int count = newHulls.Count;
			for (int j = 0; j < count; j++)
			{
				IHull hull = newHulls[0];
				IHull resultA;
				IHull resultB;
				hull.Split(points[i], normals[i], fillCut, uvMapper, out resultA, out resultB);
				newHulls.Remove(hull);
				if (!resultA.IsEmpty)
				{
					newHulls.Add(resultA);
				}
				if (!resultB.IsEmpty)
				{
					newHulls.Add(resultB);
				}
			}
		}
	}

	private void CreateNewGameObjects(IList<IHull> newHulls, out GameObject[] newGameObjects)
	{
		Mesh[] array = new Mesh[newHulls.Count];
		float[] array2 = new float[newHulls.Count];
		float num = 0f;
		for (int i = 0; i < newHulls.Count; i++)
		{
			Mesh mesh = newHulls[i].GetMesh();
			Vector3 size = mesh.bounds.size;
			float num2 = size.x * size.y * size.z;
			array[i] = mesh;
			array2[i] = num2;
			num += num2;
		}
		GetComponent<MeshFilter>().sharedMesh = null;
		MeshCollider component = GetComponent<MeshCollider>();
		if (component != null)
		{
			component.sharedMesh = null;
		}
		newGameObjects = new GameObject[newHulls.Count];
		for (int j = 0; j < newHulls.Count; j++)
		{
			IHull hull = newHulls[j];
			Mesh sharedMesh = array[j];
			float num3 = array2[j];
			GameObject gameObject = (GameObject)Object.Instantiate(base.gameObject);
			ShatterTool component2 = gameObject.GetComponent<ShatterTool>();
			if (component2 != null)
			{
				component2.hull = hull;
			}
			MeshFilter component3 = gameObject.GetComponent<MeshFilter>();
			if (component3 != null)
			{
				component3.sharedMesh = sharedMesh;
			}
			MeshCollider component4 = gameObject.GetComponent<MeshCollider>();
			if (component4 != null)
			{
				component4.sharedMesh = sharedMesh;
			}
			Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
			if (rigidbody != null)
			{
				rigidbody.mass = base.GetComponent<Rigidbody>().mass * (num3 / num);
				if (!rigidbody.isKinematic)
				{
					rigidbody.velocity = base.GetComponent<Rigidbody>().GetPointVelocity(rigidbody.worldCenterOfMass);
					rigidbody.angularVelocity = base.GetComponent<Rigidbody>().angularVelocity;
				}
			}
			component2.CalculateCenter();
			newGameObjects[j] = gameObject;
		}
	}
}
