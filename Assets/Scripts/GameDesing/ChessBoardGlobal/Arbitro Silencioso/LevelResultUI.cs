using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelResultUI : MonoBehaviour
{
    public static LevelResultUI Instance { get; private set; }

    [Header("Referencias UI")]
    public GameObject resultPanel;

    [Header("Llaves")]
    public TMP_Text keysText;
    
    public GameObject key1;
    public GameObject key2;
    public GameObject key3;
    public GameObject key1Enabled;
    public GameObject key2Enabled;
    public GameObject key3Enabled;

    [Header("Diamante")]
    public TMP_Text diamondText;
    public GameObject diamond;
    public GameObject diamondEnabled;

    [Header("Vidas")]
    public TMP_Text livesText;
    public GameObject up;
    public GameObject upEnabled;

    [Header("Score")]
    public TMP_Text scoreText;

    [Header("Ganó o perdió")]
    public TMP_Text winLoseText;

    [Header("Tiempo")]
    public TMP_Text timeLabelText; // 🔥 Nuevo: texto para el título ("Time")
    public TMP_Text timeValueText; // 🔥 Nuevo: texto para el tiempo formateado ("02:43")

    [Header("Botones")]
    public GameObject nextLevelButton;
    public GameObject tryAgainButton;
    public GameObject backToHomeButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        resultPanel.SetActive(false);
    }

    public void ShowResults(int keysCollected, bool hasDiamond, int livesRemaining, int totalScore)
    {

        gameObject.SetActive(true);
        // Si murió el Rey (o se acabaron vidas), esperar 3 segundos antes de mostrar resultados
        if (livesRemaining <= 0)
        {
            StartCoroutine(MostrarResultadosConRetraso(keysCollected, hasDiamond, livesRemaining, totalScore, 0.0001f));
        }
        else
        {
            MostrarResultadosInmediatos(keysCollected, hasDiamond, livesRemaining, totalScore);
        }
    }

    private IEnumerator MostrarResultadosConRetraso(int keysCollected, bool hasDiamond, int livesRemaining, int totalScore, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        MostrarResultadosInmediatos(keysCollected, hasDiamond, livesRemaining, totalScore);
    }

    private void MostrarResultadosInmediatos(int keysCollected, bool hasDiamond, int livesRemaining, int totalScore)
    {
        // Textos principales
        keysText.text = $"{keysCollected} / 3 keys";
        diamondText.text = hasDiamond ? "Diamond: Yes" : "Diamond: No";
        livesText.text = $"{livesRemaining} lives left";
        scoreText.text = $"Total Score: {totalScore}";

        // 🔥 Mostrar tiempo del nivel jugado
        float tiempoJugado = FindFirstObjectByType<ChessGameManager>().GetTiempoNivelAcumulado();
        int minutos = Mathf.FloorToInt(tiempoJugado / 60f);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60f);

        timeLabelText.text = "Time";
        timeValueText.text = $"{minutos:D2}:{segundos:D2}";

        // Llaves visual + Enabled
        key1Enabled.SetActive(true);
        key2Enabled.SetActive(true);
        key3Enabled.SetActive(true);

        key1.SetActive(keysCollected >= 1);
        key2.SetActive(keysCollected >= 2);
        key3.SetActive(keysCollected >= 3);

        // Diamante visual + Enabled
        diamondEnabled.SetActive(true);
        diamond.SetActive(hasDiamond);

        // UP visual + Enabled
        upEnabled.SetActive(true);
        up.SetActive(livesRemaining > 0);

        // Ganó o perdió
        bool gano = livesRemaining > 0;
        winLoseText.text = gano ? "¡You win!" : "Game Over";

        // Botones con reordenamiento
        if (gano)
        {
            MostrarBoton(nextLevelButton, true);
            MostrarBoton(tryAgainButton, true);
            MostrarBoton(backToHomeButton, true);

            nextLevelButton.transform.SetSiblingIndex(0);
            tryAgainButton.transform.SetSiblingIndex(1);
            backToHomeButton.transform.SetSiblingIndex(2);
        }
        else
        {
            MostrarBoton(nextLevelButton, false);
            MostrarBoton(tryAgainButton, true);
            MostrarBoton(backToHomeButton, true);

            tryAgainButton.transform.SetAsFirstSibling();
            backToHomeButton.transform.SetSiblingIndex(1);
            nextLevelButton.transform.SetSiblingIndex(2);
        }

        resultPanel.SetActive(true);

        Debug.Log($"🎉 Resultados mostrados -> Llaves: {keysCollected}, Diamante: {hasDiamond}, Vidas: {livesRemaining}, Score: {totalScore}, Tiempo: {minutos:D2}:{segundos:D2}");
    }

    
    private void MostrarBoton(GameObject boton, bool visible)
    {
        if (boton.TryGetComponent(out CanvasGroup cg))
        {
            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }
    }

    public void OnTryAgainClicked()
    {
        // Recarga la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnBackToHomeClicked()
    {
        // Carga la escena del menú principal
        SceneManager.LoadScene("GameHome"); // Asegúrate que el nombre coincide en Build Settings
    }

    public void OnNextLevelClicked()
    {
        // Ejemplo: cargar siguiente nivel según índice
        //int currentIndex = SceneManager.GetActiveScene().buildIndex;
        //SceneManager.LoadScene(currentIndex + 1);
    }
}