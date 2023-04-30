using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour
{
	public static UserInput instance;

	private int lastVertical = 0;
	private int lastHorizontal = 0;

	public int Vertical { get => Mathf.RoundToInt(Input.GetAxisRaw("Vertical")); }
	public int Horizontal { get => Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")); }

	public bool IsVertical { get => Vertical != 0; }
	public bool IsHorizontal { get => Horizontal != 0; }

	public bool VerticalDown { get => Vertical != 0 && lastVertical == 0; }
	public bool HorizontalDown { get => Horizontal != 0 && lastHorizontal == 0; }

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	void LateUpdate()
	{
		lastVertical = Vertical;
		lastHorizontal = Horizontal;
	}
}
