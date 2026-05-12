using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Музыка")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip musicClip;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.7f;

    [Header("Звуковые эффекты")]
    [SerializeField] private AudioClip fireSound;
    [Range(0f, 1f)][SerializeField] private float fireVolume = 0.5f;

    [SerializeField] private AudioClip explosionSound;
    [Range(0f, 1f)][SerializeField] private float explosionVolume = 0.8f;

    [SerializeField] private AudioClip bonusPickupClip;
    [Range(0f, 1f)][SerializeField] private float bonusVolume = 0.4f;

    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
            PlayMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = 1f; 
    }

    public void PlayMusic()
    {
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.volume = musicVolume;
            musicSource.time = 0f;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void RestartMusic()
    {
        StopMusic();
        PlayMusic();
    }

   
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void PlayFireSound()
    {
        PlaySFX(fireSound, fireVolume);
    }

    public void PlayExplosionSound()
    {
        PlaySFX(explosionSound, explosionVolume);
    }

    public void PlayBonusPickupSound()
    {
        PlaySFX(bonusPickupClip, bonusVolume);
    }

    
    public void SetFireVolume(float volume)
    {
        fireVolume = Mathf.Clamp01(volume);
    }

    public void SetExplosionVolume(float volume)
    {
        explosionVolume = Mathf.Clamp01(volume);
    }

    public void SetBonusVolume(float volume)
    {
        bonusVolume = Mathf.Clamp01(volume);
    }

    private void PlaySFX(AudioClip clip, float volume)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}