using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = System.Random;

public class MissionGenerator : MonoBehaviour
{
	public static MissionGenerator instance { get; private set; }

	public string[] objectNames;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	public Mission GenerateMission(Map map, TileState start, TileState end, Random random)
	{
		string name = $"x{random.Next(1, 2) + end.FloorNumber} {objectNames[random.Next(objectNames.Length)]}";
		List<PathNode> bestPath = Pathfinding.FindPath(map, start, end);
		if (bestPath == null || bestPath.Count <= 5) return null;

		int pathLength = bestPath.Count;
		int timeLimit = Mathf.CeilToInt(pathLength * Mathf.Max(3f - (end.FloorNumber * 0.2f), 1.1f));
		int score = (5 * end.FloorNumber) + (5 * (end.FloorNumber - start.FloorNumber)) + (timeLimit / 20);

		return new Mission(name, start, end, pathLength, timeLimit, score);
	}
}
