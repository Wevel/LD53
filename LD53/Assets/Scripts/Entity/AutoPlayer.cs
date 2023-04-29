using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlayer : Player
{
	private List<PathNode> path = null;

	protected override void Update()
	{
		moveX = 0;
		moveY = 0;

		if (!map.GetFloor(map.currentFloor).generated) return;

		if (path == null || path.Count == 0)
		{
			if (map.GetTile(map.currentFloor, x, y).tileType == TileState.TileType.StairsDown)
			{
				map.NextFloor(generateOverTime: true);
				return;
			}

			// Add all target entities and stairs to possible targets
			List<TileState> possibleTarget = new List<TileState>();

			foreach (Vector2Int target in map.GetFloor(map.currentFloor).Targets)
				possibleTarget.Add(map.GetTile(map.currentFloor, target.x, target.y));

			foreach (Vector2Int stairs in map.GetFloor(map.currentFloor).Stairs)
				possibleTarget.Add(map.GetTile(map.currentFloor, stairs.x, stairs.y));

			// Pick a random target
			TileState targetTile = possibleTarget[Random.Range(0, possibleTarget.Count)];

			// Find path to target
			path = Pathfinding.FindPath(map, map.GetTile(map.currentFloor, x, y), targetTile);

			if (path == null)
			{
				Debug.LogWarning("No valid path");
				return;
			}
		}

		// Move along current path
		if (Time.time - lastMoveTime > minMoveDelay)
		{
			moveX = path[0].x - x;
			moveY = path[0].y - y;
			path.RemoveAt(0);
			lastMoveTime = Time.time;
		}
	}
}
