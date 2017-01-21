using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour 
{
	public enum MapFillValues : int
	{
		FillSpace = 1,
		EmptySpace = 0
	}


	private int[,] _map;
	private const int fillSpaceValue = 1;
	private const int emptySpaceValue = 0;

	[Range(0, 100)]
	public int RandomMapFillPercentage;
	public int Height;
	public int Width;
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
				if (neighbourX >= 0 && neighbourX < Width && neighbourY >= 0 && neighbourY < Height) 
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

	public void OnDrawGizmos()
	{
		if(_map != null)
		{
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					Gizmos.color = _map[x, y] == (int)MapFillValues.FillSpace ? Color.black : Color.white;

					var position = new Vector3(-Width / 2 + x + 0.5f, 0, -Height / 2 + y + 0.5f);
					Gizmos.DrawCube (position, Vector3.one);
				}
			}
		}
	}
}