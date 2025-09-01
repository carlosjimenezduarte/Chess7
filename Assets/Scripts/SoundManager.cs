using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Clips de sonido (0 = movimiento aliado, 1 = ataque aliado, etc.)")]
    public AudioClip[] clips; // Aquí van tus 33 sonidos en orden

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D
            audioSource.ignoreListenerPause = true; 
        }
    }

    // Método genérico: puedes elegir por índice
    public void PlaySound(int index, float volume = 0.7f)
    {
        if (clips.Length > 0 && index >= 0 && index < clips.Length)
        {
            audioSource.PlayOneShot(clips[index], volume);
        }
    }
}
