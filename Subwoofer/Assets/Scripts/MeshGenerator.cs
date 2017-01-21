using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour 
{
	public SquareGrid SquareGridMap;
	public List<Vector3> Vertices;
	public List<int> Triangles;

	public void GenerateMesh(int[,] map, float squareSize)
	{
		this.SquareGridMap = new SquareGrid(map, squareSize);

		Vertices = new List<Vector3>();
		Triangles = new List<int>();

		for(int x = 0; x < SquareGridMap.Squares.GetLength(0); x++)
		{
			for(int y = 0; y < SquareGridMap.Squares.GetLength(1); y++)
			{
				TriangulateSquare(SquareGridMap.Squares[x, y]);
			}
		}

		var mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		
		mesh.vertices = Vertices.ToArray();
		mesh.triangles = Triangles.ToArray();
		mesh.RecalculateNormals();
	}

	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
	}

	public void OnDrawGizmos()
	{
		if(SquareGridMap != null)
		{
			// for(int x = 0; x < SquareGridMap.Squares.GetLength(0); x++)
			// {
			// 	for(int y = 0; y < SquareGridMap.Squares.GetLength(1); y++)
			// 	{
			// 		Gizmos.color = SquareGridMap.Squares[x, y].TopLeft.IsActive ? Color.black : Color.white;
			// 		Gizmos.DrawCube(SquareGridMap.Squares[x, y].TopLeft.Position, Vector3.one * 0.4f);

			// 		Gizmos.color = SquareGridMap.Squares[x, y].TopRight.IsActive ? Color.black : Color.white;
			// 		Gizmos.DrawCube(SquareGridMap.Squares[x, y].TopRight.Position, Vector3.one * 0.4f);

			// 		Gizmos.color = SquareGridMap.Squares[x, y].BottomRight.IsActive ? Color.black : Color.white;
			// 		Gizmos.DrawCube(SquareGridMap.Squares[x, y].BottomRight.Position, Vector3.one * 0.4f);

			// 		Gizmos.color = SquareGridMap.Squares[x, y].BottomLeft.IsActive ? Color.black : Color.white;
			// 		Gizmos.DrawCube(SquareGridMap.Squares[x, y].BottomLeft.Position, Vector3.one * 0.4f);

			// 		Gizmos.color = Color.grey;
			// 		Gizmos.DrawCube(SquareGridMap.Squares[x, y].CenterTop.Position, Vector3.one * 0.15f);
			// 		Gizmos.DrawCube(SquareGridMap.Squares[x, y].CenterRight.Position, Vector3.one * 0.15f);
			// 		Gizmos.DrawCube(SquareGridMap.Squares[x, y].CenterBottom.Position, Vector3.one * 0.15f);
			// 		Gizmos.DrawCube(SquareGridMap.Squares[x, y].CenterLeft.Position, Vector3.one * 0.15f);
			// 	}
			// }
		}
	}

	public void TriangulateSquare(SquareNode square)
	{
		switch(square.Configuration)
		{
			case 0:
				break;
			
			//1 point:
			case 1:
				CreateMeshFromPoints(square.CenterBottom, square.BottomLeft, square.CenterLeft);
				break;
			case 2:
				CreateMeshFromPoints(square.CenterRight, square.BottomRight, square.CenterBottom);
				break;
			case 4:
				CreateMeshFromPoints(square.CenterTop, square.TopRight, square.CenterRight);
				break;
			case 8:
				CreateMeshFromPoints(square.TopLeft, square.CenterTop, square.CenterLeft);
				break;
			
			//2 points:
			case 3:
				CreateMeshFromPoints(square.CenterRight, square.BottomRight, square.BottomLeft, square.CenterLeft);
				break;
			case 6:
				CreateMeshFromPoints(square.CenterTop, square.TopRight, square.BottomRight, square.CenterBottom);
				break;
			case 9:
				CreateMeshFromPoints(square.TopLeft, square.CenterTop, square.CenterBottom, square.BottomLeft);
				break;
			case 12:
				CreateMeshFromPoints(square.TopLeft, square.TopRight, square.CenterRight, square.CenterLeft);
				break;
			case 5:
				CreateMeshFromPoints(square.CenterTop, square.TopRight, square.CenterRight, square.CenterBottom, square.BottomLeft, square.CenterLeft);
				break;
			case 10:
				CreateMeshFromPoints(square.TopLeft, square.CenterTop, square.CenterRight, square.BottomRight, square.CenterBottom, square.CenterLeft);
				break;

			//3 points:
			case 7:
				CreateMeshFromPoints(square.CenterTop, square.TopRight, square.BottomRight, square.BottomLeft, square.CenterLeft);
				break;
			case 11:
				CreateMeshFromPoints(square.TopLeft, square.CenterTop, square.CenterRight, square.BottomRight, square.BottomLeft);
				break;
			case 13:
				CreateMeshFromPoints(square.TopLeft, square.TopRight, square.CenterRight, square.CenterBottom, square.BottomLeft);
				break;
			case 14:
				CreateMeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.CenterBottom, square.CenterLeft);
				break;

			//4 points:
			case 15:
				CreateMeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.BottomLeft);
				break;
		}
	}

	private void CreateMeshFromPoints(params Node[] points)
	{
		AssignVertices(points);

		if(points.Length >= 3)
			CreateTriangle(points[0], points[1], points[2]);
		if(points.Length >= 4)
			CreateTriangle(points[0], points[2], points[3]);
		if(points.Length >= 5)
			CreateTriangle(points[0], points[3], points[4]);
		if(points.Length >= 6)
			CreateTriangle(points[0], points[4], points[5]);
	}

	private void AssignVertices(Node[] points)
	{
		for(int i = 0; i < points.Length; i++)
		{
			if(points[i].VertexIndex == -1)
			{
				points[i].VertexIndex = Vertices.Count;
				Vertices.Add(points[i].Position);
			}
		}
	}

	private void CreateTriangle(Node pointA, Node pointB, Node pointC)
	{
		Triangles.Add(pointA.VertexIndex);
		Triangles.Add(pointB.VertexIndex);
		Triangles.Add(pointC.VertexIndex);
	}
}

