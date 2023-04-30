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
	public int missionButtonWidth = 35;
	public string startButtonText = "Play";
	public string restartButtonText = "Restart";
	public string muteButtonText = "Mute";
	public string unMuteButtonText = "Unmute";
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
				Sounds.instance.PlayClip("UI_Interact");
			}
		}

		if (menuButtons.Count > 0)
		{
			if (UserInput.instance.VerticalDown)
			{
				currentButton -= UserInput.instance.Vertical;
				currentButton = Mathf.Clamp(currentButton, 0, menuButtons.Count - 1);
				updateSelectedButton();
				Sounds.instance.PlayClip("UI_Interact");
			}
			else if (Input.GetButtonDown("Submit"))
			{
				menuButtons[currentButton]();
				Sounds.instance.PlayClip("UI_Interact");
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
					Sounds.instance.PlayClip("UI_Interact");
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
				Sounds.instance.PlayClip("UI_Interact");
			}
			else
			{
				menuHidden = false;
			}
		}

		updateScore();
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
		currentMap.StartTransition();

		clearMenu();
		addButton(startButtonText, StartGame);
		if (Sounds.instance.muted) addButton(unMuteButtonText, () => { UnmuteGame(); showStartMenuButtons(); currentButton = 1; updateSelectedButton(); });
		else addButton(muteButtonText, () => { MuteGame(); showStartMenuButtons(); currentButton = 1; updateSelectedButton(); });
#if UNITY_STANDALONE && !UNITY_EDITOR
		addButton(exitButtonText, ExitGame);
