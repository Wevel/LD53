using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
	public float startingMoveDelay = 0.5f;
	public float minMoveDelay = 0.1f;
	public int maxMoveSteps = 5;

	protected float lastMoveTime;
	protected int consecutiveMoves;
	protected float currentMoveDelay;

	protected override void Start()
	{
		base.Start();
		SetValue('@');
		lastMoveTime = Time.time;
		consecutiveMoves = 0;
		currentMoveDelay = startingMoveDelay;
	}

	protected override void Update()
	{
		// Check for input
		moveX = 0;
		moveY = 0;

		if (MainMenu.instance.paused) return;
		if (!map.GetFloor(map.currentFloor).generated) return;

		if (Input.GetButtonDown("Activate"))
		{
			map.ActivateTile(targetX, targetY);
		}
		else if (Time.time - lastMoveTime < currentMoveDelay)
		{
			if (Input.GetButtonDown("Vertical")) moveY = (int)Mathf.Sign(Input.GetAxisRaw("Vertical"));
			else if (Input.GetButtonDown("Horizontal")) moveX = (int)Mathf.Sign(Input.GetAxisRaw("Horizontal"));

			if (moveX != 0 || moveY != 0)
			{
				lastMoveTime = Time.time;
				currentMoveDelay = startingMoveDelay;
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

				if (consecutiveMoves < maxMoveSteps) consecutiveMoves++;
				currentMoveDelay = Mathf.Lerp(startingMoveDelay, minMoveDelay, consecutiveMoves / (float)maxMoveSteps);
			}
			else
			{
				consecutiveMoves = 0;
			}
		}
	}

	public override void SetPosition(int x, int y)
	{
		base.SetPosition(x, y);
		map.UpdateVisibility(x, y, closeViewRange, farViewRange);
	}
}
