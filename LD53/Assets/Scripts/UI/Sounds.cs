using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sounds : MonoBehaviour
{
	public static Sounds instance { get; private set; }

	public AudioClip[] soundClips;

	private readonly Dictionary<string, AudioClip> soundClipsDict = new Dictionary<string, AudioClip>();
	private AudioSource audioSource;

	public bool muted { get; private set; } = false;
	public bool muteFX { get; private set; } = true;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		if (instance == null) instance = this;
		else Destroy(gameObject);

		audioSource = GetComponent<AudioSource>();

		foreach (AudioClip clip in soundClips)
		{
			soundClipsDict.Add(clip.name, clip);
			print($"Added sound clip {clip.name}");
		}

		Unmute();
	}

	public void Mute()
	{
		muted = true;
		audioSource.Stop();
		AudioListener.volume = 0f;
	}

	public void Unmute()
	{
		muted = false;
		audioSource.Play();
		AudioListener.volume = 1f;
	}

	public void ToggleMute()
	{
		if (muted) Unmute();
		else Mute();
	}

	public void PlayClip(string name)
	{
		if (!muteFX && !muted)
		{
			if (soundClipsDict.ContainsKey(name))
			{
				AudioSource.PlayClipAtPoint(soundClipsDict[name], Camera.main.transform.position);
			}
			else
			{
				Debug.LogWarning($"Sound clip {name} not found!");
			}
		}
	}
}
