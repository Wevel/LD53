using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class Tile : MonoBehaviour
{
	public enum Layer : int
	{
		Background = 0,
		Foreground = 1,
		Overlay = 2
	}

	public RectTransform rectTransform;
	public TextMeshProUGUI text;
	public Map map;
	public bool jiggle = false;

	public int x { get; protected set; }
	public int y { get; protected set; }

	protected char[] layers;
	private bool visible = true;
	private bool target = false;

	private float jiggleSpeed = 2f;
	private float jiggleScale = 15f;

	private void Awake()
	{
		RectTransform thisRectTransform = GetComponent<RectTransform>();
		thisRectTransform.sizeDelta = rectTransform.sizeDelta;
		layers = new char[3];
		SetJiggle(false);
	}

	void Update()
	{
		if (jiggle && layers[(int)Layer.Overlay] == '\0')
		{
			rectTransform.rotation = Quaternion.Euler(0f, 0f, Animations.instance.tileJiggleCurve.Evaluate(Time.time * jiggleSpeed) * jiggleScale);
		}
	}

	public void SetJiggle(bool jiggle)
	{
		this.jiggle = jiggle;

		if (jiggle)
		{
			jiggleSpeed = 2f;
			jiggleScale = 15f + Random.Range(-4f, 4f);
			if (Random.value < 0.5) jiggleScale *= -1f;
		}
		else
		{
			rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}
	}

	public void SetTarget(bool target)
	{
		this.target = target;
		visible |= target;
		SetJiggle(target);
		UpdateDisplay();
	}

	public virtual void SetPosition(int x, int y)
	{
		this.x = x;
		this.y = y;
		rectTransform.localPosition = new Vector2(x * rectTransform.sizeDelta.x, y * rectTransform.sizeDelta.y);
	}

	public void SetVisible(bool visible)
	{
		this.visible = visible;
		UpdateDisplay();
	}

	public void SetValue(char value, Layer layer)
	{
		layers[(int)layer] = value;
		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		if (layers[(int)Layer.Overlay] != '\0' && !MainMenu.instance.menuHidden)
		{
			text.text = layers[(int)Layer.Overlay] + "";
			text.enabled = true;
			return;
		}

		for (int i = layers.Length - 2; i >= 0; i--)
		{
			if (layers[i] != '\0')
			{
				text.text = layers[i] + "";
				text.enabled = visible || map.seeAll || target;
				break;
			}
		}
	}
}
