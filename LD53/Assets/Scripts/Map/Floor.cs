using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = System.Random;

public class Floor
{
	public readonly Map map;
	public readonly int width;
	public readonly int height;
	public readonly int floorNumber;
	public readonly int seed;

	public bool generated { get; private set; } = false;

	private readonly Player player;
	private readonly TileState[,] mapTiles;
	private readonly List<Entity> entities = new List<Entity>();
	private readonly List<Vector2Int> stairs = new List<Vector2Int>();
	private readonly List<Vector2Int> targetLocations = new List<Vector2Int>();

	private Random missionRandom;

	public IEnumerable<Entity> Entities => entities;
	public IEnumerable<Vector2Int> Targets => targetLocations;
	public IEnumerable<Vector2Int> Stairs => stairs;
	public TileState SpawnTile => mapTiles[width / 2, height / 2];

	public Floor(Map map, int width, int height, int floorNumber, int seed)
	{
		this.map = map;
		this.width = width;
		this.height = height;
		this.floorNumber = floorNumber;
		this.seed = seed;
		this.player = map.player;

		mapTiles = new TileState[width, height];
	}

	public void Generate()
	{
		static void runCoroutine(IEnumerator enumerator)
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is IEnumerator subCoroutine) runCoroutine(subCoroutine);
			}
		}

		runCoroutine(generateCoroutine());
	}

	public void GenerateOverTime()
	{
		map.StartCoroutine(generateCoroutine());
	}

	private IEnumerator generateCoroutine()
	{
		Random random = new Random(seed);
		missionRandom = new Random(random.Next());

		// Generate tiles
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				mapTiles[x, y] = new TileState(this, x, y, TileState.TileType.Wall);
			}
		}

		yield return new WaitForSeconds(0.05f);

		// Generate random maze
		// Start with a grid full of walls.
		// Pick a cell, mark it as part of the maze. Add the walls of the cell to the wall list.
		// While there are walls in the list:
		// 	Pick a random wall from the list. If only one of the cells that the wall divides is visited, then:
		// 		Make the wall a passage and mark the unvisited cell as part of the maze.
		// 		Add the neighbouring walls of the cell to the wall list.
		// 	Remove the wall from the list.
		List<TileState> openWalls = new List<TileState>() { SpawnTile };

		void queueWall(TileState tile)
		{
			if (tile == null) return;
			if (tile.x == 0 || tile.x == width - 1 || tile.y == 0 || tile.y == height - 1) return;
			if (!openWalls.Contains(tile)) openWalls.Add(tile);
		}

		List<TileState> neighbours = new List<TileState>();
		TileState currentTile;
		int index;

		void queueNeighbour(TileState tile)
		{
			if (tile == null) return;
			if (tile.x == 0 || tile.x == width - 1 || tile.y == 0 || tile.y == height - 1) return;
			if (tile.tileType == TileState.TileType.Wall && !openWalls.Contains(tile)) neighbours.Add(tile);
		}

		while (openWalls.Count > 0)
		{
			index = random.Next(openWalls.Count);
			currentTile = openWalls[index];
			openWalls.RemoveAt(index);

			neighbours.Clear();
			queueNeighbour(GetTile(currentTile.x - 1, currentTile.y));
			queueNeighbour(GetTile(currentTile.x + 1, currentTile.y));
			queueNeighbour(GetTile(currentTile.x, currentTile.y - 1));
			queueNeighbour(GetTile(currentTile.x, currentTile.y + 1));

			if (neighbours.Count >= 3)
			{
				currentTile.Set(TileState.TileType.Empty);
				if (floorNumber == map.currentFloor)
					map.mapTiles[currentTile.x, currentTile.y].SetState(TileState.TileType.Empty);

				queueWall(GetTile(currentTile.x - 1, currentTile.y));
				queueWall(GetTile(currentTile.x + 1, currentTile.y));
				queueWall(GetTile(currentTile.x, currentTile.y - 1));
				queueWall(GetTile(currentTile.x, currentTile.y + 1));
			}

			yield return new WaitForSeconds(0.0005f);
		}

		// Generate rooms
		bool isWall(int x, int y)
		{
			if (x < 1 || x >= width - 1 || y < 1 || y >= height - 1) return false;
			return mapTiles[x, y].tileType == TileState.TileType.Wall;
		}

		IEnumerator fillArea(int x, int y, int width, int height, TileState.TileType tileType)
		{
			for (int x1 = x; x1 < x + width; x1++)
			{
				for (int y1 = y; y1 < y + height; y1++)
				{
					if (isWall(x1, y1))
					{
						mapTiles[x1, y1].Set(tileType);
						yield return new WaitForSeconds(0.002f);

						if (floorNumber == map.currentFloor)
							map.mapTiles[x1, y1].SetState(tileType);
					}
				}
			}
		}

		IEnumerator addRandomRoom(int x, int y)
		{
			yield return addRoom(x, y, random.Next(3, 8), random.Next(3, 8));
		}

		IEnumerator addRandomCorridor(int x, int y)
		{
			int length = random.Next(6, 10);
			if (random.Next(2) == 0) yield return fillArea(x - (length / 2), y, length, 2, TileState.TileType.Empty);
			else yield return fillArea(x, y - (length / 2), 2, length, TileState.TileType.Empty);
		}

		IEnumerator addRoom(int x, int y, int roomWidth, int roomHeight)
		{
			yield return fillArea(x - (roomWidth / 2), y - (roomHeight / 2), roomWidth, roomHeight, TileState.TileType.Empty);
		}

		int numStairs = random.Next(2, 3);
		int numTargets = random.Next(5, 7);
		int numCorridors = random.Next(20, 25);

		for (int i = 0; i < numStairs; i++)
		{
			int x = random.Next(2, width - 2);
			int y = random.Next(2, height - 2);
			mapTiles[x, y].Set(TileState.TileType.StairsDown);
			stairs.Add(new Vector2Int(x, y));
			yield return addRandomRoom(x, y);
		}

		for (int i = 0; i < numTargets; i++)
		{
			int x = random.Next(2, width - 2);
			int y = random.Next(2, height - 2);
			mapTiles[x, y].Set(TileState.TileType.Empty);
			entities.Add(map.CreateEntity(x, y, map.targetPrefab));
			targetLocations.Add(new Vector2Int(x, y));
			yield return addRandomRoom(x, y);
		}

		for (int i = 0; i < numCorridors; i++)
		{
			int x = random.Next(4, width - 4);
			int y = random.Next(4, height - 4);
			mapTiles[x, y].Set(TileState.TileType.Empty);
			yield return addRandomCorridor(x, y);
		}

		yield return addRoom(width / 2, height / 2, 8, 5);

		if (floorNumber == map.currentFloor)
			ReDraw();

		generated = true;
	}

	public void Tick()
	{
		if (IsOpen(player.targetX, player.targetY)) player.DoMove();

		foreach (Entity entity in entities)
		{
			if (IsOpen(entity.targetX, entity.targetY)) entity.DoMove();
		}
	}

	public void Hide()
	{
		// Hide entities
		foreach (Entity entity in entities) entity.gameObject.SetActive(false);
	}

	public void ReDraw()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				map.mapTiles[x, y].SetState(mapTiles[x, y].tileType);
			}
		}

		// Show entities
		foreach (Entity entity in entities) entity.gameObject.SetActive(true);
	}

	public void ActivateTile(int x, int y)
	{
		if (mapTiles[x, y].tileType == TileState.TileType.StairsDown) map.NextFloor();
	}

	public bool IsOpen(int x, int y)
	{
		if (x < 0 || x >= width || y < 0 || y >= height) return false;
		return mapTiles[x, y].tileType != TileState.TileType.Wall;
	}

	public bool IsAreaOpen(int x, int y, int width, int height)
	{
		for (int x1 = x; x1 < x + width; x1++)
		{
			for (int y1 = y; y1 < y + height; y1++)
			{
				if (!IsOpen(x1, y1)) return false;
			}
		}
		return true;
	}

	public TileState GetTile(int x, int y)
	{
		if (x < 0 || x >= width || y < 0 || y >= height) return null;
		return mapTiles[x, y];
	}

	public List<Mission> GetPossibleMissions()
	{
		List<Mission> possibleMissions = new List<Mission>();

		// Add target missions
		Mission newMission;
		foreach (Vector2Int targetLocation in targetLocations)
		{
			if (targetLocation.x == map.player.x && targetLocation.y == map.player.y) continue;
			newMission = MissionGenerator.instance.GenerateMission(map, GetTile(map.player.x, map.player.y), GetTile(targetLocation.x, targetLocation.y), missionRandom);
			if (newMission != null) possibleMissions.Add(newMission);
		}

		return possibleMissions;
	}

	public List<Mission> GetMissions()
	{
		List<Mission> possibleMissions = GetPossibleMissions();

		// Randomly include a missions on the next floor
		List<Mission> nextFloorMissions = map.GetFloor(map.currentFloor + 1).GetPossibleMissions();
		int maxNextFloorMissions = Mathf.Min(map.player.completedMissionsThisFloor, nextFloorMissions.Count);
		for (int i = 0; i < maxNextFloorMissions; i++) possibleMissions.Add(Mission.GetRandom(nextFloorMissions, missionRandom));

		if (possibleMissions.Count == 0) return new List<Mission>();

		// Pick 3 random missions
		List<Mission> missions = new List<Mission>();
		for (int i = 0; i < 3; i++)
		{
			if (possibleMissions.Count == 0) break;
			missions.Add(Mission.GetRandom(possibleMissions, missionRandom));
		}

		return missions;
	}
}
