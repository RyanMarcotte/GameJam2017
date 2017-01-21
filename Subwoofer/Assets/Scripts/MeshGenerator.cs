using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid SquareGridMap;
    public MeshFilter Walls;
    public MeshFilter Cave;

    public bool IsIn2d;

    public List<Vector3> Vertices;
    public List<int> Triangles;

    public Dictionary<int, List<Triangle>> TriangleDictionary = new Dictionary<int, List<Triangle>>();
    public List<List<int>> Outlines = new List<List<int>>();
    public HashSet<int> CheckedVertices = new HashSet<int>();

    public void GenerateMesh(int[,] map, float squareSize)
    {
        Outlines.Clear();
        Vertices.Clear();
        TriangleDictionary.Clear();

        this.SquareGridMap = new SquareGrid(map, squareSize);

        Vertices = new List<Vector3>();
        Triangles = new List<int>();

        for (int x = 0; x < SquareGridMap.Squares.GetLength(0); x++)
        {
            for (int y = 0; y < SquareGridMap.Squares.GetLength(1); y++)
            {
                TriangulateSquare(SquareGridMap.Squares[x, y]);
            }
        }

        var mesh = new Mesh();
        Cave.mesh = mesh;

        mesh.vertices = Vertices.ToArray();
        mesh.triangles = Triangles.ToArray();
        mesh.RecalculateNormals();

        if (!IsIn2d)
        {
            CreateWallMesh();
            Generate2DColliders();
        }
        else
        {
            Generate2DColliders();
        }
    }

    private void CreateWallMesh()
    {
        CalculateMeshOutlines();

        var wallVertices = new List<Vector3>();
        var wallTriangles = new List<int>();
        var wallMesh = new Mesh();
        float wallHeight = 5;

        foreach (var outline in Outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                var startIndex = wallVertices.Count;
                wallVertices.Add(Vertices[outline[i]]); //Left Vertex
                wallVertices.Add(Vertices[outline[i + 1]]); //Right Vertex
                wallVertices.Add(Vertices[outline[i]] - Vector3.back * wallHeight); //Bottom Left Vertex
                wallVertices.Add(Vertices[outline[i + 1]] - Vector3.back * wallHeight); //Bottom Right Vertex

                //Create triangle going counter-clockwise
                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                //Create triangle going counter-clockwise
                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        Walls.mesh = wallMesh;

        var wallCollider = Walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }

    private void Generate2DColliders()
    {
        var currentColliders = gameObject.GetComponents<EdgeCollider2D>();

        for (int i = 0; i < currentColliders.Length; i++)
        {
            Destroy(currentColliders[i]);
        }

        CalculateMeshOutlines();

        foreach (var outline in Outlines)
        {
            var edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            var edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++)
            {
                edgePoints[i] = new Vector2(Vertices[outline[i]].x, Vertices[outline[i]].z);
            }

            edgeCollider.points = edgePoints;
        }

    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void TriangulateSquare(SquareNode square)
    {
        switch (square.Configuration)
        {
            case 0:
                break;

            //1 point:
            case 1:
                CreateMeshFromPoints(square.CenterLeft, square.CenterBottom, square.BottomLeft);
                break;
            case 2:
                CreateMeshFromPoints(square.BottomRight, square.CenterBottom, square.CenterRight);
                break;
            case 4:
                CreateMeshFromPoints(square.TopRight, square.CenterRight, square.CenterTop);
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
                CheckedVertices.Add(square.TopLeft.VertexIndex);
                CheckedVertices.Add(square.TopRight.VertexIndex);
                CheckedVertices.Add(square.BottomRight.VertexIndex);
                CheckedVertices.Add(square.BottomLeft.VertexIndex);
                break;
        }
    }

	public bool IsLandingPadSquare(int xCoordinate, int yCoordinate)
	{
		var square = SquareGridMap.Squares[xCoordinate, yCoordinate];

		return square.Configuration == 3;

	}

    private void CreateMeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);
    }

    private void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].VertexIndex == -1)
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

        var triangle = new Triangle(pointA.VertexIndex, pointB.VertexIndex, pointC.VertexIndex);
        AddTriangleToDictionary(pointA.VertexIndex, triangle);
        AddTriangleToDictionary(pointB.VertexIndex, triangle);
        AddTriangleToDictionary(pointC.VertexIndex, triangle);
    }

    private void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (!TriangleDictionary.ContainsKey(vertexIndexKey))
            TriangleDictionary[vertexIndexKey] = new List<Triangle>();

        TriangleDictionary[vertexIndexKey].Add(triangle);
    }

    private bool IsOutlineEdge(int vertexA, int vertexB)
    {
        var trianglesContainingVertexA = TriangleDictionary[vertexA];
        var sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;

                if (sharedTriangleCount > 1)
                    break;
            }
        }

        return sharedTriangleCount == 1;
    }

    private int GetConnectedOutlineVertex(int vertexIndex)
    {
        var trianglesConntainingVertex = TriangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesConntainingVertex.Count; i++)
        {
            var triangle = trianglesConntainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                var vertexB = triangle[j];

                if (vertexIndex != vertexB && !CheckedVertices.Contains(vertexB) && IsOutlineEdge(vertexIndex, vertexB))
                    return vertexB;
            }
        }

        return -1;
    }

    private void CalculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < Vertices.Count; vertexIndex++)
        {
            if (!CheckedVertices.Contains(vertexIndex))
            {
                var newOutlineIndex = GetConnectedOutlineVertex(vertexIndex);

                if (newOutlineIndex != -1)
                {
                    CheckedVertices.Add(vertexIndex);

                    var newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    Outlines.Add(newOutline);
                    FollowOutline(newOutlineIndex, Outlines.Count - 1);
                    Outlines[Outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    private void FollowOutline(int vertexIndex, int outlineIndex)
    {
        Outlines[outlineIndex].Add(vertexIndex);
        CheckedVertices.Add(vertexIndex);
        var nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
            FollowOutline(nextVertexIndex, outlineIndex);
    }
}


public struct Triangle
{
    public int VertexIndexA;
    public int VertexIndexB;
    public int VertexIndexC;
    int[] Vertices;

    public Triangle(int vertexIndexA, int vertexIndexB, int vertexIndexC)
    {
        this.VertexIndexA = vertexIndexA;
        this.VertexIndexB = vertexIndexB;
        this.VertexIndexC = vertexIndexC;

        Vertices = new int[3];
        Vertices[0] = vertexIndexA;
        Vertices[1] = vertexIndexB;
        Vertices[2] = vertexIndexC;
    }

    public bool Contains(int vertexIndex)
    {
        return vertexIndex == VertexIndexA || vertexIndex == VertexIndexB || vertexIndex == VertexIndexC;
    }

    public int this[int index]
    {
        get
        {
            return Vertices[index];
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
        this.UpperNode = new Node(Position + Vector3.up * squareSize / 2f);
        this.RightNode = new Node(Position + Vector3.right * squareSize / 2f);
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

        if (TopLeft.IsActive)
            Configuration += 8;
        if (TopRight.IsActive)
            Configuration += 4;
        if (BottomRight.IsActive)
            Configuration += 2;
        if (BottomLeft.IsActive)
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

        for (int x = 0; x < nodeCountX; x++)
        {
            for (int y = 0; y < nodeCountY; y++)
            {
                var position = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, -mapHeight / 2 + y * squareSize + squareSize / 2, 0);
                controlNodes[x, y] = new ControlNode(position, (map[x, y] == 1), squareSize);
            }
        }

        Squares = new SquareNode[(nodeCountX - 1), (nodeCountY - 1)];

        for (int x = 0; x < (nodeCountX - 1); x++)
        {
            for (int y = 0; y < (nodeCountY - 1); y++)
            {
                Squares[x, y] = new SquareNode(controlNodes[x, (y + 1)], controlNodes[(x + 1), (y + 1)], controlNodes[(x + 1), y], controlNodes[x, y]);
            }
        }
    }
}