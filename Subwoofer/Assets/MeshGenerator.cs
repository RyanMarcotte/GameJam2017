using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour 
{
	public SquareGrid SquareGridMap;

	public void GenerateMesh(int[,] map, float squareSize)
	{
		this.SquareGridMap = new SquareGrid(map, squareSize);
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
			for(int x = 0; x < SquareGridMap.Squares.GetLength(0); x++)
			{
				for(int y = 0; y < SquareGridMap.Squares.GetLength(1); y++)
				{
					Gizmos.color = SquareGridMap.Squares[x, y].TopLeft.IsActive ? Color.black : Color.white;
					Gizmos.DrawCube(SquareGridMap.Squares[x, y].TopLeft.Position, Vector3.one * 0.4f);

					Gizmos.color = SquareGridMap.Squares[x, y].TopRight.IsActive ? Color.black : Color.white;
					Gizmos.DrawCube(SquareGridMap.Squares[x, y].TopRight.Position, Vector3.one * 0.4f);

					Gizmos.color = SquareGridMap.Squares[x, y].BottomRight.IsActive ? Color.black : Color.white;
					Gizmos.DrawCube(SquareGridMap.Squares[x, y].BottomRight.Position, Vector3.one * 0.4f);

					Gizmos.color = SquareGridMap.Squares[x, y].BottomLeft.IsActive ? Color.black : Color.white;
					Gizmos.DrawCube(SquareGridMap.Squares[x, y].BottomLeft.Position, Vector3.one * 0.4f);

					Gizmos.color = Color.grey;
					Gizmos.DrawCube(SquareGridMap.Squares[x, y].CenterTop.Position, Vector3.one * 0.15f);
					Gizmos.DrawCube(SquareGridMap.Squares[x, y].CenterRight.Position, Vector3.one * 0.15f);
					Gizmos.DrawCube(SquareGridMap.Squares[x, y].CenterBottom.Position, Vector3.one * 0.15f);
					Gizmos.DrawCube(SquareGridMap.Squares[x, y].CenterLeft.Position, Vector3.one * 0.15f);
				}
			}
		}
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

	public SquareNode(ControlNode topLeft, ControlNode topRight, ControlNode bottomLeft, ControlNode bottomRight)
	{
		this.TopLeft = topLeft;
		this.TopRight = topRight;
		this.BottomLeft = bottomLeft;
		this.BottomRight = bottomRight;

		this.CenterTop = TopLeft.RightNode;
		this.CenterRight = BottomRight.UpperNode;
		this.CenterBottom = BottomLeft.RightNode;
		this.CenterLeft = BottomLeft.UpperNode;
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
				controlNodes[x, y] = new ControlNode(position, map[x, y] == 1, squareSize);
			}
		}

		Squares = new SquareNode[nodeCountX - 1, nodeCountY -1];

		for(int x = 0; x < nodeCountX - 1; x++)
		{
			for(int y = 0; y < nodeCountY - 1; y++)
			{
				Squares[x, y] = new SquareNode(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
			}
		}
	}
}