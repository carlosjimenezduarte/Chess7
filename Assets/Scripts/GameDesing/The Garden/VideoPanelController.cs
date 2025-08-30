using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoPanelController : MonoBehaviour
{
    public GameObject panelVideo;
    public VideoPlayer videoPlayer;
    public Button closeButton;

    private void Start()
    {
        panelVideo.SetActive(false); // 🔒 Oculto al inicio
        closeButton.onClick.AddListener(CerrarVideo);
        videoPlayer.loopPointReached += OnVideoEnd; // callback cuando termina
    }

    public void AbrirVideo()
    {
        panelVideo.SetActive(true);
        videoPlayer.Play();
    }

    public void CerrarVideo()
    {
        videoPlayer.Stop();
        panelVideo.SetActive(false);

        // Regresar a la escena GameHome (#4)
        SceneManager.LoadScene(4);
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        panelVideo.SetActive(false);

        // Cuando acaba el video automáticamente -> volver a GameHome
        SceneManager.LoadScene(4);
    }
}
