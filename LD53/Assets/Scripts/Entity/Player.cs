using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
	public int startingLives = 3;
	public AnimationCurve moveDelayCurve;

	public int lives { get; protected set; }
	public int score { get; protected set; } = 0;
	public int completedMissions { get; protected set; } = 0;
	public int completedMissionsThisFloor { get; protected set; } = 0;
	public int timeRemaining { get; protected set; } = int.MaxValue;

	private Mission _currentMission = null;
	public Mission currentMission
	{
		get { return _currentMission; }
		set
		{
			if (_currentMission == value) return;
			_currentMission = value;
			map.ClearTargets();
			map.HighlightTarget(_currentMission.target);
		}
	}

	protected float lastMoveTime;
	protected int consecutiveMoves;

	protected override void Start()
	{
		base.Start();
		lastMoveTime = Time.time;
		consecutiveMoves = 0;
		lives = startingLives;
	}

	protected override void Update()
	{
		// Check for input
		moveX = 0;
		moveY = 0;

		if (MainMenu.instance.paused) return;
		if (!map.GetFloor(map.currentFloor).generated) return;
		if (currentMission == null)
		{
			MainMenu.instance.MissionSelect(Mission.Outcome.None);
		}
		else
		{
			if (currentMission.target.FloorNumber > map.currentFloor)
			{
				if (map.GetTile(map.currentFloor, x, y).tileType == TileState.TileType.StairsDown) map.ActivateTile(x, y);
			}

			if (currentMission.IsSuccess(this)) CompleteMission();
		}

		// if (Input.GetButtonDown("Activate"))
		// {
		// 	map.ActivateTile(targetX, targetY);
		// }
		// else 
		if (Time.time - lastMoveTime < moveDelayCurve.Evaluate(consecutiveMoves))
		{
			if (UserInput.instance.VerticalDown) moveY = UserInput.instance.Vertical;
			else if (UserInput.instance.HorizontalDown) moveX = UserInput.instance.Horizontal;

			if (moveX != 0 || moveY != 0)
			{
				lastMoveTime = Time.time;
				consecutiveMoves = 0;
			}
		}
		else
		{
			if (UserInput.instance.IsVertical) moveY = UserInput.instance.Vertical;
			else if (UserInput.instance.IsHorizontal) moveX = UserInput.instance.Horizontal;

			if (moveX != 0 || moveY != 0)
			{
				lastMoveTime = Time.time;
				consecutiveMoves++;
			}
			else
			{
				consecutiveMoves = 0;
			}
		}
	}

	public override void DoMove()
	{
		base.DoMove();

		if (currentMission != null)
		{
			timeRemaining--;

			if (timeRemaining <= 0)
			{
				lives--;
				Sounds.instance.PlayClip("LoseLife");
				if (lives == 0) MainMenu.instance.GameOver();
				else MainMenu.instance.MissionSelect(Mission.Outcome.Failure);
			}
		}

		for (int px = -1; px <= 1; px++)
		{
			for (int py = -1; py <= 1; py++)
			{
				if (px == 0 && py == 0) continue;
				map.mapTiles[x + px, y + py].jiggleTime = 0.25f;
			}
		}
	}

	public void StartFloor()
	{
		completedMissionsThisFloor = 0;
	}

	public void StartMission(Mission mission)
	{
		currentMission = mission;
		timeRemaining = mission.timeLimit;
	}

	public void CompleteMission()
	{
		if (currentMission != null)
		{
			score += currentMission.score;
			completedMissions++;
			completedMissionsThisFloor++;
		}

		MainMenu.instance.MissionSelect(Mission.Outcome.Success);
	}

	public override void SetPosition(int x, int y)
	{
		base.SetPosition(x, y);
		map.UpdateVisibility(x, y, closeViewRange, farViewRange);
	}
}
