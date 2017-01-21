using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapFillValues : int
{
	FillSpace = 1,
	EmptySpace = 0
}

public class MapGenerator : MonoBehaviour 
{
	private int[,] _map;

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
		_map = new int[Width, Height];
		FillMap();

		for(int i = 0; i < 5; i++)
			SmoothMap();

		ProcessMap();

		int[,] borderedMap = new int[Width + BorderSize * 2, Height + BorderSize * 2];

		for(int x = 0; x < borderedMap.GetLength(0); x++)
		{
			for(int y = 0; y < borderedMap.GetLength(1); y++)
			{
				if(x >= BorderSize && x < (Width + BorderSize) && y >= BorderSize && y < (Height + BorderSize))
				{
					borderedMap[x, y] = _map[(x - BorderSize), (y - BorderSize)];
				}
				else
				{
					borderedMap[x, y] = (int)MapFillValues.FillSpace;
				}
			}
		}

		MeshGenerator meshGenerator = GetComponent<MeshGenerator>();
		meshGenerator.GenerateMesh(borderedMap, 1);
	}

	public void FillMap()
	{
		if (UseRandomSeed)
			Seed = Time.time.ToString();

		var pseudoRandomNumberGenerator = new System.Random(Seed.GetHashCode());

		for(int x = 0; x < Width; x++)
		{
			for(int y = 0; y < Height; y++)
			{
				_map[x, y] = (pseudoRandomNumberGenerator.Next(0, 100) < RandomMapFillPercentage || (x == 0 || x == (Width - 1) || y == 0 || y == (Height - 1)))  ? (int)MapFillValues.FillSpace : (int)MapFillValues.EmptySpace;
			}
		}
		
	}

	public void SmoothMap()
	{
		for(int x = 0; x < Width; x++)
		{
			for(int y = 0; y < Height; y++)
			{
				var neighbouringFilledSpaces = GetSurroundingWallCount(x, y);

				if (neighbouringFilledSpaces > 4)
					_map[x, y] = (int)MapFillValues.FillSpace;
				else if (neighbouringFilledSpaces < 4)
					_map[x, y] = (int)MapFillValues.EmptySpace;
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
						wallCount += _map [neighbourX, neighbourY];
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

		foreach(var region in wallRegions)
		{
			if(region.Count < RegionThresholdSize)
			{
				foreach(var tile in region)
				{
					_map[tile.TileX, tile.TileY] = (int)MapFillValues.EmptySpace;
				}
			}
		}

		var roomRegions = GetRegions((int)MapFillValues.EmptySpace);

		foreach (var roomRegion in roomRegions)
		{
			if (roomRegion.Count < EmptyRegionThresholdSize)
			{
				foreach (var roomtile in roomRegion)
				{
					_map[roomtile.TileX, roomtile.TileY] = (int)MapFillValues.FillSpace;
				}
			}
		}
	}

	public void OnDrawGizmos()
	{
		if(_map != null)
		{
			//for (int x = 0; x < Width; x++)
			//{
			//	for (int y = 0; y < Height; y++)
			//	{
			//		Gizmos.color = _map[x, y] == (int)MapFillValues.FillSpace ? Color.black : Color.white;

			//		var position = new Vector3(-Width / 2 + x + 0.5f, 0, -Height / 2 + y + 0.5f);
			//		Gizmos.DrawCube (position, Vector3.one);
			//	}
			//}
		}
	}

	List<Coordinate> GetRegionTiles(int startX, int startY)
	{
		var tiles = new List<Coordinate>();
		var mapFlags = new bool[Width, Height];
		var tileType = _map[startX, startY];

		var queue = new Queue<Coordinate>();
		queue.Enqueue(new Coordinate(startX, startY));

		mapFlags[startX, startY] = true;

		while (queue.Count > 0)
		{
			var tile = queue.Dequeue();
			tiles.Add(tile);

			for(int x = (tile.TileX - 1); x <= (tile.TileX + 1); x++ )
			{
				for(int y = (tile.TileY - 1); y <= (tile.TileY + 1); y++)
				{
					//If tile is in region and is not on a diagonal and it isn't been checked and is of the right type
					if(IsInMapRange(x, y) && (y == tile.TileY || x == tile.TileX) && !mapFlags[x, y] && _map[x, y] == tileType)
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

		for(int x = 0; x < Width; x++)
		{
			for(int y = 0; y < Height; y++)
			{
				if(!mapFlags[x,y] && _map[x,y] == tileType)
				{
					var newRegion = GetRegionTiles(x, y);
					regions.Add(newRegion);

					foreach(var tile in newRegion)
						mapFlags[tile.TileX, tile.TileY] = true;
				}
			}
		}

		return regions;
	}

	bool IsInMapRange(int x, int y)
	{
		return x >= 0 && x < Width && y >= 0 && y < Height;
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
}