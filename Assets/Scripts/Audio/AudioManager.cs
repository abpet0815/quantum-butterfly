using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Volume Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;
    
    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool playMusicOnStart = true;
    
    public enum SoundType
    {
        CardFlip,
        CardMatch,
        CardMismatch,
        GameWin,
        ButtonClick,
        ComboBonus
    }
    
    [System.Serializable]
    public class SoundEntry
    {
        public SoundType soundType;
        public AudioClip audioClip;
        [Range(0f, 2f)]
        public float volume = 1f;
        [Range(0.5f, 1.5f)]
        public float pitch = 1f;
    }
    
    [Header("Sound Effects")]
    [SerializeField] private List<SoundEntry> soundEffects = new List<SoundEntry>();
    
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (playMusicOnStart && backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
        
        LoadVolumeSettings();
    }
    
    private void InitializeAudio()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        
        UpdateVolumeSettings();
    }
    
    public void PlaySound(SoundType soundType)
    {
        SoundEntry soundEntry = soundEffects.Find(s => s.soundType == soundType);
        
        if (soundEntry != null && soundEntry.audioClip != null && sfxSource != null)
        {
            sfxSource.pitch = soundEntry.pitch;
            sfxSource.PlayOneShot(soundEntry.audioClip, soundEntry.volume * sfxVolume * masterVolume);
            
            // Reset pitch for next sound
            sfxSource.pitch = 1f;
        }
        else
        {
            Debug.LogWarning($"Sound effect not found or not configured: {soundType}");
        }
    }
    
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip != null && musicSource != null)
        {
            if (musicSource.clip == musicClip && musicSource.isPlaying)
                return; // Already playing this track
                
            musicSource.clip = musicClip;
            musicSource.Play();
            Debug.Log($"🎵 Playing music: {musicClip.name}");
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("🎵 Music stopped");
        }
    }
    
    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.UnPause();
        }
    }
    
    // Volume control methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
        SaveVolumeSettings();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
        SaveVolumeSettings();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
        SaveVolumeSettings();
    }
    
    private void UpdateVolumeSettings()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
        // SFX volume is applied per-sound in PlaySound method
    }
    
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        UpdateVolumeSettings();
    }
    
    // Public getters for UI
    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public bool IsMusicPlaying() => musicSource != null && musicSource.isPlaying;
}
