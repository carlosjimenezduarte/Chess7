using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public static PlayerScore Instance { get; private set; }

    private int totalScore = 0;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Agrega puntos al score total.
    /// </summary>
    public void AgregarPuntaje(int puntos)
    {
        totalScore += puntos;
        Debug.Log($"💰 Score actualizado: +{puntos} pts -> Total: {totalScore}");
    }

    /// <summary>
    /// Devuelve el score acumulado.
    /// </summary>
    public int GetTotalScore()
    {
        return totalScore;
    }

    /// <summary>
    /// Reinicia el score (por ejemplo, al iniciar un nivel).
    /// </summary>
    public void ResetScore()
    {
        totalScore = 0;
        Debug.Log("🔄 Score reiniciado a 0.");
    }
}
