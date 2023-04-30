using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : Tile
{
	public int closeViewRange = 5;
	public int farViewRange = 10;

	public int moveX { get; protected set; }
	public int moveY { get; protected set; }

	public int targetX { get => x + moveX; }
	public int targetY { get => y + moveY; }

	protected virtual void Start()
	{
		SetVisible(true);
		moveX = 0;
		moveY = 0;
	}

	protected abstract void Update();

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

	override public void SetPosition(int x, int y)
	{
		map.mapTiles[x, y].SetVisible(true);
		base.SetPosition(x, y);
		map.mapTiles[x, y].SetVisible(false);
	}
}
