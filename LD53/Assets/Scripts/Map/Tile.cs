using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class Tile : MonoBehaviour
{
	public enum Layer : int
	{
		Map = 0,
		Entities = 1,
		Player = 2,
		Overlay = 3
	}

	public class LayerState
	{
		public char value = ' ';
		public string standardColour = "";
		public string targetColour = "";

		public string GetString(bool target)
		{
			string colour = target ? targetColour : standardColour;
			return (colour ?? "") + value;
		}
	}

	public RectTransform rectTransform;
	public TextMeshProUGUI text;
	public Map map;
	public bool jiggle = false;
	public float jiggleTime = 0f;

	public int x { get; protected set; }
	public int y { get; protected set; }

	protected LayerState[] layers;
	private bool visible = true;
	private bool target = false;

	private float jiggleSpeed = 2f;
	private float jiggleScale = 15f;

	private void Awake()
	{
		RectTransform thisRectTransform = GetComponent<RectTransform>();
		thisRectTransform.sizeDelta = rectTransform.sizeDelta;
		layers = new LayerState[4];
		SetJiggle(false);
	}

	void Update()
	{
		if (layers[(int)Layer.Overlay] == null)
		{
			if (jiggleTime > 0)
			{
				if (jiggleTime > 0) jiggleTime -= Time.deltaTime;
				if (jiggleTime <= 0) rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
				else rectTransform.rotation = Quaternion.Euler(0f, 0f, Animations.instance.tileJiggleCurve.Evaluate(Time.time * jiggleSpeed * 2) * jiggleScale / 2);
			}
			else if (jiggle)
			{
				rectTransform.rotation = Quaternion.Euler(0f, 0f, Animations.instance.tileJiggleCurve.Evaluate(Time.time * jiggleSpeed) * jiggleScale);
			}
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

	public void SetValue(char value, string standardColour, string targetColour, Layer layer)
	{
		layers[(int)layer] = new LayerState { value = value, standardColour = standardColour, targetColour = targetColour };
		UpdateDisplay();
	}

	public void ClearValue(Layer layer)
	{
		layers[(int)layer] = null;
		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		if (map.transitioning) return;

		if (layers[(int)Layer.Overlay] != null && !MainMenu.instance.menuHidden)
		{
			text.text = layers[(int)Layer.Overlay].GetString(target);
			text.enabled = true;
			return;
		}

		for (int i = layers.Length - 2; i >= 0; i--)
		{
			if (layers[i] != null)
			{
				text.text = layers[i].GetString(target);
				text.enabled = visible || map.seeAll || target;
				break;
			}
		}
	}
}
