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
	public GameObject Player;
	public GameObject Camera;
	public GameObject Goal;
	public GameObject HealthPickup;
	public GameObject FuelPickup;

	private Room _firstRoom;
	private Room _lastRoom;

	[Range(0, 100)]
	public int RandomMapFillPercentage;
	public int Height;
	public int Width;
	public int BorderSize = 5;
	public int RegionThresholdSize = 15;
	public int EmptyRegionThresholdSize = 10;
	public int NumberOfHealthPickups = 10;
	public int NumberOfFuelPickups = 15;
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

		if (SurvivingRooms != null)
			SurvivingRooms.Clear();

		FillMap();

		for (int i = 0; i < 5; i++)
			SmoothMap();

		ProcessMap();
		if (SurvivingRooms == null)
			return;

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

		var usedWorldPosition = PositionStartAndGoalObjects();

		if (HealthPickup == null || FuelPickup == null)
			return;

		PositionRemainingGameObjects(meshGenerator, usedWorldPosition);
	}

	private HashSet<Vector3> PositionStartAndGoalObjects()
	{
		var usedWorldPositions = new HashSet<Vector3>();

		_firstRoom = SurvivingRooms.OrderBy(x => x.RoomOrder).FirstOrDefault(x => x.LandingPadPosition != null);

		if (_firstRoom == null || _firstRoom.LandingPadPosition == null || Player == null || Camera == null)
			return null;
		 
		Player.gameObject.transform.position = _firstRoom.LandingPadPosition.Value;
		usedWorldPositions.Add(new Vector3(_firstRoom.LandingPadPosition.Value.x, _firstRoom.LandingPadPosition.Value.y, _firstRoom.LandingPadPosition.Value.z));
		Camera.gameObject.transform.position = new Vector3(_firstRoom.LandingPadPosition.Value.x, _firstRoom.LandingPadPosition.Value.y, -10);

		_lastRoom = SurvivingRooms.OrderBy(x => x.RoomOrder).LastOrDefault(x => x.LandingPadPosition != null);

		if (_lastRoom == null || _lastRoom.LandingPadPosition == null || Goal == null)
			return null;

		Goal.gameObject.transform.position = _lastRoom.LandingPadPosition.Value;
		usedWorldPositions.Add(new Vector3(_lastRoom.LandingPadPosition.Value.x, _lastRoom.LandingPadPosition.Value.y, _lastRoom.LandingPadPosition.Value.z));

		return usedWorldPositions;
	}

	private void PositionRemainingGameObjects(MeshGenerator meshGenerator, HashSet<Vector3> usedWorldPositions)
	{
		var roomsVisited = new HashSet<int>();

		var RoomsLookup = SurvivingRooms.ToDictionary(x => x.RoomOrder, x => x);

		if (usedWorldPositions == null)
			usedWorldPositions = new HashSet<Vector3>();

		var randomNumberGenerator = new System.Random();

		for (int i = 0; i < NumberOfFuelPickups; i++)
		{
			int roomToPopulate;
			do
			{
				roomToPopulate = (randomNumberGenerator.Next() % SurvivingRooms.Count) + 1;
			} while (!RoomsLookup.ContainsKey(roomToPopulate) || (roomsVisited.Contains(roomToPopulate)));

			var room = RoomsLookup[roomToPopulate];

			Vector3? position;
			do
			{
				var tile = room.EmptySpaceTiles[randomNumberGenerator.Next() % room.EmptySpaceTiles.Count];
				position = meshGenerator.GetSquareNodePosition(tile.TileX, tile.TileY);

			} while (position == null || usedWorldPositions.Contains(position.Value));


			Instantiate(FuelPickup, new Vector3(position.Value.x, position.Value.y, position.Value.z), Quaternion.identity);
			roomsVisited.Add(roomToPopulate);
			usedWorldPositions.Add(position.Value);

			if (roomsVisited.Count == SurvivingRooms.Count)
				roomsVisited.Clear();
		}

		roomsVisited.Clear();

		for (int i = 0; i < NumberOfHealthPickups; i++)
		{
			int roomToPopulate;
			do
			{
				roomToPopulate = (randomNumberGenerator.Next() % SurvivingRooms.Count) + 1;
			} while (!RoomsLookup.ContainsKey(roomToPopulate) || (roomsVisited.Contains(roomToPopulate)));

			var room = RoomsLookup[roomToPopulate];

			Vector3? position;
			do
			{
				var tile = room.EmptySpaceTiles[randomNumberGenerator.Next() % room.EmptySpaceTiles.Count];
				position = meshGenerator.GetSquareNodePosition(tile.TileX, tile.TileY);

			} while (position == null || usedWorldPositions.Contains(position.Value));


			Instantiate(HealthPickup, new Vector3(position.Value.x, position.Value.y, position.Value.z), Quaternion.identity);
			roomsVisited.Add(roomToPopulate);
			usedWorldPositions.Add(position.Value);

			if (roomsVisited.Count == SurvivingRooms.Count)
				roomsVisited.Clear();
		}

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
		var roomCount = 1;

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
				SurvivingRooms.Add(new Room(roomRegion, Map, roomCount));
				roomCount++;
			}
		}

		SurvivingRooms[0].IsMainRoom = true;
		SurvivingRooms[0].IsAccessibleFromMainRoom = true;

		SurvivingRooms.Sort();
		ConnectClosestRooms();
	}

	public void ConnectClosestRooms(bool forceAccessibiltyFromMainRoom = false)
	{
		var roomListA = new List<Room>();
		var roomListB = new List<Room>();

		if(forceAccessibiltyFromMainRoom)
		{
			foreach(var room in SurvivingRooms)
			{
				if (room.IsAccessibleFromMainRoom)
					roomListB.Add(room);
				else
					roomListA.Add(room);
			}
		}
		else
		{
			roomListA = SurvivingRooms;
			roomListB = SurvivingRooms;
		}

		var bestDistance = 0;
		var bestTileA = new Coordinate();
		var bestTileB = new Coordinate();
		var bestRoomA = new Room();
		var bestRoomB = new Room();
		var possibleConnectionFound = false;

		foreach(var roomA in roomListA)
		{
			if (!forceAccessibiltyFromMainRoom)
			{
				possibleConnectionFound = false;

				if (roomA.NeighbouringRooms.Count > 0)
					continue;
			}

			foreach(var roomB in roomListB)
			{
				if (roomA == roomB || roomA.IsConnected(roomB))
					continue;

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

			if (possibleConnectionFound && !forceAccessibiltyFromMainRoom)
				CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
		}


		if(!forceAccessibiltyFromMainRoom)
		{
			ConnectClosestRooms(true);
		}
		else if(possibleConnectionFound && forceAccessibiltyFromMainRoom)
		{
			CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			ConnectClosestRooms(true);
		}
	}

	public void CreatePassage(Room roomA, Room roomB, Coordinate tileA, Coordinate tileB)
	{
		Room.ConnectRooms(roomA, roomB);

		var line = GetLine(tileA, tileB);

		foreach(var coordinate in line)
		{
			DrawCircle(coordinate, 1);
		}
	}

	public void DrawCircle(Coordinate center, int radius)
	{
		for(int x = -radius; x <= radius; x++)
		{
			for(int y = -radius; y <= radius; y++)
			{
				if( (x * x) + (y * y) <= (radius * radius))
				{
					var drawX = center.TileX + x;
					var drawY = center.TileY + y;

					if (IsInMapRange(drawX, drawY))
						Map[drawX, drawY] = 0;
				}
			}
		}
	}

	List<Coordinate> GetLine(Coordinate start, Coordinate end)
	{
		var line = new List<Coordinate>();
		var xValue = start.TileX;
		var yValue = start.TileY;

		var dx = end.TileX - start.TileX;
		var dy = end.TileY - start.TileY;

		bool inverted = false;
		var step = Math.Sign(dx);
		var gradientStep = Math.Sign(dy);

		var longest = Mathf.Abs(dx);
		var shortest = Mathf.Abs(dy);

		if(longest < shortest)
		{
			inverted = true;
			longest = Mathf.Abs(dy);
			shortest = Mathf.Abs(dx);

			step = Math.Sign(dy);
			gradientStep = Math.Sign(dx);
		}

		var gradientAccumulation = longest / 2;
		
		for(int i = 0; i < longest; i++)
		{
			line.Add(new Coordinate(xValue, yValue));

			if (inverted)
				yValue += step;
			else
				xValue += step;

			gradientAccumulation += shortest;

			if(gradientAccumulation >= longest)
			{
				if (inverted)
					xValue += gradientStep;
				else
					yValue += gradientStep;

				gradientAccumulation -= longest;
			}
		}

		return line;
	}

	public Vector3 CoordinateToWorldPoint(Coordinate tile)
	{
		return new Vector3(-Width / 2 + 0.5f + tile.TileX, -Height / 2 + 0.5f + tile.TileY, 0);
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
		var foundLandingPad = false;

		foreach (var room in SurvivingRooms)
		{
			foundLandingPad = false;

			foreach (var tile in room.EdgeTiles)
			{
				if (foundLandingPad)
					continue;
				var result = meshGenerator.GetLandingPadPosition(tile.TileX, tile.TileY);

				if(result != null)
				{
					foundLandingPad = true;
					room.LandingPadPosition = result.Value;
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

public class Room : IComparable<Room>
{
	public List<Coordinate> Tiles;
	public List<Coordinate> EdgeTiles;
	public List<Coordinate> EmptySpaceTiles;
	public List<Room> NeighbouringRooms;
	public Vector3? LandingPadPosition;
	public int RoomSize;
	public int RoomOrder = 0;
	public bool IsAccessibleFromMainRoom;
	public bool IsMainRoom;

	public Room()
	{
	}

	public Room(List<Coordinate> roomTiles, int[,] map, int roomOrder)
	{
		Tiles = roomTiles;
		RoomSize = Tiles.Count;
		RoomOrder = roomOrder;
		NeighbouringRooms = new List<Room>();
		EdgeTiles = new List<Coordinate>();
		EmptySpaceTiles = new List<Coordinate>();
		
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
					else if((x == tile.TileX || y == tile.TileY) && map[x, y] == (int)MapFillValues.EmptySpace)
					{
						EmptySpaceTiles.Add(tile);
					}
				}
			}
		}
	}

	public void SetAccessibleFromMainRoom()
	{
		if(!IsAccessibleFromMainRoom)
		{
			IsAccessibleFromMainRoom = true;
			foreach(var neighbouringRoom in NeighbouringRooms)
			{
				neighbouringRoom.SetAccessibleFromMainRoom();
			}
		}
	}

	public static void ConnectRooms(Room roomA, Room roomB)
	{
		if (roomA.IsAccessibleFromMainRoom)
			roomB.SetAccessibleFromMainRoom();
		else if (roomB.IsAccessibleFromMainRoom)
			roomA.SetAccessibleFromMainRoom();

		roomA.NeighbouringRooms.Add(roomB);
		roomB.NeighbouringRooms.Add(roomA);
	}

	public bool IsConnected(Room otherRoom)
	{
		return NeighbouringRooms.Contains(otherRoom);
	}

	public int CompareTo(Room otherRoom)
	{
		return otherRoom.RoomSize.CompareTo(RoomSize);
	}
}