public class Node
{
	public Vector3 Position;
	public int VertexIndex = -1;

	public Node(Vector3 position)
	{
		this.Position = position;
	}
}

public class ControlNode : Node
{
	public bool IsActive;
	public Node UpperNode;
	public Node RightNode;

	public ControlNode(Vector3 position, bool active, float squareSize) : base(position)
	{
		this.IsActive = active;
		this.UpperNode = new Node(Position + Vector3.forward * squareSize/2f);
		this.RightNode = new Node(Position + Vector3.right * squareSize/2f);
	}
}

public class SquareNode
{
	public ControlNode TopLeft;
	public ControlNode TopRight;
	public ControlNode BottomRight;
	public ControlNode BottomLeft;

	public Node CenterTop;
	public Node CenterRight;
	public Node CenterBottom;
	public Node CenterLeft;

	public int Configuration;

	public SquareNode(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
	{
		this.TopLeft = topLeft;
		this.TopRight = topRight;
		this.BottomRight = bottomRight;
		this.BottomLeft = bottomLeft;

		this.CenterTop = TopLeft.RightNode;
		this.CenterRight = BottomRight.UpperNode;
		this.CenterBottom = BottomLeft.RightNode;
		this.CenterLeft = BottomLeft.UpperNode;

		CalculateConfiguration();
	}

	private void CalculateConfiguration()
	{
		Configuration = 0;

		if(TopLeft.IsActive)
			Configuration += 8;
		if(TopRight.IsActive)
			Configuration += 4;
		if(BottomRight.IsActive)
			Configuration += 2;
		if(BottomLeft.IsActive)
			Configuration += 1;
	}

}

public class SquareGrid
{
	public SquareNode[,] Squares;
	
	public SquareGrid(int[,] map, float squareSize)
	{
		var nodeCountX = map.GetLength(0);
		var nodeCountY = map.GetLength(1);
		var mapWidth = nodeCountX * squareSize;
		var mapHeight = nodeCountY * squareSize;

		ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

		for(int x = 0; x < nodeCountX; x++)
		{
			for(int y = 0; y < nodeCountY; y++)
			{
				var position = new Vector3(-mapWidth/2 + x * squareSize + squareSize/2, 0, -mapHeight/2 + y * squareSize + squareSize/2);
				controlNodes[x, y] = new ControlNode(position, (map[x, y] == 1), squareSize);
			}
		}

		Squares = new SquareNode[(nodeCountX - 1), (nodeCountY -1)];

		for(int x = 0; x < (nodeCountX - 1); x++)
		{
			for(int y = 0; y < (nodeCountY - 1); y++)
			{
				Squares[x, y] = new SquareNode(controlNodes[x, (y + 1)], controlNodes[(x + 1), (y + 1)], controlNodes[(x + 1), y], controlNodes[x, y]);
			}
		}
	}
}