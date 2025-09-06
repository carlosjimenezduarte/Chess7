using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(int sceneIndex)
    {
        Debug.Log("🎵 Sonando antes de cargar escena...");
        SoundManager.Instance.PlaySound(8); // 🔊 Primero sonido
        StartCoroutine(LoadSceneDelay(sceneIndex, 0.5f)); // ⏳ Luego esperamos un poco
    }

    private IEnumerator LoadSceneDelay(int sceneIndex, float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Ignora el timeScale
        SceneManager.LoadScene(sceneIndex); // 🔄 Carga la escena después
    }
}
