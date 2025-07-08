using UnityEngine;
using TMPro;

public class PlayerScore : MonoBehaviour
{
    public static PlayerScore Instance { get; private set; }

    [Header("UI")]
    public TMP_Text scoreText;

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

    private void Start()
    {
        ActualizarHUD();
    }

    public void AddScore(int puntos)
    {
        totalScore += puntos;
        Debug.Log($"💰 Score actualizado: +{puntos} pts -> Total: {totalScore}");
        ActualizarHUD();
    }

    private void ActualizarHUD()
    {
        if (scoreText != null)
            scoreText.text = totalScore.ToString();
    }

    public int GetTotalScore()
    {
        return totalScore;
    }
}
