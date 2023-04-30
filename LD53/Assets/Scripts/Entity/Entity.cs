using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
	public Map map;

	public int closeViewRange = 5;
	public int farViewRange = 10;
	public char displayChar;

	public int x { get; protected set; }
	public int y { get; protected set; }

	public int moveX { get; protected set; }
	public int moveY { get; protected set; }

	public int targetX { get => x + moveX; }
	public int targetY { get => y + moveY; }

	protected virtual void Start()
	{
		moveX = 0;
		moveY = 0;
	}

	protected virtual void Update() { }

	public virtual void DoMove()
	{
		SetPosition(targetX, targetY);
	}

	public void SetStartPosition(int x, int y)
	{
		this.x = x;
		this.y = y;
		SetPosition(x, y);
	}

	public void SetStartPosition(TileState tile)
	{
		SetStartPosition(tile.x, tile.y);
	}

	public virtual void SetPosition(int x, int y)
	{
		map.mapTiles[this.x, this.y].SetValue('\0', Tile.Layer.Foreground);
		this.x = x;
		this.y = y;
		map.mapTiles[this.x, this.y].SetValue(displayChar, Tile.Layer.Foreground);
	}
}
