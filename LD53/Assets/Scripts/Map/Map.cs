using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
	public MapTile mapTilePrefab;
	public Player playerPrefab;
	public TargetEntity targetPrefab;
	public int width = 50;
	public int height = 50;

	public bool seeAll = false;

	public MapTile[,] mapTiles;
	public Player player { get; private set; }

	private readonly List<Floor> floors = new List<Floor>();
	public int currentFloor { get; private set; } = -1;
	private int seedOffset = 0;

	// Start is called before the first frame update
	void Awake()
	{
		width = Mathf.CeilToInt(Screen.width / mapTilePrefab.rectTransform.sizeDelta.x);
		height = Mathf.CeilToInt(Screen.height / mapTilePrefab.rectTransform.sizeDelta.y);
		mapTiles = new MapTile[width, height];

		Camera.main.orthographicSize = Screen.height / 2;

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				mapTiles[x, y] = Instantiate(mapTilePrefab, transform);
				mapTiles[x, y].map = this;
				mapTiles[x, y].SetPosition(x, y);
				mapTiles[x, y].SetState(TileState.TileType.Wall);
				mapTiles[x, y].SetVisible(false);
			}
		}

		player = CreateEntity(width / 2, height / 2, playerPrefab) as Player;
	}

	// Update is called once per frame
	void Update()
	{
		if (player != null && !MainMenu.instance.paused)
		{
			if (player.moveX != 0 || player.moveY != 0) Tick();
		}
	}

	public Entity CreateEntity(int x, int y, Entity prefab)
	{
		Entity entity = Instantiate(prefab, transform);
		entity.map = this;
		entity.SetStartPosition(x, y);
		return entity;
	}

	public void GenerateFloor(bool generateOverTime = false)
	{
		Floor floor = new Floor(this, width, height, floors.Count, floors.Count + seedOffset);

		if (generateOverTime) floor.GenerateOverTime();
		else floor.Generate();

		floors.Add(floor);
	}

	public void NextFloor(bool generateOverTime = false)
	{
		if (currentFloor >= 0) floors[currentFloor].Hide();

		GenerateFloor(generateOverTime);
		currentFloor++;

		Debug.Log($"Current floor: {currentFloor}");

		floors[currentFloor].ReDraw();
		ClearVisibility();
		player.SetStartPosition(floors[currentFloor].SpawnTile);
	}

	public void GenerateMap(int mapSeed = -1, bool generateOverTime = false)
	{
		seedOffset = mapSeed;
		NextFloor(generateOverTime);
		Tick();
	}

	public void ClearVisibility()
	{
		foreach (MapTile tile in mapTiles)
		{
			tile.SetVisible(false);
		}
	}

	public void UpdateVisibility(int x, int y, int closeViewRange, int farViewRange)
	{
		int distance;
		for (int x1 = x - farViewRange; x1 <= x + farViewRange; x1++)
		{
			for (int y1 = y - farViewRange; y1 <= y + farViewRange; y1++)
			{
				if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
				{
					distance = Mathf.Abs(x1 - x) + Mathf.Abs(y1 - y);
					if (distance <= closeViewRange) mapTiles[x1, y1].SetVisible(true);
					else if (LineOfSight(x, y, x1, y1, farViewRange)) mapTiles[x1, y1].SetVisible(true);
				}
			}
		}
	}

	public void Tick()
	{
		floors[currentFloor].Tick();
	}

	public void ActivateTile(int x, int y)
	{
		floors[currentFloor].ActivateTile(x, y);
	}

	public bool LineOfSight(int x0, int y0, int x1, int y1, int viewRange)
	{
		int dx = x0 - x1;
		int dy = y0 - y1;
		int viewRangeSquared = viewRange * viewRange;

		if ((dx * dx) + (dy * dy) < viewRangeSquared)
		{
			int nx = Mathf.Abs(dx);
			int ny = Mathf.Abs(dy);
			int signX = (int)Mathf.Sign(dx);
			int signY = (int)Mathf.Sign(dy);

			int ix = 0;
			int iy = 0;

			int x = x1;
			int y = y1;

			TileState tile;
			int value;
			while (ix < nx || iy < ny)
			{
				value = (nx * ((2 * iy) + 1)) - (ny * ((2 * ix) + 1));
				if (value == 0)
				{
					x += signX;
					y += signY;
					ix++;
					iy++;
				}
				else if (value > 0)
				{
					x += signX;
					ix++;
				}
				else
				{
					y += signY;
					iy++;
				}

				if ((tile = GetTile(currentFloor, x, y)) != null && tile.BlocksView) return false;
			}

			return true;
		}

		return false;
	}

	public Floor GetFloor(int floorNumber)
	{
		if (floorNumber < 0 || floorNumber >= floors.Count) return null;
		return floors[floorNumber];
	}

	public TileState GetTile(int floorNumber, int x, int y)
	{
		if (floorNumber < 0 || floorNumber >= floors.Count) return null;
		return floors[floorNumber].GetTile(x, y);
	}
}
