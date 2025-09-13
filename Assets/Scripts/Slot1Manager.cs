using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Slot1Manager : MonoBehaviour
{
    [Header("Paneles del Slot 1")]
    public GameObject panelPlayFirst;
    public GameObject panelPlay;
    public GameObject panelReset;

    [Header("Texto de progreso global (opcional)")]
    public TMP_Text levelsProgressText; // 👈 arrastra aquí el mismo TMP del GameHome si quieres

    private string slotKey = "slot1_state";
    private const int TOTAL_NIVELES = 204;

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        string state = PlayerPrefs.GetString(slotKey, "empty");

        panelPlayFirst.SetActive(state == "empty");
        panelPlay.SetActive(state == "active");
        panelReset.SetActive(state == "reset_pending");
    }

    public void OnPlay()
    {
        PlayerPrefs.SetString("slotActivo", "slot1");
        PlayerPrefs.SetString(slotKey, "active");
        PlayerPrefs.Save();

        SceneManager.LoadScene(4); // Ir a GameHome
    }

    public void OnReiniciar()
    {
        PlayerPrefs.SetString(slotKey, "reset_pending");
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void OnConfirmYes()
    {
        BorrarProgresoSlot("slot1");

        PlayerPrefs.SetString(slotKey, "empty");
        PlayerPrefs.Save();

        Debug.Log("🧹 Slot1 completamente reiniciado.");
        UpdateUI();

        // 🔹 Refrescar visualmente el contador de progreso
        if (levelsProgressText != null)
            levelsProgressText.text = $"0/{TOTAL_NIVELES}";
    }

    public void OnConfirmNo()
    {
        PlayerPrefs.SetString(slotKey, "active");
        PlayerPrefs.Save();
        UpdateUI();
    }

    private void BorrarProgresoSlot(string slotId)
    {
        string[] claves = {
        "_nivelMax",
        "_scoreTotal",
        "_keysTotal",
        "_diamondsTotal",
        "_parchmentsTotal",
        "_trophiesTotal",
        "_medalsTotal",
        "_masterKeysTotal"
    };

        for (int lvl = 1; lvl <= 300; lvl++)
        {
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_completed");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_score");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_keys");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_diamond");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_diamonds");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_parchment");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_trophy");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_medal");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_masterKey");

            // ✅ Claves nuevas que sí usa ProgressSaver
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_bags");
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_crowns");
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_chests");
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_coins");
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsPawn"); // 👈 per-level best (anti-farmeo)
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsKnight"); // NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsBishop");
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsRook");   // 👈 NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsQueen");  // 👈 NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsRookBlack");   // 👈 NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsQueenBlack");  // 👈 NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsBishopBlack");  // NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsKnightBlack");  // NUEVO


            



        }


        foreach (var c in claves)
        {
            PlayerPrefs.DeleteKey(slotId + c);
        }

        // 🔹 Borrar estadísticas de Stadistics (los 4 objetos básicos)
        PlayerPrefs.DeleteKey(slotId + "_stats_" + TipoObjetoScore.Bag.ToString());
        PlayerPrefs.DeleteKey(slotId + "_stats_" + TipoObjetoScore.Chest.ToString());
        PlayerPrefs.DeleteKey(slotId + "_stats_" + TipoObjetoScore.RealCoin.ToString());
        PlayerPrefs.DeleteKey(slotId + "_stats_" + TipoObjetoScore.Crown.ToString());

        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_PawnRed"); // 👈 global visible en Stadistics
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_KnightRed"); // NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_BishopRed"); 
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_RookRed");   // 👈 NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_QueenRed");  // 👈 NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_RookBlack");   // 👈 NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_QueenBlack");  // 👈 NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_BishopBlack");  // NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_KnightBlack");  // NUEVO

        PlayerPrefs.DeleteKey($"{slotId}_lastRunDebt");

        
        PlayerPrefs.Save();
    }

}
