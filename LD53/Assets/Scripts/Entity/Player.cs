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
			if (_currentMission != null && _currentMission.target.FloorNumber == map.currentFloor) map.mapTiles[_currentMission.target.x, _currentMission.target.y].SetTarget(false);
			_currentMission = value;
			if (_currentMission != null && _currentMission.target.FloorNumber == map.currentFloor) map.mapTiles[_currentMission.target.x, _currentMission.target.y].SetTarget(true);
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
		if (currentMission == null) MainMenu.instance.MissionSelect(Mission.Outcome.None);
		else if (currentMission.IsSuccess(this)) CompleteMission();

		if (Input.GetButtonDown("Activate"))
		{
			map.ActivateTile(targetX, targetY);
		}
		else if (Time.time - lastMoveTime < moveDelayCurve.Evaluate(consecutiveMoves))
		{
			if (Input.GetButtonDown("Vertical")) moveY = (int)Mathf.Sign(Input.GetAxisRaw("Vertical"));
			else if (Input.GetButtonDown("Horizontal")) moveX = (int)Mathf.Sign(Input.GetAxisRaw("Horizontal"));

			if (moveX != 0 || moveY != 0)
			{
				lastMoveTime = Time.time;
				consecutiveMoves = 0;
			}
		}
		else
		{
			if (Input.GetButton("Vertical")) moveY = (int)Mathf.Sign(Input.GetAxisRaw("Vertical"));
			else if (Input.GetButton("Horizontal")) moveX = (int)Mathf.Sign(Input.GetAxisRaw("Horizontal"));

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
				if (lives == 0) MainMenu.instance.GameOver();
				else MainMenu.instance.MissionSelect(Mission.Outcome.Failure);
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
