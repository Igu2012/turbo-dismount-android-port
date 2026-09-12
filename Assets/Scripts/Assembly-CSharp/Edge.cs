#pragma warning disable 0618,0619
using UnityEngine;

public class Edge
{
	public int index;

	public Point point0;

	public Point point1;

	public Vector3 line;

	public Edge(Point point0, Point point1)
	{
		this.point0 = point0;
		this.point1 = point1;
		line = point1.position - point0.position;
	}
}
