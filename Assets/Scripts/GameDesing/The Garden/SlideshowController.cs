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

        // Ocultar todos los paneles
        foreach (var p in panels) p.SetActive(false);
        // 🔒 Ocultar botón de cerrar al salir

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        // Aquí puedes regresar a GameHome u otra acción
        SceneManager.LoadScene(4);
        Debug.Log("✅ Presentación cerrada.");
    }
}
