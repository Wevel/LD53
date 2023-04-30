using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : Tile
{
	public static readonly Dictionary<TileState.TileType, char> TileMap = new Dictionary<TileState.TileType, char>();

	static MapTile()
	{
		TileMap.Add(TileState.TileType.Empty, '.');
		TileMap.Add(TileState.TileType.Wall, '#');
		TileMap.Add(TileState.TileType.Door, '+');
		TileMap.Add(TileState.TileType.StairsUp, '^');
		TileMap.Add(TileState.TileType.StairsDown, 'v');
	}

	public TileState.TileType tileType { get; private set; }

	public void SetState(TileState.TileType tileType)
	{
		if (TileMap.TryGetValue(tileType, out char value))
		{
			this.tileType = tileType;
			SetValue(value, Layer.Background);
		}
		else
		{
			Debug.LogError("TileType " + tileType + " not found in TileMap");
			text.text = "<color=#FF00FF>?";
		}
	}
}
