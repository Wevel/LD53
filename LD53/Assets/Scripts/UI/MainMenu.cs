using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public Map mainMenuMapPrefab;
	public Map mainGameMapPrefab;

	private Map currentMap = null;

	void Start()
	{
		StartMenu();
		//StartGame();
	}

	public void StartMenu()
	{
		if (currentMap != null) Destroy(currentMap.gameObject);
		currentMap = Instantiate(mainMenuMapPrefab, transform);
		currentMap.GenerateMap(generateOverTime: true);
	}

	public void StartGame()
	{
		if (currentMap != null) Destroy(currentMap.gameObject);
		currentMap = Instantiate(mainGameMapPrefab, transform);
		currentMap.GenerateMap(0);
	}
}
