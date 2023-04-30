using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
	public Map map;

	public int closeViewRange = 5;
	public int farViewRange = 10;
	public char displayChar;
	public string standardColour;
	public string targetColour;

	public int x { get; protected set; }
	public int y { get; protected set; }

	public int moveX { get; protected set; }
	public int moveY { get; protected set; }

	public int targetX { get => x + moveX; }
	public int targetY { get => y + moveY; }

	public TileState currentTile { get => map.GetTile(map.currentFloor, x, y); }

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
		Hide();
		this.x = x;
		this.y = y;
		Show();
	}

	public void Hide()
	{
		map.mapTiles[x, y].ClearValue(this is Player ? Tile.Layer.Player : Tile.Layer.Entities);
	}

	public void Show()
	{
		map.mapTiles[x, y].SetValue(displayChar, standardColour, targetColour, this is Player ? Tile.Layer.Player : Tile.Layer.Entities);
	}
}
