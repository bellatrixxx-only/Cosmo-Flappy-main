using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Настройки музыки")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip explosionSound;

    [Header("Звуки")]
    [SerializeField] private AudioClip fireSound;

    
    private AudioSource fireSource;
    private AudioSource explosionSource;

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
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
        }
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.2f;

        if (fireSound != null)
        {
            GameObject fireObj = new GameObject("FireSoundSource");
            fireObj.transform.SetParent(transform);

          
            fireSource = fireObj.AddComponent<AudioSource>();
            fireSource.clip = fireSound;
            fireSource.playOnAwake = false;
            fireSource.volume = 0.3f;
        }

        if (explosionSound != null)
        {
            GameObject explosionObj = new GameObject("ExplosionSoundSource");
            explosionObj.transform.SetParent(transform);
            explosionSource = explosionObj.AddComponent<AudioSource>();
            explosionSource.clip = explosionSound;
            explosionSource.playOnAwake = false;
            explosionSource.volume = 10f; 
        }
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
       
        if (fireSource != null && fireSource.clip != null)
        {
            fireSource.PlayOneShot(fireSound);
        }
    }
    public void PlayExplosionSound()
    {
        if (explosionSource != null && explosionSound != null)
        {
            explosionSource.PlayOneShot(explosionSound);
        }
    }
}