#endif
		updateSelectedButton();

		currentMap.EndTransition();
	}

	public void PauseMenu()
	{
		Debug.Log("Pause Menu");
		state = MenuState.Pause;

		showPauseMenuButtons();
	}

	private void showPauseMenuButtons()
	{
		currentMap.StartTransition();

		clearMenu();
		updateScore();
		addButton(resumeButtonText, ResumeGame);
		addButton(restartButtonText, StartGame);
		if (Sounds.instance.muted) addButton(unMuteButtonText, () => { UnmuteGame(); showPauseMenuButtons(); currentButton = 2; updateSelectedButton(); });
		else addButton(muteButtonText, () => { MuteGame(); showPauseMenuButtons(); currentButton = 2; updateSelectedButton(); });
		addButton(menuButtonText, StartMenu);
#if UNITY_STANDALONE && !UNITY_EDITOR
		addButton(exitButtonText, ExitGame);
#endif
		updateSelectedButton();

		currentMap.EndTransition();
	}

	public void StartGame()
	{
		Debug.Log("Start Game");

		if (currentMap != null) Destroy(currentMap.gameObject);
		currentMap = Instantiate(mainGameMapPrefab, canvas);
		currentMap.GenerateMap();
		currentMap.GenerateFloor();
		state = MenuState.Running;
	}

	public void ResumeGame()
	{
		currentMap.StartTransition();

		Debug.Log("Resume Game");
		state = MenuState.Running;

		clearMenu();
		updateScore();

		currentMap.EndTransition();
	}

	public void MuteGame()
	{
		Debug.Log("Mute Game");
		Sounds.instance.Mute();
	}

	public void UnmuteGame()
	{
		Debug.Log("Unmute Game");
		Sounds.instance.Unmute();
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
		currentMap.StartTransition();

		clearMenu();
		updateScore();

		if (lastMissionOutcome == Mission.Outcome.Success) setButtonHeader("What'll it be today?", ButtonType.Mission);
		else if (lastMissionOutcome == Mission.Outcome.Failure) setButtonHeader("Not good enough!", ButtonType.Mission);
		else setButtonHeader("What'll you start with?", ButtonType.Mission);

		for (int i = 0; i < possibleMissions.Count; i++)
		{
			int index = i;
			addButton($"[${possibleMissions[index].score}] {possibleMissions[index].name}", () => selectMission(index), ButtonType.Mission);
		}

		updateSelectedButton();

		currentMap.EndTransition();
	}

	private void selectMission(int index)
	{
		currentMap.StartTransition();

		Debug.Log($"Selected Mission {possibleMissions[index].name} ({possibleMissions[index].score}) with length {possibleMissions[index].pathLength} time limit {possibleMissions[index].timeLimit}");
		clearMenu();
		updateScore();
		currentMap.player.StartMission(possibleMissions[index]);
		state = MenuState.Running;

		currentMap.EndTransition();
	}

	public void GameOver()
	{
		Debug.Log("Game Over");
		state = MenuState.GameOver;

		showGameOverButtons();
	}

	private void showGameOverButtons()
	{
		currentMap.StartTransition();

		clearMenu();
		updateScore();
		setButtonHeader($"You're not up to it!  F{currentMap.currentFloor} ${currentMap.player.score}", ButtonType.Mission);
		addButton(restartButtonText, StartGame, ButtonType.Mission);
		addButton(menuButtonText, StartMenu, ButtonType.Mission);
		updateSelectedButton();

		currentMap.EndTransition();
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
				currentMap.mapTiles[x, y].ClearValue(Tile.Layer.Overlay);
			}
		}
	}

	public void updateScore()
	{
		if (currentMap != null && currentMap.player != null)
		{
			string scoreText = $"${currentMap.player.score:000000}";
			string livesText = $"Lives: {currentMap.player.lives:0}";
			string timerText = currentMap.player.timeRemaining != int.MaxValue ? $"Timer: {currentMap.player.timeRemaining:000}" : "";
			string floorText = $"F{currentMap.currentFloor}";

			// Clear row
			for (int x = 2; x < currentMap.width - 2; x++) currentMap.mapTiles[x, currentMap.height - 2].SetValue(' ', "", "", Tile.Layer.Overlay);

			if (currentMap.player is not AutoPlayer)
			{
				// Draw lives in top left
				for (int i = 0; i < livesText.Length; i++) currentMap.mapTiles[i + 4, currentMap.height - 2].SetValue(livesText[i], "", "", Tile.Layer.Overlay);

				// Draw score and timer in top middle
				string text = scoreText + "     " + timerText;
				for (int i = 0; i < text.Length; i++) currentMap.mapTiles[i + (currentMap.width / 2) - 9, currentMap.height - 2].SetValue(text[i], "", "", Tile.Layer.Overlay);
			}

			// Draw floor in top right
			for (int i = 0; i < floorText.Length; i++) currentMap.mapTiles[i + currentMap.width - 7, currentMap.height - 2].SetValue(floorText[i], "", "", Tile.Layer.Overlay);
		}
	}

	private void setButtonHeader(string text, ButtonType type = ButtonType.Standard)
	{
		Debug.Log($"Set button header '{text}' (length {text.Length})");
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
			currentMap.mapTiles[buttonPositionX + i, y + 2].SetValue(' ', "", "", Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y + 1].SetValue('#', "", "", Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y].SetValue(text[i], "", "", Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y - 1].SetValue('#', "", "", Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y - 2].SetValue(' ', "", "", Tile.Layer.Overlay);
		}

		for (int i = 0; i < 5; i++)
		{
			currentMap.mapTiles[buttonPositionX - 1, y + 2 - i].SetValue(' ', "", "", Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + text.Length, y + 2 - i].SetValue(' ', "", "", Tile.Layer.Overlay);
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
			currentMap.mapTiles[buttonPositionX + i, y + 2].SetValue(' ', "", "", Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y + 1].SetValue('#', "", "", Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y].SetValue(text[i], "", "", Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y - 1].SetValue('#', "", "", Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + i, y - 2].SetValue(' ', "", "", Tile.Layer.Overlay);
		}

		for (int i = 0; i < 5; i++)
		{
			currentMap.mapTiles[buttonPositionX - 1, y + 2 - i].SetValue(' ', "", "", Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + text.Length, y + 2 - i].SetValue(' ', "", "", Tile.Layer.Overlay);
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
			currentMap.mapTiles[buttonPositionX, y].SetValue(buttonChar, "<color=#FF6400>", "<color=#FF6400>", Tile.Layer.Overlay);
			currentMap.mapTiles[buttonPositionX + currentButtonWidth - 1, y].SetValue(buttonChar, "<color=#FF6400>", "<color=#FF6400>", Tile.Layer.Overlay);
		}
	}
}
