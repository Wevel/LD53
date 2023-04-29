using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetEntity : Entity
{
	protected override void Start()
	{
		base.Start();
		SetValue('*');
	}

	protected override void Update() { }
}
