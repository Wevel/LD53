using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public delegate void MenuAction();

	public int buttonPositionX = 7;
	public int buttonPositionY = 12;
	public int buttonSpacing = 4;
	public int buttonWidth = 12;
	public string startButtonText = "Play";
	public string restartButtonText = "Restart";
	public string resumeButtonText = "Resume";
	public string menuButtonText = "Menu";
	public string exitButtonText = "Exit";

	public static MainMenu instance { get; private set; }

	public Map mainMenuMapPrefab;
	public Map mainGameMapPrefab;

	public RectTransform canvas;

	private Map currentMap = null;

	public bool paused { get; private set; } = false;
	public bool started { get; private set; } = false;

	private readonly List<MenuAction> menuButtons = new List<MenuAction>();
	private int currentButton = 0;

	void Awake()
	{
		// DontDestroyOnLoad(gameObject);
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	void Start()
	{
		StartMenu();
	}

	void Update()
	{
		if (started)
		{
			if (Input.GetButtonDown("Cancel"))
			{
				if (paused) ResumeGame();
				else PauseMenu();
			}
		}

		if (menuButtons.Count > 0)
		{
			if (Input.GetButtonDown("Vertical"))
			{
				currentButton -= (int)Mathf.Sign(Input.GetAxisRaw("Vertical"));
				currentButton = Mathf.Clamp(currentButton, 0, menuButtons.Count - 1);
				updateSelectedButton();
			}
			else if (Input.GetButtonDown("Submit"))
			{
				menuButtons[currentButton]();
			}
		}
	}

	public void StartMenu()
	{
		Debug.Log("Start Menu");

		if (currentMap != null) Destroy(currentMap.gameObject);
		currentMap = Instantiate(mainMenuMapPrefab, canvas);
		currentMap.GenerateMap(generateOverTime: true);

		clearMenu();
		addButton(startButtonText, StartGame);
#if UNITY_STANDALONE && !UNITY_EDITOR
		addButton(exitButtonText, ExitGame);
#endif
		updateSelectedButton();

		paused = false;
		started = false;
	}

	public void PauseMenu()
	{
		Debug.Log("Pause Menu");

		clearMenu();
		addButton(resumeButtonText, ResumeGame);
		addButton(restartButtonText, StartGame);
#if UNITY_STANDALONE && !UNITY_EDITOR
		addButton(exitButtonText, ExitGame);
#endif
		updateSelectedButton();

		paused = true;
	}

	public void StartGame()
	{
		Debug.Log("Start Game");

		if (currentMap != null) Destroy(currentMap.gameObject);
		currentMap = Instantiate(mainGameMapPrefab, canvas);
		currentMap.GenerateMap(0);
		paused = false;
		started = true;
	}

	public void ResumeGame()
	{
		Debug.Log("Resume Game");

		clearMenu();
		paused = false;
	}

	public void ExitGame()
	{
		Debug.Log("Exit Game");

		Application.Quit();
	}

	private void clearMenu()
	{
		menuButtons.Clear();
		currentButton = 0;

		for (int x = 0; x < currentMap.width; x++)
		{
			for (int y = 0; y < currentMap.height; y++)
			{
				currentMap.mapTiles[x, y].SetValue('\0', front: true);
			}
		}
	}

	private void addButton(string text, MenuAction action)
	{
		int y = buttonPositionY - (menuButtons.Count * buttonSpacing);
		menuButtons.Add(action);

		int leftPadding = (buttonWidth - text.Length) / 2;
		int rightPadding = buttonWidth - text.Length - leftPadding;

		text = new string(' ', leftPadding) + text + new string(' ', rightPadding);

		for (int i = 0; i < text.Length; i++)
		{
			currentMap.mapTiles[buttonPositionX + i, y + 2].SetValue(' ', front: true);
			currentMap.mapTiles[buttonPositionX + i, y + 1].SetValue('#', front: true);
			currentMap.mapTiles[buttonPositionX + i, y].SetValue(text[i], front: true);
			currentMap.mapTiles[buttonPositionX + i, y - 1].SetValue('#', front: true);
			currentMap.mapTiles[buttonPositionX + i, y - 2].SetValue(' ', front: true);
		}

		for (int i = 0; i < 5; i++)
		{
			currentMap.mapTiles[buttonPositionX - 1, y + 2 - i].SetValue(' ', front: true);
			currentMap.mapTiles[buttonPositionX + text.Length, y + 2 - i].SetValue(' ', front: true);
		}
	}

	private void updateSelectedButton()
	{
		char buttonChar;
		int y;
		for (int i = 0; i < menuButtons.Count; i++)
		{
			if (i == currentButton) buttonChar = '*';
			else buttonChar = ' ';

			y = buttonPositionY - (i * buttonSpacing);
			currentMap.mapTiles[buttonPositionX, y].SetValue(buttonChar, front: true);
			currentMap.mapTiles[buttonPositionX + buttonWidth - 1, y].SetValue(buttonChar, front: true);
		}
	}
}
