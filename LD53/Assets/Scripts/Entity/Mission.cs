using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = System.Random;

public class Mission
{
	public enum Outcome
	{
		None,
		Success,
		Failure
	}

	public readonly string name;
	public readonly TileState initialPosition;
	public readonly TileState target;
	public readonly int pathLength;
	public readonly int timeLimit;
	public readonly int score;

	public Mission(string name, TileState initialPosition, TileState target, int pathLength, int timeLimit, int score)
	{
		this.name = name;
		this.initialPosition = initialPosition;
		this.target = target;
		this.pathLength = pathLength;
		this.timeLimit = timeLimit;
		this.score = score;
	}

	public bool IsSuccess(Player player)
	{
		return player.x == target.x && player.y == target.y;
	}

	public static Mission GetRandom(List<Mission> missions, Random random)
	{
		int index = random.Next(missions.Count);
		Mission mission = missions[index];
		missions.RemoveAt(index);
		return mission;
	}
}
