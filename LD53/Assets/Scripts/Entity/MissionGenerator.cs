using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = System.Random;

public class MissionGenerator : MonoBehaviour
{
	[System.Serializable]
	public struct ItemName
	{
		public string name;
		public float initialWeight;
		public float finalWeight;
		public int minAmount;
		public int maxAmount;

		public float GetWeight(float progress)
		{
			if (initialWeight < finalWeight) return Mathf.Clamp(Mathf.Lerp(initialWeight, finalWeight, progress), initialWeight, finalWeight);
			else return Mathf.Clamp(Mathf.Lerp(initialWeight, finalWeight, progress), finalWeight, initialWeight);
		}

		public float GetQuantity(Random random, int floorNumber)
		{
			if (minAmount == maxAmount) return minAmount;
			else return random.Next(minAmount, maxAmount) + floorNumber;
		}
	}

	public static MissionGenerator instance { get; private set; }

	public ItemName[] itemNames;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	public ItemName GetRandomItem(Random random, float progress)
	{
		float totalWeight = 0;
		foreach (ItemName item in itemNames) totalWeight += item.GetWeight(progress);

		float value = (float)random.NextDouble() * totalWeight;
		foreach (ItemName item in itemNames)
		{
			value -= item.GetWeight(progress);
			if (value <= 0) return item;
		}

		return new ItemName() { name = "!!?@&&???!!", initialWeight = 0, finalWeight = 0 };
	}

	public Mission GenerateMission(Map map, TileState start, TileState end, Random random)
	{
		ItemName item = GetRandomItem(random, end.FloorNumber / 10f);
		string name = $"{item.name} x{item.GetQuantity(random, end.FloorNumber)}";
		List<PathNode> bestPath = Pathfinding.FindPath(map, start, end);
		if (bestPath == null || bestPath.Count <= 8) return null;

		int pathLength = bestPath.Count;
		int timeLimit = Mathf.CeilToInt(pathLength * Mathf.Max(3f - (end.FloorNumber * 0.2f), 1.1f));
		int score = (5 * end.FloorNumber) + (5 * (end.FloorNumber - start.FloorNumber)) + (timeLimit / 20);

		return new Mission(name, start, end, pathLength, timeLimit, score);
	}
}
