using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelResultUI : MonoBehaviour
{
    public static LevelResultUI Instance { get; private set; }

    [Header("Referencias UI")]
    public GameObject resultPanel;

    [Header("Condecoracion")]
    public TMP_Text CondecorationText;
    public GameObject Condecoracion;
    public GameObject CondecoracionEnabled;

    [Header("Ganar o perder")]
    public GameObject youWinText;
    public GameObject youLoseText; 

    [Header("Pergamino")]   
    public GameObject ParchmentText;
    public GameObject Parchment2Text; 

    public GameObject Pergamino;
    public GameObject PergaminoEnabled;

    [Header("Trofeo")]

    public TMP_Text TrophyText;

    public GameObject Trophy;
    public GameObject TrophyEnabled;

    [Header("Llaves")]
    public TMP_Text keysText;
    
    public GameObject key1;
    public GameObject key2;
    public GameObject key3;
    public GameObject key1Enabled;
    public GameObject key2Enabled;
    public GameObject key3Enabled;

    [Header("Diamante")]
    //public TMP_Text diamondText;
    public GameObject diamond;
    public GameObject diamondEnabled;

    [Header("Vidas")]
    public TMP_Text livesText;
    public GameObject up;
    public GameObject upEnabled;

    [Header("Score")]
    public TMP_Text scoreText;

    [Header("Ganó o perdió")]
    //public TMP_Text winLoseText;
    //public TMP_Text winLoseText2;
    

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
        // 🔹 Tiempo
        float tiempoJugado = FindFirstObjectByType<ChessGameManager>().GetTiempoNivelAcumulado();
        int minutos = Mathf.FloorToInt(tiempoJugado / 60f);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60f);

        timeValueText.text = $"{minutos:D2}:{segundos:D2}";
        // (El label "Time" está fijo en Unity para traducirse)

        // 🔹 Llaves (solo en niveles normales)
        key1Enabled.SetActive(true);
        key2Enabled.SetActive(true);
        key3Enabled.SetActive(true);

        key1.SetActive(keysCollected >= 1);
        key2.SetActive(keysCollected >= 2);
        key3.SetActive(keysCollected >= 3);

        // 🔹 Diamante
        diamondEnabled.SetActive(true);
        diamond.SetActive(hasDiamond);
        // (El texto "Diamond" está fijo en Unity, no lo tocamos aquí)

        // 🔹 Vidas
        upEnabled.SetActive(true);
        up.SetActive(livesRemaining > 0);
        livesText.text = $"{livesRemaining}";
        // (El label "Lives" está fijo en Unity, aquí solo la cifra)

        // 🔹 Score
        scoreText.text = $"{totalScore}";
        // (El label "Total Score" está fijo en Unity, aquí solo la cifra)

        // 🔹 Ganó o perdió
        bool gano = livesRemaining > 0;

        youWinText.SetActive(gano);   // ← GameObject TMP en Unity
        youLoseText.SetActive(!gano); // ← GameObject TMP en Unity

        // 🔹 Botones
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

        Debug.Log($"🎉 Resultados -> Llaves: {keysCollected}, Diamante: {hasDiamond}, Vidas: {livesRemaining}, Score: {totalScore}, Tiempo: {minutos:D2}:{segundos:D2}");
    }

    public void ShowParchmentResult(int totalScore, int livesRemaining, bool hasDiamond, bool pergaminoObtenido)
    {
        // 🔹 Ocultar lo que no aplica
        key1.SetActive(false);
        key2.SetActive(false);
        key3.SetActive(false);
        key1Enabled.SetActive(false);
        key2Enabled.SetActive(false);
        key3Enabled.SetActive(false);

        Trophy.SetActive(false);
        TrophyEnabled.SetActive(false);
        TrophyText.gameObject.SetActive(false);

        Condecoracion.SetActive(false);
        CondecoracionEnabled.SetActive(false);
        CondecorationText.gameObject.SetActive(false);

        // 🔹 Mostrar pergamino según el estado
        PergaminoEnabled.SetActive(true);
        Pergamino.SetActive(pergaminoObtenido);     // Solo visible si lo obtuvo
        ParchmentText.SetActive(pergaminoObtenido); // Texto “obtenido”
        Parchment2Text.SetActive(!pergaminoObtenido); // Texto “no obtenido”

        // 🔹 Tiempo
        float tiempoJugado = FindFirstObjectByType<ChessGameManager>().GetTiempoNivelAcumulado();
        int minutos = Mathf.FloorToInt(tiempoJugado / 60f);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60f);
        timeValueText.text = $"{minutos:D2}:{segundos:D2}";

        // 🔹 Diamante
        diamondEnabled.SetActive(true);
        diamond.SetActive(hasDiamond);

        // 🔹 Vidas
        upEnabled.SetActive(true);
        up.SetActive(livesRemaining > 0);
        livesText.text = $"{livesRemaining}";

        // 🔹 Score
        scoreText.text = $"{totalScore}";

        // 🔹 Ganó o perdió
        bool gano = livesRemaining > 0;
        youWinText.SetActive(gano);
        youLoseText.SetActive(!gano);

        // 🔹 Botones (igual que antes)
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

        Debug.Log($"📜 Resultado Pergamino -> {(pergaminoObtenido ? "Obtenido" : "No obtenido")}, Diamante: {hasDiamond}, Vidas: {livesRemaining}, Score: {totalScore}, Tiempo: {minutos:D2}:{segundos:D2}");
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