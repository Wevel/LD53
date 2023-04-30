using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour
{
	public static Animations instance { get; private set; }

	public AnimationCurve tileJiggleCurve;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}
}
