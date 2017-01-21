using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MapFillValues : int
{
    FillSpace = 1,
    EmptySpace = 0
}

public class MapGenerator : MonoBehaviour
{
    public int[,] Map;
	public List<Room> SurvivingRooms;

	public Room FirstRoom
	{
		get { return SurvivingRooms.FirstOrDefault(x => x.LandingPadTile.IsValidCoordinate()); }
	}

	public Room LastRoom
	{
		get { return SurvivingRooms.LastOrDefault(x => x.LandingPadTile.IsValidCoordinate()); }
	}

	[Range(0, 100)]
    public int RandomMapFillPercentage;
    public int Height;
    public int Width;
    public int BorderSize = 5;
    public int RegionThresholdSize = 50;
    public int EmptyRegionThresholdSize = 50;
    public string Seed;
    public bool UseRandomSeed;

    public void Start()
    {
        GenerateMap();
    }

    public void Update()
    {

    }

    public void GenerateMap()
    {
        Map = new int[Width, Height];

		if(SurvivingRooms != null)
			SurvivingRooms.Clear();

        FillMap();

        for (int i = 0; i < 5; i++)
            SmoothMap();

        ProcessMap();

        int[,] borderedMap = new int[Width + BorderSize * 2, Height + BorderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= BorderSize && x < (Width + BorderSize) && y >= BorderSize && y < (Height + BorderSize))
                {
                    borderedMap[x, y] = Map[(x - BorderSize), (y - BorderSize)];
                }
                else
                {
                    borderedMap[x, y] = (int)MapFillValues.FillSpace;
                }
            }
        }

        var meshGenerator = GetComponent<MeshGenerator>();
        meshGenerator.GenerateMesh(borderedMap, 1);

		DetermineSurvivingRoomsLandingPads(meshGenerator);
	}

    public void FillMap()
    {
        if (UseRandomSeed)
            Seed = Time.time.ToString();

        var pseudoRandomNumberGenerator = new System.Random(Seed.GetHashCode());

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Map[x, y] = (pseudoRandomNumberGenerator.Next(0, 100) < RandomMapFillPercentage || (x == 0 || x == (Width - 1) || y == 0 || y == (Height - 1))) ? (int)MapFillValues.FillSpace : (int)MapFillValues.EmptySpace;
            }
        }
    }

    public void SmoothMap()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var neighbouringFilledSpaces = GetSurroundingWallCount(x, y);

                if (neighbouringFilledSpaces > 4)
                    Map[x, y] = (int)MapFillValues.FillSpace;
                else if (neighbouringFilledSpaces < 4)
                    Map[x, y] = (int)MapFillValues.EmptySpace;
            }
        }
    }

    //Look at a smaller 3 x 3 grid 
    public int GetSurroundingWallCount(int xValue, int yValue)
    {
        var wallCount = 0;
        for (int neighbourX = xValue - 1; neighbourX <= xValue + 1; neighbourX++)
        {
            for (int neighbourY = yValue - 1; neighbourY <= yValue + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != xValue || neighbourY != yValue)
                        wallCount += Map[neighbourX, neighbourY];
                }
                else
                    wallCount++;
            }
        }

        return wallCount;
    }

    public void ProcessMap()
    {
        var wallRegions = GetRegions((int)MapFillValues.FillSpace);

        foreach (var region in wallRegions)
        {
            if (region.Count < RegionThresholdSize)
            {
                foreach (var tile in region)
                {
                    Map[tile.TileX, tile.TileY] = (int)MapFillValues.EmptySpace;
                }
            }
        }

        var roomRegions = GetRegions((int)MapFillValues.EmptySpace);
		SurvivingRooms = new List<Room>();

        foreach (var roomRegion in roomRegions)
        {
            if (roomRegion.Count < EmptyRegionThresholdSize)
            {
                foreach (var roomtile in roomRegion)
                {
                    Map[roomtile.TileX, roomtile.TileY] = (int)MapFillValues.FillSpace;
                }
            }
			else
			{
				SurvivingRooms.Add(new Room(roomRegion, Map));
			}
        }

		ConnectClosestRooms();
    }

	public void ConnectClosestRooms()
	{
		var bestDistance = 0;
		var bestTileA = new Coordinate();
		var bestTileB = new Coordinate();
		var bestRoomA = new Room();
		var bestRoomB = new Room();
		var possibleConnectionFound = false;

		foreach(var roomA in SurvivingRooms)
		{
			possibleConnectionFound = false;

			foreach(var roomB in SurvivingRooms)
			{
				if (roomA == roomB)
					continue;

				if (roomA.IsConnected(roomB))
				{
					possibleConnectionFound = false;
					break;
				}

				for(int tileIndexA = 0; tileIndexA < roomA.EdgeTiles.Count; tileIndexA++)
				{
					for(int tileIndexB = 0; tileIndexB < roomB.EdgeTiles.Count; tileIndexB++)
					{
						var tileA = roomA.EdgeTiles[tileIndexA];
						var tileB = roomB.EdgeTiles[tileIndexB];
						var distanceBetweenRooms = (int)(Mathf.Pow((tileA.TileX - tileB.TileX), 2) + Mathf.Pow((tileA.TileY - tileB.TileY), 2));

						if(distanceBetweenRooms < bestDistance || !possibleConnectionFound)
						{
							bestDistance = distanceBetweenRooms;
							possibleConnectionFound = true;
							bestTileA = tileA;
							bestTileB = tileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
						}
					}
				}
			}

			if (possibleConnectionFound)
				CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
		}
	}

	public void CreatePassage(Room roomA, Room roomB, Coordinate tileA, Coordinate tileB)
	{
		Room.ConnectRooms(roomA, roomB);
		Debug.DrawLine(CoordinateToWorldPoint(tileA), CoordinateToWorldPoint(tileB), Color.green, 100);
	}

	public Vector3 CoordinateToWorldPoint(Coordinate tile)
	{
		return new Vector3(-Width / 2 + 0.5f + tile.TileX, -Height / 2 + 0.5f + tile.TileY, 2);
	}

    public List<Coordinate> GetRegionTiles(int startX, int startY)
    {
        var tiles = new List<Coordinate>();
        var mapFlags = new bool[Width, Height];
        var tileType = Map[startX, startY];

        var queue = new Queue<Coordinate>();
        queue.Enqueue(new Coordinate(startX, startY));

        mapFlags[startX, startY] = true;

        while (queue.Count > 0)
        {
            var tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = (tile.TileX - 1); x <= (tile.TileX + 1); x++)
            {
                for (int y = (tile.TileY - 1); y <= (tile.TileY + 1); y++)
                {
                    //If tile is in region and is not on a diagonal and it isn't been checked and is of the right type
                    if (IsInMapRange(x, y) && (y == tile.TileY || x == tile.TileX) && !mapFlags[x, y] && Map[x, y] == tileType)
                    {
                        mapFlags[x, y] = true;
                        queue.Enqueue(new Coordinate(x, y));
                    }
                }
            }
        }

        return tiles;
    }

    List<List<Coordinate>> GetRegions(int tileType)
    {
        var regions = new List<List<Coordinate>>();
        var mapFlags = new bool[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (!mapFlags[x, y] && Map[x, y] == tileType)
                {
                    var newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (var tile in newRegion)
                        mapFlags[tile.TileX, tile.TileY] = true;
                }
            }
        }

        return regions;
    }

    public bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

	public void DetermineSurvivingRoomsLandingPads(MeshGenerator meshGenerator)
	{
		foreach (var room in SurvivingRooms)
		{
			foreach (var tile in room.EdgeTiles)
			{
				if (meshGenerator.IsLandingPadSquare(tile.TileX, tile.TileY))
				{
					room.LandingPadTile.TileX = tile.TileX;
					room.LandingPadTile.TileY = tile.TileY;
					break;
				}
			}
		}
	}
}

