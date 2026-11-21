using System.Collections.Generic;
using UnityEngine;

public class AudioMgr : MonoBehaviour
{
	public int nAudioPoolCount = 5;

	[SerializeField]
	private AudioInfoDictionary audioInfos;

	private string sKeyMusic = "";

	private int nCurPlayingIndex = -1;

	private IList<AudioSource> asSounds = new List<AudioSource>();

	private IDictionary<string, AudioSource> asLoops = new Dictionary<string, AudioSource>();

	private AudioSource asMusic;

	private static AudioMgr instance;

	public bool IsMusicOn { get; private set; }

	public bool IsSoundOn { get; private set; }

	public static AudioMgr I => instance;

	private void Awake()
	{
		instance = this;
		GameObject gameObject = new GameObject("music");
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		asMusic = gameObject.AddComponent<AudioSource>();
		for (int i = 0; i < nAudioPoolCount; i++)
		{
			GameObject gameObject2 = new GameObject("sound_" + i);
			gameObject2.transform.SetParent(base.transform, worldPositionStays: false);
			asSounds.Add(gameObject2.AddComponent<AudioSource>());
		}
	}

	public void Init()
	{
		IsMusicOn = DB.User.BMusic;
		IsSoundOn = DB.User.BSound;
	}

	public void SetMusicOn(bool b_open)
	{
		if (IsMusicOn == b_open)
		{
			return;
		}
		IsMusicOn = b_open;
		if (!b_open)
		{
			asMusic.Pause();
		}
		else if (!asMusic.isPlaying)
		{
			_PlayMusic();
		}
		else
		{
			asMusic.UnPause();
		}
	}

	public void SetSoundOn(bool b_open)
	{
		if (IsSoundOn != b_open)
		{
			IsSoundOn = b_open;
		}
	}

	private AudioSource GetLoopAudioSource(string key)
	{
		if (asLoops.ContainsKey(key))
		{
			return asLoops[key];
		}
		GameObject gameObject = new GameObject(key);
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		asLoops[key] = audioSource;
		return audioSource;
	}

	private void Play(AudioSource audioSource, AudioClip clip, float volume = 1f, bool loop = false, float delay = 0f)
	{
		audioSource.clip = clip;
		audioSource.loop = loop;
		audioSource.time = 0f;
		audioSource.volume = volume;
		if (delay <= 0f)
		{
			audioSource.Play();
		}
		else
		{
			audioSource.PlayDelayed(delay);
		}
	}

	private void _PlayMusic()
	{
		if (audioInfos.ContainsKey(sKeyMusic) && IsMusicOn)
		{
			AudioInfo audioInfo = audioInfos[sKeyMusic];
			Play(asMusic, audioInfo.clip, audioInfo.volume, loop: true);
		}
	}

	public void PlayMusic(string skey)
	{
		if (!(sKeyMusic == skey))
		{
			sKeyMusic = skey;
			_PlayMusic();
		}
	}

	public void PlaySound(string skey)
	{
		if (!IsSoundOn)
		{
			return;
		}
		if (!audioInfos.ContainsKey(skey))
		{
			Debug.LogErrorFormat("AudioMgr play not key:{0}", skey);
			return;
		}
		nCurPlayingIndex = ((nCurPlayingIndex + 1 != nAudioPoolCount) ? (nCurPlayingIndex + 1) : 0);
		AudioSource audioSource = asSounds[nCurPlayingIndex];
		if (audioSource.isPlaying)
		{
			audioSource.Stop();
		}
		AudioInfo audioInfo = audioInfos[skey];
		if (audioInfo.isReady)
		{
			Play(audioSource, audioInfo.clip, audioInfo.volume);
			audioInfo.Waiting();
		}
	}

	public void PlaySoundLoop(string skey, string sAlias = null)
	{
		if (!IsSoundOn)
		{
			return;
		}
		if (!audioInfos.ContainsKey(skey))
		{
			Debug.LogErrorFormat("AudioMgr play not key:{0}", skey);
			return;
		}
		string key = skey;
		if (sAlias != null)
		{
			key = sAlias;
		}
		AudioSource loopAudioSource = GetLoopAudioSource(key);
		AudioInfo audioInfo = audioInfos[skey];
		Play(loopAudioSource, audioInfo.clip, audioInfo.volume, loop: true);
	}

	public void StopSoundLoop(string sAlias)
	{
		if (asLoops.ContainsKey(sAlias))
		{
			AudioSource audioSource = asLoops[sAlias];
			if (audioSource.isPlaying)
			{
				audioSource.Stop();
			}
			Object.Destroy(audioSource.gameObject);
			asLoops.Remove(sAlias);
		}
	}

	public void StopSoundAll()
	{
		foreach (AudioSource asSound in asSounds)
		{
			if (asSound.isPlaying)
			{
				asSound.Stop();
			}
		}
		foreach (KeyValuePair<string, AudioSource> asLoop in asLoops)
		{
			if (asLoop.Value.isPlaying)
			{
				asLoop.Value.Stop();
			}
		}
	}

	public void StopMusic()
	{
		sKeyMusic = "";
		if (asMusic.isPlaying)
		{
			asMusic.Stop();
		}
	}

	public void PauseMusic()
	{
		asMusic.Pause();
	}

	public void ResumeMusic()
	{
		if (!asMusic.isPlaying)
		{
			_PlayMusic();
		}
		else
		{
			asMusic.UnPause();
		}
	}

	public void StopMusicVolume()
	{
		asMusic.volume = 0f;
	}

	public void ResumeMusicVolume()
	{
		if (!audioInfos.ContainsKey(sKeyMusic))
		{
			asMusic.volume = 1f;
		}
		else
		{
			asMusic.volume = audioInfos[sKeyMusic].volume;
		}
	}
}
