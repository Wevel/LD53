using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class Tile : MonoBehaviour
{
	public RectTransform rectTransform;
	public TextMeshProUGUI text;
	public Map map;

	public int x { get; protected set; }
	public int y { get; protected set; }

	private void Awake()
	{
		RectTransform thisRectTransform = GetComponent<RectTransform>();
		thisRectTransform.sizeDelta = rectTransform.sizeDelta;
	}

	// void Start()
	// {
	// 	Jiggle(10f);
	// }

	public virtual void SetPosition(int x, int y)
	{
		this.x = x;
		this.y = y;
		rectTransform.localPosition = new Vector2(x * rectTransform.sizeDelta.x, y * rectTransform.sizeDelta.y);
	}

	public void SetVisible(bool visible)
	{
		if (map.seeAll) visible = true;
		text.enabled = visible;
	}

	public void SetValue(char value)
	{
		text.text = value + "";
	}

	public void Jiggle(float duration = 0.1f)
	{
		IEnumerator jiggleCoroutine()
		{
			// Make random movement to rotation for duration
			float time = 0f;

			IEnumerator moveFrame(float amount)
			{
				time += Time.deltaTime;
				rectTransform.rotation = Quaternion.Euler(0f, 0f, amount);
				yield return null;
			}

			float speed = 8f + Random.Range(-4f, 4f);
			if (Random.value < 0.5) speed *= -1f;

			while (time < duration)
			{
				yield return moveFrame(speed);
				yield return moveFrame(speed);
				yield return moveFrame(speed);
				yield return moveFrame(speed);
				yield return moveFrame(-speed);
				yield return moveFrame(-speed);
				yield return moveFrame(-speed);
				yield return moveFrame(-speed);
				yield return moveFrame(-speed);
				yield return moveFrame(-speed);
				yield return moveFrame(-speed);
				yield return moveFrame(-speed);
				yield return moveFrame(speed);
				yield return moveFrame(speed);
				yield return moveFrame(speed);
				yield return moveFrame(speed);
			}

			// Reset rotation
			rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}

		StartCoroutine(jiggleCoroutine());
	}
}