public struct Coordinate
{
    public int TileX;
    public int TileY;

    public Coordinate(int tileX, int tileY)
    {
        this.TileX = tileX;
        this.TileY = tileY;
    }

	public bool IsValidCoordinate()
	{
		return TileX > 0 && TileY > 0;
	}
}

public class Room
{
	public List<Coordinate> Tiles;
	public List<Coordinate> EdgeTiles;
	public List<Room> NeighbouringRooms;
	public Coordinate LandingPadTile;
	public int RoomSize;

	public Room()
	{
		LandingPadTile = new Coordinate(-1, -1);
	}

	public Room(List<Coordinate> roomTiles, int[,] map)
	{
		Tiles = roomTiles;
		RoomSize = Tiles.Count;
		NeighbouringRooms = new List<Room>();
		EdgeTiles = new List<Coordinate>();
		LandingPadTile = new Coordinate(-1, -1);
		
		foreach(var tile in Tiles)
		{
			for(int x = (tile.TileX - 1); x <= (tile.TileX + 1); x++)
			{
				for(int y = (tile.TileY - 1); y <= (tile.TileY + 1); y++)
				{
					if((x == tile.TileX || y == tile.TileY) && map[x, y] == (int)MapFillValues.FillSpace)
					{
						EdgeTiles.Add(tile);
					}
				}
			}
		}
	}

	public static void ConnectRooms(Room roomA, Room roomB)
	{
		roomA.NeighbouringRooms.Add(roomB);
		roomB.NeighbouringRooms.Add(roomA);
	}

	public bool IsConnected(Room otherRoom)
	{
		return NeighbouringRooms.Contains(otherRoom);
	}
}