using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Effects")]
    public Sound[] sfxSounds;
    
    [Header("Background Music")]
    public Sound[] bgmSounds;
    
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;

    private Dictionary<string, Sound> sfxDictionary = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> bgmDictionary = new Dictionary<string, Sound>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        InitializeAudioSources();
        InitializeSoundDictionaries();
    }

    void InitializeAudioSources()
    {
        // Create SFX AudioSource if not assigned
        if (sfxSource == null)
        {
            GameObject sfxObject = new GameObject("SFX AudioSource");
            sfxObject.transform.SetParent(transform);
            sfxSource = sfxObject.AddComponent<AudioSource>();
        }

        // Create BGM AudioSource if not assigned
        if (bgmSource == null)
        {
            GameObject bgmObject = new GameObject("BGM AudioSource");
            bgmObject.transform.SetParent(transform);
            bgmSource = bgmObject.AddComponent<AudioSource>();
            bgmSource.loop = true; // BGM typically loops
        }
    }

    void InitializeSoundDictionaries()
    {
        // Initialize SFX dictionary
        foreach (Sound sound in sfxSounds)
        {
            if (sound != null && !string.IsNullOrEmpty(sound.name))
            {
                sfxDictionary[sound.name] = sound;
            }
        }

        // Initialize BGM dictionary
        foreach (Sound sound in bgmSounds)
        {
            if (sound != null && !string.IsNullOrEmpty(sound.name))
            {
                bgmDictionary[sound.name] = sound;
            }
        }
    }

    public void PlaySFX(string soundName)
    {
        if (sfxDictionary.ContainsKey(soundName))
        {
            Sound sound = sfxDictionary[soundName];
            sfxSource.PlayOneShot(sound.clip, sound.volume);
        }
        else
        {
            Debug.LogWarning($"SFX sound '{soundName}' not found!");
        }
    }

    public void PlayBGM(string soundName)
    {
        if (bgmDictionary.ContainsKey(soundName))
        {
            Sound sound = bgmDictionary[soundName];
            
            // Stop current BGM if playing
            if (bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }
            
            bgmSource.clip = sound.clip;
            bgmSource.volume = sound.volume;
            bgmSource.pitch = sound.pitch;
            bgmSource.loop = sound.loop;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM sound '{soundName}' not found!");
        }
    }

    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
