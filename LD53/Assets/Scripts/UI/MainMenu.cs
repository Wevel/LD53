using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public delegate void MenuAction();

	public enum MenuState
	{
		Start,
		Running,
		Pause,
		MissionSelect,
		GameOver,
	}

	public enum ButtonType
	{
		Standard,
		Mission
	}

	public int buttonPositionX = 7;
	public int buttonPositionY = 24;
	public int buttonSpacing = 4;
	public int buttonWidth = 12;
	public int missionButtonWidth = 32;
	public string startButtonText = "Play";
	public string restartButtonText = "Restart";
	public string resumeButtonText = "Resume";
	public string menuButtonText = "Menu";
	public string exitButtonText = "Exit";

	public static MainMenu instance { get; private set; }

	public Map mainMenuMapPrefab;
	public Map mainGameMapPrefab;


	public float gridSize = 25f;
	public float textSize = 23f;

	public RectTransform canvas { get; private set; }

	private Map currentMap = null;

	public MenuState state { get; private set; } = MenuState.Start;
	public bool paused { get => state != MenuState.Running && state != MenuState.Start; }
	public bool started { get => state != MenuState.Start; }

	public bool menuHidden { get; private set; } = false;

	private readonly List<MenuAction> menuButtons = new List<MenuAction>();
	private int currentButton = 0;
	private bool isButtonHeader = false;
	private int currentButtonWidth = 0;

	private List<Mission> possibleMissions;
	private Mission.Outcome lastMissionOutcome = Mission.Outcome.None;

	void Awake()
	{
		// DontDestroyOnLoad(gameObject);
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	void Start()
	{
		canvas = GetComponent<RectTransform>();
		StartMenu();
	}

	void Update()
	{
		if (started)
		{
			if (Input.GetButtonDown("Cancel"))
			{
				if (state == MenuState.Pause) ResumeGame();
				else if (state == MenuState.Running) PauseMenu();
			}
		}

		if (menuButtons.Count > 0)
		{
			if (UserInput.instance.VerticalDown)
			{
				currentButton -= UserInput.instance.Vertical;
				currentButton = Mathf.Clamp(currentButton, 0, menuButtons.Count - 1);
				updateSelectedButton();
			}
			else if (Input.GetButtonDown("Submit"))
			{
				menuButtons[currentButton]();
			}
		}

#if !UNITY_EDITOR
		if (state == MenuState.Start || state == MenuState.GameOver)
#endif
		{
			// Toggle entire map visibility
			if (Input.GetKeyDown(KeyCode.F1))
			{
				if (currentMap != null)
				{
					currentMap.seeAll = !currentMap.seeAll;
					currentMap.GetFloor(currentMap.currentFloor).ReDraw();
				}
			}
		}

		// Toggle menu visibility
		if (Input.GetKeyDown(KeyCode.F2))
		{
			if (currentMap != null)
			{
				menuHidden = !menuHidden;
				currentMap.UpdateDisplay();
			}
			else
			{
				menuHidden = false;
			}
		}
	}

	public void StartMenu()
	{
		Debug.Log("Start Menu");
		state = MenuState.Start;

		if (currentMap != null) Destroy(currentMap.gameObject);
		currentMap = Instantiate(mainMenuMapPrefab, canvas);
		currentMap.GenerateMap(generateOverTime: true);

		showStartMenuButtons();
	}

	private void showStartMenuButtons()
	{
		clearMenu();
		addButton(startButtonText, StartGame);
#if UNITY_STANDALONE && !UNITY_EDITOR
		addButton(exitButtonText, ExitGame);
#endif
		updateSelectedButton();
	}

	public void PauseMenu()
	{
		Debug.Log("Pause Menu");
		state = MenuState.Pause;

		showPauseMenuButtons();
	}

	private void showPauseMenuButtons()
	{
		clearMenu();
		addButton(resumeButtonText, ResumeGame);
		addButton(restartButtonText, StartGame);
#if UNITY_STANDALONE && !UNITY_EDITOR
		addButton(exitButtonText, ExitGame);
#endif
		updateSelectedButton();

	}

	public void StartGame()
	{
		Debug.Log("Start Game");

		if (currentMap != null) Destroy(currentMap.gameObject);
		currentMap = Instantiate(mainGameMapPrefab, canvas);
		currentMap.GenerateMap(0);
		currentMap.GenerateFloor();
		state = MenuState.Running;
	}

	public void ResumeGame()
	{
		Debug.Log("Resume Game");
		state = MenuState.Running;

		clearMenu();
	}

	public void MissionSelect(Mission.Outcome outcome)
	{
		Debug.Log("Mission Select");
		state = MenuState.MissionSelect;

		possibleMissions = currentMap.GetMissions();
		lastMissionOutcome = outcome;

		if (possibleMissions.Count == 0)
		{
			Debug.Log("No missions available");
			GameOver();
			return;
		}

		showMissionSelectButtons();
	}

	private void showMissionSelectButtons()
	{
		clearMenu();

		if (lastMissionOutcome == Mission.Outcome.Success) setButtonHeader("What'll it be today...", ButtonType.Mission);
		else if (lastMissionOutcome == Mission.Outcome.Failure) setButtonHeader("Not good enough...", ButtonType.Mission);
		else setButtonHeader("What'll you start with...", ButtonType.Mission);

		for (int i = 0; i < possibleMissions.Count; i++)
		{
			int index = i;
			addButton($"[#{possibleMissions[index].score}] {possibleMissions[index].name}", () => selectMission(index), ButtonType.Mission);
		}

		updateSelectedButton();
	}

	private void selectMission(int index)
	{
		Debug.Log("Selected Mission " + possibleMissions[index].name);
		clearMenu();
		currentMap.player.StartMission(possibleMissions[index]);
		state = MenuState.Running;
	}

	public void GameOver()
	{
		Debug.Log("Game Over");
		state = MenuState.GameOver;

		showGameOverButtons();
	}

	private void showGameOverButtons()
	{
		clearMenu();
		setButtonHeader($"You're not up to it... {currentMap.player.score}", ButtonType.Mission);
		addButton(restartButtonText, StartGame, ButtonType.Mission);
		addButton(menuButtonText, StartMenu, ButtonType.Mission);
		updateSelectedButton();
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
		isButtonHeader = false;

		for (int x = 0; x < currentMap.width; x++)
		{
			for (int y = 0; y < currentMap.height; y++)
			{
				currentMap.mapTiles[x, y].SetValue('\0', Tile.Layer.Overlay);
			}
		}
	}

	private void setButtonHeader(string text, ButtonType type = ButtonType.Standard)
	{
		int y = buttonPositionY;
		isButtonHeader = true;

		int width = buttonWidth;
		if (type == ButtonType.Mission) width = missionButtonWidth;
		currentButtonWidth = width;

		int leftPadding = (width - text.Length) / 2;
		int rightPadding = width - text.Length - leftPadding;

		text = new string(' ', leftPadding) + text + new string(' ', rightPadding);

		for (int i = 0; i < text.Length; i++)
		{
			currentMap.mapTiles[buttonPositionX + i, y + 2].SetValue(' ', Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y + 1].SetValue('#', Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y].SetValue(text[i], Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y - 1].SetValue('#', Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y - 2].SetValue(' ', Tile.Layer.Overlay);
		}

		for (int i = 0; i < 5; i++)
		{
			currentMap.mapTiles[buttonPositionX - 1, y + 2 - i].SetValue(' ', Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + text.Length, y + 2 - i].SetValue(' ', Tile.Layer.Overlay);
		}
	}

	private void addButton(string text, MenuAction action, ButtonType type = ButtonType.Standard)
	{
		int y = buttonPositionY - (menuButtons.Count * buttonSpacing);
		if (isButtonHeader) y -= buttonSpacing;

		menuButtons.Add(action);

		int width = buttonWidth;
		if (type == ButtonType.Mission) width = missionButtonWidth;
		currentButtonWidth = width;

		int leftPadding = (width - text.Length) / 2;
		int rightPadding = width - text.Length - leftPadding;

		text = new string(' ', leftPadding) + text + new string(' ', rightPadding);

		for (int i = 0; i < text.Length; i++)
		{
			currentMap.mapTiles[buttonPositionX + i, y + 2].SetValue(' ', Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y + 1].SetValue('#', Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y].SetValue(text[i], Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y - 1].SetValue('#', Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y - 2].SetValue(' ', Tile.Layer.Overlay);
		}

		for (int i = 0; i < 5; i++)
		{
			currentMap.mapTiles[buttonPositionX - 1, y + 2 - i].SetValue(' ', Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + text.Length, y + 2 - i].SetValue(' ', Tile.Layer.Overlay);
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
			if (isButtonHeader) y -= buttonSpacing;
			currentMap.mapTiles[buttonPositionX, y].SetValue(buttonChar, Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + currentButtonWidth - 1, y].SetValue(buttonChar, Tile.Layer.Overlay);
		}
	}
}
