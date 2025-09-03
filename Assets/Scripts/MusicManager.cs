using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Canción de fondo de este nivel")]
    public AudioClip musicaNivel;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Si quieres que sobreviva entre escenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;       // 2D
        audioSource.ignoreListenerPause = true; 
    }

    public void PlayMusic(float volume = 0.4f)
    {
        if (musicaNivel == null) return;

        if (audioSource.clip == musicaNivel && audioSource.isPlaying)
            return;

        audioSource.clip = musicaNivel;
        audioSource.volume = volume;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
