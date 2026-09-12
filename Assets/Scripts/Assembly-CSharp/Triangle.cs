#pragma warning disable 0618,0619
public class Triangle
{
	public int vertex0;

	public int vertex1;

	public int vertex2;

	public Point point0;

	public Point point1;

	public Point point2;

	public Edge edge0;

	public Edge edge1;

	public Edge edge2;

	public Triangle(int vertex0, int vertex1, int vertex2, Point point0, Point point1, Point point2, Edge edge0, Edge edge1, Edge edge2)
	{
		this.vertex0 = vertex0;
		this.vertex1 = vertex1;
		this.vertex2 = vertex2;
		this.point0 = point0;
		this.point1 = point1;
		this.point2 = point2;
		this.edge0 = edge0;
		this.edge1 = edge1;
		this.edge2 = edge2;
	}
}
