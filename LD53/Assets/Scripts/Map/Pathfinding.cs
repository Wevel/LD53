using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
	public readonly int floorNumber;
	public readonly int x;
	public readonly int y;
	public readonly TileState tile;

	// public readonly int hCost;

	public PathNode parent { get; private set; }
	public int gCost { get; private set; }
	public int fCost { get => gCost; } // + hCost; }

	public PathNode(TileState tile, PathNode parent) // , int hCost
	{
		if (tile == null) throw new System.ArgumentNullException(nameof(tile));

		this.floorNumber = tile.FloorNumber;
		this.x = tile.x;
		this.y = tile.y;
		this.tile = tile;
		this.parent = parent;
		// this.hCost = hCost;
	}

	public override bool Equals(object obj)
	{
		// Check for null values and compare run-time types.
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		PathNode other = (PathNode)obj;
		return floorNumber == other.floorNumber && x == other.x && y == other.y;
	}

	public override int GetHashCode()
	{
		return floorNumber.GetHashCode() ^ x.GetHashCode() ^ y.GetHashCode();
	}

	public override string ToString()
	{
		return $"({floorNumber}, {x}, {y})";
	}

	public void UpdateParent(PathNode parent)
	{
		this.parent = parent;
		this.gCost = parent.gCost + 1;
	}
}

public static class Pathfinding
{
	// public delegate int GetHCost(int startX, int startY, int targetX, int targetY);

	// public static int ManhattanDistance(int startX, int startY, int targetX, int targetY)
	// {
	// 	return Mathf.Abs(startX - targetX) + Mathf.Abs(startY - targetY);
	// }

	public static List<PathNode> FindPath(Map map, TileState start, TileState target) // GetHCost getHCost
	{
		// TODO A* pathfinding
		Queue<PathNode> open = new Queue<PathNode>();
		List<PathNode> closed = new List<PathNode>();

		open.Enqueue(new PathNode(start, null));
		PathNode targetNode = new PathNode(target, null);

		void tryQueue(PathNode node)
		{
			if (node.tile == null) return;
			if (node.tile.tileType == TileState.TileType.Wall) return;
			if (closed.Contains(node)) return;
			if (open.Contains(node)) return;
			open.Enqueue(node);
		}

		PathNode current = null;

		while (open.Count > 0)
		{
			current = open.Dequeue();
			closed.Add(current);

			if (current.Equals(targetNode)) break;

			tryQueue(new PathNode(map.GetTile(current.floorNumber, current.x - 1, current.y), current));
			tryQueue(new PathNode(map.GetTile(current.floorNumber, current.x + 1, current.y), current));
			tryQueue(new PathNode(map.GetTile(current.floorNumber, current.x, current.y - 1), current));
			tryQueue(new PathNode(map.GetTile(current.floorNumber, current.x, current.y + 1), current));

			if (target.FloorNumber > current.floorNumber && current.tile.tileType == TileState.TileType.StairsDown) tryQueue(new PathNode(map.GetFloor(current.floorNumber + 1).SpawnTile, current));
		}

		if (current.Equals(targetNode))
		{
			List<PathNode> path = new List<PathNode>();
			while (current != null)
			{
				path.Add(current);
				current = current.parent;
			}
			path.Reverse();

			Debug.Log($"Start {start} to target {target} length {path.Count}");
			return path;

		}
		else
		{
			return null;
		}
	}
}
