#pragma warning disable 0618,0619
public interface ITriangulator
{
	void Fill(out int[] newEdges, out int[] newTriangles, out int[] newTriangleEdges);
}
