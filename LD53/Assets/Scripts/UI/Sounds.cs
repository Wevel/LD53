using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sounds : MonoBehaviour
{
	public static Sounds instance { get; private set; }

	public AudioClip[] soundClips;

	private readonly Dictionary<string, AudioClip> soundClipsDict = new Dictionary<string, AudioClip>();

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		if (instance == null) instance = this;
		else Destroy(gameObject);

		foreach (AudioClip clip in soundClips)
		{
			soundClipsDict.Add(clip.name, clip);
			print($"Added sound clip {clip.name}");
		}
	}

	public void PlayClip(string name)
	{
		// if (soundClipsDict.ContainsKey(name))
		// {
		// 	AudioSource.PlayClipAtPoint(soundClipsDict[name], Camera.main.transform.position);
		// }
		// else
		// {
		// 	Debug.LogWarning($"Sound clip {name} not found!");
		// }
	}
}
