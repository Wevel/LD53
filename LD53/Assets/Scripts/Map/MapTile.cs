using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : Tile
{
	public static readonly Dictionary<TileState.TileType, char> TileMap = new Dictionary<TileState.TileType, char>();

	public const string FloorColour = "<color=#1C3D00>";
	public const string StairActiveColour = "<color=#00FFBA>";

	static MapTile()
	{
		// Default #74FF00
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
			string targetColour = "";
			if (tileType == TileState.TileType.StairsUp || tileType == TileState.TileType.StairsDown) targetColour = StairActiveColour;

			this.tileType = tileType;
			SetValue(value, tileType == TileState.TileType.Empty ? FloorColour : "", targetColour, Layer.Map);
		}
		else
		{
			Debug.LogError("TileType " + tileType + " not found in TileMap");
			text.text = "<color=#FF00FF>?";
		}
	}
}
