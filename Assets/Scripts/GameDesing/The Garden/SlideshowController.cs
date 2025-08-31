using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SlideshowController : MonoBehaviour
{
    [Header("Configuración de música")]
    public AudioSource musicSource;  // 🎶 Arrastrar aquí el AudioSource del MusicPanel
    public AudioClip musicClip;      // 🎵 Arrastrar aquí el .wav desde Assets

    [Header("Paneles del slideshow")]
    public GameObject[] panels;      // Tus 40 paneles en orden
    public float panelDuration = 5f; // 5 segundos cada uno

    [Header("Botón de cerrar")]
    public Button closeButton;

    private int currentIndex = 0;
    private bool isPlaying = false;

    private void Start()
    {
        // Ocultar todos los paneles al inicio
        foreach (var p in panels) p.SetActive(false);

        if (closeButton != null)
            closeButton.gameObject.SetActive(false); // 🔒 botón oculto al inicio

        if (closeButton != null)
            closeButton.onClick.AddListener(CerrarPresentacion);
    }

    public void IniciarPresentacion()
    {
        if (isPlaying) return;

        isPlaying = true;
        currentIndex = 0;

        // 🔊 Reproducir música
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
        
        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        // Iniciar slideshow
        StartCoroutine(ReproducirSlideshow());
    }

    private System.Collections.IEnumerator ReproducirSlideshow()
    {
        while (currentIndex < panels.Length && isPlaying)
        {
            panels[currentIndex].SetActive(true);

            yield return new WaitForSeconds(panelDuration);

            // ❌ No apagar el último panel (logo final)
            if (currentIndex < panels.Length - 1)
                panels[currentIndex].SetActive(false);

            currentIndex++;
        }

        // Termina automáticamente
        CerrarPresentacion();
    }

    private void CerrarPresentacion()
    {
        isPlaying = false;

        // Detener música
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();

        // Ocultar todos los paneles excepto el último
        for (int i = 0; i < panels.Length - 1; i++)
        {
            if (panels[i] != null)
                panels[i].SetActive(false);
        }

        // 🔹 Mantener el último panel visible (logo + link)
        if (panels.Length > 0 && panels[panels.Length - 1] != null)
            panels[panels.Length - 1].SetActive(true);

        // 🔒 Ocultar botón de cerrar al salir
        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        // Cambiar a GameHome sin "parpadeo del Jardín"
        SceneManager.LoadScene(4);

        Debug.Log("✅ Presentación cerrada en último panel (logo visible).");
    }
}
