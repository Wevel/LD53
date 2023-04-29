using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileState
{
	public enum TileType
	{
		Empty,
		Wall,
		Door,
		StairsUp,
		StairsDown,
	}

	public readonly Floor floor;
	public readonly int x;
	public readonly int y;
	public TileType tileType { get; private set; }

	public int FloorNumber => floor.floorNumber;
	public bool BlocksView => tileType == TileType.Wall || tileType == TileType.Door;

	public TileState(Floor floor, int x, int y, TileType tileType)
	{
		this.floor = floor;
		this.x = x;
		this.y = y;
		Set(tileType);
	}

	public void Set(TileType tileType)
	{
		this.tileType = tileType;
	}
}
