using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Музыка")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip musicClip;

    [Header("Звуковые эффекты")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip bonusPickupClip;
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
        musicSource.volume = 0.2f;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = 5f; 
    }

    public void PlayMusic()
    {
        if (musicSource != null && musicSource.clip != null)
        {
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

    public void PlayFireSound()
    {
        PlaySFX(fireSound);
    }

    public void PlayExplosionSound()
    {
        PlaySFX(explosionSound);
    }

    public void PlayBonusPickupSound()
    {
        PlaySFX(bonusPickupClip);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}